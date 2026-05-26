using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using CoffeeShop.Wpf.Infrastructure;

namespace CoffeeShop.Wpf.Services;

/// <summary>
/// Client xử lý trực tiếp QR payment trên Database local và gọi trực tiếp PayOS API online.
/// Giúp dự án WPF chạy hoàn toàn độc lập và tích hợp trực tiếp với PayOS của người dùng.
/// </summary>
public class PaymentApiClient
{
    private readonly string _connectionString;
    private readonly HttpClient _httpClient;

    public PaymentApiClient(string? baseUrl = null)
    {
        _connectionString = DbConnectionFactory.ConnectionString;

        // Khởi tạo HttpClient chấp nhận SSL bypass (phù hợp môi trường dev/local)
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private class PayOsCredentials
    {
        public string ClientId { get; }
        public string ApiKey { get; }
        public string ChecksumKey { get; }
        public string BaseUrl { get; }
        public bool HasValue => !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ChecksumKey);

        public PayOsCredentials(string clientId, string apiKey, string checksumKey, string baseUrl)
        {
            ClientId = clientId.Trim();
            ApiKey = apiKey.Trim();
            ChecksumKey = checksumKey.Trim();
            BaseUrl = baseUrl.Trim();
        }
    }

    private async Task<PayOsCredentials> GetPayOsCredentialsAsync(CancellationToken cancellationToken)
    {
        var configClientId = ConfigurationManager.AppSettings["PayOsClientId"] ?? string.Empty;
        var configApiKey = ConfigurationManager.AppSettings["PayOsApiKey"] ?? string.Empty;
        var configChecksumKey = ConfigurationManager.AppSettings["PayOsChecksumKey"] ?? string.Empty;
        var baseUrl = ConfigurationManager.AppSettings["PayOsBaseUrl"] ?? "https://api-merchant.payos.vn";

        if (string.IsNullOrEmpty(_connectionString))
        {
            return new PayOsCredentials(configClientId, configApiKey, configChecksumKey, baseUrl);
        }

        try
        {
            const string sql = @"
            SELECT TOP (1) PayOsClientId, PayOsApiKey, PayOsChecksumKey
            FROM dbo.CauHinhHeThong
            ORDER BY CauHinhHeThongId DESC;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            const string checkColSql = @"
            IF COL_LENGTH(N'dbo.CauHinhHeThong', N'PayOsClientId') IS NULL
            BEGIN
                ALTER TABLE dbo.CauHinhHeThong ADD PayOsClientId NVARCHAR(150) NULL;
            END
            IF COL_LENGTH(N'dbo.CauHinhHeThong', N'PayOsApiKey') IS NULL
            BEGIN
                ALTER TABLE dbo.CauHinhHeThong ADD PayOsApiKey NVARCHAR(150) NULL;
            END
            IF COL_LENGTH(N'dbo.CauHinhHeThong', N'PayOsChecksumKey') IS NULL
            BEGIN
                ALTER TABLE dbo.CauHinhHeThong ADD PayOsChecksumKey NVARCHAR(200) NULL;
            END";
            await using (var checkCmd = new SqlCommand(checkColSql, connection))
            {
                await checkCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var dbClientId = reader.IsDBNull(reader.GetOrdinal("PayOsClientId")) ? null : reader.GetString(reader.GetOrdinal("PayOsClientId"));
                var dbApiKey = reader.IsDBNull(reader.GetOrdinal("PayOsApiKey")) ? null : reader.GetString(reader.GetOrdinal("PayOsApiKey"));
                var dbChecksumKey = reader.IsDBNull(reader.GetOrdinal("PayOsChecksumKey")) ? null : reader.GetString(reader.GetOrdinal("PayOsChecksumKey"));

                if (!string.IsNullOrEmpty(dbClientId) && !string.IsNullOrEmpty(dbApiKey) && !string.IsNullOrEmpty(dbChecksumKey))
                {
                    return new PayOsCredentials(dbClientId, dbApiKey, dbChecksumKey, baseUrl);
                }
            }
        }
        catch
        {
            // Bỏ qua lỗi DB, dùng mặc định
        }

        return new PayOsCredentials(configClientId, configApiKey, configChecksumKey, baseUrl);
    }

    /// <summary>
    /// Thuộc tính BaseUrl giả lập (để tương thích ngược với ViewModel cũ)
    /// </summary>
    public string BaseUrl => "http://localhost-local-db-bypass";

