using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class ThongKeViewModel : BaseViewModel
{
    private readonly IThongKeService _thongKeService;
    private readonly IExportPrintService _exportPrintService;
    private readonly SessionService _sessionService;
    private readonly RelayCommand _taiThongKeCommand;
    private readonly RelayCommand _lamMoiCommand;
    private readonly RelayCommand _xuatPdfCommand;
    private readonly RelayCommand _xuatExcelCommand;

    private DateTime _fromDate = DateTime.Today.AddDays(-7);
    private DateTime _toDate = DateTime.Today;

    private int _tongSoHoaDon;
    private decimal _tongDoanhThuGop;
    private decimal _tongGiamGia;
    private decimal _tongDoanhThuThuan;

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public ThongKeViewModel(IThongKeService thongKeService, IExportPrintService exportPrintService, SessionService sessionService)
    {
        _thongKeService = thongKeService;
        _exportPrintService = exportPrintService;
        _sessionService = sessionService;

        DoanhThuTheoNgay = new ObservableCollection<ThongKeDoanhThu>();
        TopSanPhamBanChay = new ObservableCollection<ThongKeTopSanPhamDong>();
        DanhSachHoaDon = new ObservableCollection<HoaDonBanTimKiemDong>();
        DoanhThuTheoHTTT = new ObservableCollection<ThongKeDoanhThuTheoHTTT>();

        _taiThongKeCommand = new RelayCommand(ExecuteTaiThongKe, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
        _xuatPdfCommand = new RelayCommand(ExecuteXuatPdf, () => !IsBusy);
        _xuatExcelCommand = new RelayCommand(ExecuteXuatExcel, () => !IsBusy);
    }

    public ObservableCollection<ThongKeDoanhThu> DoanhThuTheoNgay { get; }

    public ObservableCollection<ThongKeTopSanPhamDong> TopSanPhamBanChay { get; }

    public ObservableCollection<HoaDonBanTimKiemDong> DanhSachHoaDon { get; }

    /// <summary>Doanh thu theo hình thức thanh toán</summary>
    public ObservableCollection<ThongKeDoanhThuTheoHTTT> DoanhThuTheoHTTT { get; }

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

    public int TongSoHoaDon
    {
        get => _tongSoHoaDon;
        private set => SetProperty(ref _tongSoHoaDon, value);
    }

    public decimal TongDoanhThuGop
    {
        get => _tongDoanhThuGop;
        private set => SetProperty(ref _tongDoanhThuGop, value);
    }

    public decimal TongGiamGia
    {
        get => _tongGiamGia;
        private set => SetProperty(ref _tongGiamGia, value);
    }

    public decimal TongDoanhThuThuan
    {
        get => _tongDoanhThuThuan;
        private set => SetProperty(ref _tongDoanhThuThuan, value);
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
                _taiThongKeCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
                _xuatPdfCommand.RaiseCanExecuteChanged();
                _xuatExcelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TaiThongKeCommand => _taiThongKeCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public ICommand XuatPdfCommand => _xuatPdfCommand;

    public ICommand XuatExcelCommand => _xuatExcelCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await TaiThongKeAsync(cancellationToken);
    }

    private async void ExecuteTaiThongKe()
    {
        await TaiThongKeAsync();
    }

    private async Task TaiThongKeAsync(CancellationToken cancellationToken = default)
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
            var result = await _thongKeService.GetTongHopAsync(FromDate, ToDate, cancellationToken);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            ApplySummary(result.Data);
            SuccessMessage = result.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải thống kê: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteLamMoi()
    {
        FromDate = DateTime.Today.AddDays(-7);
        ToDate = DateTime.Today;

        await TaiThongKeAsync();
    }

    private async void ExecuteXuatPdf()
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
            var result = await _exportPrintService.XuatPdfBaoCaoDonGianAsync(
                FromDate,
                ToDate,
                null,
                _sessionService.CurrentUser?.UserId,
                default);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = $"Xuất PDF thành công. File: {result.Data}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể xuất PDF: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteXuatExcel()
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
            var result = await _exportPrintService.XuatCsvThongKeAsync(
                FromDate,
                ToDate,
                null,
                _sessionService.CurrentUser?.UserId,
                default);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = $"Xuất Excel thành công. File: {result.Data}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể xuất Excel: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplySummary(ThongKeTongHopModel data)
    {
        DoanhThuTheoNgay.Clear();
        foreach (var item in data.DoanhThuTheoNgay)
        {
            DoanhThuTheoNgay.Add(item);
        }

        DanhSachHoaDon.Clear();
        foreach (var item in data.DanhSachHoaDon)
        {
            DanhSachHoaDon.Add(item);
        }

        TopSanPhamBanChay.Clear();
        foreach (var item in data.TopSanPhamBanChay)
        {
            TopSanPhamBanChay.Add(item);
        }

        DoanhThuTheoHTTT.Clear();
        foreach (var item in data.DoanhThuTheoHTTT)
        {
            DoanhThuTheoHTTT.Add(item);
        }

        TongSoHoaDon = data.TongSoHoaDon;
        TongDoanhThuGop = data.TongDoanhThuGop;
        TongGiamGia = data.TongGiamGia;
        TongDoanhThuThuan = data.TongDoanhThuThuan;
    }
}
