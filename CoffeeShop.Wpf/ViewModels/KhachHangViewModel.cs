using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class KhachHangViewModel : BaseViewModel
{
    private readonly IKhachHangService _khachHangService;
    private readonly RelayCommand _themCommand;
    private readonly RelayCommand _suaCommand;
    private readonly RelayCommand _timKiemCommand;
    private readonly RelayCommand _lamMoiCommand;

    private string _hoTen = string.Empty;
    private string _soDienThoai = string.Empty;
    private string _email = string.Empty;
    private bool _isActive = true;
    private string _tuKhoa = string.Empty;

    private KhachHang? _selectedKhachHang;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public KhachHangViewModel(IKhachHangService khachHangService)
    {
        _khachHangService = khachHangService;

        DanhSachKhachHang = new ObservableCollection<KhachHang>();

        _themCommand = new RelayCommand(ExecuteThem, CanExecuteThem);
        _suaCommand = new RelayCommand(ExecuteSua, CanExecuteSua);
        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<KhachHang> DanhSachKhachHang { get; }

    public KhachHang? SelectedKhachHang
    {
        get => _selectedKhachHang;
        set
        {
            if (SetProperty(ref _selectedKhachHang, value) && value is not null)
            {
                HoTen = value.HoTen;
                SoDienThoai = value.SoDienThoai ?? string.Empty;
                Email = value.Email ?? string.Empty;
                IsActive = value.IsActive;
                _suaCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string HoTen
    {
        get => _hoTen;
        set
        {
            if (SetProperty(ref _hoTen, value))
            {
                _themCommand.RaiseCanExecuteChanged();
                _suaCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SoDienThoai
    {
        get => _soDienThoai;
        set => SetProperty(ref _soDienThoai, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string TuKhoa
    {
        get => _tuKhoa;
        set => SetProperty(ref _tuKhoa, value);
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
                _themCommand.RaiseCanExecuteChanged();
                _suaCommand.RaiseCanExecuteChanged();
                _timKiemCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand ThemCommand => _themCommand;

    public ICommand SuaCommand => _suaCommand;

    public ICommand TimKiemCommand => _timKiemCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadDataAsync(TuKhoa, cancellationToken);
    }

    private bool CanExecuteThem()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(HoTen);
    }

    private bool CanExecuteSua()
    {
        return !IsBusy && SelectedKhachHang is not null;
    }

    private async void ExecuteThem()
    {
        await ThemAsync();
    }

    private async void ExecuteSua()
    {
        await SuaAsync();
    }

    private async void ExecuteTimKiem()
    {
        await LoadDataAsync(TuKhoa);
    }

    private async void ExecuteLamMoi()
    {
        HoTen = string.Empty;
        SoDienThoai = string.Empty;
        Email = string.Empty;
        IsActive = true;
        TuKhoa = string.Empty;
        SelectedKhachHang = null;
        await LoadDataAsync(null);
    }

    private async Task ThemAsync(CancellationToken cancellationToken = default)
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
            var result = await _khachHangService.TaoKhachHangAsync(
                new KhachHang
                {
                    HoTen = HoTen,
                    SoDienThoai = SoDienThoai,
                    Email = Email,
                    IsActive = IsActive,
                    DiemTichLuy = 0
                },
                cancellationToken);

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
            ErrorMessage = $"Không thể thêm khách hàng: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SuaAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || SelectedKhachHang is null)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        IsBusy = true;
        try
        {
            var result = await _khachHangService.CapNhatKhachHangAsync(
                new KhachHang
                {
                    KhachHangId = SelectedKhachHang.KhachHangId,
                    HoTen = HoTen,
                    SoDienThoai = SoDienThoai,
                    Email = Email,
                    IsActive = IsActive,
                    DiemTichLuy = SelectedKhachHang.DiemTichLuy
                },
                cancellationToken);

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
            ErrorMessage = $"Không thể cập nhật khách hàng: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
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
            var result = await _khachHangService.GetDanhSachKhachHangAsync(keyword, null, cancellationToken);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            DanhSachKhachHang.Clear();
            foreach (var item in result.Data)
            {
                DanhSachKhachHang.Add(item);
            }

            SuccessMessage = $"Đã tải {DanhSachKhachHang.Count} khách hàng.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải danh sách khách hàng: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}