    /// <summary>
    /// Tạo QR payment cho hóa đơn (gọi trực tiếp PayOS API để tạo link thanh toán thật)
    /// </summary>
    public async Task<CreateQRPaymentResponse?> CreateQrPaymentAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Database connection string has not been initialized.");
            }

            decimal thanhToan = 0;
            string? trangThaiThanhToan = null;
            string? paymentStatus = null;
            DateTime? qrExpiredAt = null;
            string? qrCodeRaw = null;
            string? checkoutUrl = null;
            string? providerPaymentId = null;
            long? providerOrderCode = null;

            // 1. Lấy thông tin hóa đơn hiện tại
            const string selectSql = @"
                SELECT ThanhToan, TrangThaiThanhToan, PaymentStatus, QRExpiredAt, 
                       QRCodeRaw, CheckoutUrl, ProviderPaymentId, ProviderOrderCode
                FROM dbo.HoaDonBan
                WHERE HoaDonBanId = @HoaDonBanId";

            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(selectSql, connection);
                command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    thanhToan = reader.GetDecimal(reader.GetOrdinal("ThanhToan"));
                    trangThaiThanhToan = reader.IsDBNull(reader.GetOrdinal("TrangThaiThanhToan")) ? null : reader.GetString(reader.GetOrdinal("TrangThaiThanhToan"));
                    paymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus")) ? null : reader.GetString(reader.GetOrdinal("PaymentStatus"));
                    qrExpiredAt = reader.IsDBNull(reader.GetOrdinal("QRExpiredAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("QRExpiredAt"));
                    qrCodeRaw = reader.IsDBNull(reader.GetOrdinal("QRCodeRaw")) ? null : reader.GetString(reader.GetOrdinal("QRCodeRaw"));
                    checkoutUrl = reader.IsDBNull(reader.GetOrdinal("CheckoutUrl")) ? null : reader.GetString(reader.GetOrdinal("CheckoutUrl"));
                    providerPaymentId = reader.IsDBNull(reader.GetOrdinal("ProviderPaymentId")) ? null : reader.GetString(reader.GetOrdinal("ProviderPaymentId"));
                    providerOrderCode = reader.IsDBNull(reader.GetOrdinal("ProviderOrderCode")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("ProviderOrderCode"));
                }
                else
                {
                    throw new InvalidOperationException($"Không tìm thấy hóa đơn {hoaDonBanId}");
                }
            }

            // 2. Kiểm tra trạng thái thanh toán
            if (paymentStatus == "PAID" || trangThaiThanhToan == "Đã thanh toán")
            {
                throw new InvalidOperationException("Hóa đơn đã được thanh toán rồi.");
            }

            // 3. Nếu đã có QR active (chưa hết hạn), trả về QR cũ
            bool hasActiveQr = paymentStatus == "PENDING" 
                && qrExpiredAt.HasValue 
                && qrExpiredAt.Value > DateTime.Now;

            if (hasActiveQr && !string.IsNullOrEmpty(qrCodeRaw) && providerOrderCode.HasValue)
            {
                return new CreateQRPaymentResponse
                {
                    HoaDonBanId = hoaDonBanId,
                    PaymentProvider = "payOS",
                    ProviderPaymentId = providerPaymentId,
                    ProviderOrderCode = providerOrderCode.Value,
                    QRCodeRaw = qrCodeRaw,
                    CheckoutUrl = checkoutUrl ?? qrCodeRaw,
                    PaymentStatus = "PENDING",
                    QRExpiredAt = qrExpiredAt,
                    Amount = thanhToan,
                    Description = $"HD{hoaDonBanId:D5}"
                };
            }

            // 4. Sinh mã orderCode duy nhất cho PayOS
            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            long orderCode = timestamp * 100000 + hoaDonBanId;
            var description = $"HD{hoaDonBanId:D5}";
            var expiredAt = DateTime.Now.AddMinutes(10);
            var expiredAtUnix = ((DateTimeOffset)expiredAt).ToUnixTimeSeconds();
            var amount = (int)thanhToan;

            string qrCodeUrl = string.Empty;
            string payOsCheckoutUrl = string.Empty;
            string paymentLinkId = string.Empty;

            // Lấy credentials động từ DB/Config
            var credentials = await GetPayOsCredentialsAsync(cancellationToken);

            if (credentials.HasValue)
            {
                // Gọi PayOS API online để tạo payment request thật
                var items = new List<PayOsItem>
                {
                    new PayOsItem { Name = description, Quantity = 1, Price = amount }
                };

                // Đường dẫn return/cancel tượng trưng vì WPF không cần webhook
                string returnUrl = "https://localhost:5001/payment/success";
                string cancelUrl = "https://localhost:5001/payment/cancel";

                // Tạo signature theo chuẩn PayOS
                var signatureData = new Dictionary<string, object?>
                {
                    { "amount", amount },
                    { "cancelUrl", cancelUrl },
                    { "description", description },
                    { "orderCode", orderCode },
                    { "returnUrl", returnUrl }
                };
                var signature = CreatePayOsSignature(signatureData, credentials.ChecksumKey);

                var requestPayload = new PayOsCreatePaymentRequest
                {
                    OrderCode = orderCode,
                    Amount = amount,
                    Description = description,
                    Items = items,
                    CancelUrl = cancelUrl,
                    ReturnUrl = returnUrl,
                    ExpiredAt = expiredAtUnix,
                    Signature = signature
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-client-id", credentials.ClientId);
                _httpClient.DefaultRequestHeaders.Add("x-api-key", credentials.ApiKey);

                var jsonRequest = JsonConvert.SerializeObject(requestPayload);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{credentials.BaseUrl}/v2/payment-requests", content, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"PayOS API returned failure. HTTP {(int)response.StatusCode}: {responseContent}");
                }

                var payOsResult = JsonConvert.DeserializeObject<PayOsCreatePaymentResponse>(responseContent);
                if (payOsResult?.Data == null)
                {
                    throw new InvalidOperationException("PayOS API returned empty data.");
                }

                qrCodeUrl = payOsResult.Data.QrCode ?? string.Empty;
                payOsCheckoutUrl = payOsResult.Data.CheckoutUrl ?? string.Empty;
                paymentLinkId = payOsResult.Data.PaymentLinkId ?? string.Empty;
            }
            else
            {
                // Fallback tạo VietQR local offline nếu chưa cấu hình PayOS key
                var encodedAddInfo = Uri.EscapeDataString($"HD{orderCode}");
                var encodedAccountName = Uri.EscapeDataString("NGUYEN VAN A");
                qrCodeUrl = $"https://img.vietqr.io/image/970422-0123456789-compact.jpg?amount={amount}&addInfo={encodedAddInfo}&accountName={encodedAccountName}";
                payOsCheckoutUrl = qrCodeUrl;
                paymentLinkId = $"VIETQR-{orderCode}";
            }

            // 5. Cập nhật thông tin thanh toán vào database
            const string updateSql = @"
                UPDATE dbo.HoaDonBan
                SET PaymentProvider = 'payOS',
                    ProviderPaymentId = @ProviderPaymentId,
                    ProviderOrderCode = @ProviderOrderCode,
                    QRCodeRaw = @QRCodeRaw,
                    CheckoutUrl = @CheckoutUrl,
                    QRExpiredAt = @QRExpiredAt,
                    PaymentStatus = 'PENDING',
                    HinhThucThanhToan = N'QR Code'
                WHERE HoaDonBanId = @HoaDonBanId";

            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(updateSql, connection);
                command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                command.Parameters.AddWithValue("@ProviderPaymentId", paymentLinkId);
                command.Parameters.AddWithValue("@ProviderOrderCode", orderCode);
                command.Parameters.AddWithValue("@QRCodeRaw", qrCodeUrl);
                command.Parameters.AddWithValue("@CheckoutUrl", payOsCheckoutUrl);
                command.Parameters.AddWithValue("@QRExpiredAt", expiredAt);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            return new CreateQRPaymentResponse
            {
                HoaDonBanId = hoaDonBanId,
                PaymentProvider = "payOS",
                ProviderPaymentId = paymentLinkId,
                ProviderOrderCode = orderCode,
                QRCodeRaw = qrCodeUrl,
                CheckoutUrl = payOsCheckoutUrl,
                PaymentStatus = "PENDING",
                QRExpiredAt = expiredAt,
                Amount = thanhToan,
                Description = description
            };
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Tạo thanh toán thất bại: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Lấy trạng thái thanh toán của hóa đơn (Tự động gọi PayOS đối soát trạng thái)
    /// </summary>
    public async Task<PaymentStatusResponse?> GetPaymentStatusAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT HoaDonBanId, ThanhToan, TrangThaiThanhToan, MaGiaoDich,
                   PaymentProvider, ProviderPaymentId, ProviderOrderCode,
                   PaymentStatus, PaymentConfirmedAt, QRExpiredAt
            FROM dbo.HoaDonBan
            WHERE HoaDonBanId = @HoaDonBanId";

        try
        {
            PaymentStatusResponse? status = null;

            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    var paymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus")) ? null : reader.GetString(reader.GetOrdinal("PaymentStatus"));
                    var qrExpiredAt = reader.IsDBNull(reader.GetOrdinal("QRExpiredAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("QRExpiredAt"));

                    if (paymentStatus == "PENDING" && qrExpiredAt.HasValue && qrExpiredAt.Value < DateTime.Now)
                    {
                        paymentStatus = "EXPIRED";
                    }

                    status = new PaymentStatusResponse
                    {
                        HoaDonBanId = reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
                        ThanhToan = reader.GetDecimal(reader.GetOrdinal("ThanhToan")),
                        TrangThaiThanhToan = reader.IsDBNull(reader.GetOrdinal("TrangThaiThanhToan")) ? "Chưa thanh toán" : reader.GetString(reader.GetOrdinal("TrangThaiThanhToan")),
                        MaGiaoDich = reader.IsDBNull(reader.GetOrdinal("MaGiaoDich")) ? null : reader.GetString(reader.GetOrdinal("MaGiaoDich")),
                        PaymentProvider = reader.IsDBNull(reader.GetOrdinal("PaymentProvider")) ? null : reader.GetString(reader.GetOrdinal("PaymentProvider")),
                        ProviderPaymentId = reader.IsDBNull(reader.GetOrdinal("ProviderPaymentId")) ? null : reader.GetString(reader.GetOrdinal("ProviderPaymentId")),
                        ProviderOrderCode = reader.IsDBNull(reader.GetOrdinal("ProviderOrderCode")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("ProviderOrderCode")),
                        PaymentStatus = paymentStatus,
                        PaymentConfirmedAt = reader.IsDBNull(reader.GetOrdinal("PaymentConfirmedAt")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("PaymentConfirmedAt")),
                        QRExpiredAt = qrExpiredAt
                    };
                }
            }

            // Nếu DB đang ghi là PENDING và có mã đơn hàng, tự động gọi PayOS API online để đối soát
            if (status != null && status.PaymentStatus == "PENDING" && status.ProviderOrderCode.HasValue)
            {
                var credentials = await GetPayOsCredentialsAsync(cancellationToken);
                if (credentials.HasValue)
                {
                    try
                    {
                        _httpClient.DefaultRequestHeaders.Clear();
                        _httpClient.DefaultRequestHeaders.Add("x-client-id", credentials.ClientId);
                        _httpClient.DefaultRequestHeaders.Add("x-api-key", credentials.ApiKey);

                        var response = await _httpClient.GetAsync($"{credentials.BaseUrl}/v2/payment-requests/{status.ProviderOrderCode.Value}", cancellationToken);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync(cancellationToken);
                        var payOsQuery = JsonConvert.DeserializeObject<PayOsQueryPaymentResponse>(body);
                        var serverStatus = payOsQuery?.Data?.Status?.ToUpper();

                        if (serverStatus == "PAID" || serverStatus == "SUCCESS")
                        {
                            // Thực hiện xác nhận thanh toán local thành công trực tiếp
                            var success = await ConfirmPaymentManualAsync(hoaDonBanId, cancellationToken);
                            if (success)
                            {
                                // Load lại trạng thái mới từ database
                                return await GetPaymentStatusAsync(hoaDonBanId, cancellationToken);
                            }
                        }
                        else if (serverStatus == "EXPIRED")
                        {
                            // Cập nhật DB cục bộ thành EXPIRED
                            const string expireSql = "UPDATE dbo.HoaDonBan SET PaymentStatus = 'EXPIRED' WHERE HoaDonBanId = @Id";
                            await using var connection = new SqlConnection(_connectionString);
                            await connection.OpenAsync(cancellationToken);
                            await using var cmd = new SqlCommand(expireSql, connection);
                            cmd.Parameters.AddWithValue("@Id", hoaDonBanId);
                            await cmd.ExecuteNonQueryAsync(cancellationToken);
                            status.PaymentStatus = "EXPIRED";
                        }
                    }
                }
                catch
                {
                    // Bỏ qua lỗi kết nối đối soát, giữ trạng thái PENDING cục bộ
                }
            }
            }

            return status;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Hủy QR payment
    /// </summary>
    public async Task<bool> CancelPaymentAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.HoaDonBan
            SET PaymentStatus = 'CANCELLED'
            WHERE HoaDonBanId = @HoaDonBanId
              AND (PaymentStatus IS NULL OR PaymentStatus IN ('PENDING', 'PROCESSING'))
              AND (QRExpiredAt IS NULL OR QRExpiredAt > GETDATE())";

        try
        {
            // Lấy ProviderPaymentId trước khi cancel
            string? providerPaymentId = null;
            const string selectPaymentId = "SELECT ProviderPaymentId FROM dbo.HoaDonBan WHERE HoaDonBanId = @Id";
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await using var cmd = new SqlCommand(selectPaymentId, connection);
                cmd.Parameters.AddWithValue("@Id", hoaDonBanId);
                providerPaymentId = await cmd.ExecuteScalarAsync(cancellationToken) as string;
            }

            // Gọi PayOS API online để hủy thanh toán nếu có cấu hình
            if (!string.IsNullOrEmpty(providerPaymentId) && !providerPaymentId.StartsWith("VIETQR-"))
            {
                var credentials = await GetPayOsCredentialsAsync(cancellationToken);
                if (credentials.HasValue)
                {
                    try
                    {
                        _httpClient.DefaultRequestHeaders.Clear();
                        _httpClient.DefaultRequestHeaders.Add("x-client-id", credentials.ClientId);
                        _httpClient.DefaultRequestHeaders.Add("x-api-key", credentials.ApiKey);

                        // PayOS API: POST /v2/payment-requests/{id}/cancel
                        var response = await _httpClient.PostAsync($"{credentials.BaseUrl}/v2/payment-requests/{providerPaymentId}/cancel", null, cancellationToken);
                    response.EnsureSuccessStatusCode();
                }
                catch
                {
                    // Tiếp tục cập nhật database local kể cả gọi API hủy thất bại
                }
            }
            }

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, conn);
            command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Giả lập xác nhận thanh toán thành công trực tiếp (thay thế webhook của API)
    /// </summary>
    public async Task<bool> ConfirmPaymentManualAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Kiểm tra hóa đơn có tồn tại và đang ở trạng thái chờ thanh toán không
                const string checkSql = @"
                    SELECT HoaDonBanId, TrangThaiThanhToan, PaymentStatus, CreatedByUserId,
                           KhachHangId, DiemSuDung, DiemCong, ProviderOrderCode
                    FROM dbo.HoaDonBan WITH (UPDLOCK, HOLDLOCK)
                    WHERE HoaDonBanId = @HoaDonBanId";

                int createdByUserId;
                int? khachHangId;
                int diemSuDung;
                int diemCong;
                string? trangThaiThanhToan;
                string? paymentStatus;
                long? providerOrderCode;

                await using (var checkCmd = new SqlCommand(checkSql, connection, (SqlTransaction)transaction))
                {
                    checkCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                    await using var reader = await checkCmd.ExecuteReaderAsync(cancellationToken);
                    
                    if (!await reader.ReadAsync(cancellationToken))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return false;
                    }

                    trangThaiThanhToan = reader.IsDBNull(reader.GetOrdinal("TrangThaiThanhToan")) ? null : reader.GetString(reader.GetOrdinal("TrangThaiThanhToan"));
                    paymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus")) ? null : reader.GetString(reader.GetOrdinal("PaymentStatus"));
                    createdByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId"));
                    khachHangId = reader.IsDBNull(reader.GetOrdinal("KhachHangId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("KhachHangId"));
                    diemSuDung = reader.GetInt32(reader.GetOrdinal("DiemSuDung"));
                    diemCong = reader.GetInt32(reader.GetOrdinal("DiemCong"));
                    providerOrderCode = reader.IsDBNull(reader.GetOrdinal("ProviderOrderCode")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("ProviderOrderCode"));
                }

                // 2. Nếu đã PAID rồi thì thành công (Idempotency)
                if (paymentStatus == "PAID" || trangThaiThanhToan == "Đã thanh toán")
                {
                    await transaction.CommitAsync(cancellationToken);
                    return true;
                }

                // 3. Kiểm tra trạng thái hợp lệ
                if (trangThaiThanhToan != "Chờ thanh toán")
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return false;
                }

                // 4. Lấy chi tiết hóa đơn
                const string getChiTietSql = @"
                    SELECT MonId, SoLuong, TenMon = (SELECT TenMon FROM dbo.Mon WHERE MonId = ct.MonId)
                    FROM dbo.ChiTietHoaDonBan ct
                    WHERE HoaDonBanId = @HoaDonBanId";

                var chiTietList = new List<(int MonId, int SoLuong, string TenMon)>();
                await using (var getChiTietCmd = new SqlCommand(getChiTietSql, connection, (SqlTransaction)transaction))
                {
                    getChiTietCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                    await using var reader = await getChiTietCmd.ExecuteReaderAsync(cancellationToken);
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        chiTietList.Add((
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetString(2)
                        ));
                    }
                }

                // 5. Kiểm tra tồn kho món
                foreach (var (monId, soLuong, tenMon) in chiTietList)
                {
                    const string checkTonKhoSql = "SELECT TonKho FROM dbo.Mon WHERE MonId = @MonId";
                    await using var checkTonKhoCmd = new SqlCommand(checkTonKhoSql, connection, (SqlTransaction)transaction);
                    checkTonKhoCmd.Parameters.AddWithValue("@MonId", monId);
                    var tonKho = (int)(await checkTonKhoCmd.ExecuteScalarAsync(cancellationToken) ?? 0);

                    if (tonKho < soLuong)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return false;
                    }
                }

                // 6. Kiểm tra tồn kho nguyên liệu
                foreach (var (monId, soLuong, tenMon) in chiTietList)
                {
                    const string getCongThucSql = @"
                        SELECT ct.NguyenLieuId, ct.DinhLuong, nl.TenNguyenLieu, nl.DonViTinh, nl.TonKho
                        FROM dbo.CongThucMon ct
                        INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
                        WHERE ct.MonId = @MonId AND ct.IsActive = 1";

                    await using var getCongThucCmd = new SqlCommand(getCongThucSql, connection, (SqlTransaction)transaction);
                    getCongThucCmd.Parameters.AddWithValue("@MonId", monId);
                    await using var reader = await getCongThucCmd.ExecuteReaderAsync(cancellationToken);

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var dinhLuong = reader.GetDecimal(1);
                        var tenNguyenLieu = reader.GetString(2);
                        var donViTinh = reader.GetString(3);
                        var tonKhoNguyenLieu = reader.GetDecimal(4);
                        var soLuongCanDung = dinhLuong * soLuong;

                        if (tonKhoNguyenLieu < soLuongCanDung)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return false;
                        }
                    }
                }

                // 7. Trừ tồn kho món và ghi lịch sử
                foreach (var (monId, soLuong, tenMon) in chiTietList)
                {
                    // Lấy tồn trước
                    const string getTonTruocSql = "SELECT TonKho FROM dbo.Mon WHERE MonId = @MonId";
                    await using var getTonTruocCmd = new SqlCommand(getTonTruocSql, connection, (SqlTransaction)transaction);
                    getTonTruocCmd.Parameters.AddWithValue("@MonId", monId);
                    var tonTruoc = (int)(await getTonTruocCmd.ExecuteScalarAsync(cancellationToken) ?? 0);

                    // Trừ tồn kho
                    const string updateTonKhoSql = @"
                        UPDATE dbo.Mon
                        SET TonKho = TonKho - @SoLuong
                        WHERE MonId = @MonId AND TonKho >= @SoLuong";

                    await using var updateTonKhoCmd = new SqlCommand(updateTonKhoSql, connection, (SqlTransaction)transaction);
                    updateTonKhoCmd.Parameters.AddWithValue("@MonId", monId);
                    updateTonKhoCmd.Parameters.AddWithValue("@SoLuong", soLuong);
                    var rowsAffected = await updateTonKhoCmd.ExecuteNonQueryAsync(cancellationToken);

                    if (rowsAffected == 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return false;
                    }

                    var tonSau = tonTruoc - soLuong;

                    // Ghi lịch sử tồn kho
                    const string insertLichSuSql = @"
                        INSERT INTO dbo.LichSuTonKho (MonId, LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, HoaDonBanId, GhiChu, NguoiDungId, ThoiGian)
                        VALUES (@MonId, 'BanHang', @SoLuongThayDoi, @TonTruoc, @TonSau, @HoaDonBanId, @GhiChu, @NguoiDungId, GETDATE())";

                    await using var insertLichSuCmd = new SqlCommand(insertLichSuSql, connection, (SqlTransaction)transaction);
                    insertLichSuCmd.Parameters.AddWithValue("@MonId", monId);
                    insertLichSuCmd.Parameters.AddWithValue("@SoLuongThayDoi", -soLuong);
                    insertLichSuCmd.Parameters.AddWithValue("@TonTruoc", tonTruoc);
                    insertLichSuCmd.Parameters.AddWithValue("@TonSau", tonSau);
                    insertLichSuCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                    insertLichSuCmd.Parameters.AddWithValue("@GhiChu", $"Local QR Payment - Hóa đơn #{hoaDonBanId}");
                    insertLichSuCmd.Parameters.AddWithValue("@NguoiDungId", createdByUserId);
                    await insertLichSuCmd.ExecuteNonQueryAsync(cancellationToken);
                }

                // 8. Trừ tồn kho nguyên liệu và ghi lịch sử
                foreach (var (monId, soLuong, tenMon) in chiTietList)
                {
                    const string getCongThucSql = @"
                        SELECT NguyenLieuId, DinhLuong
                        FROM dbo.CongThucMon
                        WHERE MonId = @MonId AND IsActive = 1";

                    await using var getCongThucCmd = new SqlCommand(getCongThucSql, connection, (SqlTransaction)transaction);
                    getCongThucCmd.Parameters.AddWithValue("@MonId", monId);
                    await using var reader = await getCongThucCmd.ExecuteReaderAsync(cancellationToken);

                    var congThucList = new List<(int NguyenLieuId, decimal DinhLuong)>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        congThucList.Add((reader.GetInt32(0), reader.GetDecimal(1)));
                    }

                    foreach (var (nguyenLieuId, dinhLuong) in congThucList)
                    {
                        var soLuongCanDung = dinhLuong * soLuong;

                        // Lấy tồn trước nguyên liệu
                        const string getTonTruocNLSql = "SELECT TonKho FROM dbo.NguyenLieu WHERE NguyenLieuId = @NguyenLieuId";
                        await using var getTonTruocNLCmd = new SqlCommand(getTonTruocNLSql, connection, (SqlTransaction)transaction);
                        getTonTruocNLCmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);
                        var tonTruocNL = (decimal)(await getTonTruocNLCmd.ExecuteScalarAsync(cancellationToken) ?? 0m);

                        // Trừ tồn kho nguyên liệu
                        const string updateTonKhoNLSql = @"
                            UPDATE dbo.NguyenLieu
                            SET TonKho = TonKho - @SoLuong
                            WHERE NguyenLieuId = @NguyenLieuId AND TonKho >= @SoLuong";

                        await using var updateTonKhoNLCmd = new SqlCommand(updateTonKhoNLSql, connection, (SqlTransaction)transaction);
                        updateTonKhoNLCmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);
                        updateTonKhoNLCmd.Parameters.AddWithValue("@SoLuong", soLuongCanDung);
                        var rowsAffected = await updateTonKhoNLCmd.ExecuteNonQueryAsync(cancellationToken);

                        if (rowsAffected == 0)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return false;
                        }

                        var tonSauNL = tonTruocNL - soLuongCanDung;

                        // Ghi lịch sử nguyên liệu
                        const string insertLichSuNLSql = @"
                            INSERT INTO dbo.LichSuNguyenLieu (NguyenLieuId, LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, HoaDonBanId, GhiChu, NguoiDungId, ThoiGian)
                            VALUES (@NguyenLieuId, 'XuatKho', @SoLuongThayDoi, @TonTruoc, @TonSau, @HoaDonBanId, @GhiChu, @NguoiDungId, GETDATE())";

                        await using var insertLichSuNLCmd = new SqlCommand(insertLichSuNLSql, connection, (SqlTransaction)transaction);
                        insertLichSuNLCmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);
                        insertLichSuNLCmd.Parameters.AddWithValue("@SoLuongThayDoi", -soLuongCanDung);
                        insertLichSuNLCmd.Parameters.AddWithValue("@TonTruoc", tonTruocNL);
                        insertLichSuNLCmd.Parameters.AddWithValue("@TonSau", tonSauNL);
                        insertLichSuNLCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                        insertLichSuNLCmd.Parameters.AddWithValue("@GhiChu", $"Local QR Payment cho món '{tenMon}' - HĐ #{hoaDonBanId}");
                        insertLichSuNLCmd.Parameters.AddWithValue("@NguoiDungId", createdByUserId);
                        await insertLichSuNLCmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                }

                // 9. Cập nhật điểm khách hàng
                if (khachHangId.HasValue && (diemSuDung > 0 || diemCong > 0))
                {
                    const string updateDiemSql = @"
                        UPDATE dbo.KhachHang
                        SET DiemTichLuy = DiemTichLuy - @DiemSuDung + @DiemCong
                        WHERE KhachHangId = @KhachHangId
                          AND IsActive = 1
                          AND DiemTichLuy >= @DiemSuDung";

                    await using var updateDiemCmd = new SqlCommand(updateDiemSql, connection, (SqlTransaction)transaction);
                    updateDiemCmd.Parameters.AddWithValue("@KhachHangId", khachHangId.Value);
                    updateDiemCmd.Parameters.AddWithValue("@DiemSuDung", diemSuDung);
                    updateDiemCmd.Parameters.AddWithValue("@DiemCong", diemCong);
                    var rowsAffected = await updateDiemCmd.ExecuteNonQueryAsync(cancellationToken);

                    if (rowsAffected == 0 && diemSuDung > 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return false;
                    }
                }

                // 10. Cập nhật trạng thái hóa đơn
                var maGiaoDich = $"MANUAL-{providerOrderCode ?? (long)hoaDonBanId}";
                const string updateHoaDonSql = @"
                    UPDATE dbo.HoaDonBan
                    SET PaymentStatus = 'PAID',
                        TrangThaiThanhToan = N'Đã thanh toán',
                        MaGiaoDich = @MaGiaoDich,
                        PaymentConfirmedAt = GETDATE()
                    WHERE HoaDonBanId = @HoaDonBanId";

                await using var updateHoaDonCmd = new SqlCommand(updateHoaDonSql, connection, (SqlTransaction)transaction);
                updateHoaDonCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                updateHoaDonCmd.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);
                await updateHoaDonCmd.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Xác nhận thanh toán thất bại: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tạo signature PayOS bằng thuật toán HMAC SHA256
    /// </summary>
    private string CreatePayOsSignature(IDictionary<string, object?> data, string checksumKey)
    {
        var sortedKeys = data.Keys.OrderBy(k => k).ToList();
        var dataString = string.Join("&", sortedKeys.Select(key =>
        {
            var value = data[key];
            return $"{key}={value}";
        }));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}

