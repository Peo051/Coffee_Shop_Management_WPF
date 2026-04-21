namespace CoffeeShop.Wpf.Models;

public sealed class HoaDonBanTimKiemDong
{
    public int HoaDonBanId { get; set; }

    public DateTime NgayBan { get; set; }

    public decimal TongTien { get; set; }

    public decimal GiamGia { get; set; }

    public decimal ThanhToan => TongTien - GiamGia;

    public int CreatedByUserId { get; set; }

    public int? BanId { get; set; }

    public int? CaLamViecId { get; set; }
}
