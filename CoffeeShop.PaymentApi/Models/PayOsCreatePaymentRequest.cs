namespace CoffeeShop.PaymentApi.Models;

/// <summary>
/// Request model for PayOS create payment API
/// </summary>
public class PayOsCreatePaymentRequest
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public long? ExpiredAt { get; set; }
    public string Signature { get; set; } = string.Empty;
}
