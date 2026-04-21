namespace CoffeeShop.Wpf.Models;

public sealed class UserAuthRecord
{
    public int UserId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
