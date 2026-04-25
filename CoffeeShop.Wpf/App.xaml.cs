using System.Windows;
using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Repositories;
using CoffeeShop.Wpf.Services;
using CoffeeShop.Wpf.ViewModels;

namespace CoffeeShop.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            DbConnectionFactory.InitializeFromConfig();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi Cấu Hình Kết Nối", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        var navigationService = new NavigationService();
        var sessionService = new SessionService();
        var dialogService = new DialogService();
        var auditLogRepository = new AuditLogRepository();
        var auditLogService = new AuditLogService(auditLogRepository);
        var userRepository = new UserRepository();
        var authService = new AuthService(userRepository, auditLogService);
        var userSecurityService = new UserSecurityService(userRepository, auditLogService);
        var permissionService = new PermissionService();

        var dashboardRepository = new DashboardRepository();
        var dashboardService = new DashboardService(dashboardRepository);
        var dashboardViewModel = new DashboardViewModel(dashboardService);

        var auditLogViewModel = new AuditLogViewModel(auditLogService);

        var khuyenMaiRepository = new KhuyenMaiRepository();
        var khuyenMaiService = new KhuyenMaiService(khuyenMaiRepository);
        var khuyenMaiViewModel = new KhuyenMaiViewModel(khuyenMaiService);

        var khachHangRepository = new KhachHangRepository();
        var khachHangService = new KhachHangService(khachHangRepository);
        var khachHangViewModel = new KhachHangViewModel(khachHangService);

        var doiMatKhauViewModel = new DoiMatKhauViewModel(userSecurityService, sessionService);
        var quanLyTaiKhoanViewModel = new QuanLyTaiKhoanViewModel(userSecurityService, sessionService, dialogService);

        var cauHinhHeThongRepository = new CauHinhHeThongRepository();
        var cauHinhHeThongService = new CauHinhHeThongService(cauHinhHeThongRepository, auditLogService, sessionService);
        var cauHinhHeThongViewModel = new CauHinhHeThongViewModel(cauHinhHeThongService);

        var exportPrintRepository = new ExportPrintRepository();
        var exportPrintService = new ExportPrintService(exportPrintRepository, auditLogService, cauHinhHeThongService);
        var exportPrintViewModel = new ExportPrintViewModel(exportPrintService, sessionService);

        var danhMucRepository = new DanhMucRepository();
        var danhMucService = new DanhMucService(danhMucRepository);
        var danhMucViewModel = new DanhMucViewModel(danhMucService);

        var nhaCungCapRepository = new NhaCungCapRepository();
        var nhaCungCapService = new NhaCungCapService(nhaCungCapRepository);
        var nhaCungCapViewModel = new NhaCungCapViewModel(nhaCungCapService);

        var monRepository = new MonRepository();
        var monService = new MonService(monRepository, danhMucService);
        var monViewModel = new MonViewModel(monService, danhMucService);

        var lichSuTonKhoRepository = new LichSuTonKhoRepository();
        var lichSuNguyenLieuRepository = new LichSuNguyenLieuRepository();
        var nguyenLieuRepository = new NguyenLieuRepository();
        var nguyenLieuService = new NguyenLieuService(nguyenLieuRepository);
        var nguyenLieuViewModel = new NguyenLieuViewModel(nguyenLieuService);

        var congThucMonRepository = new CongThucMonRepository();
        var congThucMonService = new CongThucMonService(congThucMonRepository, monService, nguyenLieuService);
        var congThucMonViewModel = new CongThucMonViewModel(congThucMonService, monService, nguyenLieuService);

        var khoRepository = new KhoRepository();
        var khoService = new KhoService(khoRepository, auditLogService, sessionService);
        var trangThaiSanPhamViewModel = new TrangThaiSanPhamViewModel(khoService, danhMucService);
        var canhBaoTonKhoViewModel = new CanhBaoTonKhoViewModel(khoService, danhMucService);
        var timKiemSanPhamViewModel = new TimKiemSanPhamViewModel(monService, danhMucService);

        var hoaDonNhapRepository = new HoaDonNhapRepository(lichSuTonKhoRepository);
        var hoaDonNhapService = new HoaDonNhapService(hoaDonNhapRepository, nhaCungCapService, monService, auditLogService);
        var hoaDonNhapViewModel = new HoaDonNhapViewModel(hoaDonNhapService, nhaCungCapService, monService, sessionService);

        // Module Quản lý bàn đã bị gỡ - giữ lại BanRepository và BanService vì HoaDonBanService còn dùng
        var banRepository = new BanRepository();
        var banService = new BanService(banRepository, auditLogService, sessionService);
        // var quanLyBanViewModel = new QuanLyBanViewModel(banService); // Đã gỡ

        var caLamViecRepository = new CaLamViecRepository();
        var caLamViecService = new CaLamViecService(caLamViecRepository, auditLogService);
        var caLamViecViewModel = new CaLamViecViewModel(caLamViecService, sessionService);

        var lichSuHoaDonRepository = new LichSuHoaDonRepository();
        var lichSuHoaDonService = new LichSuHoaDonService(lichSuHoaDonRepository, auditLogService);
        var lichSuHoaDonViewModel = new LichSuHoaDonViewModel(lichSuHoaDonService, exportPrintService, dialogService, sessionService);

        var hoaDonBanRepository = new HoaDonBanRepository(
            lichSuTonKhoRepository,
            congThucMonRepository,
            nguyenLieuRepository,
            lichSuNguyenLieuRepository);
        var hoaDonBanService = new HoaDonBanService(
            hoaDonBanRepository,
            monService,
            banRepository,
            caLamViecRepository,
            auditLogService,
            khuyenMaiService,
            khachHangService);
        var hoaDonBanViewModel = new HoaDonBanViewModel(
            hoaDonBanService,
            monService,
            caLamViecService,
            khachHangService,
            khuyenMaiService,
            sessionService);

        var thongKeRepository = new ThongKeRepository();
        var thongKeService = new ThongKeService(thongKeRepository, hoaDonBanRepository);
        var thongKeViewModel = new ThongKeViewModel(thongKeService);
        var topSanPhamService = new TopSanPhamService(thongKeRepository);
        var topSanPhamBanChayViewModel = new TopSanPhamBanChayViewModel(topSanPhamService);

        var baoCaoRepository = new BaoCaoRepository();
        var baoCaoService = new BaoCaoService(baoCaoRepository);
        var baoCaoViewModel = new BaoCaoViewModel(baoCaoService);

        var phaCheRepository = new PhaCheRepository();
        var phaCheService = new PhaCheService(phaCheRepository);
        var phaCheViewModel = new PhaCheViewModel(phaCheService);

        var mainShellViewModel = new MainShellViewModel(
            permissionService,
            dashboardViewModel,
            auditLogViewModel,
            exportPrintViewModel,
            khuyenMaiViewModel,
            khachHangViewModel,
            doiMatKhauViewModel,
            quanLyTaiKhoanViewModel,
            cauHinhHeThongViewModel,
            danhMucViewModel,
            nhaCungCapViewModel,
            monViewModel,
            nguyenLieuViewModel,
            congThucMonViewModel,
            trangThaiSanPhamViewModel,
            canhBaoTonKhoViewModel,
            timKiemSanPhamViewModel,
            hoaDonNhapViewModel,
            // Module Quản lý bàn đã bị gỡ
            // quanLyBanViewModel,
            caLamViecViewModel,
            hoaDonBanViewModel,
            lichSuHoaDonViewModel,
            topSanPhamBanChayViewModel,
            thongKeViewModel,
            baoCaoViewModel,
            phaCheViewModel);

        var loginViewModel = new LoginViewModel(
            authService,
            sessionService,
            navigationService,
            mainShellViewModel);
        mainShellViewModel.ConfigureSessionControl(sessionService, navigationService, loginViewModel);

        var mainWindowViewModel = new MainWindowViewModel(navigationService);

        navigationService.Navigate(loginViewModel);

        var mainWindow = new MainWindow
        {
            DataContext = mainWindowViewModel
        };

        mainWindow.Show();
    }
}

