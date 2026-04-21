namespace CoffeeShop.Wpf.Models;

public sealed class ChiTietHoaDonNhap
{
    public int ChiTietHoaDonNhapId { get; set; }

    public int HoaDonNhapId { get; set; }

    public int MonId { get; set; }

    public decimal DonGiaNhap { get; set; }

    public int SoLuong { get; set; }

    public decimal ThanhTien => DonGiaNhap * SoLuong;
}
