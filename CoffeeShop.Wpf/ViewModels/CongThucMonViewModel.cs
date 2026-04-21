using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class CongThucMonViewModel : BaseViewModel
{
    private readonly ICongThucMonService _congThucMonService;
    private readonly IMonService _monService;
    private readonly INguyenLieuService _nguyenLieuService;
    private readonly RelayCommand _themNguyenLieuCommand;
    private readonly RelayCommand<CongThucMon> _xoaNguyenLieuCommand;

    private Mon? _selectedMon;
    private NguyenLieu? _selectedNguyenLieu;
    private string _dinhLuong = "0";
    private string _ghiChu = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public CongThucMonViewModel(
        ICongThucMonService congThucMonService,
        IMonService monService,
        INguyenLieuService nguyenLieuService)
    {
        _congThucMonService = congThucMonService;
        _monService = monService;
        _nguyenLieuService = nguyenLieuService;

        Mons = new ObservableCollection<Mon>();
        NguyenLieus = new ObservableCollection<NguyenLieu>();
        CongThucMons = new ObservableCollection<CongThucMon>();

        _themNguyenLieuCommand = new RelayCommand(ExecuteThemNguyenLieu, CanExecuteThemNguyenLieu);
        _xoaNguyenLieuCommand = new RelayCommand<CongThucMon>(ExecuteXoaNguyenLieu, CanExecuteXoaNguyenLieu);
    }

    public ObservableCollection<Mon> Mons { get; }

    public ObservableCollection<NguyenLieu> NguyenLieus { get; }

    public ObservableCollection<CongThucMon> CongThucMons { get; }

    public Mon? SelectedMon
    {
        get => _selectedMon;
        set
        {
            if (SetProperty(ref _selectedMon, value))
            {
                _ = LoadCongThucMonAsync();
                _themNguyenLieuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public NguyenLieu? SelectedNguyenLieu
    {
        get => _selectedNguyenLieu;
        set
        {
            if (SetProperty(ref _selectedNguyenLieu, value))
            {
                _themNguyenLieuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string DinhLuong
    {
        get => _dinhLuong;
        set
        {
            if (SetProperty(ref _dinhLuong, value))
            {
                _themNguyenLieuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string GhiChu
    {
        get => _ghiChu;
        set => SetProperty(ref _ghiChu, value);
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
                _themNguyenLieuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand ThemNguyenLieuCommand => _themNguyenLieuCommand;

    public ICommand XoaNguyenLieuCommand => _xoaNguyenLieuCommand;

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
            await LoadMonsAsync(cancellationToken);
            await LoadNguyenLieusAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteThemNguyenLieu()
    {
        return !IsBusy
               && SelectedMon != null
               && SelectedNguyenLieu != null
               && !string.IsNullOrWhiteSpace(DinhLuong);
    }

    private async void ExecuteThemNguyenLieu()
    {
        await ThemNguyenLieuAsync();
    }

    private async Task ThemNguyenLieuAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || SelectedMon == null || SelectedNguyenLieu == null)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (!TryParseDecimal(DinhLuong, out var dinhLuongValue))
        {
            ErrorMessage = "Định lượng không hợp lệ.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _congThucMonService.CreateAsync(
                SelectedMon.MonId,
                SelectedNguyenLieu.NguyenLieuId,
                dinhLuongValue,
                GhiChu,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            DinhLuong = "0";
            GhiChu = string.Empty;
            SelectedNguyenLieu = null;

            await LoadCongThucMonAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể thêm nguyên liệu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteXoaNguyenLieu(CongThucMon? congThucMon)
    {
        return !IsBusy && congThucMon != null;
    }

    private async void ExecuteXoaNguyenLieu(CongThucMon? congThucMon)
    {
        if (congThucMon == null)
        {
            return;
        }

        await XoaNguyenLieuAsync(congThucMon);
    }

    private async Task XoaNguyenLieuAsync(CongThucMon congThucMon, CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        IsBusy = true;

        try
        {
            var result = await _congThucMonService.DeleteAsync(congThucMon.CongThucMonId, cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;

            await LoadCongThucMonAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể xóa nguyên liệu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadMonsAsync(CancellationToken cancellationToken)
    {
        var mons = await _monService.SearchAsync(null, null, cancellationToken);

        Mons.Clear();
        foreach (var item in mons.OrderBy(x => x.TenMon))
        {
            Mons.Add(item);
        }

        if (Mons.Count > 0 && SelectedMon == null)
        {
            SelectedMon = Mons[0];
        }
    }

    private async Task LoadNguyenLieusAsync(CancellationToken cancellationToken)
    {
        var nguyenLieus = await _nguyenLieuService.GetAllAsync(activeOnly: true, cancellationToken);

        NguyenLieus.Clear();
        foreach (var item in nguyenLieus.OrderBy(x => x.TenNguyenLieu))
        {
            NguyenLieus.Add(item);
        }
    }

    private async Task LoadCongThucMonAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedMon == null)
        {
            CongThucMons.Clear();
            return;
        }

        try
        {
            var congThucMons = await _congThucMonService.GetByMonAsync(SelectedMon.MonId, activeOnly: true, cancellationToken);

            CongThucMons.Clear();
            foreach (var item in congThucMons)
            {
                CongThucMons.Add(item);
            }

            if (CongThucMons.Count == 0)
            {
                SuccessMessage = $"Món '{SelectedMon.TenMon}' chưa có công thức.";
            }
            else
            {
                SuccessMessage = $"Món '{SelectedMon.TenMon}' có {CongThucMons.Count} nguyên liệu.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải công thức: {ex.Message}";
        }
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
        {
            return true;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }
}
