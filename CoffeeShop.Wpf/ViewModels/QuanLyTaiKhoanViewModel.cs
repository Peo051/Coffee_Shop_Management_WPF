using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class QuanLyTaiKhoanViewModel : BaseViewModel
{
    private readonly IUserSecurityService _userSecurityService;
    private readonly SessionService _sessionService;
    private readonly IDialogService _dialogService;
    private readonly RelayCommand _timKiemCommand;
    private readonly RelayCommand _khoaTaiKhoanCommand;
    private readonly RelayCommand _moKhoaTaiKhoanCommand;
    private readonly RelayCommand _taoMoiCommand;
    private readonly RelayCommand _luuCommand;
    private readonly RelayCommand _huyCommand;
    private readonly RelayCommand _resetMatKhauCommand;

    private string _tuKhoa = string.Empty;
    private TaiKhoanNguoiDung? _selectedTaiKhoan;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    private string _tenDangNhapMoi = string.Empty;
    private string _matKhauMoi = string.Empty;
    private string _xacNhanMatKhauMoi = string.Empty;
    private string _hoTenMoi = string.Empty;
    private VaiTro? _selectedVaiTro;
    private bool _isFormVisible;

    public QuanLyTaiKhoanViewModel(IUserSecurityService userSecurityService, SessionService sessionService, IDialogService dialogService)
    {
        _userSecurityService = userSecurityService;
        _sessionService = sessionService;
        _dialogService = dialogService;

        DanhSachTaiKhoan = new ObservableCollection<TaiKhoanNguoiDung>();
        DanhSachVaiTro = new ObservableCollection<VaiTro>();

        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _khoaTaiKhoanCommand = new RelayCommand(ExecuteKhoaTaiKhoan, CanExecuteKhoaTaiKhoan);
        _moKhoaTaiKhoanCommand = new RelayCommand(ExecuteMoKhoaTaiKhoan, CanExecuteMoKhoaTaiKhoan);
        _taoMoiCommand = new RelayCommand(ExecuteTaoMoi, () => !IsBusy);
        _luuCommand = new RelayCommand(ExecuteLuu, CanExecuteLuu);
        _huyCommand = new RelayCommand(ExecuteHuy, () => IsFormVisible);
        _resetMatKhauCommand = new RelayCommand(ExecuteResetMatKhau, CanExecuteResetMatKhau);
    }

    public ObservableCollection<TaiKhoanNguoiDung> DanhSachTaiKhoan { get; }

    public ObservableCollection<VaiTro> DanhSachVaiTro { get; }

    public string TuKhoa
    {
        get => _tuKhoa;
        set => SetProperty(ref _tuKhoa, value);
    }

    public TaiKhoanNguoiDung? SelectedTaiKhoan
    {
        get => _selectedTaiKhoan;
        set
        {
            if (SetProperty(ref _selectedTaiKhoan, value))
            {
                _khoaTaiKhoanCommand.RaiseCanExecuteChanged();
                _moKhoaTaiKhoanCommand.RaiseCanExecuteChanged();
                _resetMatKhauCommand.RaiseCanExecuteChanged();
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
                _timKiemCommand.RaiseCanExecuteChanged();
                _khoaTaiKhoanCommand.RaiseCanExecuteChanged();
                _moKhoaTaiKhoanCommand.RaiseCanExecuteChanged();
                _taoMoiCommand.RaiseCanExecuteChanged();
                _luuCommand.RaiseCanExecuteChanged();
                _resetMatKhauCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TenDangNhapMoi
    {
        get => _tenDangNhapMoi;
        set
        {
            if (SetProperty(ref _tenDangNhapMoi, value))
            {
                _luuCommand.RaiseCanExecuteChanged();
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
                _luuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string XacNhanMatKhauMoi
    {
        get => _xacNhanMatKhauMoi;
        set
        {
            if (SetProperty(ref _xacNhanMatKhauMoi, value))
            {
                _luuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string HoTenMoi
    {
        get => _hoTenMoi;
        set
        {
            if (SetProperty(ref _hoTenMoi, value))
            {
                _luuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public VaiTro? SelectedVaiTro
    {
        get => _selectedVaiTro;
        set
        {
            if (SetProperty(ref _selectedVaiTro, value))
            {
                _luuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsFormVisible
    {
        get => _isFormVisible;
        private set
        {
            if (SetProperty(ref _isFormVisible, value))
            {
                _huyCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TimKiemCommand => _timKiemCommand;

    public ICommand KhoaTaiKhoanCommand => _khoaTaiKhoanCommand;

    public ICommand MoKhoaTaiKhoanCommand => _moKhoaTaiKhoanCommand;

    public ICommand TaoMoiCommand => _taoMoiCommand;

    public ICommand LuuCommand => _luuCommand;

    public ICommand HuyCommand => _huyCommand;

    public ICommand ResetMatKhauCommand => _resetMatKhauCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadVaiTroAsync(cancellationToken);
        await LoadDataAsync(TuKhoa, cancellationToken);
    }

    private bool CanExecuteKhoaTaiKhoan()
    {
        return !IsBusy && SelectedTaiKhoan is { IsActive: true };
    }

    private bool CanExecuteMoKhoaTaiKhoan()
    {
        return !IsBusy && SelectedTaiKhoan is { IsActive: false };
    }

    private bool CanExecuteLuu()
    {
        return !IsBusy
               && !string.IsNullOrWhiteSpace(TenDangNhapMoi)
               && !string.IsNullOrWhiteSpace(MatKhauMoi)
               && !string.IsNullOrWhiteSpace(XacNhanMatKhauMoi)
               && !string.IsNullOrWhiteSpace(HoTenMoi)
               && SelectedVaiTro is not null;
    }

    private bool CanExecuteResetMatKhau()
    {
        return !IsBusy && SelectedTaiKhoan is not null;
    }

    private async void ExecuteTimKiem()
    {
        await LoadDataAsync(TuKhoa);
    }

    private async void ExecuteKhoaTaiKhoan()
    {
        if (SelectedTaiKhoan is null)
        {
            return;
        }

        var confirmed = _dialogService.ShowConfirmation(
            $"Bạn có chắc chắn muốn khóa tài khoản '{SelectedTaiKhoan.TenDangNhap}'?",
            "Xác nhận khóa tài khoản");

        if (!confirmed)
        {
            return;
        }

        await DoiTrangThaiTaiKhoanAsync(isKhoa: true);
    }

    private async void ExecuteMoKhoaTaiKhoan()
    {
        if (SelectedTaiKhoan is null)
        {
            return;
        }

        var confirmed = _dialogService.ShowConfirmation(
            $"Bạn có chắc chắn muốn mở khóa tài khoản '{SelectedTaiKhoan.TenDangNhap}'?",
            "Xác nhận mở khóa tài khoản");

        if (!confirmed)
        {
            return;
        }

        await DoiTrangThaiTaiKhoanAsync(isKhoa: false);
    }

    private void ExecuteTaoMoi()
    {
        TenDangNhapMoi = string.Empty;
        MatKhauMoi = string.Empty;
        XacNhanMatKhauMoi = string.Empty;
        HoTenMoi = string.Empty;
        SelectedVaiTro = null;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsFormVisible = true;
    }

    private async void ExecuteLuu()
    {
        await TaoTaiKhoanMoiAsync();
    }

    private void ExecuteHuy()
    {
        IsFormVisible = false;
        TenDangNhapMoi = string.Empty;
        MatKhauMoi = string.Empty;
        XacNhanMatKhauMoi = string.Empty;
        HoTenMoi = string.Empty;
        SelectedVaiTro = null;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    private async void ExecuteResetMatKhau()
    {
        if (SelectedTaiKhoan is null)
        {
            return;
        }

        var matKhauMoi = _dialogService.ShowPasswordInput(
            $"Nhập mật khẩu mới cho tài khoản '{SelectedTaiKhoan.TenDangNhap}':",
            "Reset mật khẩu");

        if (matKhauMoi is not null)
        {
            await ResetMatKhauAsync(matKhauMoi);
        }
    }

    private async Task DoiTrangThaiTaiKhoanAsync(bool isKhoa, CancellationToken cancellationToken = default)
    {
        if (IsBusy || SelectedTaiKhoan is null)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var adminUserId = _sessionService.CurrentUser?.UserId ?? 0;
        if (adminUserId <= 0)
        {
            ErrorMessage = "Không xác định được phiên đăng nhập quản trị.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = isKhoa
                ? await _userSecurityService.KhoaTaiKhoanAsync(adminUserId, SelectedTaiKhoan.NguoiDungId, cancellationToken)
                : await _userSecurityService.MoKhoaTaiKhoanAsync(adminUserId, SelectedTaiKhoan.NguoiDungId, cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            await LoadDataAsync(TuKhoa, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể cập nhật trạng thái tài khoản: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TaoTaiKhoanMoiAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || SelectedVaiTro is null)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var adminUserId = _sessionService.CurrentUser?.UserId ?? 0;
        if (adminUserId <= 0)
        {
            ErrorMessage = "Không xác định được phiên đăng nhập quản trị.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _userSecurityService.TaoTaiKhoanMoiAsync(
                adminUserId,
                TenDangNhapMoi,
                MatKhauMoi,
                XacNhanMatKhauMoi,
                HoTenMoi,
                SelectedVaiTro.VaiTroId,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            IsFormVisible = false;
            TenDangNhapMoi = string.Empty;
            MatKhauMoi = string.Empty;
            XacNhanMatKhauMoi = string.Empty;
            HoTenMoi = string.Empty;
            SelectedVaiTro = null;

            await LoadDataAsync(TuKhoa, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tạo tài khoản mới: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResetMatKhauAsync(string matKhauMoi, CancellationToken cancellationToken = default)
    {
        if (IsBusy || SelectedTaiKhoan is null)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var adminUserId = _sessionService.CurrentUser?.UserId ?? 0;
        if (adminUserId <= 0)
        {
            ErrorMessage = "Không xác định được phiên đăng nhập quản trị.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _userSecurityService.ResetMatKhauAsync(
                adminUserId,
                SelectedTaiKhoan.NguoiDungId,
                matKhauMoi,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể reset mật khẩu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadVaiTroAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userSecurityService.GetDanhSachVaiTroAsync(cancellationToken);
            if (result.IsSuccess && result.Data is not null)
            {
                DanhSachVaiTro.Clear();
                foreach (var item in result.Data)
                {
                    DanhSachVaiTro.Add(item);
                }
            }
        }
        catch
        {
            // Không chặn luồng chính
        }
    }

    private async Task LoadDataAsync(string? keyword, CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        IsBusy = true;
        try
        {
            var result = await _userSecurityService.GetDanhSachTaiKhoanAsync(keyword, cancellationToken);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            DanhSachTaiKhoan.Clear();
            foreach (var item in result.Data)
            {
                DanhSachTaiKhoan.Add(item);
            }

            if (!IsFormVisible)
            {
                SuccessMessage = $"Đã tải {DanhSachTaiKhoan.Count} tài khoản.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải danh sách tài khoản: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
