using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public sealed class PermissionService
{
    public IReadOnlyList<MenuItemModel> GetMenuByRole(string role)
    {
        return role switch
        {
            "Admin" =>
            [
                // 1. Tổng quan
                new MenuItemModel("Dashboard", "Dashboard"),
                new MenuItemModel("ThongKe", "Thống kê nhanh"),
                
                // 2. Bán hàng
                new MenuItemModel("HoaDonBan", "POS / Bán hàng tại quầy"),
                new MenuItemModel("LichSuHoaDon", "Lịch sử hóa đơn"),
                
                // 3. Báo cáo
                new MenuItemModel("BaoCao", "Báo cáo doanh thu"),
                new MenuItemModel("TopSanPhamBanChay", "Báo cáo sản phẩm bán chạy"),
                // Note: BaoCaoCaLam and BaoCaoKho can be added later if needed
                
                // 4. Kho & Nhập hàng
                new MenuItemModel("HoaDonNhap", "Nhập hàng"),
                new MenuItemModel("NguyenLieu", "Kho nguyên liệu"),
                new MenuItemModel("CanhBaoTonKho", "Cảnh báo tồn kho thấp"),
                new MenuItemModel("NhaCungCap", "Nhà cung cấp"),
                
                // 5. Sản phẩm & Menu
                new MenuItemModel("Mon", "Quản lý sản phẩm"),
                new MenuItemModel("DanhMuc", "Quản lý danh mục"),
                new MenuItemModel("CongThucMon", "Công thức món"),
                new MenuItemModel("TrangThaiSanPham", "Trạng thái sản phẩm"),
                
                // 6. Khách hàng & Marketing
                new MenuItemModel("KhachHang", "Khách hàng thân thiết"),
                new MenuItemModel("KhuyenMai", "Khuyến mãi"),
                
                // 7. Vận hành
                new MenuItemModel("PhaChe", "Quầy pha chế"),
                new MenuItemModel("CaLamViec", "Ca làm việc"),
                
                // 8. Quản trị hệ thống
                new MenuItemModel("QuanLyTaiKhoan", "Quản lý tài khoản"),
                new MenuItemModel("CauHinhHeThong", "Cấu hình hệ thống"),
                new MenuItemModel("AuditLog", "Nhật ký thao tác"),
                
                // 9. Tài khoản
                new MenuItemModel("DoiMatKhau", "Đổi mật khẩu")
            ],
            "Kho" =>
            [
                // 4. Kho & Nhập hàng
                new MenuItemModel("HoaDonNhap", "Nhập hàng"),
                new MenuItemModel("NguyenLieu", "Kho nguyên liệu"),
                new MenuItemModel("CanhBaoTonKho", "Cảnh báo tồn kho thấp"),
                new MenuItemModel("NhaCungCap", "Nhà cung cấp"),
                
                // 5. Sản phẩm & Menu
                new MenuItemModel("Mon", "Quản lý sản phẩm"),
                new MenuItemModel("DanhMuc", "Quản lý danh mục"),
                new MenuItemModel("CongThucMon", "Công thức món"),
                new MenuItemModel("TrangThaiSanPham", "Trạng thái sản phẩm"),
                
                // 9. Tài khoản
                new MenuItemModel("DoiMatKhau", "Đổi mật khẩu")
            ],
            "ThuNgan" =>
            [
                // 2. Bán hàng
                new MenuItemModel("HoaDonBan", "POS / Bán hàng tại quầy"),
                new MenuItemModel("LichSuHoaDon", "Lịch sử hóa đơn"),
                
                // 6. Khách hàng & Marketing
                new MenuItemModel("KhachHang", "Khách hàng thân thiết"),
                
                // 7. Vận hành
                new MenuItemModel("CaLamViec", "Ca làm việc"),
                
                // 9. Tài khoản
                new MenuItemModel("DoiMatKhau", "Đổi mật khẩu")
            ],
            "PhaChe" =>
            [
                // 7. Vận hành
                new MenuItemModel("PhaChe", "Quầy pha chế"),
                new MenuItemModel("TrangThaiSanPham", "Trạng thái sản phẩm"),
                
                // 9. Tài khoản
                new MenuItemModel("DoiMatKhau", "Đổi mật khẩu")
            ],
            _ => []
        };
    }

    public bool HasPermission(string role, string moduleCode)
    {
        var menu = GetMenuByRole(role);
        return menu.Any(m => string.Equals(m.Code, moduleCode, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsAdmin(string role)
    {
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsKho(string role)
    {
        return string.Equals(role, "Kho", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsThuNgan(string role)
    {
        return string.Equals(role, "ThuNgan", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsPhaChe(string role)
    {
        return string.Equals(role, "PhaChe", StringComparison.OrdinalIgnoreCase);
    }
}

