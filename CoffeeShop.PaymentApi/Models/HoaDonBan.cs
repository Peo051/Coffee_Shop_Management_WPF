namespace CoffeeShop.PaymentApi.Models;

/// <summary>
/// Model hóa đơn bán (chỉ các field cần thiết cho payment)
/// </summary>
public class HoaDonBan
{
    public int HoaDonBanId { get; set; }
    public DateTime NgayBan { get; set; }
    public decimal TongTien { get; set; }
    public decimal GiamGia { get; set; }
    public decimal ThanhToan { get; set; }
    public int CreatedByUserId { get; set; }
    public string? TrangThaiThanhToan { get; set; }
    public string? HinhThucThanhToan { get; set; }
    public string? MaGiaoDich { get; set; }

    // QR Payment fields
    public string? PaymentProvider { get; set; }
    public string? ProviderPaymentId { get; set; }
    public long? ProviderOrderCode { get; set; }
    public string? QRCodeRaw { get; set; }
    public string? CheckoutUrl { get; set; }
    public DateTime? QRExpiredAt { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? PaymentConfirmedAt { get; set; }
}
