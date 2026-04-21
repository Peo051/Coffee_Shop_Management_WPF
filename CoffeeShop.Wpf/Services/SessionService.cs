using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public sealed class SessionService
{
    public UserSessionModel? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public void SetCurrentUser(UserSessionModel user)
    {
        CurrentUser = user;
    }

    public void Clear()
    {
        CurrentUser = null;
    }
}
