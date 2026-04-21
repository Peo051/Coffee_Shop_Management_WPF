namespace CoffeeShop.Wpf.Services;

public interface INavigationService
{
    event EventHandler? CurrentViewModelChanged;

    object? CurrentViewModel { get; }

    void Navigate(object viewModel);
}
