namespace CoffeeShop.Wpf.Models;

/// <summary>
/// Model quản lý nguyên liệu thô trong kho (cà phê, sữa, đường...)
/// </summary>
public sealed class NguyenLieu
{
    public int NguyenLieuId { get; set; }

    public string TenNguyenLieu { get; set; } = string.Empty;

    public string DonViTinh { get; set; } = string.Empty;

    /// <summary>
    /// Tồn kho NGUYÊN LIỆU THÔ - Số lượng nguyên liệu đầu vào (đơn vị: kg, lít, gram...).
    /// Trừ tự động theo công thức món khi bán hàng để theo dõi chi phí thực tế.
    /// Khác với Mon.TonKho (tồn thành phẩm).
    /// </summary>
    public decimal TonKho { get; set; }

    /// <summary>
    /// Ngưỡng cảnh báo tồn kho nguyên liệu thấp
    /// </summary>
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
