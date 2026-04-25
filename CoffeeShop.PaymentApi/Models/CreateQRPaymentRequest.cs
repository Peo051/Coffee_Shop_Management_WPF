namespace CoffeeShop.PaymentApi.Models;

/// <summary>
/// Request để tạo QR payment cho hóa đơn
/// </summary>
public class CreateQRPaymentRequest
{
    /// <summary>
    /// ID hóa đơn bán cần tạo QR payment
    /// </summary>
    public int HoaDonBanId { get; set; }
}
