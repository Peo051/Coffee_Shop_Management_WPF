namespace CoffeeShop.PaymentApi.Models;

/// <summary>
/// Response sau khi tạo QR payment thành công
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
