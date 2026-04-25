using CoffeeShop.PaymentApi.Models;

namespace CoffeeShop.PaymentApi.Services;

/// <summary>
/// Interface cho payment gateway service
/// </summary>
public interface IPaymentGatewayService
{
    /// <summary>
    /// Tạo QR payment cho hóa đơn
    /// </summary>
    Task<CreateQRPaymentResponse> CreateQrAsync(int hoaDonBanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy trạng thái thanh toán
    /// </summary>
    Task<PaymentStatusResponse?> GetStatusAsync(int hoaDonBanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query trạng thái payment từ PayOS API (để đối soát)
    /// </summary>
    Task<string?> QueryPaymentStatusFromProviderAsync(long orderCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xử lý webhook từ payOS
    /// </summary>
    Task<WebhookResult> HandleWebhookAsync(string rawJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy payment
    /// </summary>
    Task<bool> CancelAsync(int hoaDonBanId, CancellationToken cancellationToken = default);
}
