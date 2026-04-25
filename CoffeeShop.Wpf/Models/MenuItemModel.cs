namespace CoffeeShop.Wpf.Models;

public sealed class MenuItemModel
{
    public MenuItemModel(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
        GroupName = ResolveGroupName(code);
    }

    public string Code { get; }

    public string DisplayName { get; }

    public string GroupName { get; }

    private static string ResolveGroupName(string code)
    {
        return code switch
        {
            "Dashboard" => "Tổng quan",
            "ThongKe" => "Tổng quan",
            "BaoCao" => "Tổng quan",
            "TopSanPhamBanChay" => "Tổng quan",
            "HoaDonNhap" => "Nghiệp vụ",
            "HoaDonBan" => "Nghiệp vụ",
            "ExportPrint" => "Nghiệp vụ",
            "LichSuHoaDon" => "Nghiệp vụ",
            // Module Quản lý bàn đã bị gỡ
            // "QuanLyBan" => "Vận hành",
            "CaLamViec" => "Vận hành",
            "TrangThaiSanPham" => "Vận hành",
            "CanhBaoTonKho" => "Vận hành",
            "TimKiemSanPham" => "Vận hành",
            "DanhMuc" => "Danh mục",
            "NhaCungCap" => "Danh mục",
            "Mon" => "Danh mục",
            "NguyenLieu" => "Danh mục",
            "CongThucMon" => "Danh mục",
            "KhuyenMai" => "Danh mục",
            "KhachHang" => "Danh mục",
            "QuanLyTaiKhoan" => "Quản trị",
            "CauHinhHeThong" => "Quản trị",
            "AuditLog" => "Quản trị",
            "DoiMatKhau" => "Tài khoản",
            _ => "Khác"
        };
    }
}
