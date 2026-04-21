namespace CoffeeShop.Wpf.Models;

public sealed class LichSuNguyenLieu
{
    public int LichSuNguyenLieuId { get; set; }
    public int NguyenLieuId { get; set; }
    public string TenNguyenLieu { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public string LoaiPhatSinh { get; set; } = string.Empty;
    public decimal SoLuongThayDoi { get; set; }
    public decimal TonTruoc { get; set; }
    public decimal TonSau { get; set; }
    public int? HoaDonBanId { get; set; }
    public int? HoaDonNhapId { get; set; }
    public string? GhiChu { get; set; }
    public DateTime ThoiGian { get; set; }
    public int? NguoiDungId { get; set; }
    public string? TenNguoiDung { get; set; }

    public string SoLuongThayDoiHienThi => $"{SoLuongThayDoi:N2} {DonViTinh}";
    public string TonTruocHienThi => $"{TonTruoc:N2} {DonViTinh}";
    public string TonSauHienThi => $"{TonSau:N2} {DonViTinh}";
}
