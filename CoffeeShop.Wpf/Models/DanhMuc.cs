namespace CoffeeShop.Wpf.Models;

public sealed class DanhMuc
{
    public int DanhMucId { get; set; }

    public string TenDanhMuc { get; set; } = string.Empty;

    public string? MoTa { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
