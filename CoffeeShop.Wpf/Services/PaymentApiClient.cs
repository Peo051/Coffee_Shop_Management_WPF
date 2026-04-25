using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CoffeeShop.Wpf.Services;

/// <summary>
/// Client gọi Payment API backend để xử lý QR payment
/// </summary>
public class PaymentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    // Constant fallback nếu không có config
    private const string DefaultBaseUrl = "https://localhost:5002";

    public PaymentApiClient(string? baseUrl = null)
    {
        // Ưu tiên: 1. Parameter truyền vào, 2. App.config, 3. Default constant
        _baseUrl = (baseUrl 
                    ?? ConfigurationManager.AppSettings["PaymentApiBaseUrl"] 
                    ?? DefaultBaseUrl)
                   .TrimEnd('/');
        
        // Configure HttpClient to accept self-signed certificates in development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        
        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Lấy BaseUrl hiện tại (để debug/logging)
    /// </summary>
    public string BaseUrl => _baseUrl;

    /// <summary>
    /// Tạo QR payment cho hóa đơn
    /// </summary>
    public async Task<CreateQRPaymentResponse?> CreateQrPaymentAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { hoaDonBanId };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/api/Payments/qr/create",
                content,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API returned {response.StatusCode}: {responseBody}");
            }

            return JsonConvert.DeserializeObject<CreateQRPaymentResponse>(responseBody);
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HTTP errors with details
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to connect to Payment API at {_baseUrl}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Lấy trạng thái thanh toán của hóa đơn
    /// </summary>
    public async Task<PaymentStatusResponse?> GetPaymentStatusAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/api/payments/{hoaDonBanId}/status",
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<PaymentStatusResponse>(responseBody);
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
        try
        {
            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/api/payments/{hoaDonBanId}/cancel",
                null,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Response từ API khi tạo QR payment
/// </summary>
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

/// <summary>
/// Response trạng thái thanh toán
/// </summary>
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
