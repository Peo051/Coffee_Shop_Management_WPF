namespace CoffeeShop.Wpf.Models;

public sealed class KhuVuc
{
    public int KhuVucId { get; set; }

    public string TenKhuVuc { get; set; } = string.Empty;

    public string? MoTa { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

