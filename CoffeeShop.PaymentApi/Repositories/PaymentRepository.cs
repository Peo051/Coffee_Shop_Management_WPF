using CoffeeShop.PaymentApi.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.PaymentApi.Repositories;

/// <summary>
/// Repository xử lý database cho payment
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly string _connectionString;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(IConfiguration configuration, ILogger<PaymentRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("CoffeeShopDb")
            ?? throw new InvalidOperationException("Connection string 'CoffeeShopDb' not found.");
        _logger = logger;
    }

    public async Task<HoaDonBan?> GetHoaDonForPaymentAsync(int hoaDonBanId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT HoaDonBanId, NgayBan, TongTien, GiamGia, ThanhToan, CreatedByUserId,
                   TrangThaiThanhToan, HinhThucThanhToan, MaGiaoDich,
                   PaymentProvider, ProviderPaymentId, ProviderOrderCode,
                   QRCodeRaw, CheckoutUrl, QRExpiredAt, PaymentStatus, PaymentConfirmedAt
            FROM dbo.HoaDonBan
            WHERE HoaDonBanId = @HoaDonBanId";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapHoaDonBan(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HoaDonBan for payment: {HoaDonBanId}", hoaDonBanId);
            return null;
        }
    }

    public async Task<bool> SaveCreatedQrPaymentAsync(
        int hoaDonBanId,
        string paymentProvider,
        string providerPaymentId,
        long providerOrderCode,
        string qrCodeRaw,
        string checkoutUrl,
        DateTime qrExpiredAt,
        string paymentStatus,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.HoaDonBan
            SET PaymentProvider = @PaymentProvider,
                ProviderPaymentId = @ProviderPaymentId,
                ProviderOrderCode = @ProviderOrderCode,
                QRCodeRaw = @QRCodeRaw,
                CheckoutUrl = @CheckoutUrl,
                QRExpiredAt = @QRExpiredAt,
                PaymentStatus = @PaymentStatus,
                HinhThucThanhToan = N'QR Code'
            WHERE HoaDonBanId = @HoaDonBanId";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
            command.Parameters.AddWithValue("@PaymentProvider", paymentProvider);
            command.Parameters.AddWithValue("@ProviderPaymentId", providerPaymentId);
            command.Parameters.AddWithValue("@ProviderOrderCode", providerOrderCode);
            command.Parameters.AddWithValue("@QRCodeRaw", qrCodeRaw);
            command.Parameters.AddWithValue("@CheckoutUrl", checkoutUrl);
            command.Parameters.AddWithValue("@QRExpiredAt", qrExpiredAt);
            command.Parameters.AddWithValue("@PaymentStatus", paymentStatus);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving QR payment info for HoaDonBan: {HoaDonBanId}", hoaDonBanId);
            return false;
        }
    }

    public async Task<PaymentStatusResponse?> GetPaymentStatusAsync(int hoaDonBanId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT HoaDonBanId, ThanhToan, TrangThaiThanhToan, MaGiaoDich,
                   PaymentProvider, ProviderPaymentId, ProviderOrderCode,
                   PaymentStatus, PaymentConfirmedAt, QRExpiredAt
            FROM dbo.HoaDonBan
            WHERE HoaDonBanId = @HoaDonBanId";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var paymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("PaymentStatus"));

                var qrExpiredAt = reader.IsDBNull(reader.GetOrdinal("QRExpiredAt"))
                    ? (DateTime?)null
                    : reader.GetDateTime(reader.GetOrdinal("QRExpiredAt"));

                // Chuẩn hóa trạng thái EXPIRED
                // Nếu PaymentStatus = PENDING và QRExpiredAt < now thì coi là EXPIRED
                if (paymentStatus == "PENDING" && qrExpiredAt.HasValue && qrExpiredAt.Value < DateTime.Now)
                {
                    paymentStatus = "EXPIRED";
                }

                return new PaymentStatusResponse
                {
                    HoaDonBanId = reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
                    ThanhToan = reader.GetDecimal(reader.GetOrdinal("ThanhToan")),
                    TrangThaiThanhToan = reader.IsDBNull(reader.GetOrdinal("TrangThaiThanhToan"))
                        ? "Chưa thanh toán"
                        : reader.GetString(reader.GetOrdinal("TrangThaiThanhToan")),
                    MaGiaoDich = reader.IsDBNull(reader.GetOrdinal("MaGiaoDich"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("MaGiaoDich")),
                    PaymentProvider = reader.IsDBNull(reader.GetOrdinal("PaymentProvider"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("PaymentProvider")),
                    ProviderPaymentId = reader.IsDBNull(reader.GetOrdinal("ProviderPaymentId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ProviderPaymentId")),
                    ProviderOrderCode = reader.IsDBNull(reader.GetOrdinal("ProviderOrderCode"))
                        ? null
                        : reader.GetInt64(reader.GetOrdinal("ProviderOrderCode")),
                    PaymentStatus = paymentStatus,
                    PaymentConfirmedAt = reader.IsDBNull(reader.GetOrdinal("PaymentConfirmedAt"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("PaymentConfirmedAt")),
                    QRExpiredAt = qrExpiredAt
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for HoaDonBan: {HoaDonBanId}", hoaDonBanId);
            return null;
        }
    }

    public async Task<bool> MarkPaymentPaidAsync(
        int hoaDonBanId,
        string maGiaoDich,
        DateTime paymentConfirmedAt,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.HoaDonBan
            SET PaymentStatus = 'PAID',
                TrangThaiThanhToan = N'Đã thanh toán',
                MaGiaoDich = @MaGiaoDich,
                PaymentConfirmedAt = @PaymentConfirmedAt
            WHERE HoaDonBanId = @HoaDonBanId
              AND (PaymentStatus IS NULL OR PaymentStatus != 'PAID')";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
            command.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);
            command.Parameters.AddWithValue("@PaymentConfirmedAt", paymentConfirmedAt);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment paid for HoaDonBan: {HoaDonBanId}", hoaDonBanId);
            return false;
        }
    }

    public async Task<bool> MarkPaymentCancelledAsync(int hoaDonBanId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.HoaDonBan
            SET PaymentStatus = 'CANCELLED'
            WHERE HoaDonBanId = @HoaDonBanId
              AND (PaymentStatus IS NULL OR PaymentStatus IN ('PENDING', 'PROCESSING'))
              AND (QRExpiredAt IS NULL OR QRExpiredAt > GETDATE())";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment cancelled for HoaDonBan: {HoaDonBanId}", hoaDonBanId);
            return false;
        }
    }

    public async Task<HoaDonBan?> FindHoaDonByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT HoaDonBanId, NgayBan, TongTien, GiamGia, ThanhToan, CreatedByUserId,
                   TrangThaiThanhToan, HinhThucThanhToan, MaGiaoDich,
                   PaymentProvider, ProviderPaymentId, ProviderOrderCode,
                   QRCodeRaw, CheckoutUrl, QRExpiredAt, PaymentStatus, PaymentConfirmedAt
            FROM dbo.HoaDonBan
            WHERE ProviderOrderCode = @OrderCode";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderCode", orderCode);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapHoaDonBan(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding HoaDonBan by OrderCode: {OrderCode}", orderCode);
            return null;
        }
    }

    public async Task<bool> IsAlreadyPaidAsync(int hoaDonBanId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM dbo.HoaDonBan
            WHERE HoaDonBanId = @HoaDonBanId
              AND (PaymentStatus = 'PAID' OR TrangThaiThanhToan = N'Đã thanh toán')";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);

            var count = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if HoaDonBan is paid: {HoaDonBanId}", hoaDonBanId);
            return false;
        }
    }

    /// <summary>
    /// Finalize QR payment: trừ kho, nguyên liệu, điểm khi webhook xác nhận PAID
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> FinalizeQrPaymentAsync(
        int hoaDonBanId,
        string maGiaoDich,
        DateTime paymentConfirmedAt,
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
                           KhachHangId, DiemSuDung, DiemCong
                    FROM dbo.HoaDonBan WITH (UPDLOCK, HOLDLOCK)
                    WHERE HoaDonBanId = @HoaDonBanId";

                int createdByUserId;
                int? khachHangId;
                int diemSuDung;
                int diemCong;
                string? trangThaiThanhToan;
                string? paymentStatus;

                await using (var checkCmd = new SqlCommand(checkSql, connection, (SqlTransaction)transaction))
                {
                    checkCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                    await using var reader = await checkCmd.ExecuteReaderAsync(cancellationToken);
                    
                    if (!await reader.ReadAsync(cancellationToken))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return (false, $"Không tìm thấy hóa đơn {hoaDonBanId}");
                    }

                    trangThaiThanhToan = reader.IsDBNull(reader.GetOrdinal("TrangThaiThanhToan"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("TrangThaiThanhToan"));
                    paymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("PaymentStatus"));
                    createdByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId"));
                    khachHangId = reader.IsDBNull(reader.GetOrdinal("KhachHangId"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("KhachHangId"));
                    diemSuDung = reader.GetInt32(reader.GetOrdinal("DiemSuDung"));
                    diemCong = reader.GetInt32(reader.GetOrdinal("DiemCong"));
                }

                // 2. Idempotency: Nếu đã PAID rồi thì return success
                if (paymentStatus == "PAID" || trangThaiThanhToan == "Đã thanh toán")
                {
                    await transaction.CommitAsync(cancellationToken);
                    _logger.LogInformation("Payment already finalized for HoaDonBan {HoaDonBanId} (idempotency)", hoaDonBanId);
                    return (true, null);
                }

                // 3. Kiểm tra trạng thái hợp lệ
                if (trangThaiThanhToan != "Chờ thanh toán")
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return (false, $"Hóa đơn không ở trạng thái 'Chờ thanh toán' (hiện tại: {trangThaiThanhToan})");
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
                        return (false, $"Không đủ tồn kho món '{tenMon}' (cần {soLuong}, còn {tonKho})");
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
                            return (false, $"Không đủ nguyên liệu '{tenNguyenLieu}' cho món '{tenMon}' (cần {soLuongCanDung:N2} {donViTinh}, còn {tonKhoNguyenLieu:N2} {donViTinh})");
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
                        return (false, $"Không thể trừ tồn kho món '{tenMon}'");
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
                    insertLichSuCmd.Parameters.AddWithValue("@GhiChu", $"QR Payment finalized - Hóa đơn #{hoaDonBanId}");
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
                            return (false, $"Không thể trừ tồn kho nguyên liệu cho món '{tenMon}'");
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
                        insertLichSuNLCmd.Parameters.AddWithValue("@GhiChu", $"QR Payment finalized cho món '{tenMon}' - HĐ #{hoaDonBanId}");
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
                        return (false, $"Khách hàng không đủ điểm tích lũy (yêu cầu: {diemSuDung} điểm)");
                    }
                }

                // 10. Cập nhật trạng thái hóa đơn
                const string updateHoaDonSql = @"
                    UPDATE dbo.HoaDonBan
                    SET PaymentStatus = 'PAID',
                        TrangThaiThanhToan = N'Đã thanh toán',
                        MaGiaoDich = @MaGiaoDich,
                        PaymentConfirmedAt = @PaymentConfirmedAt
                    WHERE HoaDonBanId = @HoaDonBanId";

                await using var updateHoaDonCmd = new SqlCommand(updateHoaDonSql, connection, (SqlTransaction)transaction);
                updateHoaDonCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                updateHoaDonCmd.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);
                updateHoaDonCmd.Parameters.AddWithValue("@PaymentConfirmedAt", paymentConfirmedAt);
                await updateHoaDonCmd.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Successfully finalized QR payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error in transaction while finalizing QR payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
                return (false, ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing QR payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
            return (false, ex.Message);
        }
    }

    private static HoaDonBan MapHoaDonBan(SqlDataReader reader)
    {
        return new HoaDonBan
        {
            HoaDonBanId = reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
            NgayBan = reader.GetDateTime(reader.GetOrdinal("NgayBan")),
            TongTien = reader.GetDecimal(reader.GetOrdinal("TongTien")),
            GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
            ThanhToan = reader.GetDecimal(reader.GetOrdinal("ThanhToan")),
            CreatedByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId")),
            TrangThaiThanhToan = reader.IsDBNull(reader.GetOrdinal("TrangThaiThanhToan"))
                ? null
                : reader.GetString(reader.GetOrdinal("TrangThaiThanhToan")),
            HinhThucThanhToan = reader.IsDBNull(reader.GetOrdinal("HinhThucThanhToan"))
                ? null
                : reader.GetString(reader.GetOrdinal("HinhThucThanhToan")),
            MaGiaoDich = reader.IsDBNull(reader.GetOrdinal("MaGiaoDich"))
                ? null
                : reader.GetString(reader.GetOrdinal("MaGiaoDich")),
            PaymentProvider = reader.IsDBNull(reader.GetOrdinal("PaymentProvider"))
                ? null
                : reader.GetString(reader.GetOrdinal("PaymentProvider")),
            ProviderPaymentId = reader.IsDBNull(reader.GetOrdinal("ProviderPaymentId"))
                ? null
                : reader.GetString(reader.GetOrdinal("ProviderPaymentId")),
            ProviderOrderCode = reader.IsDBNull(reader.GetOrdinal("ProviderOrderCode"))
                ? null
                : reader.GetInt64(reader.GetOrdinal("ProviderOrderCode")),
            QRCodeRaw = reader.IsDBNull(reader.GetOrdinal("QRCodeRaw"))
                ? null
                : reader.GetString(reader.GetOrdinal("QRCodeRaw")),
            CheckoutUrl = reader.IsDBNull(reader.GetOrdinal("CheckoutUrl"))
                ? null
                : reader.GetString(reader.GetOrdinal("CheckoutUrl")),
            QRExpiredAt = reader.IsDBNull(reader.GetOrdinal("QRExpiredAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("QRExpiredAt")),
            PaymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus"))
                ? null
                : reader.GetString(reader.GetOrdinal("PaymentStatus")),
            PaymentConfirmedAt = reader.IsDBNull(reader.GetOrdinal("PaymentConfirmedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("PaymentConfirmedAt"))
        };
    }
}
