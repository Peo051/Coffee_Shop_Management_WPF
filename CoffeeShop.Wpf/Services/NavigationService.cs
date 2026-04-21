namespace CoffeeShop.Wpf.Services;

public sealed class NavigationService : INavigationService
{
    private object? _currentViewModel;

    public event EventHandler? CurrentViewModelChanged;

    public object? CurrentViewModel => _currentViewModel;

    public void Navigate(object viewModel)
    {
        _currentViewModel = viewModel;
        CurrentViewModelChanged?.Invoke(this, EventArgs.Empty);
    }
}
