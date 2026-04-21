using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class TrangThaiSanPhamViewModel : BaseViewModel
{
    private readonly IKhoService _khoService;
    private readonly IDanhMucService _danhMucService;
    private readonly RelayCommand _timKiemCommand;
    private readonly RelayCommand _lamMoiCommand;
    private readonly RelayCommand _khoaMoSanPhamCommand;
    private readonly RelayCommand _capNhatNguongCommand;

    private string _tuKhoaTimKiem = string.Empty;
    private DanhMuc? _selectedDanhMucTimKiem;
    private TrangThaiFilterItem? _selectedTrangThaiTimKiem;
    private TrangThaiSanPhamDong? _selectedSanPham;
    private string _mucCanhBaoTonKhoInput = "10";
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public TrangThaiSanPhamViewModel(IKhoService khoService, IDanhMucService danhMucService)
    {
        _khoService = khoService;
        _danhMucService = danhMucService;

        DanhSachSanPham = new ObservableCollection<TrangThaiSanPhamDong>();
        DanhMucs = new ObservableCollection<DanhMuc>();
        TrangThaiFilters = new ObservableCollection<TrangThaiFilterItem>
        {
            new("Tất cả", null),
            new("Đang kinh doanh", true),
            new("Ngừng kinh doanh", false)
        };

        _selectedTrangThaiTimKiem = TrangThaiFilters[0];

        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
        _khoaMoSanPhamCommand = new RelayCommand(ExecuteKhoaMoSanPham, CanExecuteKhoaMoSanPham);
        _capNhatNguongCommand = new RelayCommand(ExecuteCapNhatNguong, CanExecuteCapNhatNguong);
    }

    public ObservableCollection<TrangThaiSanPhamDong> DanhSachSanPham { get; }

    public ObservableCollection<DanhMuc> DanhMucs { get; }

    public ObservableCollection<TrangThaiFilterItem> TrangThaiFilters { get; }

    public string TuKhoaTimKiem
    {
        get => _tuKhoaTimKiem;
        set => SetProperty(ref _tuKhoaTimKiem, value);
    }

    public DanhMuc? SelectedDanhMucTimKiem
    {
        get => _selectedDanhMucTimKiem;
        set => SetProperty(ref _selectedDanhMucTimKiem, value);
    }

    public TrangThaiFilterItem? SelectedTrangThaiTimKiem
    {
        get => _selectedTrangThaiTimKiem;
        set => SetProperty(ref _selectedTrangThaiTimKiem, value);
    }

    public TrangThaiSanPhamDong? SelectedSanPham
    {
        get => _selectedSanPham;
        set
        {
            if (SetProperty(ref _selectedSanPham, value))
            {
                if (value is not null)
                {
                    MucCanhBaoTonKhoInput = value.MucCanhBaoTonKho.ToString();
                }

                _khoaMoSanPhamCommand.RaiseCanExecuteChanged();
                _capNhatNguongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string MucCanhBaoTonKhoInput
    {
        get => _mucCanhBaoTonKhoInput;
        set
        {
            if (SetProperty(ref _mucCanhBaoTonKhoInput, value))
            {
                _capNhatNguongCommand.RaiseCanExecuteChanged();
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
                _lamMoiCommand.RaiseCanExecuteChanged();
                _khoaMoSanPhamCommand.RaiseCanExecuteChanged();
                _capNhatNguongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TimKiemCommand => _timKiemCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public ICommand KhoaMoSanPhamCommand => _khoaMoSanPhamCommand;

    public ICommand CapNhatNguongCommand => _capNhatNguongCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            await LoadDanhMucAsync(cancellationToken);
            await TaiDanhSachAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu trạng thái sản phẩm: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteKhoaMoSanPham()
    {
        return !IsBusy && SelectedSanPham is not null;
    }

    private bool CanExecuteCapNhatNguong()
    {
        return !IsBusy && SelectedSanPham is not null && !string.IsNullOrWhiteSpace(MucCanhBaoTonKhoInput);
    }

    private async void ExecuteTimKiem()
    {
        await TaiDanhSachAsync();
    }

    private async void ExecuteLamMoi()
    {
        TuKhoaTimKiem = string.Empty;
        SelectedDanhMucTimKiem = null;
        SelectedTrangThaiTimKiem = TrangThaiFilters.FirstOrDefault();
        SelectedSanPham = null;

        await TaiDanhSachAsync();
    }

    private async void ExecuteKhoaMoSanPham()
    {
        if (SelectedSanPham is null || IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsBusy = true;

        try
        {
            var result = await _khoService.CapNhatTrangThaiKinhDoanhAsync(
                SelectedSanPham.MonId,
                !SelectedSanPham.IsActive);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            await TaiDanhSachAsyncCore();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể cập nhật trạng thái sản phẩm: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteCapNhatNguong()
    {
        if (SelectedSanPham is null || IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (!int.TryParse(MucCanhBaoTonKhoInput, out var nguong))
        {
            ErrorMessage = "Mức cảnh báo tồn kho phải là số nguyên.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _khoService.CapNhatMucCanhBaoTonKhoAsync(SelectedSanPham.MonId, nguong);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            await TaiDanhSachAsyncCore();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể cập nhật mức cảnh báo tồn kho: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TaiDanhSachAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            await TaiDanhSachAsyncCore(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải danh sách trạng thái sản phẩm: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TaiDanhSachAsyncCore(CancellationToken cancellationToken = default)
    {
        var selectedMonId = SelectedSanPham?.MonId;

        var result = await _khoService.GetTrangThaiSanPhamAsync(
            TuKhoaTimKiem,
            SelectedDanhMucTimKiem?.DanhMucId,
            SelectedTrangThaiTimKiem?.Value,
            cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message;
            return;
        }

        DanhSachSanPham.Clear();
        foreach (var item in result.Data)
        {
            DanhSachSanPham.Add(item);
        }

        if (selectedMonId.HasValue)
        {
            SelectedSanPham = DanhSachSanPham.FirstOrDefault(x => x.MonId == selectedMonId.Value);
        }

        if (DanhSachSanPham.Count == 0)
        {
            SuccessMessage = "Không có sản phẩm phù hợp điều kiện lọc.";
        }
        else
        {
            SuccessMessage = $"Đã tải {DanhSachSanPham.Count} sản phẩm.";
        }
    }

    private async Task LoadDanhMucAsync(CancellationToken cancellationToken)
    {
        var danhMucs = await _danhMucService.GetAllAsync(cancellationToken: cancellationToken);

        DanhMucs.Clear();
        foreach (var item in danhMucs.OrderBy(x => x.TenDanhMuc))
        {
            DanhMucs.Add(item);
        }
    }
}

public sealed class TrangThaiFilterItem
{
    public TrangThaiFilterItem(string displayName, bool? value)
    {
        DisplayName = displayName;
        Value = value;
    }

    public string DisplayName { get; }

    public bool? Value { get; }
}
