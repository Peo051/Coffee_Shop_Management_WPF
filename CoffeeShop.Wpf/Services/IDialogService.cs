namespace CoffeeShop.Wpf.Services;

public interface IDialogService
{
    bool ShowConfirmation(string message, string title);
    
    string? ShowPasswordInput(string message, string title);
    
    void ShowError(string message, string title = "Lỗi");
    
    void ShowInformation(string message, string title = "Thông báo");
}
