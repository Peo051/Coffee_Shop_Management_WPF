using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoffeeShop.Wpf.Models;

public sealed class MenuItemModel : INotifyPropertyChanged
{
    private bool _isActive;

    public MenuItemModel(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
        GroupName = ResolveGroupName(code);
    }

    public string Code { get; }

    public string DisplayName { get; }

    public string GroupName { get; }

    /// <summary>Trạng thái active của menu item</summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static string ResolveGroupName(string code)
    {
        return code switch
        {
            // Tổng quan
            "Dashboard" => "Tổng quan",
            "ThongKe" => "Tổng quan",
            
            // Bán hàng
            "HoaDonBan" => "Bán hàng",
            "LichSuHoaDon" => "Bán hàng",
            
            // Báo cáo
            "BaoCao" => "Báo cáo",
            "TopSanPhamBanChay" => "Báo cáo",
            "BaoCaoCaLam" => "Báo cáo",
            "BaoCaoKho" => "Báo cáo",
            
            // Kho & Nhập hàng
            "HoaDonNhap" => "Kho & Nhập hàng",
            "NguyenLieu" => "Kho & Nhập hàng",
            "CanhBaoTonKho" => "Kho & Nhập hàng",
            "NhaCungCap" => "Kho & Nhập hàng",
            
            // Sản phẩm & Menu
            "Mon" => "Sản phẩm & Menu",
            "DanhMuc" => "Sản phẩm & Menu",
            "CongThucMon" => "Sản phẩm & Menu",
            "TrangThaiSanPham" => "Sản phẩm & Menu",
            
            // Khách hàng & Marketing
            "KhachHang" => "Khách hàng & Marketing",
            "KhuyenMai" => "Khách hàng & Marketing",
            
            // Vận hành
            "PhaChe" => "Vận hành",
            "CaLamViec" => "Vận hành",
            
            // Quản trị hệ thống
            "QuanLyTaiKhoan" => "Quản trị hệ thống",
            "CauHinhHeThong" => "Quản trị hệ thống",
            "AuditLog" => "Quản trị hệ thống",
            
            // Tài khoản
            "DoiMatKhau" => "Tài khoản",
            
            // Deprecated - should not appear in new menu
            "TimKiemSanPham" => "Khác",
            "ExportPrint" => "Khác",
            
            _ => "Khác"
        };
    }
}
