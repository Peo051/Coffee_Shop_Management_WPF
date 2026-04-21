namespace CoffeeShop.Wpf.Models;

/// <summary>
/// Model quản lý nguyên liệu trong kho
/// </summary>
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

    // === Computed properties ===

    /// <summary>Kiểm tra nguyên liệu sắp hết hàng</summary>
    public bool SapHetHang => TonKho > 0 && TonKho <= TonKhoToiThieu;

    /// <summary>Trạng thái tồn kho hiển thị</summary>
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

    /// <summary>Hiển thị tồn kho với đơn vị</summary>
    public string TonKhoHienThi => $"{TonKho:N2} {DonViTinh}";

    /// <summary>Hiển thị đơn giá nhập</summary>
    public string DonGiaNhapHienThi => $"{DonGiaNhap:N0} đ/{DonViTinh}";
}
