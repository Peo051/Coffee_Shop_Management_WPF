namespace CoffeeShop.Wpf.Models;

public sealed class UserSessionModel
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime LoginAt { get; set; }
}
