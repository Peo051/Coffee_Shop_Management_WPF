namespace CoffeeShop.Wpf.Models;

public sealed class LichSuHoaDonDong
{
    public int HoaDonBanId { get; set; }

    public DateTime NgayBan { get; set; }

    public decimal TongTien { get; set; }

    public decimal GiamGia { get; set; }

    public decimal ThanhToan => TongTien - GiamGia;

    public int CreatedByUserId { get; set; }

    public string TenNhanVien { get; set; } = string.Empty;

    public int? BanId { get; set; }

    public string? TenBan { get; set; }

    public string? TenKhuVuc { get; set; }

    public int? CaLamViecId { get; set; }

    // === Thông tin khách hàng ===

    /// <summary>Tên khách hàng (JOIN từ bảng KhachHang)</summary>
    public string TenKhachHang { get; set; } = "Khách lẻ";

    /// <summary>Số điện thoại khách hàng</summary>
    public string? SoDienThoai { get; set; }

    // === Thông tin thanh toán ===

    /// <summary>Hình thức thanh toán: Tiền mặt, Chuyển khoản, Thẻ, Ví điện tử</summary>
    public string HinhThucThanhToan { get; set; } = "Tiền mặt";

    /// <summary>Trạng thái: Đã thanh toán, Chưa thanh toán, Đã hủy</summary>
    public string TrangThaiThanhToan { get; set; } = "Đã thanh toán";

    // === Trạng thái pha chế ===

    /// <summary>Trạng thái pha chế: ChoPhaChe, DangPhaChe, DaHoanThanh, DaGiaoKhach, DaHuy</summary>
    public string TrangThaiPhaChe { get; set; } = TrangThaiPhaCheConst.ChoPhaChe;

    /// <summary>Trạng thái pha chế hiển thị tiếng Việt</summary>
    public string TrangThaiPhaCheHienThi => TrangThaiPhaCheConst.ToDisplayName(TrangThaiPhaChe);

    // === Hình thức phục vụ ===

    /// <summary>Hình thức phục vụ: UongTaiQuan, MangDi</summary>
    public string HinhThucPhucVu { get; set; } = HinhThucPhucVuConst.UongTaiQuan;

    /// <summary>Hình thức phục vụ hiển thị tiếng Việt</summary>
    public string HinhThucPhucVuHienThi => HinhThucPhucVuConst.ToDisplayName(HinhThucPhucVu);
}

