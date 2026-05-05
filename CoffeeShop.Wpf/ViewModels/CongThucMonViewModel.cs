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
    private readonly RelayCommand<CongThucMon> _suaNguyenLieuCommand;

    private Mon? _selectedMon;
    private NguyenLieu? _selectedNguyenLieu;
    private string _dinhLuong = "0";
    private string _ghiChu = string.Empty;
    private string _cacBuocThucHien = string.Empty;
    private string _tyLeHaoHut = "0";
    private string _donViTinhHienThi = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;
    private bool _isEditMode;
    private int _editingCongThucMonId;

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
        LichSuCapNhats = new ObservableCollection<LichSuCapNhatCongThuc>();

        _themNguyenLieuCommand = new RelayCommand(ExecuteThemNguyenLieu, CanExecuteThemNguyenLieu);
        _xoaNguyenLieuCommand = new RelayCommand<CongThucMon>(ExecuteXoaNguyenLieu, CanExecuteXoaNguyenLieu);
        _suaNguyenLieuCommand = new RelayCommand<CongThucMon>(ExecuteSuaNguyenLieu, CanExecuteSuaNguyenLieu);
    }

    public ObservableCollection<Mon> Mons { get; }

    public ObservableCollection<NguyenLieu> NguyenLieus { get; }

    public ObservableCollection<CongThucMon> CongThucMons { get; }

    public ObservableCollection<LichSuCapNhatCongThuc> LichSuCapNhats { get; }

    public Mon? SelectedMon
    {
        get => _selectedMon;
        set
        {
            if (SetProperty(ref _selectedMon, value))
            {
                _ = LoadCongThucMonAsync();
                _ = LoadLichSuCapNhatAsync();
                _themNguyenLieuCommand.RaiseCanExecuteChanged();
                _suaNguyenLieuCommand.RaiseCanExecuteChanged();
                _xoaNguyenLieuCommand.RaiseCanExecuteChanged();
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
                // Yêu cầu 1: Hiển thị đơn vị tính ngay khi chọn nguyên liệu
                DonViTinhHienThi = value?.DonViTinh ?? string.Empty;
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
                TinhToanGiaVon();
            }
        }
    }

    public string GhiChu
    {
        get => _ghiChu;
        set => SetProperty(ref _ghiChu, value);
    }

    public string CacBuocThucHien
    {
        get => _cacBuocThucHien;
        set => SetProperty(ref _cacBuocThucHien, value);
    }

    public string TyLeHaoHut
    {
        get => _tyLeHaoHut;
        set
        {
            if (SetProperty(ref _tyLeHaoHut, value))
            {
                TinhToanGiaVon();
            }
        }
    }

    public string DonViTinhHienThi
    {
        get => _donViTinhHienThi;
        private set => SetProperty(ref _donViTinhHienThi, value);
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

    public bool IsEditMode
    {
        get => _isEditMode;
        private set
        {
            if (SetProperty(ref _isEditMode, value))
            {
                OnPropertyChanged(nameof(ThemNguyenLieuButtonText));
                _themNguyenLieuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private decimal _giaVonDaTinh;
    public decimal GiaVonDaTinh
    {
        get => _giaVonDaTinh;
        private set
        {
            if (SetProperty(ref _giaVonDaTinh, value))
            {
                OnPropertyChanged(nameof(GiaVonDaTinhHienThi));
            }
        }
    }

    public string GiaVonDaTinhHienThi => $"{GiaVonDaTinh:N0} đ";

    public string ThemNguyenLieuButtonText => IsEditMode ? "Cập nhật nguyên liệu" : "Thêm nguyên liệu";

    public ICommand ThemNguyenLieuCommand => _themNguyenLieuCommand;

    public ICommand XoaNguyenLieuCommand => _xoaNguyenLieuCommand;

    public ICommand SuaNguyenLieuCommand => _suaNguyenLieuCommand;

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
               && (IsEditMode || SelectedNguyenLieu != null)
               && (!IsEditMode || _editingCongThucMonId > 0)
               && !string.IsNullOrWhiteSpace(DinhLuong);
    }

    private async void ExecuteThemNguyenLieu()
    {
        if (IsEditMode)
        {
            await CapNhatNguyenLieuAsync();
        }
        else
        {
            await ThemNguyenLieuAsync();
        }
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

        if (!TryParseDecimal(TyLeHaoHut, out var tyLeHaoHutValue))
        {
            ErrorMessage = "Tỷ lệ hao hụt không hợp lệ.";
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
                CacBuocThucHien,
                tyLeHaoHutValue,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            ResetForm();

            await LoadCongThucMonAsync(cancellationToken);
            await LoadLichSuCapNhatAsync(cancellationToken);
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

    private bool CanExecuteSuaNguyenLieu(CongThucMon? congThucMon)
    {
        return !IsBusy && congThucMon != null;
    }

    private void ExecuteSuaNguyenLieu(CongThucMon? congThucMon)
    {
        if (congThucMon == null)
        {
            return;
        }

        // Yêu cầu 3: Đẩy dữ liệu lên form để chỉnh sửa
        IsEditMode = true;
        _editingCongThucMonId = congThucMon.CongThucMonId;

        SelectedNguyenLieu = NguyenLieus.FirstOrDefault(nl => nl.NguyenLieuId == congThucMon.NguyenLieuId);
        DinhLuong = congThucMon.DinhLuong.ToString(CultureInfo.CurrentCulture);
        GhiChu = congThucMon.GhiChu ?? string.Empty;
        CacBuocThucHien = congThucMon.CacBuocThucHien ?? string.Empty;
        TyLeHaoHut = congThucMon.TyLeHaoHut.ToString(CultureInfo.CurrentCulture);

        SuccessMessage = $"Đang chỉnh sửa '{congThucMon.TenNguyenLieu}'. Cập nhật xong nhấn 'Cập nhật nguyên liệu'.";
    }

    private async Task CapNhatNguyenLieuAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
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

        if (!TryParseDecimal(TyLeHaoHut, out var tyLeHaoHutValue))
        {
            ErrorMessage = "Tỷ lệ hao hụt không hợp lệ.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _congThucMonService.UpdateAsync(
                _editingCongThucMonId,
                dinhLuongValue,
                GhiChu,
                CacBuocThucHien,
                tyLeHaoHutValue,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            SuccessMessage = result.Message;
            ResetForm();

            await LoadCongThucMonAsync(cancellationToken);
            await LoadLichSuCapNhatAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể cập nhật nguyên liệu: {ex.Message}";
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
            await LoadLichSuCapNhatAsync(cancellationToken);
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

            // Yêu cầu 4: Tự động tính giá vốn
            TinhToanGiaVon();

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

    private async Task LoadLichSuCapNhatAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedMon == null)
        {
            LichSuCapNhats.Clear();
            return;
        }

        try
        {
            var lichSu = await _congThucMonService.GetLichSuCapNhatAsync(SelectedMon.MonId, cancellationToken);

            LichSuCapNhats.Clear();
            foreach (var item in lichSu)
            {
                LichSuCapNhats.Add(item);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải lịch sử: {ex.Message}";
        }
    }

    /// <summary>
    /// Yêu cầu 4 & 6: Tính tổng giá vốn nguyên liệu theo tỷ lệ hao hụt đã lưu trên từng dòng công thức.
    /// </summary>
    private void TinhToanGiaVon()
    {
        if (CongThucMons.Count == 0)
        {
            GiaVonDaTinh = 0;
            return;
        }

        GiaVonDaTinh = CongThucMons.Sum(TinhGiaVonSauHaoHut);
    }

    private static decimal TinhGiaVonSauHaoHut(CongThucMon congThucMon)
    {
        if (congThucMon.TyLeHaoHut <= 0 || congThucMon.TyLeHaoHut >= 100)
        {
            return congThucMon.GiaVonNguyenLieu;
        }

        return congThucMon.GiaVonNguyenLieu / (1 - congThucMon.TyLeHaoHut / 100);
    }

    private void ResetForm()
    {
        DinhLuong = "0";
        GhiChu = string.Empty;
        CacBuocThucHien = string.Empty;
        TyLeHaoHut = "0";
        SelectedNguyenLieu = null;
        DonViTinhHienThi = string.Empty;
        IsEditMode = false;
        _editingCongThucMonId = 0;
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
