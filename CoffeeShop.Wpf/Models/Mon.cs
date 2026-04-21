namespace CoffeeShop.Wpf.Models;

public sealed class Mon
{
    public int MonId { get; set; }

    public string TenMon { get; set; } = string.Empty;

    public int DanhMucId { get; set; }

    public string? TenDanhMuc { get; set; }

    public decimal DonGia { get; set; }

    public int TonKho { get; set; }

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
