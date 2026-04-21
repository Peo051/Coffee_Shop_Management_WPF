using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class QuanLyBanViewModel : BaseViewModel
{
    private readonly IBanService _banService;
    private readonly RelayCommand _taiDanhSachCommand;
    private readonly RelayCommand _themBanCommand;
    private readonly RelayCommand _doiTrangThaiBanCommand;
    private readonly RelayCommand _chuyenBanCommand;
    private readonly RelayCommand _gopBanCommand;
    private readonly RelayCommand _lamMoiCommand;

    private string _tuKhoaTimKiem = string.Empty;
    private KhuVuc? _selectedKhuVucTimKiem;
    private TrangThaiBanOption? _selectedTrangThaiTimKiem;
    private Ban? _selectedBan;
    private Ban? _selectedBanDich;
    private string _tenBanMoi = string.Empty;
    private KhuVuc? _selectedKhuVucThem;
    private TrangThaiBanOption? _selectedTrangThaiCapNhat;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public QuanLyBanViewModel(IBanService banService)
    {
        _banService = banService;

        DanhSachBan = new ObservableCollection<Ban>();
        DanhSachKhuVuc = new ObservableCollection<KhuVuc>();
        DanhSachTrangThai = new ObservableCollection<TrangThaiBanOption>
        {
            new("Tất cả", null),
            new("Trống", TrangThaiBanConst.Trong),
            new("Đang có khách", TrangThaiBanConst.DangCoKhach),
            new("Tạm khóa", TrangThaiBanConst.TamKhoa)
        };

        _selectedTrangThaiTimKiem = DanhSachTrangThai[0];

        _taiDanhSachCommand = new RelayCommand(ExecuteTaiDanhSach, () => !IsBusy);
        _themBanCommand = new RelayCommand(ExecuteThemBan, CanExecuteThemBan);
        _doiTrangThaiBanCommand = new RelayCommand(ExecuteDoiTrangThaiBan, CanExecuteDoiTrangThaiBan);
        _chuyenBanCommand = new RelayCommand(ExecuteChuyenBan, CanExecuteChuyenBan);
        _gopBanCommand = new RelayCommand(ExecuteGopBan, CanExecuteGopBan);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<Ban> DanhSachBan { get; }

    public ObservableCollection<KhuVuc> DanhSachKhuVuc { get; }

    public ObservableCollection<TrangThaiBanOption> DanhSachTrangThai { get; }

    public string TuKhoaTimKiem
    {
        get => _tuKhoaTimKiem;
        set => SetProperty(ref _tuKhoaTimKiem, value);
    }

    public KhuVuc? SelectedKhuVucTimKiem
    {
        get => _selectedKhuVucTimKiem;
        set => SetProperty(ref _selectedKhuVucTimKiem, value);
    }

    public TrangThaiBanOption? SelectedTrangThaiTimKiem
    {
        get => _selectedTrangThaiTimKiem;
        set => SetProperty(ref _selectedTrangThaiTimKiem, value);
    }

    public Ban? SelectedBan
    {
        get => _selectedBan;
        set
        {
            if (SetProperty(ref _selectedBan, value))
            {
                SelectedBanDich = value is null
                    ? null
                    : DanhSachBan.FirstOrDefault(x => x.BanId != value.BanId);

                if (value is not null)
                {
                    SelectedTrangThaiCapNhat = DanhSachTrangThai.FirstOrDefault(x => x.Value == value.TrangThaiBan);
                }

                _doiTrangThaiBanCommand.RaiseCanExecuteChanged();
                _chuyenBanCommand.RaiseCanExecuteChanged();
                _gopBanCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public Ban? SelectedBanDich
    {
        get => _selectedBanDich;
        set
        {
            if (SetProperty(ref _selectedBanDich, value))
            {
                _chuyenBanCommand.RaiseCanExecuteChanged();
                _gopBanCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TenBanMoi
    {
        get => _tenBanMoi;
        set
        {
            if (SetProperty(ref _tenBanMoi, value))
            {
                _themBanCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public KhuVuc? SelectedKhuVucThem
    {
        get => _selectedKhuVucThem;
        set
        {
            if (SetProperty(ref _selectedKhuVucThem, value))
            {
                _themBanCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public TrangThaiBanOption? SelectedTrangThaiCapNhat
    {
        get => _selectedTrangThaiCapNhat;
        set
        {
            if (SetProperty(ref _selectedTrangThaiCapNhat, value))
            {
                _doiTrangThaiBanCommand.RaiseCanExecuteChanged();
            }
        }
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
                _themBanCommand.RaiseCanExecuteChanged();
                _doiTrangThaiBanCommand.RaiseCanExecuteChanged();
                _chuyenBanCommand.RaiseCanExecuteChanged();
                _gopBanCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TaiDanhSachCommand => _taiDanhSachCommand;
    public ICommand ThemBanCommand => _themBanCommand;
    public ICommand DoiTrangThaiBanCommand => _doiTrangThaiBanCommand;
    public ICommand ChuyenBanCommand => _chuyenBanCommand;
    public ICommand GopBanCommand => _gopBanCommand;
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
            await LoadKhuVucAsync(cancellationToken);
            await TaiDanhSachCoreAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu quản lý bàn: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteThemBan()
    {
        return !IsBusy
               && !string.IsNullOrWhiteSpace(TenBanMoi)
               && SelectedKhuVucThem is not null;
    }

    private bool CanExecuteDoiTrangThaiBan()
    {
        return !IsBusy
               && SelectedBan is not null
               && SelectedTrangThaiCapNhat?.Value is not null;
    }

    private bool CanExecuteChuyenBan()
    {
        return !IsBusy
               && SelectedBan is not null
               && SelectedBanDich is not null
               && SelectedBan.BanId != SelectedBanDich.BanId;
    }

    private bool CanExecuteGopBan()
    {
        return !IsBusy
               && SelectedBan is not null
               && SelectedBanDich is not null
               && SelectedBan.BanId != SelectedBanDich.BanId;
    }

    private async void ExecuteTaiDanhSach()
    {
        await TaiDanhSachAsync();
    }

    private async void ExecuteThemBan()
    {
        if (IsBusy || SelectedKhuVucThem is null)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _banService.TaoBanAsync(
                SelectedKhuVucThem.KhuVucId,
                TenBanMoi,
                TrangThaiBanConst.Trong);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            TenBanMoi = string.Empty;
            await TaiDanhSachCoreAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tạo bàn mới: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteDoiTrangThaiBan()
    {
        if (IsBusy || SelectedBan is null || SelectedTrangThaiCapNhat?.Value is null)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _banService.CapNhatTrangThaiBanAsync(
                SelectedBan.BanId,
                SelectedTrangThaiCapNhat.Value);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            await TaiDanhSachCoreAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể cập nhật trạng thái bàn: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteChuyenBan()
    {
        if (IsBusy || SelectedBan is null || SelectedBanDich is null)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _banService.ChuyenBanAsync(SelectedBan.BanId, SelectedBanDich.BanId);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            await TaiDanhSachCoreAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể chuyển bàn: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteGopBan()
    {
        if (IsBusy || SelectedBan is null || SelectedBanDich is null)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _banService.GopBanAsync(SelectedBan.BanId, SelectedBanDich.BanId);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            await TaiDanhSachCoreAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể gộp bàn: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteLamMoi()
    {
        TuKhoaTimKiem = string.Empty;
        SelectedKhuVucTimKiem = null;
        SelectedTrangThaiTimKiem = DanhSachTrangThai.FirstOrDefault();
        SelectedBan = null;
        SelectedBanDich = null;
        SelectedTrangThaiCapNhat = null;

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
            await TaiDanhSachCoreAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải danh sách bàn: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TaiDanhSachCoreAsync(CancellationToken cancellationToken = default)
    {
        var selectedBanId = SelectedBan?.BanId;
        var selectedBanDichId = SelectedBanDich?.BanId;

        var result = await _banService.GetDanhSachBanAsync(
            TuKhoaTimKiem,
            SelectedKhuVucTimKiem?.KhuVucId,
            true,
            SelectedTrangThaiTimKiem?.Value,
            cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message;
            return;
        }

        DanhSachBan.Clear();
        foreach (var item in result.Data)
        {
            DanhSachBan.Add(item);
        }

        SelectedBan = selectedBanId.HasValue
            ? DanhSachBan.FirstOrDefault(x => x.BanId == selectedBanId.Value)
            : DanhSachBan.FirstOrDefault();

        SelectedBanDich = selectedBanDichId.HasValue
            ? DanhSachBan.FirstOrDefault(x => x.BanId == selectedBanDichId.Value)
            : DanhSachBan.FirstOrDefault(x => SelectedBan is null || x.BanId != SelectedBan.BanId);

        SuccessMessage = DanhSachBan.Count == 0
            ? "Không có dữ liệu bàn phù hợp."
            : $"Đã tải {DanhSachBan.Count} bàn.";
    }

    private async Task LoadKhuVucAsync(CancellationToken cancellationToken)
    {
        var result = await _banService.GetDanhSachKhuVucAsync(true, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message;
            return;
        }

        DanhSachKhuVuc.Clear();
        foreach (var item in result.Data)
        {
            DanhSachKhuVuc.Add(item);
        }

        if (SelectedKhuVucThem is null && DanhSachKhuVuc.Count > 0)
        {
            SelectedKhuVucThem = DanhSachKhuVuc[0];
        }
    }
}

public sealed class TrangThaiBanOption
{
    public TrangThaiBanOption(string displayName, string? value)
    {
        DisplayName = displayName;
        Value = value;
    }

    public string DisplayName { get; }

    public string? Value { get; }
}

