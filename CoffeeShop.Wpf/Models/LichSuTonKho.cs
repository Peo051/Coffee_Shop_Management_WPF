namespace CoffeeShop.Wpf.Models;

/// <summary>
/// Model lưu lịch sử thay đổi tồn kho của món
/// </summary>
public sealed class LichSuTonKho
{
    public int LichSuTonKhoId { get; set; }

    public int MonId { get; set; }

    public string? TenMon { get; set; }

    /// <summary>
    /// Loại phát sinh: NhapHang, BanHang, HuyHang, DieuChinh
    /// </summary>
    public string LoaiPhatSinh { get; set; } = string.Empty;

    /// <summary>
    /// Số lượng thay đổi (dương = tăng, âm = giảm)
    /// </summary>
    public int SoLuongThayDoi { get; set; }

    /// <summary>
    /// Tồn kho trước khi thay đổi
    /// </summary>
    public int TonTruoc { get; set; }

    /// <summary>
    /// Tồn kho sau khi thay đổi
    /// </summary>
    public int TonSau { get; set; }

    public int? HoaDonBanId { get; set; }

    public int? HoaDonNhapId { get; set; }

    public string? GhiChu { get; set; }

    public DateTime ThoiGian { get; set; }

    public int? NguoiDungId { get; set; }

    public string? TenNguoiDung { get; set; }

    // === Computed properties ===

    /// <summary>
    /// Hiển thị số lượng thay đổi với dấu +/-
    /// </summary>
    public string SoLuongThayDoiHienThi => SoLuongThayDoi >= 0 
        ? $"+{SoLuongThayDoi}" 
        : SoLuongThayDoi.ToString();

    /// <summary>
    /// Hiển thị loại phát sinh với icon
    /// </summary>
    public string LoaiPhatSinhHienThi => LoaiPhatSinh switch
    {
        "NhapHang" => "📦 Nhập hàng",
        "BanHang" => "🛒 Bán hàng",
        "HuyHang" => "❌ Hủy hàng",
        "DieuChinh" => "⚙️ Điều chỉnh",
        _ => LoaiPhatSinh
    };

    /// <summary>
    /// Hiển thị thời gian định dạng
    /// </summary>
    public string ThoiGianHienThi => ThoiGian.ToString("dd/MM/yyyy HH:mm:ss");
}
