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
                new MenuItemModel("Dashboard", "Dashboard quản trị"),
                new MenuItemModel("ThongKe", "Thống kê doanh thu"),
                new MenuItemModel("BaoCao", "Báo cáo"),
                new MenuItemModel("TopSanPhamBanChay", "Top sản phẩm bán chạy"),
                new MenuItemModel("HoaDonNhap", "Hóa đơn nhập"),
                new MenuItemModel("HoaDonBan", "Bán hàng tại quầy"),
                new MenuItemModel("PhaChe", "Quầy pha chế"),
                new MenuItemModel("ExportPrint", "Xuất / In"),
                new MenuItemModel("LichSuHoaDon", "Lịch sử hóa đơn"),
                // Module "Quản lý bàn" đã bị gỡ - quán hoạt động theo mô hình order tại quầy, không quản lý bàn
                new MenuItemModel("CaLamViec", "Ca làm việc"),
                new MenuItemModel("TrangThaiSanPham", "Trạng thái sản phẩm"),
                new MenuItemModel("CanhBaoTonKho", "Cảnh báo tồn kho thấp"),
                new MenuItemModel("TimKiemSanPham", "Tìm kiếm sản phẩm"),
                new MenuItemModel("DanhMuc", "Quản lý danh mục"),
                new MenuItemModel("NhaCungCap", "Quản lý nhà cung cấp"),
                new MenuItemModel("Mon", "Quản lý sản phẩm"),
                new MenuItemModel("NguyenLieu", "Kho nguyên liệu"),
                new MenuItemModel("CongThucMon", "Công thức món"),
                new MenuItemModel("KhuyenMai", "Khuyến mãi"),
                new MenuItemModel("KhachHang", "Khách hàng thân thiết"),
                new MenuItemModel("QuanLyTaiKhoan", "Quản lý tài khoản"),
                new MenuItemModel("CauHinhHeThong", "Cấu hình hệ thống"),
                new MenuItemModel("AuditLog", "Nhật ký thao tác"),
                new MenuItemModel("DoiMatKhau", "Đổi mật khẩu")
            ],
            "Kho" =>
            [
                new MenuItemModel("DanhMuc", "Quản lý danh mục"),
                new MenuItemModel("NhaCungCap", "Quản lý nhà cung cấp"),
                new MenuItemModel("Mon", "Quản lý sản phẩm"),
                new MenuItemModel("NguyenLieu", "Kho nguyên liệu"),
                new MenuItemModel("CongThucMon", "Công thức món"),
                new MenuItemModel("TrangThaiSanPham", "Trạng thái sản phẩm"),
                new MenuItemModel("CanhBaoTonKho", "Cảnh báo tồn kho thấp"),
                new MenuItemModel("TimKiemSanPham", "Tìm kiếm sản phẩm"),
                new MenuItemModel("HoaDonNhap", "Hóa đơn nhập"),
                new MenuItemModel("DoiMatKhau", "Đổi mật khẩu")
            ],
            "ThuNgan" =>
            [
                // Chỉ giữ các module trọng tâm đối với nhân viên thu ngân để dễ tập trung bảo vệ đồ án
                new MenuItemModel("CaLamViec", "Ca làm việc"),
                new MenuItemModel("HoaDonBan", "Bán hàng tại quầy"),
                new MenuItemModel("KhachHang", "Khách hàng thân thiết"),
                new MenuItemModel("LichSuHoaDon", "Lịch sử hóa đơn"),
                new MenuItemModel("ThongKe", "Thống kê doanh thu"),
                new MenuItemModel("ExportPrint", "Xuất / In"),
                new MenuItemModel("DoiMatKhau", "Đổi mật khẩu")
                // Module "Quản lý bàn" đã bị gỡ - quán hoạt động theo mô hình order tại quầy, không quản lý bàn
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
}

