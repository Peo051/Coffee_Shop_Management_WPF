using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class ExportPrintViewModel : BaseViewModel
{
    private readonly IExportPrintService _exportPrintService;
    private readonly SessionService _sessionService;
    private readonly RelayCommand _xuatPdfBaoCaoDonGianCommand;
    private readonly RelayCommand _xuatPdfBaoCaoNangCaoCommand;
    private readonly RelayCommand _xuatCsvThongKeCommand;
    private readonly RelayCommand _inHoaDonCommand;
    private readonly RelayCommand _previewBaoCaoCommand;
    private readonly RelayCommand _previewHoaDonCommand;
    private readonly RelayCommand _inPhieuPhaCheCommand;

    private DateTime _fromDate = DateTime.Today.AddDays(-7);
    private DateTime _toDate = DateTime.Today;
    private string _hoaDonBanIdInput = string.Empty;
    private string _thuMucXuat = string.Empty;
    private string _loaiPreview = "DonGian";
    private string _lastOutputPath = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public ExportPrintViewModel(IExportPrintService exportPrintService, SessionService sessionService)
    {
        _exportPrintService = exportPrintService;
        _sessionService = sessionService;

        _xuatPdfBaoCaoDonGianCommand = new RelayCommand(ExecuteXuatPdfBaoCaoDonGian, () => !IsBusy);
        _xuatPdfBaoCaoNangCaoCommand = new RelayCommand(ExecuteXuatPdfBaoCaoNangCao, () => !IsBusy);
        _xuatCsvThongKeCommand = new RelayCommand(ExecuteXuatCsvThongKe, () => !IsBusy);
        _inHoaDonCommand = new RelayCommand(ExecuteInHoaDon, () => !IsBusy);
        _previewBaoCaoCommand = new RelayCommand(ExecutePreviewBaoCao, () => !IsBusy);
        _previewHoaDonCommand = new RelayCommand(ExecutePreviewHoaDon, () => !IsBusy);
        _inPhieuPhaCheCommand = new RelayCommand(ExecuteInPhieuPhaChe, () => !IsBusy);
    }

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

    public string HoaDonBanIdInput
    {
        get => _hoaDonBanIdInput;
        set => SetProperty(ref _hoaDonBanIdInput, value);
    }

    public string ThuMucXuat
    {
        get => _thuMucXuat;
        set => SetProperty(ref _thuMucXuat, value);
    }

    public string LoaiPreview
    {
        get => _loaiPreview;
        set => SetProperty(ref _loaiPreview, value);
    }

    public string LastOutputPath
    {
        get => _lastOutputPath;
        private set => SetProperty(ref _lastOutputPath, value);
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
                _xuatPdfBaoCaoDonGianCommand.RaiseCanExecuteChanged();
                _xuatPdfBaoCaoNangCaoCommand.RaiseCanExecuteChanged();
                _xuatCsvThongKeCommand.RaiseCanExecuteChanged();
                _inHoaDonCommand.RaiseCanExecuteChanged();
                _previewBaoCaoCommand.RaiseCanExecuteChanged();
                _previewHoaDonCommand.RaiseCanExecuteChanged();
                _inPhieuPhaCheCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand XuatPdfBaoCaoDonGianCommand => _xuatPdfBaoCaoDonGianCommand;

    public ICommand XuatPdfBaoCaoNangCaoCommand => _xuatPdfBaoCaoNangCaoCommand;

    public ICommand XuatCsvThongKeCommand => _xuatCsvThongKeCommand;

    public ICommand InHoaDonCommand => _inHoaDonCommand;

    public ICommand PreviewBaoCaoCommand => _previewBaoCaoCommand;

    public ICommand PreviewHoaDonCommand => _previewHoaDonCommand;

    public ICommand InPhieuPhaCheCommand => _inPhieuPhaCheCommand;

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private async void ExecuteXuatPdfBaoCaoDonGian()
    {
        await RunAsync(ct =>
            _exportPrintService.XuatPdfBaoCaoDonGianAsync(
                FromDate,
                ToDate,
                ThuMucXuat,
                _sessionService.CurrentUser?.UserId,
                ct));
    }

    private async void ExecuteXuatPdfBaoCaoNangCao()
    {
        await RunAsync(ct =>
            _exportPrintService.XuatPdfBaoCaoNangCaoAsync(
                FromDate,
                ToDate,
                ThuMucXuat,
                _sessionService.CurrentUser?.UserId,
                ct));
    }

    private async void ExecuteXuatCsvThongKe()
    {
        await RunAsync(ct =>
            _exportPrintService.XuatCsvThongKeAsync(
                FromDate,
                ToDate,
                ThuMucXuat,
                _sessionService.CurrentUser?.UserId,
                ct));
    }

    private async void ExecuteInHoaDon()
    {
        if (!int.TryParse(HoaDonBanIdInput, out var hoaDonBanId) || hoaDonBanId <= 0)
        {
            ErrorMessage = "Vui lòng nhập mã hóa đơn bán hợp lệ để in.";
            SuccessMessage = string.Empty;
            return;
        }

        await RunAsync(ct =>
            _exportPrintService.InHoaDonBanAsync(
                hoaDonBanId,
                ThuMucXuat,
                _sessionService.CurrentUser?.UserId,
                ct));
    }

    private async void ExecutePreviewBaoCao()
    {
        await RunAsync(ct =>
            _exportPrintService.PreviewBaoCaoAsync(
                FromDate,
                ToDate,
                LoaiPreview,
                ThuMucXuat,
                _sessionService.CurrentUser?.UserId,
                ct));
    }

    private async void ExecutePreviewHoaDon()
    {
        if (!int.TryParse(HoaDonBanIdInput, out var hoaDonBanId) || hoaDonBanId <= 0)
        {
            ErrorMessage = "Vui lòng nhập mã hóa đơn bán hợp lệ để xem.";
            SuccessMessage = string.Empty;
            return;
        }

        await RunAsync(ct =>
            _exportPrintService.PreviewHoaDonBanAsync(
                hoaDonBanId,
                ThuMucXuat,
                _sessionService.CurrentUser?.UserId,
                ct));
    }

    private async void ExecuteInPhieuPhaChe()
    {
        if (!int.TryParse(HoaDonBanIdInput, out var hoaDonBanId) || hoaDonBanId <= 0)
        {
            ErrorMessage = "Vui lòng nhập mã hóa đơn bán hợp lệ để in phiếu pha chế.";
            SuccessMessage = string.Empty;
            return;
        }

        await RunAsync(ct =>
            _exportPrintService.InPhieuPhaCheAsync(
                hoaDonBanId,
                ThuMucXuat,
                _sessionService.CurrentUser?.UserId,
                ct));
    }

    private async Task RunAsync(Func<CancellationToken, Task<ServiceResult<string>>> action, CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        LastOutputPath = string.Empty;

        try
        {
            var result = await action(cancellationToken);
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Data))
            {
                ErrorMessage = result.Message;
                return;
            }

            LastOutputPath = result.Data;
            SuccessMessage = $"{result.Message} Tệp: {result.Data}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể thực hiện thao tác xuất/in: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
