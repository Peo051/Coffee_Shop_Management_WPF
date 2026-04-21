using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class HoaDonNhapViewModel : BaseViewModel
{
    private readonly IHoaDonNhapService _hoaDonNhapService;
    private readonly INhaCungCapService _nhaCungCapService;
    private readonly IMonService _monService;
    private readonly SessionService _sessionService;

    private readonly RelayCommand _themDongCommand;
    private readonly RelayCommand _xoaDongCommand;
    private readonly RelayCommand _luuHoaDonCommand;
    private readonly RelayCommand _lamMoiCommand;

    private NhaCungCap? _selectedNhaCungCap;
    private string _ghiChu = string.Empty;

    private Mon? _selectedMon;
    private string _soLuongNhap = "1";
    private string _donGiaNhap = string.Empty;

    private HoaDonNhapChiTietDongModel? _selectedDongChiTiet;
    private decimal _tongTien;

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public HoaDonNhapViewModel(
        IHoaDonNhapService hoaDonNhapService,
        INhaCungCapService nhaCungCapService,
        IMonService monService,
        SessionService sessionService)
    {
        _hoaDonNhapService = hoaDonNhapService;
        _nhaCungCapService = nhaCungCapService;
        _monService = monService;
        _sessionService = sessionService;

        NhaCungCaps = new ObservableCollection<NhaCungCap>();
        Mons = new ObservableCollection<Mon>();
        ChiTietLines = new ObservableCollection<HoaDonNhapChiTietDongModel>();

        _themDongCommand = new RelayCommand(ExecuteThemDong, CanExecuteThemDong);
        _xoaDongCommand = new RelayCommand(ExecuteXoaDong, CanExecuteXoaDong);
        _luuHoaDonCommand = new RelayCommand(ExecuteLuuHoaDon, CanExecuteLuuHoaDon);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<NhaCungCap> NhaCungCaps { get; }

    public ObservableCollection<Mon> Mons { get; }

    public ObservableCollection<HoaDonNhapChiTietDongModel> ChiTietLines { get; }

    public NhaCungCap? SelectedNhaCungCap
    {
        get => _selectedNhaCungCap;
        set
        {
            if (SetProperty(ref _selectedNhaCungCap, value))
            {
                _luuHoaDonCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string GhiChu
    {
        get => _ghiChu;
        set => SetProperty(ref _ghiChu, value);
    }

    public Mon? SelectedMon
    {
        get => _selectedMon;
        set
        {
            if (SetProperty(ref _selectedMon, value))
            {
                _themDongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SoLuongNhap
    {
        get => _soLuongNhap;
        set
        {
            if (SetProperty(ref _soLuongNhap, value))
            {
                _themDongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string DonGiaNhap
    {
        get => _donGiaNhap;
        set
        {
            if (SetProperty(ref _donGiaNhap, value))
            {
                _themDongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public HoaDonNhapChiTietDongModel? SelectedDongChiTiet
    {
        get => _selectedDongChiTiet;
        set
        {
            if (SetProperty(ref _selectedDongChiTiet, value))
            {
                _xoaDongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public decimal TongTien
    {
        get => _tongTien;
        private set => SetProperty(ref _tongTien, value);
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
                _themDongCommand.RaiseCanExecuteChanged();
                _xoaDongCommand.RaiseCanExecuteChanged();
                _luuHoaDonCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand ThemDongCommand => _themDongCommand;

    public ICommand XoaDongCommand => _xoaDongCommand;

    public ICommand LuuHoaDonCommand => _luuHoaDonCommand;

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
            await LoadNhaCungCapAsync(cancellationToken);
            await LoadMonAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu hóa đơn nhập: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteThemDong()
    {
        return !IsBusy
               && SelectedMon is not null
               && !string.IsNullOrWhiteSpace(SoLuongNhap)
               && !string.IsNullOrWhiteSpace(DonGiaNhap);
    }

    private bool CanExecuteXoaDong()
    {
        return !IsBusy && SelectedDongChiTiet is not null;
    }

    private bool CanExecuteLuuHoaDon()
    {
        return !IsBusy && SelectedNhaCungCap is not null && ChiTietLines.Count > 0;
    }

    private void ExecuteThemDong()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (SelectedMon is null)
        {
            ErrorMessage = "Vui lòng chọn sản phẩm.";
            return;
        }

        if (!int.TryParse(SoLuongNhap, out var soLuong) || soLuong <= 0)
        {
            ErrorMessage = "Số lượng nhập phải là số nguyên lớn hơn 0.";
            return;
        }

        if (!TryParseDecimal(DonGiaNhap, out var donGiaNhap) || donGiaNhap <= 0)
        {
            ErrorMessage = "Đơn giá nhập phải là số lớn hơn 0.";
            return;
        }

        if (ChiTietLines.Any(x => x.MonId == SelectedMon.MonId))
        {
            ErrorMessage = "Sản phẩm đã tồn tại trong chi tiết hóa đơn. Vui lòng sửa hoặc xóa dòng cũ.";
            return;
        }

        ChiTietLines.Add(new HoaDonNhapChiTietDongModel
        {
            MonId = SelectedMon.MonId,
            TenMon = SelectedMon.TenMon,
            SoLuong = soLuong,
            DonGiaNhap = donGiaNhap
        });

        RecalculateTongTien();

        SoLuongNhap = "1";
        DonGiaNhap = string.Empty;
        SelectedMon = null;

        _luuHoaDonCommand.RaiseCanExecuteChanged();
        SuccessMessage = "Đã thêm dòng chi tiết.";
    }

    private void ExecuteXoaDong()
    {
        if (SelectedDongChiTiet is null)
        {
            return;
        }

        ChiTietLines.Remove(SelectedDongChiTiet);
        SelectedDongChiTiet = null;
        RecalculateTongTien();

        _luuHoaDonCommand.RaiseCanExecuteChanged();
        SuccessMessage = "Đã xóa dòng chi tiết.";
    }

    private async void ExecuteLuuHoaDon()
    {
        await LuuHoaDonAsync();
    }

    private async Task LuuHoaDonAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (SelectedNhaCungCap is null)
        {
            ErrorMessage = "Vui lòng chọn nhà cung cấp.";
            return;
        }

        if (ChiTietLines.Count == 0)
        {
            ErrorMessage = "Hóa đơn nhập phải có ít nhất 1 dòng chi tiết.";
            return;
        }

        var currentUserId = _sessionService.CurrentUser?.UserId ?? 0;
        if (currentUserId <= 0)
        {
            ErrorMessage = "Không xác định được phiên đăng nhập. Vui lòng đăng nhập lại.";
            return;
        }

        var chiTietInput = ChiTietLines
            .Select(x => new HoaDonNhapChiTietInputModel
            {
                MonId = x.MonId,
                SoLuong = x.SoLuong,
                DonGiaNhap = x.DonGiaNhap
            })
            .ToList();

        IsBusy = true;

        try
        {
            var result = await _hoaDonNhapService.CreateAsync(
                SelectedNhaCungCap.NhaCungCapId,
                currentUserId,
                GhiChu,
                chiTietInput,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = $"{result.Message} Mã hóa đơn: {result.Data?.HoaDonNhapId}";
            ResetFormAfterSave();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể lưu hóa đơn nhập: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ExecuteLamMoi()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        SelectedNhaCungCap = NhaCungCaps.FirstOrDefault();
        GhiChu = string.Empty;
        SelectedMon = null;
        SoLuongNhap = "1";
        DonGiaNhap = string.Empty;
        SelectedDongChiTiet = null;

        ChiTietLines.Clear();
        RecalculateTongTien();
    }

    private async Task LoadNhaCungCapAsync(CancellationToken cancellationToken)
    {
        var data = await _nhaCungCapService.GetAllAsync(cancellationToken: cancellationToken);

        NhaCungCaps.Clear();
        foreach (var item in data.Where(x => x.IsActive).OrderBy(x => x.TenNhaCungCap))
        {
            NhaCungCaps.Add(item);
        }

        if (SelectedNhaCungCap is null && NhaCungCaps.Count > 0)
        {
            SelectedNhaCungCap = NhaCungCaps[0];
        }
    }

    private async Task LoadMonAsync(CancellationToken cancellationToken)
    {
        var data = await _monService.SearchAsync(null, null, cancellationToken);

        Mons.Clear();
        foreach (var item in data.Where(x => x.IsActive).OrderBy(x => x.TenMon))
        {
            Mons.Add(item);
        }
    }

    private void ResetFormAfterSave()
    {
        GhiChu = string.Empty;
        SelectedMon = null;
        SoLuongNhap = "1";
        DonGiaNhap = string.Empty;
        SelectedDongChiTiet = null;
        ChiTietLines.Clear();
        RecalculateTongTien();
    }

    private void RecalculateTongTien()
    {
        TongTien = ChiTietLines.Sum(x => x.ThanhTien);
    }

    private static bool TryParseDecimal(string input, out decimal result)
    {
        if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
        {
            return true;
        }

        return decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }
}
