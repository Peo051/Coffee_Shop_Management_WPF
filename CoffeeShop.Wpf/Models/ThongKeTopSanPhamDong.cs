namespace CoffeeShop.Wpf.Models;

public sealed class ThongKeTopSanPhamDong
{
    public int MonId { get; init; }

    public string TenMon { get; init; } = string.Empty;

    public int TongSoLuongBan { get; init; }

    public int SoHoaDon { get; init; }

    public decimal TongDoanhThu { get; init; }
}
