namespace CoffeeShop.Wpf.Models;

public sealed class AuditLogDong
{
    public int AuditLogId { get; set; }

    public int? NguoiDungId { get; set; }

    public string? TenDangNhap { get; set; }

    public string HanhDong { get; set; } = string.Empty;

    public string? DoiTuong { get; set; }

    public string? DuLieuTomTat { get; set; }

    public DateTime ThoiGianTao { get; set; }

    public string? MayTram { get; set; }
}

