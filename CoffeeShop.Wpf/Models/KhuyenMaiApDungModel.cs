namespace CoffeeShop.Wpf.Models;

public sealed class KhuyenMaiApDungModel
{
    public int? KhuyenMaiId { get; set; }

    public string? TenKhuyenMai { get; set; }

    public decimal TongTienTruocGiam { get; set; }

    public decimal SoTienGiam { get; set; }

    public decimal TongTienSauGiam => TongTienTruocGiam - SoTienGiam < 0 ? 0 : TongTienTruocGiam - SoTienGiam;
}

