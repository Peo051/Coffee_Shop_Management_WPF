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

    public string KichCo { get; set; } = "Mặc định";

    public decimal PhuThuKichCo { get; set; }

    public string? GhiChuMon { get; set; }
}
