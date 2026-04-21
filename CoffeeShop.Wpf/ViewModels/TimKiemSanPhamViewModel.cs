using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class TimKiemSanPhamViewModel : BaseViewModel
{
    private readonly IMonService _monService;
    private readonly IDanhMucService _danhMucService;
    private readonly RelayCommand _timKiemCommand;
    private readonly RelayCommand _lamMoiCommand;

    private string _tuKhoa = string.Empty;
    private DanhMuc? _selectedDanhMuc;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public TimKiemSanPhamViewModel(IMonService monService, IDanhMucService danhMucService)
    {
        _monService = monService;
        _danhMucService = danhMucService;

        KetQuaMon = new ObservableCollection<Mon>();
        DanhMucs = new ObservableCollection<DanhMuc>();

        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<Mon> KetQuaMon { get; }

    public ObservableCollection<DanhMuc> DanhMucs { get; }

    public string TuKhoa
    {
        get => _tuKhoa;
        set => SetProperty(ref _tuKhoa, value);
    }

    public DanhMuc? SelectedDanhMuc
    {
        get => _selectedDanhMuc;
        set => SetProperty(ref _selectedDanhMuc, value);
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
            await LoadKetQuaAsync(null, null, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu tìm kiếm: {ex.Message}";
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
            await LoadKetQuaAsync(TuKhoa, SelectedDanhMuc?.DanhMucId);
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

        TuKhoa = string.Empty;
        SelectedDanhMuc = null;

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            await LoadKetQuaAsync(null, null);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải lại dữ liệu tìm kiếm: {ex.Message}";
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
    }

    private async Task LoadKetQuaAsync(string? keyword, int? danhMucId, CancellationToken cancellationToken = default)
    {
        var data = await _monService.SearchAsync(keyword, danhMucId, cancellationToken);

        KetQuaMon.Clear();
        foreach (var item in data)
        {
            KetQuaMon.Add(item);
        }

        if (KetQuaMon.Count == 0)
        {
            SuccessMessage = "Không có kết quả phù hợp.";
        }
        else if (string.IsNullOrWhiteSpace(keyword) && danhMucId is null)
        {
            SuccessMessage = $"Đã tải {KetQuaMon.Count} sản phẩm.";
        }
        else
        {
            SuccessMessage = $"Tìm thấy {KetQuaMon.Count} sản phẩm theo điều kiện lọc.";
        }
    }
}
