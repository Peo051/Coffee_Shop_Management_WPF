namespace CoffeeShop.Wpf.Models;

public sealed class KhachHang
{
    public int KhachHangId { get; set; }

    public string HoTen { get; set; } = string.Empty;

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public int DiemTichLuy { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string HienThi => string.IsNullOrWhiteSpace(SoDienThoai)
        ? $"{HoTen} - {DiemTichLuy} điểm"
        : $"{HoTen} - {SoDienThoai} - {DiemTichLuy} điểm";
}

