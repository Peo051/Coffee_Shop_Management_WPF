namespace CoffeeShop.Wpf.Models;

/// <summary>
/// Model quản lý món ăn/đồ uống (thành phẩm)
/// </summary>
public sealed class Mon
{
    public int MonId { get; set; }

    public string TenMon { get; set; } = string.Empty;

    public int DanhMucId { get; set; }

    public string? TenDanhMuc { get; set; }

    public decimal DonGia { get; set; }

    /// <summary>
    /// Tồn kho THÀNH PHẨM - Số lượng món có thể bán ngay (đơn vị: phần/ly/suất).
    /// Trừ trực tiếp khi bán hàng để kiểm soát khả năng phục vụ.
    /// Khác với NguyenLieu.TonKho (tồn nguyên liệu thô).
    /// </summary>
    public int TonKho { get; set; }

    /// <summary>
    /// Ngưỡng cảnh báo tồn kho thành phẩm thấp
    /// </summary>
    public int TonKhoToiThieu { get; set; }

    public string? HinhAnhPath { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // === Computed properties ===

    /// <summary>Kiểm tra món sắp hết hàng</summary>
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
}
