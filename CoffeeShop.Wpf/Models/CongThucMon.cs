namespace CoffeeShop.Wpf.Models;

/// <summary>
/// Model công thức món - định nghĩa món dùng nguyên liệu gì, bao nhiêu
/// </summary>
public sealed class CongThucMon
{
    public int CongThucMonId { get; set; }

    public int MonId { get; set; }

    public string? TenMon { get; set; }

    public int NguyenLieuId { get; set; }

    public string? TenNguyenLieu { get; set; }

    public string? DonViTinh { get; set; }

    /// <summary>
    /// Định lượng nguyên liệu cần dùng cho 1 phần món
    /// </summary>
    public decimal DinhLuong { get; set; }

    public string? GhiChu { get; set; }

    public bool IsActive { get; set; } = true;

    // === Computed properties ===

    /// <summary>Hiển thị định lượng với đơn vị</summary>
    public string DinhLuongHienThi => $"{DinhLuong:N2} {DonViTinh}";

    /// <summary>Hiển thị định lượng dạng grams/ml nếu < 1</summary>
    public string DinhLuongChiTiet
    {
        get
        {
            if (DinhLuong < 1)
            {
                // Chuyển đổi sang đơn vị nhỏ hơn
                if (DonViTinh == "kg")
                    return $"{DinhLuong * 1000:N0}g";
                if (DonViTinh == "lít")
                    return $"{DinhLuong * 1000:N0}ml";
            }
            return DinhLuongHienThi;
        }
    }
}
