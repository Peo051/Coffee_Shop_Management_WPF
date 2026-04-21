namespace CoffeeShop.Wpf.Models;

public sealed class Ban
{
    public int BanId { get; set; }

    public int KhuVucId { get; set; }

    public string TenKhuVuc { get; set; } = string.Empty;

    public string TenBan { get; set; } = string.Empty;

    public string TrangThaiBan { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string TenBanHienThi => string.IsNullOrWhiteSpace(TenKhuVuc)
        ? TenBan
        : $"{TenKhuVuc} - {TenBan}";

    public string TrangThaiBanHienThi => TrangThaiBan switch
    {
        TrangThaiBanConst.Trong => "Trống",
        TrangThaiBanConst.DangCoKhach => "Đang có khách",
        "ChoThanhToan" => "Đang có khách",
        TrangThaiBanConst.TamKhoa => "Tạm khóa",
        _ => TrangThaiBan
    };
}
