namespace CoffeeShop.Wpf.Models;

public sealed class ChiTietHoaDonBan
{
    public int ChiTietHoaDonBanId { get; set; }

    public int HoaDonBanId { get; set; }

    public int MonId { get; set; }

    public string? TenMon { get; set; }

    public decimal DonGiaBan { get; set; }

    public int SoLuong { get; set; }

    public decimal ThanhTien => DonGiaBan * SoLuong;

    // === Size & Ghi chú ===

    /// <summary>Kích cỡ: Nhỏ, Vừa, Lớn</summary>
    public string KichCo { get; set; } = "Mặc định";

    /// <summary>Phụ thu theo kích cỡ</summary>
    public decimal PhuThuKichCo { get; set; }

    /// <summary>Ghi chú riêng cho món (ít đá, không đường...)</summary>
    public string? GhiChuMon { get; set; }
}
