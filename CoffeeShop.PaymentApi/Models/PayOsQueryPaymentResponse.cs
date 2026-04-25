namespace CoffeeShop.PaymentApi.Models;

/// <summary>
/// Response từ PayOS API khi query payment status
/// GET /v2/payment-requests/{orderCode}
/// </summary>
public class PayOsQueryPaymentResponse
{
    public string? Code { get; set; }
    public string? Desc { get; set; }
    public PayOsQueryPaymentData? Data { get; set; }
}

public class PayOsQueryPaymentData
{
    public string? Id { get; set; }
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountRemaining { get; set; }
    public string? Status { get; set; } // PENDING, PAID, CANCELLED, EXPIRED
    public DateTime? CreatedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    public string? CancellationReason { get; set; }
}