public class PayOsCreatePaymentRequest
{
    [JsonProperty("orderCode")]
    public long OrderCode { get; set; }
    [JsonProperty("amount")]
    public int Amount { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
    [JsonProperty("items")]
    public List<PayOsItem> Items { get; set; } = new();
    [JsonProperty("cancelUrl")]
    public string CancelUrl { get; set; } = string.Empty;
    [JsonProperty("returnUrl")]
    public string ReturnUrl { get; set; } = string.Empty;
    [JsonProperty("expiredAt")]
    public long ExpiredAt { get; set; }
    [JsonProperty("signature")]
    public string Signature { get; set; } = string.Empty;
}

public class PayOsItem
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty("quantity")]
    public int Quantity { get; set; }
    [JsonProperty("price")]
    public int Price { get; set; }
}

public class PayOsCreatePaymentResponse
{
    [JsonProperty("code")]
    public string? Code { get; set; }
    [JsonProperty("desc")]
    public string? Desc { get; set; }
    [JsonProperty("data")]
    public PayOsCreatePaymentData? Data { get; set; }
}

public class PayOsCreatePaymentData
{
    [JsonProperty("bin")]
    public string? Bin { get; set; }
    [JsonProperty("accountNumber")]
    public string? AccountNumber { get; set; }
    [JsonProperty("accountName")]
    public string? AccountName { get; set; }
    [JsonProperty("amount")]
    public int Amount { get; set; }
    [JsonProperty("description")]
    public string? Description { get; set; }
    [JsonProperty("orderCode")]
    public long OrderCode { get; set; }
    [JsonProperty("qrCode")]
    public string? QrCode { get; set; }
    [JsonProperty("checkoutUrl")]
    public string? CheckoutUrl { get; set; }
    [JsonProperty("paymentLinkId")]
    public string? PaymentLinkId { get; set; }
}

