using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class AuditLogViewModel : BaseViewModel
{
    private readonly IAuditLogService _auditLogService;
    private readonly RelayCommand _timKiemCommand;
    private readonly RelayCommand _lamMoiCommand;

    private DateTime _fromDate = DateTime.Today.AddDays(-7);
    private DateTime _toDate = DateTime.Today;
    private string _nguoiDungIdInput = string.Empty;
    private string _hanhDong = string.Empty;
    private string _tuKhoa = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public AuditLogViewModel(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;

        DanhSachLog = new ObservableCollection<AuditLogDong>();

        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<AuditLogDong> DanhSachLog { get; }

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

    public string NguoiDungIdInput
    {
        get => _nguoiDungIdInput;
        set => SetProperty(ref _nguoiDungIdInput, value);
    }

    public string HanhDong
    {
        get => _hanhDong;
        set => SetProperty(ref _hanhDong, value);
    }

    public string TuKhoa
    {
        get => _tuKhoa;
        set => SetProperty(ref _tuKhoa, value);
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
            }
        }
    }

    public ICommand TimKiemCommand => _timKiemCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await TimKiemAsync(cancellationToken);
    }

    private async void ExecuteTimKiem()
    {
        await TimKiemAsync();
    }

    private async void ExecuteLamMoi()
    {
        FromDate = DateTime.Today.AddDays(-7);
        ToDate = DateTime.Today;
        NguoiDungIdInput = string.Empty;
        HanhDong = string.Empty;
        TuKhoa = string.Empty;

        await TimKiemAsync();
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
            int? nguoiDungId = null;
            if (!string.IsNullOrWhiteSpace(NguoiDungIdInput))
            {
                if (!int.TryParse(NguoiDungIdInput, out var parsed) || parsed <= 0)
                {
                    ErrorMessage = "Mã người dùng phải là số nguyên dương.";
                    return;
                }

                nguoiDungId = parsed;
            }

            var result = await _auditLogService.TimKiemLogAsync(
                FromDate,
                ToDate,
                nguoiDungId,
                HanhDong,
                TuKhoa,
                500,
                cancellationToken);

            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            DanhSachLog.Clear();
            foreach (var row in result.Data)
            {
                DanhSachLog.Add(row);
            }

            SuccessMessage = DanhSachLog.Count == 0
                ? "Không có nhật ký phù hợp điều kiện lọc."
                : $"Đã tải {DanhSachLog.Count} bản ghi nhật ký.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải nhật ký thao tác: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

