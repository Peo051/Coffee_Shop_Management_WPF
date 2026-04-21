namespace CoffeeShop.Wpf.Models;

public sealed class HoaDonNhapChiTietDongModel
{
    public int MonId { get; set; }

    public string TenMon { get; set; } = string.Empty;

    public int SoLuong { get; set; }

    public decimal DonGiaNhap { get; set; }

    public decimal ThanhTien => SoLuong * DonGiaNhap;
}
