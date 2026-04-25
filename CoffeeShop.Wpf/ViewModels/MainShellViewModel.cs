using System.Collections.ObjectModel;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;
using CoffeeShop.Wpf.Commands;
using System.Windows.Input;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class MainShellViewModel : BaseViewModel
{
    private readonly PermissionService _permissionService;
    private readonly DanhMucViewModel _danhMucViewModel;
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly AuditLogViewModel _auditLogViewModel;
    private readonly ExportPrintViewModel _exportPrintViewModel;
    private readonly KhuyenMaiViewModel _khuyenMaiViewModel;
    private readonly KhachHangViewModel _khachHangViewModel;
    private readonly DoiMatKhauViewModel _doiMatKhauViewModel;
    private readonly QuanLyTaiKhoanViewModel _quanLyTaiKhoanViewModel;
    private readonly CauHinhHeThongViewModel _cauHinhHeThongViewModel;
    private readonly NhaCungCapViewModel _nhaCungCapViewModel;
    private readonly MonViewModel _monViewModel;
    private readonly NguyenLieuViewModel _nguyenLieuViewModel;
    private readonly CongThucMonViewModel _congThucMonViewModel;
    private readonly TrangThaiSanPhamViewModel _trangThaiSanPhamViewModel;
    private readonly CanhBaoTonKhoViewModel _canhBaoTonKhoViewModel;
    private readonly TimKiemSanPhamViewModel _timKiemSanPhamViewModel;
    private readonly HoaDonNhapViewModel _hoaDonNhapViewModel;
    // Module Quản lý bàn đã bị gỡ - không còn sử dụng
    // private readonly QuanLyBanViewModel _quanLyBanViewModel;
    private readonly CaLamViecViewModel _caLamViecViewModel;
    private readonly HoaDonBanViewModel _hoaDonBanViewModel;
    private readonly LichSuHoaDonViewModel _lichSuHoaDonViewModel;
    private readonly TopSanPhamBanChayViewModel _topSanPhamBanChayViewModel;
    private readonly ThongKeViewModel _thongKeViewModel;
    private readonly BaoCaoViewModel _baoCaoViewModel;
    private readonly PhaCheViewModel _phaCheViewModel;
    private readonly RelayCommand _dangXuatCommand;

    private SessionService? _sessionService;
    private INavigationService? _navigationService;
    private LoginViewModel? _loginViewModel;
    private string _currentUserRole = string.Empty;

    private string _welcomeText = "Chưa có phiên đăng nhập.";
    private string _currentModuleTitle = "Trang chủ";
    private string _currentModuleDescription = "Theo dõi nhanh hiệu suất vận hành và thao tác theo quyền.";
    private MenuItemModel? _selectedMenuItem;
    private object? _currentContentViewModel;

    public MainShellViewModel(
        PermissionService permissionService,
        DashboardViewModel dashboardViewModel,
        AuditLogViewModel auditLogViewModel,
        ExportPrintViewModel exportPrintViewModel,
        KhuyenMaiViewModel khuyenMaiViewModel,
        KhachHangViewModel khachHangViewModel,
        DoiMatKhauViewModel doiMatKhauViewModel,
        QuanLyTaiKhoanViewModel quanLyTaiKhoanViewModel,
        CauHinhHeThongViewModel cauHinhHeThongViewModel,
        DanhMucViewModel danhMucViewModel,
        NhaCungCapViewModel nhaCungCapViewModel,
        MonViewModel monViewModel,
        NguyenLieuViewModel nguyenLieuViewModel,
        CongThucMonViewModel congThucMonViewModel,
        TrangThaiSanPhamViewModel trangThaiSanPhamViewModel,
        CanhBaoTonKhoViewModel canhBaoTonKhoViewModel,
        TimKiemSanPhamViewModel timKiemSanPhamViewModel,
        HoaDonNhapViewModel hoaDonNhapViewModel,
        // Module Quản lý bàn đã bị gỡ
        // QuanLyBanViewModel quanLyBanViewModel,
        CaLamViecViewModel caLamViecViewModel,
        HoaDonBanViewModel hoaDonBanViewModel,
        LichSuHoaDonViewModel lichSuHoaDonViewModel,
        TopSanPhamBanChayViewModel topSanPhamBanChayViewModel,
        ThongKeViewModel thongKeViewModel,
        BaoCaoViewModel baoCaoViewModel,
        PhaCheViewModel phaCheViewModel)
    {
        _permissionService = permissionService;
        _dashboardViewModel = dashboardViewModel;
        _auditLogViewModel = auditLogViewModel;
        _exportPrintViewModel = exportPrintViewModel;
        _khuyenMaiViewModel = khuyenMaiViewModel;
        _khachHangViewModel = khachHangViewModel;
        _doiMatKhauViewModel = doiMatKhauViewModel;
        _quanLyTaiKhoanViewModel = quanLyTaiKhoanViewModel;
        _cauHinhHeThongViewModel = cauHinhHeThongViewModel;
        _danhMucViewModel = danhMucViewModel;
        _nhaCungCapViewModel = nhaCungCapViewModel;
        _monViewModel = monViewModel;
        _nguyenLieuViewModel = nguyenLieuViewModel;
        _congThucMonViewModel = congThucMonViewModel;
        _trangThaiSanPhamViewModel = trangThaiSanPhamViewModel;
        _canhBaoTonKhoViewModel = canhBaoTonKhoViewModel;
        _timKiemSanPhamViewModel = timKiemSanPhamViewModel;
        _hoaDonNhapViewModel = hoaDonNhapViewModel;
        // Module Quản lý bàn đã bị gỡ
        // _quanLyBanViewModel = quanLyBanViewModel;
        _caLamViecViewModel = caLamViecViewModel;
        _hoaDonBanViewModel = hoaDonBanViewModel;
        _lichSuHoaDonViewModel = lichSuHoaDonViewModel;
        _topSanPhamBanChayViewModel = topSanPhamBanChayViewModel;
        _thongKeViewModel = thongKeViewModel;
        _baoCaoViewModel = baoCaoViewModel;
        _phaCheViewModel = phaCheViewModel;
        _dangXuatCommand = new RelayCommand(ExecuteDangXuat, CanExecuteDangXuat);

        MenuItems = new ObservableCollection<MenuItemModel>();
    }

    public ObservableCollection<MenuItemModel> MenuItems { get; }

    public string WelcomeText
    {
        get => _welcomeText;
        private set => SetProperty(ref _welcomeText, value);
    }

    public MenuItemModel? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            if (SetProperty(ref _selectedMenuItem, value) && value is not null)
            {
                NavigateMenu(value);
            }
        }
    }

    public object? CurrentContentViewModel
    {
        get => _currentContentViewModel;
        private set => SetProperty(ref _currentContentViewModel, value);
    }

    public string CurrentModuleTitle
    {
        get => _currentModuleTitle;
        private set => SetProperty(ref _currentModuleTitle, value);
    }

    public string CurrentModuleDescription
    {
        get => _currentModuleDescription;
        private set => SetProperty(ref _currentModuleDescription, value);
    }

    public ICommand DangXuatCommand => _dangXuatCommand;

    public void ConfigureSessionControl(
        SessionService sessionService,
        INavigationService navigationService,
        LoginViewModel loginViewModel)
    {
        _sessionService = sessionService;
        _navigationService = navigationService;
        _loginViewModel = loginViewModel;
        _dangXuatCommand.RaiseCanExecuteChanged();
    }

    public void ApplySession(UserSessionModel userSession)
    {
        WelcomeText = $"Xin chào {userSession.DisplayName} ({userSession.Role})";
        _currentUserRole = userSession.Role;

        MenuItems.Clear();
        foreach (var item in _permissionService.GetMenuByRole(userSession.Role))
        {
            MenuItems.Add(item);
        }

        // Ưu tiên hiển thị màn hình 'Ca làm việc' cho ThuNgân nếu đã đăng nhập
        // Nếu có Service kiểm tra ca, có thể direct thẳng sang 'HoaDonBan' nếu đã có ca
        var defaultSelectedItem = MenuItems.FirstOrDefault(m => m.Code == "CaLamViec") 
                               ?? MenuItems.FirstOrDefault(m => m.Code == "HoaDonBan") 
                               ?? MenuItems.FirstOrDefault();
        
        SelectedMenuItem = defaultSelectedItem;
        _dangXuatCommand.RaiseCanExecuteChanged();

        // Set navigation callback for dashboard
        _dashboardViewModel.SetNavigationCallback(NavigateToModuleByCode);
    }

    public void NavigateToModuleByCode(string moduleCode)
    {
        var menuItem = MenuItems.FirstOrDefault(m => m.Code == moduleCode);
        if (menuItem is not null)
        {
            SelectedMenuItem = menuItem;
        }
    }

    private bool CanExecuteDangXuat()
    {
        return _sessionService?.IsAuthenticated ?? false;
    }

    private void ExecuteDangXuat()
    {
        if (_sessionService is null || _navigationService is null || _loginViewModel is null)
        {
            return;
        }

        _sessionService.Clear();
        _currentUserRole = string.Empty;
        _loginViewModel.PrepareForLoginScreen();

        WelcomeText = "Chưa có phiên đăng nhập.";
        CurrentModuleTitle = "Trang chủ";
        CurrentModuleDescription = "Theo dõi nhanh hiệu suất vận hành và thao tác theo quyền.";
        MenuItems.Clear();
        SelectedMenuItem = null;
        CurrentContentViewModel = null;

        _navigationService.Navigate(_loginViewModel);
        _dangXuatCommand.RaiseCanExecuteChanged();
    }

    private void NavigateMenu(MenuItemModel menuItem)
    {
        if (string.IsNullOrWhiteSpace(_currentUserRole)
            || !_permissionService.HasPermission(_currentUserRole, menuItem.Code))
        {
            return;
        }

        CurrentModuleTitle = menuItem.DisplayName;
        CurrentModuleDescription = BuildModuleDescription(menuItem.Code);

        switch (menuItem.Code)
        {
            case "Dashboard":
                CurrentContentViewModel = _dashboardViewModel;
                _ = _dashboardViewModel.LoadAsync();
                return;
            case "AuditLog":
                CurrentContentViewModel = _auditLogViewModel;
                _ = _auditLogViewModel.LoadAsync();
                return;
            case "ExportPrint":
                CurrentContentViewModel = _exportPrintViewModel;
                _ = _exportPrintViewModel.LoadAsync();
                return;
            case "KhuyenMai":
                CurrentContentViewModel = _khuyenMaiViewModel;
                _ = _khuyenMaiViewModel.LoadAsync();
                return;
            case "KhachHang":
                CurrentContentViewModel = _khachHangViewModel;
                _ = _khachHangViewModel.LoadAsync();
                return;
            case "DoiMatKhau":
                CurrentContentViewModel = _doiMatKhauViewModel;
                _ = _doiMatKhauViewModel.LoadAsync();
                return;
            case "QuanLyTaiKhoan":
                CurrentContentViewModel = _quanLyTaiKhoanViewModel;
                _ = _quanLyTaiKhoanViewModel.LoadAsync();
                return;
            case "CauHinhHeThong":
                CurrentContentViewModel = _cauHinhHeThongViewModel;
                _ = _cauHinhHeThongViewModel.LoadAsync();
                return;
            case "DanhMuc":
                CurrentContentViewModel = _danhMucViewModel;
                _ = _danhMucViewModel.LoadAsync();
                return;
            case "NhaCungCap":
                CurrentContentViewModel = _nhaCungCapViewModel;
                _ = _nhaCungCapViewModel.LoadAsync();
                return;
            case "Mon":
                CurrentContentViewModel = _monViewModel;
                _ = _monViewModel.LoadAsync();
                return;
            case "NguyenLieu":
                CurrentContentViewModel = _nguyenLieuViewModel;
                _ = _nguyenLieuViewModel.LoadAsync();
                return;
            case "CongThucMon":
                CurrentContentViewModel = _congThucMonViewModel;
                _ = _congThucMonViewModel.LoadAsync();
                return;
            case "TrangThaiSanPham":
                CurrentContentViewModel = _trangThaiSanPhamViewModel;
                _ = _trangThaiSanPhamViewModel.LoadAsync();
                return;
            case "CanhBaoTonKho":
                CurrentContentViewModel = _canhBaoTonKhoViewModel;
                _ = _canhBaoTonKhoViewModel.LoadAsync();
                return;
            case "TimKiemSanPham":
                CurrentContentViewModel = _timKiemSanPhamViewModel;
                _ = _timKiemSanPhamViewModel.LoadAsync();
                return;
            case "HoaDonNhap":
                CurrentContentViewModel = _hoaDonNhapViewModel;
                _ = _hoaDonNhapViewModel.LoadAsync();
                return;
            // Module Quản lý bàn đã bị gỡ - không còn sử dụng
            // case "QuanLyBan":
            //     CurrentContentViewModel = _quanLyBanViewModel;
            //     _ = _quanLyBanViewModel.LoadAsync();
            //     return;
            case "CaLamViec":
                CurrentContentViewModel = _caLamViecViewModel;
                _ = _caLamViecViewModel.LoadAsync();
                return;
            case "HoaDonBan":
                CurrentContentViewModel = _hoaDonBanViewModel;
                _ = _hoaDonBanViewModel.LoadAsync();
                return;
            case "LichSuHoaDon":
                CurrentContentViewModel = _lichSuHoaDonViewModel;
                _ = _lichSuHoaDonViewModel.LoadAsync();
                return;
            case "TopSanPhamBanChay":
                CurrentContentViewModel = _topSanPhamBanChayViewModel;
                _ = _topSanPhamBanChayViewModel.LoadAsync();
                return;
            case "ThongKe":
                CurrentContentViewModel = _thongKeViewModel;
                _ = _thongKeViewModel.LoadAsync();
                return;
            case "BaoCao":
                CurrentContentViewModel = _baoCaoViewModel;
                _ = _baoCaoViewModel.LoadAsync();
                return;
            case "PhaChe":
                CurrentContentViewModel = _phaCheViewModel;
                _ = _phaCheViewModel.LoadAsync();
                return;
            default:
                // Thay vì hiển thị "chưa triển khai", dùng thông báo lịch sự hơn phù hợp demo / nộp đồ án
                // Đặc biệt khi vai trò ThuNgan truy cập vào một số module
                CurrentContentViewModel = new ModulePlaceholderViewModel(
                    menuItem.DisplayName,
                    "Chức năng này hiện không nằm trong phạm vi sử dụng của vai trò hiện tại hoặc đang được ẩn để tập trung nghiệp vụ chính.");
                return;
        }
    }

    private static string BuildModuleDescription(string code)
    {
        return code switch
        {
            "Dashboard" => "Theo dõi nhanh tình hình hoạt động trong ngày và các chỉ số cần chú ý.",
            "HoaDonNhap" => "Lập phiếu nhập, kiểm tra số lượng, đơn giá và cập nhật tồn kho.",
            "HoaDonBan" => "Khách order tại quầy, thanh toán ngay, nhận nước sau. Mỗi lần mua thêm sẽ tạo một hóa đơn mới.",
            "ThongKe" => "Xem doanh thu, số lượng bán và kết quả kinh doanh theo thời gian.",
            "BaoCao" => "Tổng hợp số liệu để đối chiếu, in báo cáo.",
            "PhaChe" => "Theo dõi các đơn đã thanh toán và cập nhật trạng thái pha chế.",
            // Module Quản lý bàn đã bị gỡ
            // "QuanLyBan" => "Theo dõi trạng thái bàn và hỗ trợ sắp xếp phục vụ tại quán.",
            "AuditLog" => "Xem lại lịch sử thao tác để kiểm tra và đối chiếu khi cần.",
            "QuanLyTaiKhoan" => "Quản lý tài khoản người dùng, vai trò và trạng thái hoạt động.",
            "DoiMatKhau" => "Đổi mật khẩu đăng nhập để bảo vệ tài khoản cá nhân.",
            _ => ""
        };
    }
}

