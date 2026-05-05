using System.Collections.ObjectModel;
using System.Globalization;
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
            }
        }
    }

    public NguyenLieu? SelectedNguyenLieu
    {
        get => _selectedNguyenLieu;
        set => SetProperty(ref _selectedNguyenLieu, value);
    }

    public bool IsDetailVisible
    {
        get => _isDetailVisible;
        set => SetProperty(ref _isDetailVisible, value);
    }

    public string FormTitle => IsEditing ? "Chỉnh sửa nguyên liệu" : "Quản lý nguyên liệu";

    public string SubmitButtonText => IsEditing ? "Cập nhật" : "Tạo mới";

    public ICommand TaoMoiCommand => _taoMoiCommand;

    public ICommand TimKiemCommand => _timKiemCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public ICommand EditCommand => _editCommand;

    public ICommand DeleteCommand => _deleteCommand;

    public ICommand ViewDetailCommand => _viewDetailCommand;

    public ICommand HuyChinhSuaCommand => _huyChinhSuaCommand;

    public ICommand CloseDetailCommand => _closeDetailCommand;

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
