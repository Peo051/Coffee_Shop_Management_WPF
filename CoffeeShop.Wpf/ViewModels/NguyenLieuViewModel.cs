using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
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
    private readonly RelayCommand<NguyenLieu> _editCommand;
    private readonly RelayCommand<NguyenLieu> _deleteCommand;
    private readonly RelayCommand<NguyenLieu> _viewDetailCommand;
    private readonly RelayCommand _huyChinhSuaCommand;
    private readonly RelayCommand _closeDetailCommand;
    private readonly RelayCommand _nhapThemNguyenLieuCommand;
    private readonly RelayCommand _dieuChinhTonKhoCommand;

    private string _tenNguyenLieu = string.Empty;
    private string _donViTinh = string.Empty;
    private string _tonKho = "0";
    private string _tonKhoToiThieu = "10";
    private string _donGiaNhap = "0";
    private string _tuKhoaTimKiem = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;
    private bool _isEditing;
    private int _editingNguyenLieuId;
    private NguyenLieu? _selectedNguyenLieu;
    private NguyenLieu? _selectedNguyenLieuTonKho;
    private string _soLuongNhapThem = "0";
    private string _donGiaNhapMoi = string.Empty;
    private string _ghiChuNhapThem = string.Empty;
    private string _tonKhoMoiDieuChinh = "0";
    private string _lyDoDieuChinh = string.Empty;
    private bool _isDetailVisible;

    public NguyenLieuViewModel(INguyenLieuService nguyenLieuService)
    {
        _nguyenLieuService = nguyenLieuService;

        NguyenLieus = new ObservableCollection<NguyenLieu>();

        _taoMoiCommand = new RelayCommand(ExecuteTaoMoi, CanExecuteTaoMoi);
        _timKiemCommand = new RelayCommand(ExecuteTimKiem, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
        _editCommand = new RelayCommand<NguyenLieu>(ExecuteEdit, _ => !IsBusy);
        _deleteCommand = new RelayCommand<NguyenLieu>(ExecuteDelete, _ => !IsBusy);
        _viewDetailCommand = new RelayCommand<NguyenLieu>(ExecuteViewDetail, _ => !IsBusy);
        _huyChinhSuaCommand = new RelayCommand(ExecuteHuyChinhSua, () => _isEditing);
        _closeDetailCommand = new RelayCommand(ExecuteCloseDetail);
        _nhapThemNguyenLieuCommand = new RelayCommand(ExecuteNhapThemNguyenLieu, CanExecuteNhapThemNguyenLieu);
        _dieuChinhTonKhoCommand = new RelayCommand(ExecuteDieuChinhTonKho, CanExecuteDieuChinhTonKho);
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
                _editCommand.RaiseCanExecuteChanged();
                _deleteCommand.RaiseCanExecuteChanged();
                _viewDetailCommand.RaiseCanExecuteChanged();
                _nhapThemNguyenLieuCommand.RaiseCanExecuteChanged();
                _dieuChinhTonKhoCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set
        {
            if (SetProperty(ref _isEditing, value))
            {
                _huyChinhSuaCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(FormTitle));
                OnPropertyChanged(nameof(SubmitButtonText));
                OnPropertyChanged(nameof(IsTonKhoEditable));
            }
        }
    }

    public NguyenLieu? SelectedNguyenLieu
    {
        get => _selectedNguyenLieu;
        set => SetProperty(ref _selectedNguyenLieu, value);
    }

    public NguyenLieu? SelectedNguyenLieuTonKho
    {
        get => _selectedNguyenLieuTonKho;
        set
        {
            if (SetProperty(ref _selectedNguyenLieuTonKho, value))
            {
                TonKhoMoiDieuChinh = value?.TonKho.ToString(CultureInfo.CurrentCulture) ?? "0";
                OnPropertyChanged(nameof(ThongTinNguyenLieuDangChon));
                _nhapThemNguyenLieuCommand.RaiseCanExecuteChanged();
                _dieuChinhTonKhoCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SoLuongNhapThem
    {
        get => _soLuongNhapThem;
        set
        {
            if (SetProperty(ref _soLuongNhapThem, value))
            {
                _nhapThemNguyenLieuCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string DonGiaNhapMoi
    {
        get => _donGiaNhapMoi;
        set => SetProperty(ref _donGiaNhapMoi, value);
    }

    public string GhiChuNhapThem
    {
        get => _ghiChuNhapThem;
        set => SetProperty(ref _ghiChuNhapThem, value);
    }

    public string TonKhoMoiDieuChinh
    {
        get => _tonKhoMoiDieuChinh;
        set
        {
            if (SetProperty(ref _tonKhoMoiDieuChinh, value))
            {
                _dieuChinhTonKhoCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string LyDoDieuChinh
    {
        get => _lyDoDieuChinh;
        set
        {
            if (SetProperty(ref _lyDoDieuChinh, value))
            {
                _dieuChinhTonKhoCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsDetailVisible
    {
        get => _isDetailVisible;
        set => SetProperty(ref _isDetailVisible, value);
    }

    public string FormTitle => IsEditing ? "Chỉnh sửa nguyên liệu" : "Quản lý nguyên liệu";

    public string SubmitButtonText => IsEditing ? "Cập nhật" : "Tạo mới";

    public bool IsTonKhoEditable => !IsEditing;

    public string ThongTinNguyenLieuDangChon => SelectedNguyenLieuTonKho == null
        ? "Chưa chọn nguyên liệu."
        : $"{SelectedNguyenLieuTonKho.TenNguyenLieu} (Tồn hiện tại: {SelectedNguyenLieuTonKho.TonKho:N2} {SelectedNguyenLieuTonKho.DonViTinh})";

    public ICommand TaoMoiCommand => _taoMoiCommand;

    public ICommand TimKiemCommand => _timKiemCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public ICommand EditCommand => _editCommand;

    public ICommand DeleteCommand => _deleteCommand;

    public ICommand ViewDetailCommand => _viewDetailCommand;

    public ICommand HuyChinhSuaCommand => _huyChinhSuaCommand;

    public ICommand CloseDetailCommand => _closeDetailCommand;

    public ICommand NhapThemNguyenLieuCommand => _nhapThemNguyenLieuCommand;

    public ICommand DieuChinhTonKhoCommand => _dieuChinhTonKhoCommand;

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
        if (IsEditing)
        {
            await CapNhatAsync();
        }
        else
        {
            await TaoMoiAsync();
        }
    }

    private bool CanExecuteNhapThemNguyenLieu()
    {
        return !IsBusy
               && SelectedNguyenLieuTonKho != null
               && TryParseDecimal(SoLuongNhapThem, out var soLuongNhap)
               && soLuongNhap > 0;
    }

    private bool CanExecuteDieuChinhTonKho()
    {
        return !IsBusy
               && SelectedNguyenLieuTonKho != null
               && TryParseDecimal(TonKhoMoiDieuChinh, out _)
               && !string.IsNullOrWhiteSpace(LyDoDieuChinh);
    }

    private async void ExecuteNhapThemNguyenLieu()
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (SelectedNguyenLieuTonKho == null)
        {
            ErrorMessage = "Vui lòng chọn nguyên liệu cần nhập thêm.";
            return;
        }

        if (!TryParseDecimal(SoLuongNhapThem, out var soLuongNhap))
        {
            ErrorMessage = "Số lượng nhập không hợp lệ.";
            return;
        }

        if (soLuongNhap <= 0)
        {
            ErrorMessage = "Số lượng nhập phải lớn hơn 0.";
            return;
        }

        decimal? donGiaNhapMoi = null;
        if (!string.IsNullOrWhiteSpace(DonGiaNhapMoi))
        {
            if (!TryParseDecimal(DonGiaNhapMoi, out var parsedDonGia))
            {
                ErrorMessage = "Đơn giá nhập không hợp lệ.";
                return;
            }

            if (parsedDonGia < 0)
            {
                ErrorMessage = "Đơn giá nhập không được âm.";
                return;
            }

            donGiaNhapMoi = parsedDonGia;
        }

        if (_nguyenLieuService is not NguyenLieuService nguyenLieuService)
        {
            ErrorMessage = "Không thể thực hiện nhập nguyên liệu do cấu hình service không tương thích.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await nguyenLieuService.NhapThemNguyenLieuAsync(
                SelectedNguyenLieuTonKho.NguyenLieuId,
                soLuongNhap,
                donGiaNhapMoi,
                GhiChuNhapThem,
                null);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            var operationMessage = result.Message;
            SoLuongNhapThem = "0";
            DonGiaNhapMoi = string.Empty;
            GhiChuNhapThem = string.Empty;
            LyDoDieuChinh = string.Empty;

            var selectedId = SelectedNguyenLieuTonKho.NguyenLieuId;
            await LoadNguyenLieuAsync(TuKhoaTimKiem);
            SelectedNguyenLieuTonKho = NguyenLieus.FirstOrDefault(x => x.NguyenLieuId == selectedId);
            SuccessMessage = operationMessage;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể nhập thêm nguyên liệu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteDieuChinhTonKho()
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (SelectedNguyenLieuTonKho == null)
        {
            ErrorMessage = "Vui lòng chọn nguyên liệu cần điều chỉnh.";
            return;
        }

        if (!TryParseDecimal(TonKhoMoiDieuChinh, out var tonKhoMoi))
        {
            ErrorMessage = "Tồn kho mới không hợp lệ.";
            return;
        }

        if (tonKhoMoi < 0)
        {
            ErrorMessage = "Tồn kho sau điều chỉnh không được âm.";
            return;
        }

        if (string.IsNullOrWhiteSpace(LyDoDieuChinh))
        {
            ErrorMessage = "Lý do điều chỉnh không được để trống.";
            return;
        }

        if (_nguyenLieuService is not NguyenLieuService nguyenLieuService)
        {
            ErrorMessage = "Không thể thực hiện điều chỉnh do cấu hình service không tương thích.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await nguyenLieuService.DieuChinhTonKhoAsync(
                SelectedNguyenLieuTonKho.NguyenLieuId,
                tonKhoMoi,
                LyDoDieuChinh,
                null);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            var operationMessage = result.Message;
            LyDoDieuChinh = string.Empty;

            var selectedId = SelectedNguyenLieuTonKho.NguyenLieuId;
            await LoadNguyenLieuAsync(TuKhoaTimKiem);
            SelectedNguyenLieuTonKho = NguyenLieus.FirstOrDefault(x => x.NguyenLieuId == selectedId);
            SuccessMessage = operationMessage;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể điều chỉnh tồn kho nguyên liệu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
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
            ClearForm();

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

    private async Task CapNhatAsync(CancellationToken cancellationToken = default)
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
            var result = await _nguyenLieuService.UpdateAsync(
                _editingNguyenLieuId,
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
            ClearForm();

            await LoadNguyenLieuAsync(TuKhoaTimKiem, cancellationToken);
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

    private void ExecuteEdit(NguyenLieu? nguyenLieu)
    {
        if (nguyenLieu == null || IsBusy)
        {
            return;
        }

        IsDetailVisible = false;

        IsEditing = true;
        _editingNguyenLieuId = nguyenLieu.NguyenLieuId;

        TenNguyenLieu = nguyenLieu.TenNguyenLieu;
        DonViTinh = nguyenLieu.DonViTinh;
        TonKho = nguyenLieu.TonKho.ToString(CultureInfo.CurrentCulture);
        TonKhoToiThieu = nguyenLieu.TonKhoToiThieu.ToString(CultureInfo.CurrentCulture);
        DonGiaNhap = nguyenLieu.DonGiaNhap.ToString(CultureInfo.CurrentCulture);

        ErrorMessage = string.Empty;
        SuccessMessage = $"Đang chỉnh sửa: {nguyenLieu.TenNguyenLieu}";
    }

    private async void ExecuteDelete(NguyenLieu? nguyenLieu)
    {
        if (nguyenLieu == null || IsBusy)
        {
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn ngừng sử dụng nguyên liệu '{nguyenLieu.TenNguyenLieu}'?\n\n" +
            "Nguyên liệu sẽ được đánh dấu là 'Ngừng hoạt động' và không hiển thị trong danh sách mặc định.",
            "Xác nhận ngừng sử dụng",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var serviceResult = await _nguyenLieuService.SetActiveAsync(
                nguyenLieu.NguyenLieuId,
                false);

            if (!serviceResult.IsSuccess)
            {
                ErrorMessage = serviceResult.Message;
                return;
            }

            SuccessMessage = serviceResult.Message;

            if (IsDetailVisible && SelectedNguyenLieu?.NguyenLieuId == nguyenLieu.NguyenLieuId)
            {
                IsDetailVisible = false;
                SelectedNguyenLieu = null;
            }

            await LoadNguyenLieuAsync(TuKhoaTimKiem);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể ngừng sử dụng nguyên liệu: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteViewDetail(NguyenLieu? nguyenLieu)
    {
        if (nguyenLieu == null || IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var detail = await _nguyenLieuService.GetByIdAsync(nguyenLieu.NguyenLieuId);

            if (detail == null)
            {
                ErrorMessage = "Không tìm thấy thông tin chi tiết nguyên liệu.";
                IsDetailVisible = false;
                SelectedNguyenLieu = null;
                return;
            }

            SelectedNguyenLieu = detail;
            IsDetailVisible = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải chi tiết nguyên liệu: {ex.Message}";
            IsDetailVisible = false;
            SelectedNguyenLieu = null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ExecuteHuyChinhSua()
    {
        ClearForm();
    }

    private void ExecuteCloseDetail()
    {
        IsDetailVisible = false;
        SelectedNguyenLieu = null;
    }

    private void ClearForm()
    {
        IsEditing = false;
        _editingNguyenLieuId = 0;

        TenNguyenLieu = string.Empty;
        DonViTinh = string.Empty;
        TonKho = "0";
        TonKhoToiThieu = "10";
        DonGiaNhap = "0";
    }
}
