using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class KhuyenMaiViewModel : BaseViewModel
{
    private readonly IKhuyenMaiService _khuyenMaiService;
    private readonly RelayCommand _themCommand;
    private readonly RelayCommand _suaCommand;
    private readonly RelayCommand _lamMoiCommand;
    private readonly RelayCommand _timKiemCommand;

    private string _tenKhuyenMai = string.Empty;
    private string _loaiKhuyenMai = "PhanTramHoaDon";
    private string _giaTri = "0";
    private DateTime _tuNgay = DateTime.Today;
    private DateTime _denNgay = DateTime.Today.AddDays(30);
    private bool _isActive = true;
    private string _moTa = string.Empty;
    private string _tuKhoa = string.Empty;
    private string _giaTriDonToiThieuText = string.Empty;
    private string _soTienGiamToiDaText = string.Empty;

    private KhuyenMai? _selectedKhuyenMai;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public KhuyenMaiViewModel(IKhuyenMaiService khuyenMaiService)
    {
        _khuyenMaiService = khuyenMaiService;

        DanhSachKhuyenMai = new ObservableCollection<KhuyenMai>();
        LoaiKhuyenMaiOptions = new ObservableCollection<string>
        {
            "PhanTramHoaDon",
            "SoTienCoDinh"
        };

        _themCommand = new RelayCommand(ExecuteThem, CanExecuteThem);
        _suaCommand = new RelayCommand(ExecuteSua, CanExecuteSua);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
    }

    public ObservableCollection<KhuyenMai> DanhSachKhuyenMai { get; }

    public ObservableCollection<string> LoaiKhuyenMaiOptions { get; }

    public KhuyenMai? SelectedKhuyenMai
    {
        get => _selectedKhuyenMai;
        set
        {
            if (SetProperty(ref _selectedKhuyenMai, value) && value is not null)
            {
                TenKhuyenMai = value.TenKhuyenMai;
                LoaiKhuyenMai = value.LoaiKhuyenMai;
                GiaTri = value.GiaTri.ToString("0.##", CultureInfo.CurrentCulture);
                TuNgay = value.TuNgay;
                DenNgay = value.DenNgay;
                IsActive = value.IsActive;
                MoTa = value.MoTa ?? string.Empty;
                GiaTriDonToiThieuText = value.GiaTriDonHangToiThieu?.ToString("0", CultureInfo.CurrentCulture) ?? string.Empty;
                SoTienGiamToiDaText = value.SoTienGiamToiDa?.ToString("0", CultureInfo.CurrentCulture) ?? string.Empty;
                _suaCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TenKhuyenMai
    {
        get => _tenKhuyenMai;
        set
        {
            if (SetProperty(ref _tenKhuyenMai, value))
            {
                _themCommand.RaiseCanExecuteChanged();
                _suaCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string LoaiKhuyenMai
    {
        get => _loaiKhuyenMai;
        set => SetProperty(ref _loaiKhuyenMai, value);
    }

    public string GiaTri
    {
        get => _giaTri;
        set => SetProperty(ref _giaTri, value);
    }

    public DateTime TuNgay
    {
        get => _tuNgay;
        set => SetProperty(ref _tuNgay, value);
    }

    public DateTime DenNgay
    {
        get => _denNgay;
        set => SetProperty(ref _denNgay, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string MoTa
    {
        get => _moTa;
        set => SetProperty(ref _moTa, value);
    }

    public string TuKhoa
    {
        get => _tuKhoa;
        set => SetProperty(ref _tuKhoa, value);
    }

    public string GiaTriDonToiThieuText
    {
        get => _giaTriDonToiThieuText;
        set => SetProperty(ref _giaTriDonToiThieuText, value);
    }

    public string SoTienGiamToiDaText
    {
        get => _soTienGiamToiDaText;
        set => SetProperty(ref _soTienGiamToiDaText, value);
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
                _themCommand.RaiseCanExecuteChanged();
                _suaCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
                _timKiemCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand ThemCommand => _themCommand;

    public ICommand SuaCommand => _suaCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public ICommand TimKiemCommand => _timKiemCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadDataAsync(TuKhoa, cancellationToken);
    }

    private bool CanExecuteThem()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(TenKhuyenMai);
    }

    private bool CanExecuteSua()
    {
        return !IsBusy && SelectedKhuyenMai is not null;
    }

    private async void ExecuteThem()
    {
        await ThemAsync();
    }

    private async void ExecuteSua()
    {
        await SuaAsync();
    }

    private async void ExecuteLamMoi()
    {
        TenKhuyenMai = string.Empty;
        LoaiKhuyenMai = "PhanTramHoaDon";
        GiaTri = "0";
        TuNgay = DateTime.Today;
        DenNgay = DateTime.Today.AddDays(30);
        IsActive = true;
        MoTa = string.Empty;
        GiaTriDonToiThieuText = string.Empty;
        SoTienGiamToiDaText = string.Empty;
        TuKhoa = string.Empty;
        SelectedKhuyenMai = null;
        await LoadDataAsync(null);
    }

    private async void ExecuteTimKiem()
    {
        await LoadDataAsync(TuKhoa);
    }

    private async Task ThemAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (!TryParseGiaTri(out var giaTri))
        {
            ErrorMessage = "Giá trị khuyến mãi không hợp lệ.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _khuyenMaiService.TaoKhuyenMaiAsync(
                    new KhuyenMai
                    {
                        TenKhuyenMai = TenKhuyenMai,
                        LoaiKhuyenMai = LoaiKhuyenMai,
                        GiaTri = giaTri,
                        TuNgay = TuNgay,
                        DenNgay = DenNgay,
                        IsActive = IsActive,
                        MoTa = MoTa,
                        GiaTriDonHangToiThieu = ParseNullableDecimal(GiaTriDonToiThieuText),
                        SoTienGiamToiDa = ParseNullableDecimal(SoTienGiamToiDaText)
                    },
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            await LoadDataAsync(TuKhoa, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể thêm khuyến mãi: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SuaAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || SelectedKhuyenMai is null)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (!TryParseGiaTri(out var giaTri))
        {
            ErrorMessage = "Giá trị khuyến mãi không hợp lệ.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _khuyenMaiService.CapNhatKhuyenMaiAsync(
                new KhuyenMai
                {
                    KhuyenMaiId = SelectedKhuyenMai.KhuyenMaiId,
                    TenKhuyenMai = TenKhuyenMai,
                    LoaiKhuyenMai = LoaiKhuyenMai,
                    GiaTri = giaTri,
                    TuNgay = TuNgay,
                    DenNgay = DenNgay,
                    IsActive = IsActive,
                    MoTa = MoTa,
                    GiaTriDonHangToiThieu = ParseNullableDecimal(GiaTriDonToiThieuText),
                    SoTienGiamToiDa = ParseNullableDecimal(SoTienGiamToiDaText)
                },
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            await LoadDataAsync(TuKhoa, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể cập nhật khuyến mãi: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadDataAsync(string? keyword, CancellationToken cancellationToken = default)
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
            var result = await _khuyenMaiService.GetDanhSachKhuyenMaiAsync(keyword, null, cancellationToken);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            DanhSachKhuyenMai.Clear();
            foreach (var item in result.Data)
            {
                DanhSachKhuyenMai.Add(item);
            }

            SuccessMessage = $"Đã tải {DanhSachKhuyenMai.Count} khuyến mãi.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải danh sách khuyến mãi: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool TryParseGiaTri(out decimal giaTri)
    {
        if (decimal.TryParse(GiaTri, NumberStyles.Number, CultureInfo.CurrentCulture, out giaTri))
        {
            return true;
        }

        return decimal.TryParse(GiaTri, NumberStyles.Number, CultureInfo.InvariantCulture, out giaTri);
    }

    private static decimal? ParseNullableDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var result)
            || decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out result))
        {
            return result;
        }

        return null;
    }
}