public class PayOsQueryPaymentResponse
{
    [JsonProperty("code")]
    public string? Code { get; set; }
    [JsonProperty("desc")]
    public string? Desc { get; set; }
    [JsonProperty("data")]
    public PayOsQueryPaymentData? Data { get; set; }
}

public class PayOsQueryPaymentData
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("orderCode")]
    public long OrderCode { get; set; }
    [JsonProperty("amount")]
    public int Amount { get; set; }
    [JsonProperty("amountPaid")]
    public int AmountPaid { get; set; }
    [JsonProperty("amountRemaining")]
    public int AmountRemaining { get; set; }
    [JsonProperty("status")]
    public string? Status { get; set; }
    [JsonProperty("createdAt")]
    public string? CreatedAt { get; set; }
}

public class CreateQRPaymentResponse
{
    public int HoaDonBanId { get; set; }
    public string PaymentProvider { get; set; } = string.Empty;
    public string? ProviderPaymentId { get; set; }
    public long? ProviderOrderCode { get; set; }
    public string? QRCodeRaw { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? QRExpiredAt { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class PaymentStatusResponse
{
    public int HoaDonBanId { get; set; }
    public string? PaymentProvider { get; set; }
    public string? ProviderPaymentId { get; set; }
    public long? ProviderOrderCode { get; set; }
    public string? PaymentStatus { get; set; }
    public string? MaGiaoDich { get; set; }
    public DateTime? PaymentConfirmedAt { get; set; }
    public decimal ThanhToan { get; set; }
    public string TrangThaiThanhToan { get; set; } = string.Empty;
    public DateTime? QRExpiredAt { get; set; }
}
