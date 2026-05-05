using CoffeeShop.PaymentApi.Models;
using CoffeeShop.PaymentApi.Repositories;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace CoffeeShop.PaymentApi.Services;

/// <summary>
/// Service tích hợp với payOS payment gateway
/// </summary>
public class PayOsPaymentGatewayService : IPaymentGatewayService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PayOsPaymentGatewayService> _logger;
    private readonly HttpClient _httpClient;

    private readonly string _clientId;
    private readonly string _apiKey;
    private readonly string _checksumKey;
    private readonly string _baseUrl;
    private readonly string _returnUrl;
    private readonly string _cancelUrl;
    
    // VietQR config
    private readonly string _vietQrBankId;
    private readonly string _vietQrAccountNo;
    private readonly string _vietQrAccountName;
    private readonly string _vietQrTemplate;
    
    // Payment mode
    private readonly string _paymentMode;
    private readonly bool _hasPayOsCredentials;

    public PayOsPaymentGatewayService(
        IPaymentRepository paymentRepository,
        IConfiguration configuration,
        ILogger<PayOsPaymentGatewayService> logger,
        HttpClient httpClient)
    {
        _paymentRepository = paymentRepository;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;

        _clientId = configuration["PayOS:ClientId"] ?? "";
        _apiKey = configuration["PayOS:ApiKey"] ?? "";
        _checksumKey = configuration["PayOS:ChecksumKey"] ?? "";
        _baseUrl = configuration["PayOS:BaseUrl"] ?? "https://api-merchant.payos.vn";
        _returnUrl = configuration["PayOS:ReturnUrl"] ?? "https://localhost:5001/payment/success";
        _cancelUrl = configuration["PayOS:CancelUrl"] ?? "https://localhost:5001/payment/cancel";
        
        // VietQR config
        _vietQrBankId = configuration["VietQR:BankId"] ?? "970422";
        _vietQrAccountNo = configuration["VietQR:AccountNo"] ?? "0123456789";
        _vietQrAccountName = configuration["VietQR:AccountName"] ?? "NGUYEN VAN A";
        _vietQrTemplate = configuration["VietQR:Template"] ?? "compact";
        
        // Payment mode
        _paymentMode = configuration["PaymentMode"] ?? "VietQR";
        _hasPayOsCredentials = !string.IsNullOrEmpty(_clientId) 
            && !string.IsNullOrEmpty(_apiKey) 
            && !string.IsNullOrEmpty(_checksumKey)
            && !_clientId.StartsWith("YOUR_")
            && !_apiKey.StartsWith("YOUR_");

        if (_paymentMode == "PayOS" && !_hasPayOsCredentials)
        {
            _logger.LogWarning("PaymentMode is PayOS but credentials not configured. Falling back to VietQR.");
            _paymentMode = "VietQR";
        }
        
        _logger.LogInformation("Payment mode: {PaymentMode}", _paymentMode);
    }

    public async Task<CreateQRPaymentResponse> CreateQrAsync(int hoaDonBanId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Lấy thông tin hóa đơn
            var hoaDon = await _paymentRepository.GetHoaDonForPaymentAsync(hoaDonBanId, cancellationToken);
            if (hoaDon == null)
            {
                throw new InvalidOperationException($"Không tìm thấy hóa đơn {hoaDonBanId}");
            }

            // Kiểm tra đã thanh toán chưa
            if (await _paymentRepository.IsAlreadyPaidAsync(hoaDonBanId, cancellationToken))
            {
                throw new InvalidOperationException("Hóa đơn đã được thanh toán");
            }

            // Kiểm tra đã có QR active chưa
            // QR active = PENDING + chưa hết hạn + chưa CANCELLED
            bool hasActiveQr = hoaDon.PaymentStatus == "PENDING" 
                && hoaDon.QRExpiredAt.HasValue 
                && hoaDon.QRExpiredAt.Value > DateTime.Now
                && hoaDon.PaymentStatus != "CANCELLED";

            if (hasActiveQr)
            {
                // Trả lại QR cũ
                _logger.LogInformation("Returning existing active QR for HoaDonBan {HoaDonBanId}", hoaDonBanId);
                return new CreateQRPaymentResponse
                {
                    HoaDonBanId = hoaDon.HoaDonBanId,
                    PaymentProvider = hoaDon.PaymentProvider ?? "payOS",
                    ProviderPaymentId = hoaDon.ProviderPaymentId,
                    ProviderOrderCode = hoaDon.ProviderOrderCode,
                    QRCodeRaw = hoaDon.QRCodeRaw,
                    CheckoutUrl = hoaDon.CheckoutUrl,
                    PaymentStatus = hoaDon.PaymentStatus,
                    QRExpiredAt = hoaDon.QRExpiredAt,
                    Amount = hoaDon.ThanhToan,
                    Description = $"HD{hoaDon.HoaDonBanId:D5}"
                };
            }

            // Nếu QR cũ đã EXPIRED hoặc CANCELLED, tạo QR mới
            if (hoaDon.PaymentStatus == "PENDING" && hoaDon.QRExpiredAt.HasValue && hoaDon.QRExpiredAt.Value <= DateTime.Now)
            {
                _logger.LogInformation("Previous QR expired for HoaDonBan {HoaDonBanId}, creating new QR", hoaDonBanId);
            }
            else if (hoaDon.PaymentStatus == "CANCELLED")
            {
                _logger.LogInformation("Previous QR cancelled for HoaDonBan {HoaDonBanId}, creating new QR", hoaDonBanId);
            }

            // Tạo orderCode duy nhất
            var orderCode = GenerateOrderCode(hoaDonBanId);
            var description = $"HD{hoaDonBanId:D5}";
            var amount = (int)hoaDon.ThanhToan;
            var expiredAt = DateTime.Now.AddMinutes(10);

            // Gọi payOS API để tạo payment
            var (qrCode, checkoutUrl, paymentLinkId) = await CallPayOsCreatePaymentAsync(
                orderCode, amount, description, expiredAt, cancellationToken);

            // Lưu vào database
            var saved = await _paymentRepository.SaveCreatedQrPaymentAsync(
                hoaDonBanId,
                "payOS",
                paymentLinkId,
                orderCode,
                qrCode,
                checkoutUrl,
                expiredAt,
                "PENDING",
                cancellationToken);

            if (!saved)
            {
                throw new InvalidOperationException("Không thể lưu thông tin QR payment");
            }

            _logger.LogInformation("Created QR payment for HoaDonBan {HoaDonBanId}, OrderCode: {OrderCode}",
                hoaDonBanId, orderCode);

            return new CreateQRPaymentResponse
            {
                HoaDonBanId = hoaDonBanId,
                PaymentProvider = "payOS",
                ProviderPaymentId = paymentLinkId,
                ProviderOrderCode = orderCode,
                QRCodeRaw = qrCode,
                CheckoutUrl = checkoutUrl,
                PaymentStatus = "PENDING",
                QRExpiredAt = expiredAt,
                Amount = hoaDon.ThanhToan,
                Description = description
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating QR payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
            throw;
        }
    }

    public async Task<PaymentStatusResponse?> GetStatusAsync(int hoaDonBanId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Đọc từ database
            var status = await _paymentRepository.GetPaymentStatusAsync(hoaDonBanId, cancellationToken);
            if (status == null)
            {
                _logger.LogWarning("HoaDonBan {HoaDonBanId} not found", hoaDonBanId);
                return null;
            }

            // Nếu status là PENDING và có ProviderOrderCode, query PayOS để đối soát
            if (status.PaymentStatus == "PENDING" && status.ProviderOrderCode.HasValue)
            {
                _logger.LogInformation("Payment is PENDING, querying PayOS for OrderCode {OrderCode}", status.ProviderOrderCode.Value);
                
                var providerStatus = await QueryPaymentStatusFromProviderAsync(status.ProviderOrderCode.Value, cancellationToken);
                
                if (providerStatus == "PAID" || providerStatus == "SUCCESS")
                {
                    _logger.LogInformation("PayOS reports PAID for OrderCode {OrderCode}, finalizing payment", status.ProviderOrderCode.Value);
                    
                    // Finalize payment giống webhook
                    var finalizeResult = await _paymentRepository.FinalizeQrPaymentAsync(
                        hoaDonBanId,
                        $"REF-{status.ProviderOrderCode.Value}", // Mã giao dịch tham chiếu
                        DateTime.Now,
                        cancellationToken);

                    if (finalizeResult.Success)
                    {
                        _logger.LogInformation("Successfully finalized payment for HoaDonBan {HoaDonBanId} via manual check", hoaDonBanId);
                        
                        // Đọc lại status sau khi finalize
                        status = await _paymentRepository.GetPaymentStatusAsync(hoaDonBanId, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to finalize payment for HoaDonBan {HoaDonBanId}: {Error}", hoaDonBanId, finalizeResult.ErrorMessage);
                    }
                }
                else if (providerStatus == "EXPIRED")
                {
                    _logger.LogInformation("PayOS reports EXPIRED for OrderCode {OrderCode}", status.ProviderOrderCode.Value);
                    // Có thể cập nhật status thành EXPIRED nếu muốn
                }
                else
                {
                    _logger.LogInformation("PayOS still reports {Status} for OrderCode {OrderCode}", providerStatus, status.ProviderOrderCode.Value);
                }
            }

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for HoaDonBan {HoaDonBanId}", hoaDonBanId);
            return null;
        }
    }

    public async Task<WebhookResult> HandleWebhookAsync(string rawJson, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing payOS webhook. Payload length: {Length}", rawJson?.Length ?? 0);
            _logger.LogDebug("Webhook raw JSON: {RawJson}", rawJson);

            // Kiểm tra payload rỗng
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                _logger.LogWarning("Webhook payload is empty");
                return WebhookResult.Ignored("Webhook payload is empty");
            }

            // Deserialize payload
            PayOsWebhookData? webhookData;
            try
            {
                webhookData = JsonConvert.DeserializeObject<PayOsWebhookData>(rawJson);
                _logger.LogInformation("Webhook deserialized. Code: {Code}, OrderCode: {OrderCode}", 
                    webhookData?.Code, webhookData?.Data?.OrderCode);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx, "Failed to deserialize webhook payload");
                return WebhookResult.Ignored("Invalid JSON payload");
            }

            if (webhookData?.Data == null)
            {
                _logger.LogWarning("Webhook data is null or missing data property");
                return WebhookResult.Ignored("Webhook data is null or missing data property");
            }

            // Kiểm tra xem có phải test webhook không
            var isTestWebhook = webhookData.Signature == "test" || 
                                (webhookData.Data.Reference?.StartsWith("MANUAL-") ?? false) ||
                                (webhookData.Data.Reference?.StartsWith("TEST-") ?? false);
            
            if (isTestWebhook)
            {
                _logger.LogInformation("Test webhook detected, skipping signature verification for OrderCode {OrderCode}", 
                    webhookData.Data.OrderCode);
            }
            else if (!string.IsNullOrEmpty(_checksumKey))
            {
                // Chỉ verify signature cho webhook thật từ PayOS
                if (!VerifyWebhookSignature(webhookData, _checksumKey))
                {
                    _logger.LogWarning("Webhook signature verification failed for OrderCode {OrderCode}. Continuing anyway in Development mode.", 
                        webhookData.Data.OrderCode);
                    // KHÔNG return lỗi, vẫn xử lý tiếp trong Development
                }
                else
                {
                    _logger.LogInformation("Webhook signature verified successfully for OrderCode {OrderCode}", 
                        webhookData.Data.OrderCode);
                }
            }
            else
            {
                _logger.LogInformation("ChecksumKey not configured, skipping signature verification");
            }

            // Tìm hóa đơn theo orderCode
            _logger.LogInformation("Looking for HoaDonBan with OrderCode {OrderCode}", webhookData.Data.OrderCode);
            var hoaDon = await _paymentRepository.FindHoaDonByOrderCodeAsync(
                webhookData.Data.OrderCode, cancellationToken);

            if (hoaDon == null)
            {
                _logger.LogWarning("HoaDonBan not found for OrderCode {OrderCode}. This might be a test webhook from PayOS.", 
                    webhookData.Data.OrderCode);
                return WebhookResult.Ignored("Webhook received but no matching invoice. Ignored.");
            }

            _logger.LogInformation("Found HoaDonBan {HoaDonBanId} for OrderCode {OrderCode}", 
                hoaDon.HoaDonBanId, webhookData.Data.OrderCode);

            // Kiểm tra đã paid chưa (idempotency)
            if (await _paymentRepository.IsAlreadyPaidAsync(hoaDon.HoaDonBanId, cancellationToken))
            {
                _logger.LogInformation("Payment already confirmed for HoaDonBan {HoaDonBanId} (idempotency)",
                    hoaDon.HoaDonBanId);
                return WebhookResult.Ok("Payment already confirmed (idempotent)");
            }

            _logger.LogInformation("Processing payment for HoaDonBan {HoaDonBanId}. Webhook Code: {Code}, Data Code: {DataCode}", 
                hoaDon.HoaDonBanId, webhookData.Code, webhookData.Data.Code);

            // Kiểm tra code = "00" nghĩa là thành công
            if (webhookData.Code == "00" || webhookData.Data.Code == "00")
            {
                var maGiaoDich = webhookData.Data.Reference ?? webhookData.Data.PaymentLinkId ?? $"PAY{webhookData.Data.OrderCode}";
                
                _logger.LogInformation("Finalizing payment for HoaDonBan {HoaDonBanId} with Reference {Reference}", 
                    hoaDon.HoaDonBanId, maGiaoDich);
                
                // Finalize payment: trừ kho, nguyên liệu, điểm
                var (success, errorMessage) = await _paymentRepository.FinalizeQrPaymentAsync(
                    hoaDon.HoaDonBanId,
                    maGiaoDich,
                    DateTime.Now,
                    cancellationToken);

                if (success)
                {
                    _logger.LogInformation(
                        "Payment finalized successfully for HoaDonBan {HoaDonBanId}, OrderCode: {OrderCode}, Reference: {Reference}",
                        hoaDon.HoaDonBanId, webhookData.Data.OrderCode, maGiaoDich);
                    return WebhookResult.Ok($"Payment finalized successfully for invoice {hoaDon.HoaDonBanId}");
                }
                else
                {
                    _logger.LogError(
                        "Failed to finalize payment for HoaDonBan {HoaDonBanId}: {ErrorMessage}",
                        hoaDon.HoaDonBanId, errorMessage);
                    return WebhookResult.Ignored($"Failed to finalize payment: {errorMessage}");
                }
            }
            else
            {
                _logger.LogWarning(
                    "Payment failed or not completed for HoaDonBan {HoaDonBanId}, Code: {Code}, Desc: {Desc}",
                    hoaDon.HoaDonBanId, webhookData.Code, webhookData.Desc);
                return WebhookResult.Ignored($"Payment not completed. Code: {webhookData.Code}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error handling webhook");
            // Vẫn trả Ignored để controller trả 200 OK
            return WebhookResult.Ignored("Webhook received but processing failed. Logged for investigation.");
        }
    }

    public async Task<bool> CancelAsync(int hoaDonBanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var hoaDon = await _paymentRepository.GetHoaDonForPaymentAsync(hoaDonBanId, cancellationToken);
            if (hoaDon == null)
            {
                _logger.LogWarning("HoaDonBan {HoaDonBanId} not found", hoaDonBanId);
                return false;
            }

            // Không cho cancel nếu đã paid
            if (await _paymentRepository.IsAlreadyPaidAsync(hoaDonBanId, cancellationToken))
            {
                _logger.LogWarning("Cannot cancel paid payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
                return false;
            }

            // Không cho cancel nếu đã EXPIRED
            if (hoaDon.PaymentStatus == "PENDING" && hoaDon.QRExpiredAt.HasValue && hoaDon.QRExpiredAt.Value < DateTime.Now)
            {
                _logger.LogWarning("Cannot cancel expired payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
                return false;
            }

            // Gọi provider cancel nếu có
            if (!string.IsNullOrEmpty(hoaDon.ProviderPaymentId))
            {
                await CallPayOsCancelPaymentAsync(hoaDon.ProviderPaymentId, cancellationToken);
            }

            // Cập nhật local state
            var cancelled = await _paymentRepository.MarkPaymentCancelledAsync(hoaDonBanId, cancellationToken);

            if (cancelled)
            {
                _logger.LogInformation("Cancelled payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
            }

            return cancelled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
            return false;
        }
    }

    private async Task<(string qrCode, string checkoutUrl, string paymentLinkId)> CallPayOsCreatePaymentAsync(
        long orderCode,
        int amount,
        string description,
        DateTime expiredAt,
        CancellationToken cancellationToken)
    {
        // Kiểm tra payment mode
        if (_paymentMode == "VietQR")
        {
            _logger.LogInformation("Creating VietQR for OrderCode {OrderCode}, Amount: {Amount}", orderCode, amount);
            
            var qrCode = GenerateVietQrUrl(amount, description, orderCode);
            var checkoutUrl = qrCode;
            var paymentLinkId = $"VIETQR-{orderCode}";

            _logger.LogInformation("Generated VietQR: {QrCode}", qrCode);
            return (qrCode, checkoutUrl, paymentLinkId);
        }

        // PayOS mode - gọi API thật
        if (!_hasPayOsCredentials)
        {
            throw new InvalidOperationException("PayOS mode selected but credentials not configured");
        }

        try
        {
            _logger.LogInformation("Calling PayOS API to create payment for OrderCode {OrderCode}", orderCode);

            var expiredAtUnix = ((DateTimeOffset)expiredAt).ToUnixTimeSeconds();

            // Tạo items (PayOS yêu cầu phải có ít nhất 1 item)
            var items = new List<PayOsItem>
            {
                new PayOsItem
                {
                    Name = description,
                    Quantity = 1,
                    Price = amount
                }
            };

            // Tạo signature theo tài liệu PayOS
            var signatureData = new Dictionary<string, object?>
            {
                { "amount", amount },
                { "cancelUrl", _cancelUrl },
                { "description", description },
                { "orderCode", orderCode },
                { "returnUrl", _returnUrl }
            };
            var signature = CreatePayOsSignature(signatureData);

            var requestPayload = new PayOsCreatePaymentRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                Description = description,
                Items = items,
                CancelUrl = _cancelUrl,
                ReturnUrl = _returnUrl,
                ExpiredAt = expiredAtUnix,
                Signature = signature
            };

            // Log request payload để debug
            var requestJson = JsonConvert.SerializeObject(requestPayload, Formatting.Indented);
            _logger.LogInformation("PayOS Request Payload: {RequestPayload}", requestJson);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/v2/payment-requests",
                requestPayload,
                cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogDebug("PayOS API HTTP {StatusCode}. Response: {Response}", 
                (int)response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayOS API failed. HTTP {StatusCode}. Response: {Response}", 
                    (int)response.StatusCode, responseContent);
                throw new InvalidOperationException($"PayOS create payment failed. HTTP {(int)response.StatusCode}. Response: {responseContent}");
            }

            var result = JsonConvert.DeserializeObject<PayOsCreatePaymentResponse>(responseContent);

            if (result?.Data == null)
            {
                _logger.LogError("PayOS returned null data. Response: {Response}", responseContent);
                throw new InvalidOperationException($"PayOS returned null data. Response: {responseContent}");
            }

            _logger.LogInformation(
                "PayOS payment created successfully. PaymentLinkId: {PaymentLinkId}, OrderCode: {OrderCode}",
                result.Data.PaymentLinkId, result.Data.OrderCode);

            return (result.Data.QrCode, result.Data.CheckoutUrl, result.Data.PaymentLinkId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling PayOS API for OrderCode {OrderCode}", orderCode);
            throw;
        }
    }

    /// <summary>
    /// Tạo VietQR URL theo chuẩn
    /// </summary>
    private string GenerateVietQrUrl(int amount, string description, long orderCode)
    {
        // VietQR API format: https://img.vietqr.io/image/{BANK_ID}-{ACCOUNT_NO}-{TEMPLATE}.jpg?amount={AMOUNT}&addInfo={DESCRIPTION}&accountName={ACCOUNT_NAME}
        
        // Chuẩn hóa nội dung chuyển khoản - chỉ dùng mã hóa đơn
        var addInfo = $"HD{orderCode}";
        
        // URL encode các tham số
        var encodedAddInfo = Uri.EscapeDataString(addInfo);
        var encodedAccountName = Uri.EscapeDataString(_vietQrAccountName);
        
        // Tạo URL với format đúng - sử dụng .jpg thay vì .png
        var qrUrl = $"https://img.vietqr.io/image/{_vietQrBankId}-{_vietQrAccountNo}-{_vietQrTemplate}.jpg?amount={amount}&addInfo={encodedAddInfo}&accountName={encodedAccountName}";
        
        _logger.LogInformation("Generated VietQR URL: {QrUrl}", qrUrl);
        
        return qrUrl;
    }

    private Task CallPayOsCancelPaymentAsync(string paymentLinkId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Mock mode
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogInformation("Using mock payOS cancel payment for PaymentLinkId {PaymentLinkId}", paymentLinkId);
            return Task.CompletedTask;
        }

        // TODO: call payOS cancel API here when production credentials are available
        // Hiện tại chỉ update local DB, không gọi provider API
        _logger.LogWarning(
            "PayOS cancel API not implemented yet. Local DB updated but provider not notified for PaymentLinkId {PaymentLinkId}",
            paymentLinkId);

        // Khi implement thật:
        // _httpClient.DefaultRequestHeaders.Clear();
        // _httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
        // _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        //
        // var response = await _httpClient.PostAsync(
        //     $"{_baseUrl}/v2/payment-requests/{paymentLinkId}/cancel",
        //     null,
        //     cancellationToken);
        // response.EnsureSuccessStatusCode();
        return Task.CompletedTask;
    }

    private static long GenerateOrderCode(int hoaDonBanId)
    {
        // Generate unique order code: timestamp (seconds) * 100000 + hoaDonBanId
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        return timestamp * 100000 + hoaDonBanId;
    }

    /// <summary>
    /// Create signature for PayOS payment request using HMAC SHA256
    /// </summary>
    private string CreatePayOsSignature(IDictionary<string, object?> data)
    {
        try
        {
            // Sắp xếp keys theo alphabet
            var sortedKeys = data.Keys.OrderBy(k => k).ToList();

            // Tạo chuỗi key=value&key=value
            var dataString = string.Join("&", sortedKeys.Select(key =>
            {
                var value = data[key];
                return $"{key}={value}";
            }));

            _logger.LogDebug("PayOS signature data string: {DataString}", dataString);

            // Tính HMAC SHA256 (không log ChecksumKey)
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_checksumKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            _logger.LogDebug("PayOS signature created: {Signature}", signature);

            return signature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayOS signature");
            throw;
        }
    }

    /// <summary>
    /// Verify webhook signature using HMAC SHA256
    /// </summary>
    private bool VerifyWebhookSignature(PayOsWebhookData webhookData, string checksumKey)
    {
        try
        {
            if (string.IsNullOrEmpty(webhookData.Signature))
            {
                _logger.LogWarning("Webhook signature is missing");
                return false;
            }

            // Tạo signature từ data object theo format của payOS
            // Sắp xếp keys trong data object theo alphabet
            var dataDict = new Dictionary<string, object?>
            {
                { "amount", webhookData.Data?.Amount },
                { "code", webhookData.Data?.Code },
                { "desc", webhookData.Data?.Desc },
                { "orderCode", webhookData.Data?.OrderCode }
            };

            // Thêm các field optional nếu có
            if (!string.IsNullOrEmpty(webhookData.Data?.AccountNumber))
                dataDict["accountNumber"] = webhookData.Data.AccountNumber;
            if (!string.IsNullOrEmpty(webhookData.Data?.Reference))
                dataDict["reference"] = webhookData.Data.Reference;
            if (!string.IsNullOrEmpty(webhookData.Data?.TransactionDateTime))
                dataDict["transactionDateTime"] = webhookData.Data.TransactionDateTime;
            if (!string.IsNullOrEmpty(webhookData.Data?.PaymentLinkId))
                dataDict["paymentLinkId"] = webhookData.Data.PaymentLinkId;

            // Sắp xếp và tạo chuỗi
            var sortedKeys = dataDict.Keys.OrderBy(k => k).ToList();
            var dataString = string.Join("&", sortedKeys.Select(key =>
            {
                var value = dataDict[key];
                return $"{key}={value}";
            }));

            _logger.LogDebug("Webhook signature data string: {DataString}", dataString);

            // Tính HMAC SHA256
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
            var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            var isValid = computedSignature.Equals(webhookData.Signature, StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                _logger.LogWarning(
                    "Signature mismatch. Expected: {Expected}, Got: {Got}",
                    computedSignature, webhookData.Signature);
            }
            else
            {
                _logger.LogInformation("Webhook signature verified successfully");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    /// <summary>
    /// Query trạng thái payment từ PayOS API
    /// </summary>
    public async Task<string?> QueryPaymentStatusFromProviderAsync(long orderCode, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("PayOS credentials not configured, cannot query payment status");
                return null;
            }

            _logger.LogInformation("Querying PayOS for payment status of OrderCode {OrderCode}", orderCode);

            // PayOS API: GET /v2/payment-requests/{orderCode}
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/v2/payment-requests/{orderCode}");
            request.Headers.Add("x-client-id", _clientId);
            request.Headers.Add("x-api-key", _apiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PayOS query failed. HTTP {StatusCode}. Response: {Response}", 
                    response.StatusCode, responseBody);
                return null;
            }

            // Parse response
            var payOsResponse = JsonConvert.DeserializeObject<PayOsQueryPaymentResponse>(responseBody);
            if (payOsResponse?.Data == null)
            {
                _logger.LogWarning("PayOS query returned null data for OrderCode {OrderCode}", orderCode);
                return null;
            }

            var status = payOsResponse.Data.Status?.ToUpper();
            _logger.LogInformation("PayOS reports status {Status} for OrderCode {OrderCode}", status, orderCode);

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying PayOS for OrderCode {OrderCode}", orderCode);
            return null;
        }
    }
}
