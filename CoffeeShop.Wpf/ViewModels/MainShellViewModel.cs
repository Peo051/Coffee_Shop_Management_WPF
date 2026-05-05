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
    // ExportPrintViewModel removed - functionality moved to individual pages
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
    // TimKiemSanPhamViewModel removed - search functionality integrated into product pages
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
    private readonly RelayCommand _toggleSidebarCommand;

    private SessionService? _sessionService;
    private INavigationService? _navigationService;
    private LoginViewModel? _loginViewModel;
    private string _currentUserRole = string.Empty;

    private string _welcomeText = "Chưa có phiên đăng nhập.";
    private string _currentModuleTitle = "Trang chủ";
    private string _currentModuleDescription = "Theo dõi nhanh hiệu suất vận hành và thao tác theo quyền.";
    private MenuItemModel? _selectedMenuItem;
    private object? _currentContentViewModel;
    private bool _isSidebarCollapsed;
    private double _sidebarWidth = 280;

    public MainShellViewModel(
        PermissionService permissionService,
        DashboardViewModel dashboardViewModel,
        AuditLogViewModel auditLogViewModel,
        // ExportPrintViewModel removed - functionality moved to individual pages
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
        // TimKiemSanPhamViewModel removed - search functionality integrated into product pages
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
        // _exportPrintViewModel removed
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
        // _timKiemSanPhamViewModel removed
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
        _toggleSidebarCommand = new RelayCommand(ExecuteToggleSidebar);

        MenuItems = new ObservableCollection<MenuItemModel>();
        MenuGroups = new ObservableCollection<MenuGroupModel>();
    }

    public ObservableCollection<MenuItemModel> MenuItems { get; }

    /// <summary>Danh sách nhóm menu cho accordion sidebar</summary>
    public ObservableCollection<MenuGroupModel> MenuGroups { get; }

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
            // Bỏ active của item cũ
            if (_selectedMenuItem != null)
            {
                _selectedMenuItem.IsActive = false;
            }

            if (SetProperty(ref _selectedMenuItem, value) && value is not null)
            {
                // Set active cho item mới
                value.IsActive = true;

                // Mở nhóm chứa item được chọn và đóng các nhóm khác
                foreach (var group in MenuGroups)
                {
                    group.IsExpanded = group.Items.Contains(value);
                }

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

    /// <summary>Trạng thái sidebar: true = collapsed (thu gọn), false = expanded (mở rộng)</summary>
    public bool IsSidebarCollapsed
    {
        get => _isSidebarCollapsed;
        set
        {
            if (SetProperty(ref _isSidebarCollapsed, value))
            {
                SidebarWidth = value ? 72 : 280;
            }
        }
    }

    /// <summary>Chiều rộng sidebar: 280px (expanded) hoặc 72px (collapsed)</summary>
    public double SidebarWidth
    {
        get => _sidebarWidth;
        private set => SetProperty(ref _sidebarWidth, value);
    }

    public ICommand DangXuatCommand => _dangXuatCommand;

    public ICommand ToggleSidebarCommand => _toggleSidebarCommand;

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
        MenuGroups.Clear();

        var menuItems = _permissionService.GetMenuByRole(userSession.Role);
        
        // Nhóm menu items theo GroupName
        var groupedItems = menuItems.GroupBy(m => m.GroupName).OrderBy(g => GetGroupOrder(g.Key));

        foreach (var group in groupedItems)
        {
            var icon = GetGroupIcon(group.Key);
            var menuGroup = new MenuGroupModel(group.Key, icon);
            
            foreach (var item in group)
            {
                MenuItems.Add(item);
                menuGroup.Items.Add(item);
            }

            MenuGroups.Add(menuGroup);
        }

        // Ưu tiên hiển thị màn hình phù hợp với vai trò
        MenuItemModel? defaultSelectedItem = userSession.Role switch
        {
            "ThuNgan" => MenuItems.FirstOrDefault(m => m.Code == "CaLamViec") 
                      ?? MenuItems.FirstOrDefault(m => m.Code == "HoaDonBan"),
            "PhaChe" => MenuItems.FirstOrDefault(m => m.Code == "PhaChe"),
            _ => MenuItems.FirstOrDefault(m => m.Code == "Dashboard") 
              ?? MenuItems.FirstOrDefault()
        };
        
        if (defaultSelectedItem != null)
        {
            SelectedMenuItem = defaultSelectedItem;
            // Mở nhóm chứa item được chọn
            var groupContainingItem = MenuGroups.FirstOrDefault(g => g.Items.Contains(defaultSelectedItem));
            if (groupContainingItem != null)
            {
                groupContainingItem.IsExpanded = true;
            }
        }

        _dangXuatCommand.RaiseCanExecuteChanged();

        // Set navigation callback for dashboard
        _dashboardViewModel.SetNavigationCallback(NavigateToModuleByCode);
    }

    /// <summary>Xác định thứ tự hiển thị của các nhóm menu</summary>
    private static int GetGroupOrder(string groupName)
    {
        return groupName switch
        {
            "Tổng quan" => 1,
            "Bán hàng" => 2,
            "Báo cáo" => 3,
            "Kho & Nhập hàng" => 4,
            "Sản phẩm & Menu" => 5,
            "Khách hàng & Marketing" => 6,
            "Vận hành" => 7,
            "Quản trị hệ thống" => 8,
            "Tài khoản" => 9,
            _ => 99
        };
    }

    /// <summary>Lấy icon cho nhóm menu (hiển thị khi sidebar collapsed)</summary>
    private static string GetGroupIcon(string groupName)
    {
        return groupName switch
        {
            "Tổng quan" => "🏠",
            "Bán hàng" => "🛒",
            "Báo cáo" => "📊",
            "Kho & Nhập hàng" => "📦",
            "Sản phẩm & Menu" => "☕",
            "Khách hàng & Marketing" => "👥",
            "Vận hành" => "🍹",
            "Quản trị hệ thống" => "⚙",
            "Tài khoản" => "👤",
            _ => "📁"
        };
    }

    public void NavigateToModuleByCode(string moduleCode)
    {
        var menuItem = MenuItems.FirstOrDefault(m => m.Code == moduleCode);
        if (menuItem is not null)
        {
            SelectedMenuItem = menuItem;
            
            // Mở nhóm chứa item được chọn
            var groupContainingItem = MenuGroups.FirstOrDefault(g => g.Items.Contains(menuItem));
            if (groupContainingItem != null)
            {
                groupContainingItem.IsExpanded = true;
            }
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

    private void ExecuteToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;
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
            // ExportPrint case removed - functionality moved to individual pages
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
            // TimKiemSanPham case removed - search functionality integrated into product pages
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
            "ThongKe" => "Xem doanh thu, số lượng bán và kết quả kinh doanh theo thời gian.",
            "BaoCao" => "Tổng hợp số liệu doanh thu để đối chiếu, xuất PDF/Excel.",
            "TopSanPhamBanChay" => "Xem danh sách sản phẩm bán chạy nhất theo thời gian, xuất báo cáo.",
            "HoaDonNhap" => "Lập phiếu nhập, kiểm tra số lượng, đơn giá và cập nhật tồn kho.",
            "HoaDonBan" => "Khách order tại quầy, thanh toán ngay, in hóa đơn và phiếu pha chế.",
            "LichSuHoaDon" => "Tra cứu, xem chi tiết, in lại hóa đơn và phiếu pha chế.",
            "PhaChe" => "Theo dõi các đơn đã thanh toán và cập nhật trạng thái pha chế.",
            "CaLamViec" => "Quản lý ca làm việc, mở ca, đóng ca và xem báo cáo ca.",
            "NguyenLieu" => "Quản lý kho nguyên liệu, theo dõi tồn kho và lịch sử nhập xuất.",
            "CanhBaoTonKho" => "Cảnh báo nguyên liệu sắp hết để kịp thời nhập hàng.",
            "NhaCungCap" => "Quản lý thông tin nhà cung cấp nguyên liệu.",
            "Mon" => "Quản lý sản phẩm, giá bán, trạng thái còn hàng.",
            "DanhMuc" => "Quản lý danh mục sản phẩm để phân loại menu.",
            "CongThucMon" => "Quản lý công thức món, định mức nguyên liệu cho từng sản phẩm.",
            "TrangThaiSanPham" => "Theo dõi trạng thái còn hàng/hết hàng của sản phẩm.",
            "KhachHang" => "Quản lý khách hàng thân thiết, tích điểm và ưu đãi.",
            "KhuyenMai" => "Quản lý chương trình khuyến mãi và giảm giá.",
            "QuanLyTaiKhoan" => "Quản lý tài khoản người dùng, vai trò và trạng thái hoạt động.",
            "CauHinhHeThong" => "Cấu hình các thông số hệ thống và tùy chỉnh.",
            "AuditLog" => "Xem lại lịch sử thao tác để kiểm tra và đối chiếu khi cần.",
            "DoiMatKhau" => "Đổi mật khẩu đăng nhập để bảo vệ tài khoản cá nhân.",
            _ => ""
        };
    }
}

