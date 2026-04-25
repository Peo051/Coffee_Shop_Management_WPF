namespace CoffeeShop.PaymentApi.Models;

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
