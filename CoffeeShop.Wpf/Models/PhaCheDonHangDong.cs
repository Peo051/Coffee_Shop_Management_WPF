namespace CoffeeShop.Wpf.Models;

/// <summary>
/// Dòng hiển thị trên màn hình Quầy pha chế.
/// </summary>
public sealed class PhaCheDonHangDong
{
    public int HoaDonBanId { get; set; }

    /// <summary>Mã hóa đơn hiển thị: HD00042</summary>
    public string MaHoaDonHienThi => $"HD{HoaDonBanId:D5}";

    /// <summary>Số gọi món hiển thị (nếu có)</summary>
    public int? SoThuTuGoiMon { get; set; }

    public string SoGoiMonHienThi => SoThuTuGoiMon.HasValue
        ? SoThuTuGoiMon.Value.ToString("D3")
        : HoaDonBanId.ToString("D3");

    public DateTime NgayBan { get; set; }

    public string TenNhanVien { get; set; } = string.Empty;

    public string TenKhachHang { get; set; } = "Khách lẻ";

    public string? TenBan { get; set; }

    public string? TenKhuVuc { get; set; }

    /// <summary>Vị trí hiển thị: "Bàn 3 / Tầng 1" hoặc "N/A"</summary>
    public string ViTriHienThi =>
        $"{(string.IsNullOrWhiteSpace(TenBan) ? "N/A" : TenBan)} / {(string.IsNullOrWhiteSpace(TenKhuVuc) ? "N/A" : TenKhuVuc)}";

    public decimal ThanhToan { get; set; }

    public string TrangThaiPhaChe { get; set; } = TrangThaiPhaCheConst.ChoPhaChe;

    public string TrangThaiPhaCheHienThi => TrangThaiPhaCheConst.ToDisplayName(TrangThaiPhaChe);

    /// <summary>Tóm tắt đơn: "Trà đào x1, Bạc xỉu x2"</summary>
    public string DanhSachMonTomTat { get; set; } = string.Empty;

    public DateTime? ThoiGianBatDauPhaChe { get; set; }

    public DateTime? ThoiGianHoanThanhPhaChe { get; set; }

    public DateTime? ThoiGianGiaoKhach { get; set; }
}
