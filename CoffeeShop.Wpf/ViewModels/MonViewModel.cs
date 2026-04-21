using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class MonViewModel : BaseViewModel
{
    private readonly IMonService _monService;
    private readonly IDanhMucService _danhMucService;
    private readonly RelayCommand _taoMoiCommand;
    private readonly RelayCommand _timKiemCommand;
    private readonly RelayCommand _lamMoiCommand;

    private string _tenMon = string.Empty;
    private string _donGia = string.Empty;
    private string _tonKhoBanDau = "0";
    private string _tonKhoToiThieu = "10";
    private string _hinhAnhPath = string.Empty;
    private string _tuKhoaTimKiem = string.Empty;
    private DanhMuc? _selectedDanhMucTaoMoi;
    private DanhMuc? _selectedDanhMucTimKiem;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public MonViewModel(IMonService monService, IDanhMucService danhMucService)
    {
        _monService = monService;
        _danhMucService = danhMucService;

        Mons = new ObservableCollection<Mon>();
        DanhMucs = new ObservableCollection<DanhMuc>();

        _taoMoiCommand = new RelayCommand(ExecuteTaoMoi, CanExecuteTaoMoi);
        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<Mon> Mons { get; }

    public ObservableCollection<DanhMuc> DanhMucs { get; }

    public string TenMon
    {
        get => _tenMon;
        set
        {
            if (SetProperty(ref _tenMon, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string DonGia
    {
        get => _donGia;
        set
        {
            if (SetProperty(ref _donGia, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TonKhoBanDau
    {
        get => _tonKhoBanDau;
        set
        {
            if (SetProperty(ref _tonKhoBanDau, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TonKhoToiThieu
    {
        get => _tonKhoToiThieu;
        set
        {
            if (SetProperty(ref _tonKhoToiThieu, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string HinhAnhPath
    {
        get => _hinhAnhPath;
        set
        {
            if (SetProperty(ref _hinhAnhPath, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public DanhMuc? SelectedDanhMucTaoMoi
    {
        get => _selectedDanhMucTaoMoi;
        set
        {
            if (SetProperty(ref _selectedDanhMucTaoMoi, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

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
            await LoadMonAsync(TuKhoaTimKiem, SelectedDanhMucTimKiem?.DanhMucId, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu sản phẩm: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteTaoMoi()
    {
        return !IsBusy
               && !string.IsNullOrWhiteSpace(TenMon)
               && !string.IsNullOrWhiteSpace(DonGia)
               && !string.IsNullOrWhiteSpace(TonKhoBanDau)
               && !string.IsNullOrWhiteSpace(TonKhoToiThieu)
               && !string.IsNullOrWhiteSpace(HinhAnhPath)
               && SelectedDanhMucTaoMoi is not null;
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

        if (SelectedDanhMucTaoMoi is null)
        {
            ErrorMessage = "Vui lòng chọn danh mục cho sản phẩm.";
            return;
        }

        if (!TryParseDecimal(DonGia, out var donGiaValue))
        {
            ErrorMessage = "Đơn giá không hợp lệ.";
            return;
        }

        if (!int.TryParse(TonKhoBanDau, out var tonKhoValue) || tonKhoValue < 0)
        {
            ErrorMessage = "Tồn kho ban đầu phải là số nguyên không âm.";
            return;
        }

        if (!int.TryParse(TonKhoToiThieu, out var tonKhoToiThieuValue) || tonKhoToiThieuValue < 0)
        {
            ErrorMessage = "Tồn kho tối thiểu phải là số nguyên không âm.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _monService.CreateAsync(
                TenMon,
                SelectedDanhMucTaoMoi.DanhMucId,
                donGiaValue,
                tonKhoValue,
                tonKhoToiThieuValue,
                HinhAnhPath,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            TenMon = string.Empty;
            DonGia = string.Empty;
            TonKhoBanDau = "0";
            TonKhoToiThieu = "10";
            HinhAnhPath = string.Empty;

            await LoadMonAsync(TuKhoaTimKiem, SelectedDanhMucTimKiem?.DanhMucId, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tạo sản phẩm: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteTimKiem()
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
            await LoadMonAsync(TuKhoaTimKiem, SelectedDanhMucTimKiem?.DanhMucId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tìm kiếm sản phẩm: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteLamMoi()
    {
        if (IsBusy)
        {
            return;
        }

        TuKhoaTimKiem = string.Empty;
        SelectedDanhMucTimKiem = null;

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            await LoadMonAsync(null, null);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải lại dữ liệu sản phẩm: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
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

        if (SelectedDanhMucTaoMoi is null && DanhMucs.Count > 0)
        {
            SelectedDanhMucTaoMoi = DanhMucs[0];
        }
    }

    private async Task LoadMonAsync(string? keyword, int? danhMucId, CancellationToken cancellationToken = default)
    {
        var mons = await _monService.SearchAsync(keyword, danhMucId, cancellationToken);

        Mons.Clear();
        foreach (var item in mons)
        {
            Mons.Add(item);
        }

        if (Mons.Count == 0)
        {
            SuccessMessage = "Không có dữ liệu phù hợp.";
        }
        else if (string.IsNullOrWhiteSpace(keyword) && danhMucId is null)
        {
            SuccessMessage = $"Đã tải {Mons.Count} sản phẩm.";
        }
        else
        {
            SuccessMessage = $"Tìm thấy {Mons.Count} sản phẩm theo điều kiện lọc.";
        }
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
        {
            return true;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }
}
