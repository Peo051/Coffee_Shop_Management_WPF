using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class LichSuHoaDonViewModel : BaseViewModel
{
    private readonly ILichSuHoaDonService _lichSuHoaDonService;
    private readonly IExportPrintService _exportPrintService;
    private readonly IDialogService _dialogService;
    private readonly SessionService _sessionService;

    private readonly RelayCommand _timKiemCommand;
    private readonly RelayCommand _xemChiTietCommand;
    private readonly RelayCommand _lamMoiCommand;
    private readonly RelayCommand _inHoaDonCommand;
    private readonly RelayCommand _huyHoaDonCommand;

    private DateTime _fromDate = DateTime.Today.AddDays(-7);
    private DateTime _toDate = DateTime.Today;
    private string _maHoaDon = string.Empty;
    private string _maNhanVien = string.Empty;
    private string _maBan = string.Empty;
    private string _maCaLamViec = string.Empty;
    private string _tenKhachHang = string.Empty;
    private string _soDienThoai = string.Empty;
    private string? _hinhThucThanhToanFilter;
    private string? _trangThaiThanhToanFilter;
    private string? _trangThaiPhaCheFilter;
    private LichSuHoaDonDong? _selectedHoaDon;
    private string _lyDoHuy = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public LichSuHoaDonViewModel(
        ILichSuHoaDonService lichSuHoaDonService,
        IExportPrintService exportPrintService,
        IDialogService dialogService,
        SessionService sessionService)
    {
        _lichSuHoaDonService = lichSuHoaDonService;
        _exportPrintService = exportPrintService;
        _dialogService = dialogService;
        _sessionService = sessionService;

        DanhSachHoaDon = new ObservableCollection<LichSuHoaDonDong>();
        ChiTietHoaDon = new ObservableCollection<LichSuHoaDonChiTietDong>();

        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _xemChiTietCommand = new RelayCommand(ExecuteXemChiTiet, () => !IsBusy && SelectedHoaDon is not null);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
        _inHoaDonCommand = new RelayCommand(ExecuteInHoaDon, () => !IsBusy && SelectedHoaDon is not null);
        _huyHoaDonCommand = new RelayCommand(ExecuteHuyHoaDon, () => !IsBusy && SelectedHoaDon is not null);
    }

    public ObservableCollection<LichSuHoaDonDong> DanhSachHoaDon { get; }

    public ObservableCollection<LichSuHoaDonChiTietDong> ChiTietHoaDon { get; }

    public DateTime FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTime ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
    }

    public string MaHoaDon
    {
        get => _maHoaDon;
        set => SetProperty(ref _maHoaDon, value);
    }

    public string MaNhanVien
    {
        get => _maNhanVien;
        set => SetProperty(ref _maNhanVien, value);
    }

    public string MaBan
    {
        get => _maBan;
        set => SetProperty(ref _maBan, value);
    }

    public string MaCaLamViec
    {
        get => _maCaLamViec;
        set => SetProperty(ref _maCaLamViec, value);
    }

    /// <summary>Filter theo tên khách hàng</summary>
    public string TenKhachHang
    {
        get => _tenKhachHang;
        set => SetProperty(ref _tenKhachHang, value);
    }

    /// <summary>Filter theo số điện thoại</summary>
    public string SoDienThoai
    {
        get => _soDienThoai;
        set => SetProperty(ref _soDienThoai, value);
    }

    /// <summary>Filter theo hình thức thanh toán</summary>
    public string? HinhThucThanhToanFilter
    {
        get => _hinhThucThanhToanFilter;
        set => SetProperty(ref _hinhThucThanhToanFilter, value);
    }

    /// <summary>Filter theo trạng thái thanh toán</summary>
    public string? TrangThaiThanhToanFilter
    {
        get => _trangThaiThanhToanFilter;
        set => SetProperty(ref _trangThaiThanhToanFilter, value);
    }

    /// <summary>Danh sách HTTT cho ComboBox filter</summary>
    public IReadOnlyList<string> DanhSachHinhThucThanhToan { get; } =
        ["", "Tiền mặt", "Chuyển khoản", "Thẻ", "Ví điện tử"];

    /// <summary>Danh sách trạng thái cho ComboBox filter</summary>
    public IReadOnlyList<string> DanhSachTrangThaiThanhToan { get; } =
        ["", "Đã thanh toán", "Chưa thanh toán", "Đã hủy"];

    /// <summary>Filter theo trạng thái pha chế</summary>
    public string? TrangThaiPhaCheFilter
    {
        get => _trangThaiPhaCheFilter;
        set => SetProperty(ref _trangThaiPhaCheFilter, value);
    }

    /// <summary>Danh sách trạng thái pha chế cho ComboBox filter</summary>
    public IReadOnlyList<string> DanhSachTrangThaiPhaChe { get; } =
        ["", "ChoPhaChe", "DangPhaChe", "DaHoanThanh", "DaGiaoKhach", "DaHuy"];

    public LichSuHoaDonDong? SelectedHoaDon
    {
        get => _selectedHoaDon;
        set
        {
            if (SetProperty(ref _selectedHoaDon, value))
            {
                _xemChiTietCommand.RaiseCanExecuteChanged();
                _inHoaDonCommand.RaiseCanExecuteChanged();
                _huyHoaDonCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Lý do hủy hóa đơn (binding từ UI)</summary>
    public string LyDoHuy
    {
        get => _lyDoHuy;
        set => SetProperty(ref _lyDoHuy, value);
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
                _xemChiTietCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
                _inHoaDonCommand.RaiseCanExecuteChanged();
                _huyHoaDonCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TimKiemCommand => _timKiemCommand;
    public ICommand XemChiTietCommand => _xemChiTietCommand;
    public ICommand LamMoiCommand => _lamMoiCommand;
    public ICommand InHoaDonCommand => _inHoaDonCommand;
    public ICommand HuyHoaDonCommand => _huyHoaDonCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await TimKiemAsync(cancellationToken);
    }

    private async void ExecuteTimKiem()
    {
        await TimKiemAsync();
    }

    private async void ExecuteXemChiTiet()
    {
        await XemChiTietAsync();
    }

    private async void ExecuteLamMoi()
    {
        FromDate = DateTime.Today.AddDays(-7);
        ToDate = DateTime.Today;
        MaHoaDon = string.Empty;
        MaNhanVien = string.Empty;
        MaBan = string.Empty;
        MaCaLamViec = string.Empty;
        TenKhachHang = string.Empty;
        SoDienThoai = string.Empty;
        HinhThucThanhToanFilter = null;
        TrangThaiThanhToanFilter = null;
        TrangThaiPhaCheFilter = null;
        LyDoHuy = string.Empty;
        SelectedHoaDon = null;
        ChiTietHoaDon.Clear();
        await TimKiemAsync();
    }

    private async void ExecuteInHoaDon()
    {
        await InHoaDonAsync();
    }

    private async void ExecuteHuyHoaDon()
    {
        await HuyHoaDonAsync();
    }

    private async Task TimKiemAsync(CancellationToken cancellationToken = default)
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
            int? hoaDonId = ParseNullableInt(MaHoaDon);
            int? nhanVienId = ParseNullableInt(MaNhanVien);
            int? banId = ParseNullableInt(MaBan);
            int? caId = ParseNullableInt(MaCaLamViec);

            var result = await _lichSuHoaDonService.TimKiemHoaDonAsync(
                FromDate,
                ToDate,
                hoaDonId,
                nhanVienId,
                banId,
                caId,
                string.IsNullOrWhiteSpace(TenKhachHang) ? null : TenKhachHang.Trim(),
                string.IsNullOrWhiteSpace(SoDienThoai) ? null : SoDienThoai.Trim(),
                string.IsNullOrWhiteSpace(HinhThucThanhToanFilter) ? null : HinhThucThanhToanFilter,
                string.IsNullOrWhiteSpace(TrangThaiThanhToanFilter) ? null : TrangThaiThanhToanFilter,
                string.IsNullOrWhiteSpace(TrangThaiPhaCheFilter) ? null : TrangThaiPhaCheFilter,
                cancellationToken);

            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            DanhSachHoaDon.Clear();
            foreach (var item in result.Data)
            {
                DanhSachHoaDon.Add(item);
            }

            SuccessMessage = DanhSachHoaDon.Count == 0
                ? "Không có hóa đơn phù hợp điều kiện tìm kiếm."
                : $"Tìm thấy {DanhSachHoaDon.Count} hóa đơn.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải lịch sử hóa đơn: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task XemChiTietAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || SelectedHoaDon is null)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _lichSuHoaDonService.GetChiTietHoaDonAsync(SelectedHoaDon.HoaDonBanId, cancellationToken);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            ChiTietHoaDon.Clear();
            foreach (var item in result.Data)
            {
                ChiTietHoaDon.Add(item);
            }

            SuccessMessage = $"Đã tải {ChiTietHoaDon.Count} dòng chi tiết của hóa đơn #{SelectedHoaDon.HoaDonBanId}.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải chi tiết hóa đơn: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>In hóa đơn đang chọn trong danh sách lịch sử</summary>
    private async Task InHoaDonAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || SelectedHoaDon is null)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var nguoiDungId = _sessionService.CurrentUser?.UserId;
            var result = await _exportPrintService.InHoaDonBanAsync(
                SelectedHoaDon.HoaDonBanId,
                null,
                nguoiDungId,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = $"In hóa đơn #{SelectedHoaDon.HoaDonBanId} thành công. File: {result.Data}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể in hóa đơn: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Hủy hóa đơn đang chọn: xác nhận, nhập lý do, hoàn kho + trừ điểm</summary>
    private async Task HuyHoaDonAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || SelectedHoaDon is null)
        {
            return;
        }

        // Kiểm tra hóa đơn đã hủy
        if (string.Equals(SelectedHoaDon.TrangThaiThanhToan, "Đã hủy", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "Hóa đơn này đã được hủy trước đó.";
            return;
        }

        // Kiểm tra lý do
        if (string.IsNullOrWhiteSpace(LyDoHuy))
        {
            ErrorMessage = "Vui lòng nhập lý do hủy hóa đơn.";
            return;
        }

        // Xác nhận
        var confirmed = _dialogService.ShowConfirmation(
            $"Bạn có chắc muốn hủy hóa đơn #{SelectedHoaDon.HoaDonBanId}?\n\nLý do: {LyDoHuy.Trim()}\n\nThao tác sẽ hoàn tồn kho và trừ điểm khách hàng (nếu có).",
            "Xác nhận hủy hóa đơn");

        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var nguoiHuy = _sessionService.CurrentUser?.DisplayName ?? "N/A";
            var result = await _lichSuHoaDonService.HuyHoaDonAsync(
                SelectedHoaDon.HoaDonBanId,
                LyDoHuy.Trim(),
                nguoiHuy,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = $"Hủy hóa đơn #{SelectedHoaDon.HoaDonBanId} thành công. Đã hoàn tồn kho và trừ điểm.";
            LyDoHuy = string.Empty;

            // Tải lại danh sách
            await TimKiemAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể hủy hóa đơn: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static int? ParseNullableInt(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        return int.TryParse(input.Trim(), out var value) ? value : null;
    }
}
