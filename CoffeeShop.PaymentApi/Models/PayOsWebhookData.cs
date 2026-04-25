namespace CoffeeShop.PaymentApi.Models;

/// <summary>
/// Model để deserialize webhook payload từ payOS
/// </summary>
public class PayOsWebhookData
{
    public string Code { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public bool Success { get; set; }
    public PayOsWebhookDataDetail? Data { get; set; }
    public string? Signature { get; set; }
}

public class PayOsWebhookDataDetail
{
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? Reference { get; set; }
    public string? TransactionDateTime { get; set; }
    public string? PaymentLinkId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
    public string? VirtualAccountName { get; set; }
    public string? VirtualAccountNumber { get; set; }
}
