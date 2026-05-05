namespace CoffeeShop.PaymentApi.Models;

/// <summary>
/// Request model for PayOS create payment API
/// </summary>
public class PayOsCreatePaymentRequest
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<PayOsItem> Items { get; set; } = new();
    public string CancelUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public long? ExpiredAt { get; set; }
    public string? Signature { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }
    public string? BuyerAddress { get; set; }
}

public class PayOsItem
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Price { get; set; }
}
