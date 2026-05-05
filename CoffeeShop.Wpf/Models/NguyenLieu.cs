namespace CoffeeShop.Wpf.Models;

public sealed class NguyenLieu
{
    public int NguyenLieuId { get; set; }

    public string TenNguyenLieu { get; set; } = string.Empty;

    public string DonViTinh { get; set; } = string.Empty;

    public decimal TonKho { get; set; }

    public decimal TonKhoToiThieu { get; set; }

    public decimal DonGiaNhap { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool SapHetHang => TonKho > 0 && TonKho <= TonKhoToiThieu;

    public string TrangThaiTonKhoHienThi
    {
        get
        {
            if (TonKho <= 0)
                return "Hết hàng";
            if (TonKho <= TonKhoToiThieu)
                return "Sắp hết";
            return "Còn hàng";
        }
    }

    public string TonKhoHienThi => $"{TonKho:N2} {DonViTinh}";
    public string DonGiaNhapHienThi => $"{DonGiaNhap:N0} đ/{DonViTinh}";

    public string TrangThaiHoatDongHienThi => IsActive ? "Đang hoạt động" : "Ngừng hoạt động";

    public decimal SoLuongCanBoSung => Math.Max(0, TonKhoToiThieu - TonKho);
}
