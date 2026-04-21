namespace CoffeeShop.Wpf.Models;

public sealed class NhaCungCap
{
    public int NhaCungCapId { get; set; }

    public string TenNhaCungCap { get; set; } = string.Empty;

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
