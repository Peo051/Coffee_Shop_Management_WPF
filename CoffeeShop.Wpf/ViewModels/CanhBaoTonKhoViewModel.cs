using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class CanhBaoTonKhoViewModel : BaseViewModel
{
    private readonly IKhoService _khoService;
    private readonly IDanhMucService _danhMucService;
    private readonly RelayCommand _taiDanhSachCommand;
    private readonly RelayCommand _lamMoiCommand;

    private string _tuKhoaTimKiem = string.Empty;
    private DanhMuc? _selectedDanhMuc;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public CanhBaoTonKhoViewModel(IKhoService khoService, IDanhMucService danhMucService)
    {
        _khoService = khoService;
        _danhMucService = danhMucService;

        DanhMucs = new ObservableCollection<DanhMuc>();
        DanhSachCanhBao = new ObservableCollection<CanhBaoTonKhoThapDong>();

        _taiDanhSachCommand = new RelayCommand(ExecuteTaiDanhSach, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<DanhMuc> DanhMucs { get; }

    public ObservableCollection<CanhBaoTonKhoThapDong> DanhSachCanhBao { get; }

    public string TuKhoaTimKiem
    {
        get => _tuKhoaTimKiem;
        set => SetProperty(ref _tuKhoaTimKiem, value);
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
                _taiDanhSachCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TaiDanhSachCommand => _taiDanhSachCommand;

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
            await TaiDanhSachAsyncCore(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu cảnh báo tồn kho: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteTaiDanhSach()
    {
        await TaiDanhSachAsync();
    }

    private async void ExecuteLamMoi()
    {
        TuKhoaTimKiem = string.Empty;
        SelectedDanhMuc = null;
        await TaiDanhSachAsync();
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
            ErrorMessage = $"Không thể tải danh sách cảnh báo tồn kho: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TaiDanhSachAsyncCore(CancellationToken cancellationToken = default)
    {
        var result = await _khoService.GetCanhBaoTonKhoThapAsync(
            TuKhoaTimKiem,
            SelectedDanhMuc?.DanhMucId,
            cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message;
            return;
        }

        DanhSachCanhBao.Clear();
        foreach (var item in result.Data)
        {
            DanhSachCanhBao.Add(item);
        }

        SuccessMessage = DanhSachCanhBao.Count == 0
            ? "Không có sản phẩm nào đang ở mức tồn kho cảnh báo."
            : $"Có {DanhSachCanhBao.Count} sản phẩm cảnh báo tồn kho thấp.";
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
