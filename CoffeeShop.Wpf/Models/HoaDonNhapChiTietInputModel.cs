namespace CoffeeShop.Wpf.Models;

public sealed class HoaDonNhapChiTietInputModel
{
    public int MonId { get; set; }

    public int SoLuong { get; set; }

    public decimal DonGiaNhap { get; set; }

    public decimal ThanhTien => SoLuong * DonGiaNhap;
}
