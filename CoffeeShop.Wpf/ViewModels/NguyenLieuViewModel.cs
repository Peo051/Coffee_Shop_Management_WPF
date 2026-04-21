using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class NguyenLieuViewModel : BaseViewModel
{
    private readonly INguyenLieuService _nguyenLieuService;
    private readonly RelayCommand _taoMoiCommand;
    private readonly RelayCommand _timKiemCommand;
    private readonly RelayCommand _lamMoiCommand;

    private string _tenNguyenLieu = string.Empty;
    private string _donViTinh = string.Empty;
    private string _tonKho = "0";
    private string _tonKhoToiThieu = "10";
    private string _donGiaNhap = "0";
    private string _tuKhoaTimKiem = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public NguyenLieuViewModel(INguyenLieuService nguyenLieuService)
    {
        _nguyenLieuService = nguyenLieuService;

        NguyenLieus = new ObservableCollection<NguyenLieu>();

        _taoMoiCommand = new RelayCommand(ExecuteTaoMoi, CanExecuteTaoMoi);
        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<NguyenLieu> NguyenLieus { get; }

    public string TenNguyenLieu
    {
        get => _tenNguyenLieu;
        set
        {
            if (SetProperty(ref _tenNguyenLieu, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string DonViTinh
    {
        get => _donViTinh;
        set
        {
            if (SetProperty(ref _donViTinh, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TonKho
    {
        get => _tonKho;
        set
        {
            if (SetProperty(ref _tonKho, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TonKhoToiThieu
    {
        get => _tonKhoToiThieu;
        set
        {
            if (SetProperty(ref _tonKhoToiThieu, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string DonGiaNhap
    {
        get => _donGiaNhap;
        set
        {
            if (SetProperty(ref _donGiaNhap, value))
            {
                _taoMoiCommand.RaiseCanExecuteChanged();
            }
        }
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
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            await LoadNguyenLieuAsync(TuKhoaTimKiem, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu nguyên liệu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteTaoMoi()
    {
        return !IsBusy
               && !string.IsNullOrWhiteSpace(TenNguyenLieu)
               && !string.IsNullOrWhiteSpace(DonViTinh)
               && !string.IsNullOrWhiteSpace(TonKho)
               && !string.IsNullOrWhiteSpace(TonKhoToiThieu)
               && !string.IsNullOrWhiteSpace(DonGiaNhap);
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

        if (!TryParseDecimal(TonKho, out var tonKhoValue))
        {
            ErrorMessage = "Tồn kho không hợp lệ.";
            return;
        }

        if (!TryParseDecimal(TonKhoToiThieu, out var tonKhoToiThieuValue))
        {
            ErrorMessage = "Tồn kho tối thiểu không hợp lệ.";
            return;
        }

        if (!TryParseDecimal(DonGiaNhap, out var donGiaNhapValue))
        {
            ErrorMessage = "Đơn giá nhập không hợp lệ.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _nguyenLieuService.CreateAsync(
                TenNguyenLieu,
                DonViTinh,
                tonKhoValue,
                tonKhoToiThieuValue,
                donGiaNhapValue,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            TenNguyenLieu = string.Empty;
            DonViTinh = string.Empty;
            TonKho = "0";
            TonKhoToiThieu = "10";
            DonGiaNhap = "0";

            await LoadNguyenLieuAsync(TuKhoaTimKiem, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tạo nguyên liệu: {ex.Message}";
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
            await LoadNguyenLieuAsync(TuKhoaTimKiem);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tìm kiếm nguyên liệu: {ex.Message}";
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

        TuKhoaTimKiem = string.Empty;

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            await LoadNguyenLieuAsync(null);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải lại dữ liệu nguyên liệu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadNguyenLieuAsync(string? keyword, CancellationToken cancellationToken = default)
    {
        var nguyenLieus = await _nguyenLieuService.SearchAsync(keyword, activeOnly: true, cancellationToken);

        NguyenLieus.Clear();
        foreach (var item in nguyenLieus)
        {
            NguyenLieus.Add(item);
        }

        if (NguyenLieus.Count == 0)
        {
            SuccessMessage = "Không có dữ liệu phù hợp.";
        }
        else if (string.IsNullOrWhiteSpace(keyword))
        {
            SuccessMessage = $"Đã tải {NguyenLieus.Count} nguyên liệu.";
        }
        else
        {
            SuccessMessage = $"Tìm thấy {NguyenLieus.Count} nguyên liệu theo điều kiện lọc.";
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
