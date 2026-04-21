using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class CaLamViecViewModel : BaseViewModel
{
    private readonly ICaLamViecService _caLamViecService;
    private readonly SessionService _sessionService;
    private readonly RelayCommand _moCaCommand;
    private readonly RelayCommand _dongCaCommand;
    private readonly RelayCommand _taiLichSuCaCommand;
    private readonly RelayCommand _taiTongKetCaCommand;

    private DateTime _fromDate = DateTime.Today.AddDays(-7);
    private DateTime _toDate = DateTime.Today;
    private string _ghiChu = string.Empty;
    private string _tienDauCaText = "0";
    private string _tienMatThucDemText = "0";
    private string _ghiChuDoiSoat = string.Empty;
    private CaLamViec? _caDangMo;
    private CaTongKetModel? _tongKetCa;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public CaLamViecViewModel(ICaLamViecService caLamViecService, SessionService sessionService)
    {
        _caLamViecService = caLamViecService;
        _sessionService = sessionService;

        LichSuCa = new ObservableCollection<CaLamViec>();

        _moCaCommand = new RelayCommand(ExecuteMoCa, () => !IsBusy);
        _dongCaCommand = new RelayCommand(ExecuteDongCa, () => !IsBusy);
        _taiLichSuCaCommand = new RelayCommand(ExecuteTaiLichSuCa, () => !IsBusy);
        _taiTongKetCaCommand = new RelayCommand(ExecuteTaiTongKetCa, () => !IsBusy);
    }

    public ObservableCollection<CaLamViec> LichSuCa { get; }

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

    public string GhiChu
    {
        get => _ghiChu;
        set => SetProperty(ref _ghiChu, value);
    }

    public string TienDauCaText
    {
        get => _tienDauCaText;
        set => SetProperty(ref _tienDauCaText, value);
    }

    public string TienMatThucDemText
    {
        get => _tienMatThucDemText;
        set => SetProperty(ref _tienMatThucDemText, value);
    }

    public string GhiChuDoiSoat
    {
        get => _ghiChuDoiSoat;
        set => SetProperty(ref _ghiChuDoiSoat, value);
    }

    public CaLamViec? CaDangMo
    {
        get => _caDangMo;
        private set => SetProperty(ref _caDangMo, value);
    }

    public CaTongKetModel? TongKetCa
    {
        get => _tongKetCa;
        private set => SetProperty(ref _tongKetCa, value);
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
                _moCaCommand.RaiseCanExecuteChanged();
                _dongCaCommand.RaiseCanExecuteChanged();
                _taiLichSuCaCommand.RaiseCanExecuteChanged();
                _taiTongKetCaCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand MoCaCommand => _moCaCommand;
    public ICommand DongCaCommand => _dongCaCommand;
    public ICommand TaiLichSuCaCommand => _taiLichSuCaCommand;
    public ICommand TaiTongKetCaCommand => _taiTongKetCaCommand;

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
            await LoadCaDangMoAsync(cancellationToken);
            await TaiLichSuCaCoreAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu ca làm việc: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteMoCa()
    {
        if (IsBusy)
        {
            return;
        }

        var currentUserId = _sessionService.CurrentUser?.UserId ?? 0;
        if (currentUserId <= 0)
        {
            ErrorMessage = "Không xác định được phiên đăng nhập.";
            return;
        }

        if (!TryParseDecimal(TienDauCaText, out var tienDauCa) || tienDauCa < 0)
        {
            ErrorMessage = "Tiền đầu ca không hợp lệ. Vui lòng nhập số >= 0.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _caLamViecService.MoCaAsync(currentUserId, tienDauCa, GhiChu);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            CaDangMo = result.Data;
            SuccessMessage = $"Mở ca thành công. Tiền đầu ca: {tienDauCa:N0} đ.";
            TongKetCa = null;
            await TaiLichSuCaCoreAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể mở ca làm việc: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteTaiTongKetCa()
    {
        if (IsBusy)
        {
            return;
        }

        if (CaDangMo is null)
        {
            ErrorMessage = "Hiện không có ca đang mở để tải tổng kết.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _caLamViecService.GetTongKetCaAsync(CaDangMo.CaLamViecId);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            TongKetCa = result.Data;
            // Gợi ý tiền mặt thực đếm = dự kiến cuối ca
            TienMatThucDemText = TongKetCa.TienMatDuKienCuoiCa.ToString("0");
            SuccessMessage = "Đã tải tổng kết ca.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải tổng kết ca: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteDongCa()
    {
        if (IsBusy)
        {
            return;
        }

        var currentUserId = _sessionService.CurrentUser?.UserId ?? 0;
        if (currentUserId <= 0)
        {
            ErrorMessage = "Không xác định được phiên đăng nhập.";
            return;
        }

        if (!TryParseDecimal(TienMatThucDemText, out var tienMatThucDem) || tienMatThucDem < 0)
        {
            ErrorMessage = "Tiền mặt thực đếm không hợp lệ. Vui lòng nhập số >= 0.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _caLamViecService.DongCaAsync(
                currentUserId, tienMatThucDem, GhiChu, GhiChuDoiSoat);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            TongKetCa = null;
            GhiChuDoiSoat = string.Empty;
            TienMatThucDemText = "0";
            await LoadCaDangMoAsync();
            await TaiLichSuCaCoreAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể đóng ca làm việc: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteTaiLichSuCa()
    {
        await TaiLichSuCaAsync();
    }

    private async Task TaiLichSuCaAsync(CancellationToken cancellationToken = default)
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
            await TaiLichSuCaCoreAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải lịch sử ca làm việc: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadCaDangMoAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _sessionService.CurrentUser?.UserId ?? 0;
        if (currentUserId <= 0)
        {
            CaDangMo = null;
            return;
        }

        var result = await _caLamViecService.GetCaDangMoAsync(currentUserId, cancellationToken);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Message;
            return;
        }

        CaDangMo = result.Data;
    }

    private async Task TaiLichSuCaCoreAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _sessionService.CurrentUser?.UserId;
        var result = await _caLamViecService.GetLichSuCaAsync(FromDate, ToDate, currentUserId, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message;
            return;
        }

        LichSuCa.Clear();
        foreach (var item in result.Data)
        {
            LichSuCa.Add(item);
        }

        SuccessMessage = $"Đã tải {LichSuCa.Count} ca làm việc.";
    }

    private static bool TryParseDecimal(string? text, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return decimal.TryParse(text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out result)
            || decimal.TryParse(text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }
}
