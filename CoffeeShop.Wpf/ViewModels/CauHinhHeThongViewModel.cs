using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class CauHinhHeThongViewModel : BaseViewModel
{
    private readonly ICauHinhHeThongService _cauHinhService;
    private readonly RelayCommand _luuCommand;
    private readonly RelayCommand _taiLaiCommand;

    private string _tenQuan = string.Empty;
    private string _diaChi = string.Empty;
    private string _soDienThoai = string.Empty;
    private string _footerHoaDon = string.Empty;
    private string _logoPath = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public CauHinhHeThongViewModel(ICauHinhHeThongService cauHinhService)
    {
        _cauHinhService = cauHinhService;
        _luuCommand = new RelayCommand(ExecuteLuu, CanExecuteLuu);
        _taiLaiCommand = new RelayCommand(ExecuteTaiLai, () => !IsBusy);
    }

    public string TenQuan
    {
        get => _tenQuan;
        set
        {
            if (SetProperty(ref _tenQuan, value))
            {
                _luuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string DiaChi
    {
        get => _diaChi;
        set => SetProperty(ref _diaChi, value);
    }

    public string SoDienThoai
    {
        get => _soDienThoai;
        set => SetProperty(ref _soDienThoai, value);
    }

    public string FooterHoaDon
    {
        get => _footerHoaDon;
        set => SetProperty(ref _footerHoaDon, value);
    }

    public string LogoPath
    {
        get => _logoPath;
        set => SetProperty(ref _logoPath, value);
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
                _luuCommand.RaiseCanExecuteChanged();
                _taiLaiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand LuuCommand => _luuCommand;

    public ICommand TaiLaiCommand => _taiLaiCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
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
            var result = await _cauHinhService.GetCauHinhAsync(cancellationToken);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            MapFromModel(result.Data);
            SuccessMessage = result.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải cấu hình hệ thống: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteLuu()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(TenQuan);
    }

    private async void ExecuteLuu()
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
            var result = await _cauHinhService.LuuCauHinhAsync(
                new CauHinhHeThong
                {
                    TenQuan = TenQuan,
                    DiaChi = DiaChi,
                    SoDienThoai = SoDienThoai,
                    FooterHoaDon = FooterHoaDon,
                    LogoPath = LogoPath
                });

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể lưu cấu hình hệ thống: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteTaiLai()
    {
        await LoadAsync();
    }

    private void MapFromModel(CauHinhHeThong data)
    {
        TenQuan = data.TenQuan;
        DiaChi = data.DiaChi ?? string.Empty;
        SoDienThoai = data.SoDienThoai ?? string.Empty;
        FooterHoaDon = data.FooterHoaDon ?? string.Empty;
        LogoPath = data.LogoPath ?? string.Empty;
    }
}



