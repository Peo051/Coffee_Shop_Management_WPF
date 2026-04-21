using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class DoiMatKhauViewModel : BaseViewModel
{
    private readonly IUserSecurityService _userSecurityService;
    private readonly SessionService _sessionService;
    private readonly RelayCommand _doiMatKhauCommand;
    private readonly RelayCommand _lamMoiCommand;

    private string _matKhauCu = string.Empty;
    private string _matKhauMoi = string.Empty;
    private string _xacNhanMatKhau = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public DoiMatKhauViewModel(IUserSecurityService userSecurityService, SessionService sessionService)
    {
        _userSecurityService = userSecurityService;
        _sessionService = sessionService;

        _doiMatKhauCommand = new RelayCommand(ExecuteDoiMatKhau, CanExecuteDoiMatKhau);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public string MatKhauCu
    {
        get => _matKhauCu;
        set
        {
            if (SetProperty(ref _matKhauCu, value))
            {
                _doiMatKhauCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string MatKhauMoi
    {
        get => _matKhauMoi;
        set
        {
            if (SetProperty(ref _matKhauMoi, value))
            {
                _doiMatKhauCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string XacNhanMatKhau
    {
        get => _xacNhanMatKhau;
        set
        {
            if (SetProperty(ref _xacNhanMatKhau, value))
            {
                _doiMatKhauCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        private set => SetProperty(ref _successMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                _doiMatKhauCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand DoiMatKhauCommand => _doiMatKhauCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        return Task.CompletedTask;
    }

    private bool CanExecuteDoiMatKhau()
    {
        return !IsBusy
               && !string.IsNullOrWhiteSpace(MatKhauCu)
               && !string.IsNullOrWhiteSpace(MatKhauMoi)
               && !string.IsNullOrWhiteSpace(XacNhanMatKhau);
    }

    private async void ExecuteDoiMatKhau()
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var userId = _sessionService.CurrentUser?.UserId ?? 0;
        if (userId <= 0)
        {
            ErrorMessage = "Không xác định được phiên đăng nhập hiện tại.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _userSecurityService.DoiMatKhauAsync(userId, MatKhauCu, MatKhauMoi, XacNhanMatKhau);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            ExecuteLamMoi();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể đổi mật khẩu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ExecuteLamMoi()
    {
        MatKhauCu = string.Empty;
        MatKhauMoi = string.Empty;
        XacNhanMatKhau = string.Empty;
    }
}



