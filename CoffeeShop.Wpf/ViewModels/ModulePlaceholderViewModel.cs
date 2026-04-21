namespace CoffeeShop.Wpf.ViewModels;

public sealed class ModulePlaceholderViewModel : BaseViewModel
{
    public ModulePlaceholderViewModel(string title, string message)
    {
        Title = title;
        Message = message;
    }

    public string Title { get; }

    public string Message { get; }
}
