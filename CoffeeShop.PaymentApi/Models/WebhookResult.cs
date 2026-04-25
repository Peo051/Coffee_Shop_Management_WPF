namespace CoffeeShop.PaymentApi.Models;

/// <summary>
/// Kết quả xử lý webhook
/// </summary>
public class WebhookResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static WebhookResult Ok(string message) => new() { Success = true, Message = message };
    public static WebhookResult Ignored(string message) => new() { Success = false, Message = message };
}
