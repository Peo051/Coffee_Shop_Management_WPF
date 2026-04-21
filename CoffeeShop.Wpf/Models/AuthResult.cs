namespace CoffeeShop.Wpf.Models;

public sealed class AuthResult
{
    private AuthResult(bool isSuccess, int userId, string username, string displayName, string role, string errorMessage)
    {
        IsSuccess = isSuccess;
        UserId = userId;
        Username = username;
        DisplayName = displayName;
        Role = role;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public int UserId { get; }

    public string Username { get; }

    public string DisplayName { get; }

    public string Role { get; }

    public string ErrorMessage { get; }

    public static AuthResult Success(int userId, string username, string displayName, string role)
    {
        return new AuthResult(true, userId, username, displayName, role, string.Empty);
    }

    public static AuthResult Fail(string errorMessage)
    {
        return new AuthResult(false, 0, string.Empty, string.Empty, string.Empty, errorMessage);
    }
}
