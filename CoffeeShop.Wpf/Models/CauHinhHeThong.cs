namespace CoffeeShop.Wpf.Models;

public sealed class CauHinhHeThong
{
    public int CauHinhHeThongId { get; set; }

    public string TenQuan { get; set; } = string.Empty;

    public string? DiaChi { get; set; }

    public string? SoDienThoai { get; set; }

    public string? FooterHoaDon { get; set; }

    public string? LogoPath { get; set; }

    public DateTime UpdatedAt { get; set; }
}

