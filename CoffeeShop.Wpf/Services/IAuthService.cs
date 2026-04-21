using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
