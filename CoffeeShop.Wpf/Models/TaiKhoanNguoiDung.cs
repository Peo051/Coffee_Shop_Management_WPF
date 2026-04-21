namespace CoffeeShop.Wpf.Models;

public sealed class TaiKhoanNguoiDung
{
    public int NguoiDungId { get; set; }

    public string TenDangNhap { get; set; } = string.Empty;

    public string HoTen { get; set; } = string.Empty;

    public string MaVaiTro { get; set; } = string.Empty;

    public string TenVaiTro { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

