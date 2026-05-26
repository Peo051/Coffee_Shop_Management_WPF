using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class BaoCaoViewModel : BaseViewModel
{
    private readonly IBaoCaoService _baoCaoService;
    private readonly IExportPrintService _exportPrintService;
    private readonly SessionService _sessionService;
    private readonly RelayCommand _taiBaoCaoCommand;
    private readonly RelayCommand _lamMoiCommand;
    private readonly RelayCommand _xuatPdfCommand;
    private readonly RelayCommand _xuatPdfNangCaoCommand;
    private readonly RelayCommand _xuatExcelCommand;
    private readonly RelayCommand _moCrystalReportCommand;

    private DateTime _fromDate = DateTime.Today.AddDays(-7);
    private DateTime _toDate = DateTime.Today;

    private int _tongSoHoaDon;
    private decimal _tongDoanhThuThuan;
    private int _tongSoLuongBan;
    private decimal _tongDoanhThuSanPham;

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public BaoCaoViewModel(IBaoCaoService baoCaoService, IExportPrintService exportPrintService, SessionService sessionService)
    {
        _baoCaoService = baoCaoService;
        _exportPrintService = exportPrintService;
        _sessionService = sessionService;

        BaoCaoDonGianRows = new ObservableCollection<BaoCaoDonGianDong>();
        BaoCaoNangCaoRows = new ObservableCollection<BaoCaoNangCaoDong>();

        _taiBaoCaoCommand = new RelayCommand(ExecuteTaiBaoCao, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
        _xuatPdfCommand = new RelayCommand(ExecuteXuatPdf, () => !IsBusy);
        _xuatPdfNangCaoCommand = new RelayCommand(ExecuteXuatPdfNangCao, () => !IsBusy);
        _xuatExcelCommand = new RelayCommand(ExecuteXuatExcel, () => !IsBusy);
        _moCrystalReportCommand = new RelayCommand(ExecuteMoCrystalReport);
    }

    public ObservableCollection<BaoCaoDonGianDong> BaoCaoDonGianRows { get; }

    public ObservableCollection<BaoCaoNangCaoDong> BaoCaoNangCaoRows { get; }

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

    public decimal TongDoanhThuThuan
    {
        get => _tongDoanhThuThuan;
        private set => SetProperty(ref _tongDoanhThuThuan, value);
    }

    public int TongSoLuongBan
    {
        get => _tongSoLuongBan;
        private set => SetProperty(ref _tongSoLuongBan, value);
    }

    public decimal TongDoanhThuSanPham
    {
        get => _tongDoanhThuSanPham;
        private set => SetProperty(ref _tongDoanhThuSanPham, value);
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
                _taiBaoCaoCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
                _xuatPdfCommand.RaiseCanExecuteChanged();
                _xuatPdfNangCaoCommand.RaiseCanExecuteChanged();
                _xuatExcelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TaiBaoCaoCommand => _taiBaoCaoCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public ICommand XuatPdfCommand => _xuatPdfCommand;

    public ICommand XuatPdfNangCaoCommand => _xuatPdfNangCaoCommand;

    public ICommand XuatExcelCommand => _xuatExcelCommand;

    /// <summary>
    /// Mở module Crystal Reports (project CoffeeShop.CrystalReports.Wpf, .NET Framework 4.8).
    /// Module này tách project riêng vì project chính chạy .NET 8 không tương thích
    /// với SAP Crystal Reports for Visual Studio.
    /// </summary>
    public ICommand MoCrystalReportCommand => _moCrystalReportCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await TaiBaoCaoAsync(cancellationToken);
    }

    private async void ExecuteTaiBaoCao()
    {
        await TaiBaoCaoAsync();
    }

    private async Task TaiBaoCaoAsync(CancellationToken cancellationToken = default)
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
            var result = await _baoCaoService.GetTongHopAsync(FromDate, ToDate, cancellationToken);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            BaoCaoDonGianRows.Clear();
            foreach (var row in result.Data.BaoCaoDonGian.OrderByDescending(x => x.Ngay))
            {
                BaoCaoDonGianRows.Add(row);
            }

            BaoCaoNangCaoRows.Clear();
            foreach (var row in result.Data.BaoCaoNangCao)
            {
                BaoCaoNangCaoRows.Add(row);
            }

            TongSoHoaDon = result.Data.TongSoHoaDon;
            TongDoanhThuThuan = result.Data.TongDoanhThuThuan;
            TongSoLuongBan = result.Data.TongSoLuongBan;
            TongDoanhThuSanPham = result.Data.TongDoanhThuSanPham;

            SuccessMessage = result.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải báo cáo: {ex.Message}";
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

        await TaiBaoCaoAsync();
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

            SuccessMessage = $"Xuất PDF đơn giản thành công. File: {result.Data}";
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

    private async void ExecuteXuatPdfNangCao()
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
            var result = await _exportPrintService.XuatPdfBaoCaoNangCaoAsync(
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

            SuccessMessage = $"Xuất PDF nâng cao thành công. File: {result.Data}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể xuất PDF nâng cao: {ex.Message}";
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

    /// <summary>
    /// Mở module CoffeeShop.CrystalReports.Wpf bằng <see cref="System.Diagnostics.Process"/>.
    /// Tìm exe theo đường dẫn tương đối (trong solution build chung output) trước,
    /// nếu không thấy sẽ thông báo người dùng cần build project Crystal Reports.
    /// </summary>
    private void ExecuteMoCrystalReport()
    {
        try
        {
            var exePath = TimDuongDanCrystalReportExe();
            if (string.IsNullOrEmpty(exePath))
            {
                ErrorMessage =
                    "Chưa tìm thấy 'CoffeeShop.CrystalReports.Wpf.exe'. " +
                    "Hãy mở Visual Studio, build project CoffeeShop.CrystalReports.Wpf (Debug | x86) trước.";
                return;
            }

            var psi = new System.Diagnostics.ProcessStartInfo(exePath)
            {
                UseShellExecute = true,
                WorkingDirectory = System.IO.Path.GetDirectoryName(exePath) ?? string.Empty
            };
            System.Diagnostics.Process.Start(psi);
            SuccessMessage = "Đã mở module Báo cáo Crystal Reports.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không mở được Crystal Reports: {ex.Message}";
        }
    }

    /// <summary>
    /// Tìm exe của module Crystal Reports trong cùng solution.
    /// Bắt đầu từ thư mục chạy hiện tại, đi lên 5 cấp để tìm thư mục project anh em.
    /// </summary>
    private static string TimDuongDanCrystalReportExe()
    {
        const string exeName = "CoffeeShop.CrystalReports.Wpf.exe";
        var searchRoots = new System.Collections.Generic.List<string>();

        // 1) Lấy thư mục đang chạy của app chính rồi đi lên dần.
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 6 && !string.IsNullOrEmpty(dir); i++)
        {
            searchRoots.Add(dir);
            dir = System.IO.Path.GetDirectoryName(dir) ?? string.Empty;
        }

        // 2) Trong từng root, tìm theo các đường dẫn build chuẩn của .NET Framework 4.8.
        var probePaths = new[]
        {
            System.IO.Path.Combine("CoffeeShop.CrystalReports.Wpf", "bin", "Debug", exeName),
            System.IO.Path.Combine("CoffeeShop.CrystalReports.Wpf", "bin", "x86", "Debug", exeName),
            System.IO.Path.Combine("CoffeeShop.CrystalReports.Wpf", "bin", "Release", exeName),
            System.IO.Path.Combine("CoffeeShop.CrystalReports.Wpf", "bin", "x86", "Release", exeName)
        };

        foreach (var root in searchRoots)
        {
            foreach (var p in probePaths)
            {
                var full = System.IO.Path.Combine(root, p);
                if (System.IO.File.Exists(full))
                {
                    return full;
                }
            }
        }
        return string.Empty;
    }
}
