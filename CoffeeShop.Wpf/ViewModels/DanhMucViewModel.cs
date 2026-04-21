using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class DanhMucViewModel : BaseViewModel
{
    private readonly IDanhMucService _danhMucService;
    private readonly RelayCommand _taoMoiCommand;
    private readonly RelayCommand _timKiemCommand;
    private readonly RelayCommand _lamMoiCommand;

    private string _tenDanhMuc = string.Empty;
    private string _moTa = string.Empty;
    private string _tuKhoaTimKiem = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public DanhMucViewModel(IDanhMucService danhMucService)
    {
        _danhMucService = danhMucService;
        DanhMucs = new ObservableCollection<DanhMuc>();

        _taoMoiCommand = new RelayCommand(ExecuteTaoMoi, CanExecuteTaoMoi);
        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<DanhMuc> DanhMucs { get; }

    public string TenDanhMuc
    {
        get => _tenDanhMuc;
        set
        {
            if (SetProperty(ref _tenDanhMuc, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string MoTa
    {
        get => _moTa;
        set => SetProperty(ref _moTa, value);
    }

    public string TuKhoaTimKiem
    {
        get => _tuKhoaTimKiem;
        set => SetProperty(ref _tuKhoaTimKiem, value);
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
        await LoadDataAsync(TuKhoaTimKiem, cancellationToken);
    }

    private bool CanExecuteTaoMoi()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(TenDanhMuc);
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

        if (string.IsNullOrWhiteSpace(TenDanhMuc))
        {
            ErrorMessage = "Tên danh mục không được để trống.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _danhMucService.CreateAsync(TenDanhMuc, MoTa, cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            TenDanhMuc = string.Empty;
            MoTa = string.Empty;

            await LoadDataAsync(TuKhoaTimKiem, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tạo danh mục: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteTimKiem()
    {
        await LoadDataAsync(TuKhoaTimKiem);
    }

    private async void ExecuteLamMoi()
    {
        TuKhoaTimKiem = string.Empty;
        await LoadDataAsync(null);
    }

    private async Task LoadDataAsync(string? keyword, CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;

        IsBusy = true;
        try
        {
            var danhMucs = await _danhMucService.GetAllAsync(keyword, cancellationToken);

            DanhMucs.Clear();
            foreach (var item in danhMucs)
            {
                DanhMucs.Add(item);
            }

            if (DanhMucs.Count == 0)
            {
                SuccessMessage = "Không có dữ liệu phù hợp.";
            }
            else if (string.IsNullOrWhiteSpace(keyword))
            {
                SuccessMessage = $"Đã tải {DanhMucs.Count} danh mục.";
            }
            else
            {
                SuccessMessage = $"Tìm thấy {DanhMucs.Count} danh mục theo từ khóa '{keyword}'.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải danh mục: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
