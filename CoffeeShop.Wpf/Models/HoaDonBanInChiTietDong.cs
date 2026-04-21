namespace CoffeeShop.Wpf.Models;

public sealed class HoaDonBanInChiTietDong
{
    public int MonId { get; set; }

    public string TenMon { get; set; } = string.Empty;

    public int SoLuong { get; set; }

    public decimal DonGiaBan { get; set; }

    public decimal ThanhTien { get; set; }

    // === Size & Ghi chú ===

    /// <summary>Kích cỡ: Nhỏ, Vừa, Lớn</summary>
    public string KichCo { get; set; } = "Mặc định";

    /// <summary>Phụ thu theo kích cỡ</summary>
    public decimal PhuThuKichCo { get; set; }

    /// <summary>Ghi chú riêng cho món</summary>
    public string? GhiChuMon { get; set; }
}

