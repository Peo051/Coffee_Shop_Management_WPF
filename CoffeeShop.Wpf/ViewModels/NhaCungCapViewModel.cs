using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class NhaCungCapViewModel : BaseViewModel
{
    private readonly INhaCungCapService _nhaCungCapService;
    private readonly RelayCommand _taoMoiCommand;
    private readonly RelayCommand _timKiemCommand;
    private readonly RelayCommand _lamMoiCommand;

    private string _tenNhaCungCap = string.Empty;
    private string _soDienThoai = string.Empty;
    private string _diaChi = string.Empty;
    private string _tuKhoaTimKiem = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public NhaCungCapViewModel(INhaCungCapService nhaCungCapService)
    {
        _nhaCungCapService = nhaCungCapService;
        NhaCungCaps = new ObservableCollection<NhaCungCap>();

        _taoMoiCommand = new RelayCommand(ExecuteTaoMoi, CanExecuteTaoMoi);
        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<NhaCungCap> NhaCungCaps { get; }

    public string TenNhaCungCap
    {
        get => _tenNhaCungCap;
        set
        {
            if (SetProperty(ref _tenNhaCungCap, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SoDienThoai
    {
        get => _soDienThoai;
        set => SetProperty(ref _soDienThoai, value);
    }

    public string DiaChi
    {
        get => _diaChi;
        set => SetProperty(ref _diaChi, value);
    }

    public string TuKhoaTimKiem
    {
        get => _tuKhoaTimKiem;
        set => SetProperty(ref _tuKhoaTimKiem, value);
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
                _taoMoiCommand.RaiseCanExecuteChanged();
                _timKiemCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TaoMoiCommand => _taoMoiCommand;

    public ICommand TimKiemCommand => _timKiemCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadDataAsync(TuKhoaTimKiem, cancellationToken);
    }

    private bool CanExecuteTaoMoi()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(TenNhaCungCap);
    }

    private async void ExecuteTaoMoi()
    {
        await TaoMoiAsync();
    }

    private async Task TaoMoiAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(TenNhaCungCap))
        {
            ErrorMessage = "Tên nhà cung cấp không được để trống.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _nhaCungCapService.CreateAsync(TenNhaCungCap, SoDienThoai, DiaChi, cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            TenNhaCungCap = string.Empty;
            SoDienThoai = string.Empty;
            DiaChi = string.Empty;

            await LoadDataAsync(TuKhoaTimKiem, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tạo nhà cung cấp: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteTimKiem()
    {
        await LoadDataAsync(TuKhoaTimKiem);
    }

    private async void ExecuteLamMoi()
    {
        TuKhoaTimKiem = string.Empty;
        await LoadDataAsync(null);
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
            var nhaCungCaps = await _nhaCungCapService.GetAllAsync(keyword, cancellationToken);

            NhaCungCaps.Clear();
            foreach (var item in nhaCungCaps)
            {
                NhaCungCaps.Add(item);
            }

            if (NhaCungCaps.Count == 0)
            {
                SuccessMessage = "Không có dữ liệu phù hợp.";
            }
            else if (string.IsNullOrWhiteSpace(keyword))
            {
                SuccessMessage = $"Đã tải {NhaCungCaps.Count} nhà cung cấp.";
            }
            else
            {
                SuccessMessage = $"Tìm thấy {NhaCungCaps.Count} nhà cung cấp theo từ khóa '{keyword}'.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải nhà cung cấp: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
