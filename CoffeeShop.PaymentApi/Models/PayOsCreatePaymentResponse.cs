namespace CoffeeShop.PaymentApi.Models;

/// <summary>
/// Response model from PayOS create payment API
/// </summary>
public class PayOsCreatePaymentResponse
{
    public string Code { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public PayOsCreatePaymentResponseData? Data { get; set; }
    public string? Signature { get; set; }
}

public class PayOsCreatePaymentResponseData
{
    public string Bin { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public long OrderCode { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentLinkId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
}
