using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;
using System.Windows.Input;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly SessionService _sessionService;
    private readonly INavigationService _navigationService;
    private readonly MainShellViewModel _mainShellViewModel;
    private readonly RelayCommand _loginCommand;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(
        IAuthService authService,
        SessionService sessionService,
        INavigationService navigationService,
        MainShellViewModel mainShellViewModel)
    {
        _authService = authService;
        _sessionService = sessionService;
        _navigationService = navigationService;
        _mainShellViewModel = mainShellViewModel;

        _loginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
    }

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                _loginCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                _loginCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                _loginCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand LoginCommand => _loginCommand;

    public void PrepareForLoginScreen()
    {
        Username = string.Empty;
        Password = string.Empty;
        ErrorMessage = string.Empty;
    }

    private bool CanExecuteLogin()
    {
        return !IsBusy
               && !string.IsNullOrWhiteSpace(Username)
               && !string.IsNullOrWhiteSpace(Password);
    }

    private async void ExecuteLogin()
    {
        await LoginAsync();
    }

    private async Task LoginAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _authService.LoginAsync(Username.Trim(), Password, cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            var session = new UserSessionModel
            {
                UserId = result.UserId,
                Username = result.Username,
                DisplayName = result.DisplayName,
                Role = result.Role,
                LoginAt = DateTime.Now
            };

            _sessionService.SetCurrentUser(session);
            _mainShellViewModel.ApplySession(session);
            _navigationService.Navigate(_mainShellViewModel);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể đăng nhập: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
