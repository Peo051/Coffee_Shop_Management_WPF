namespace CoffeeShop.Wpf.Models;

public sealed class HoaDonNhap
{
    public int HoaDonNhapId { get; set; }

    public DateTime NgayNhap { get; set; }

    public int NhaCungCapId { get; set; }

    public decimal TongTien { get; set; }

    public string? GhiChu { get; set; }

    public int CreatedByUserId { get; set; }
}
