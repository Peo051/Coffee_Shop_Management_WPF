namespace CoffeeShop.Wpf.Models;

public sealed class CaLamViec
{
    public int CaLamViecId { get; set; }

    public int NguoiDungId { get; set; }

    public string TenNhanVien { get; set; } = string.Empty;

    public DateTime ThoiGianMoCa { get; set; }

    public DateTime? ThoiGianDongCa { get; set; }

    public string TrangThaiCa { get; set; } = string.Empty;

    public string? GhiChu { get; set; }

    public int SoHoaDon { get; set; }

    public decimal TongDoanhThu { get; set; }

    // === Đối soát tiền ===

    /// <summary>Tiền mặt đầu ca (thu ngân nhập khi mở ca)</summary>
    public decimal TienDauCa { get; set; }

    /// <summary>Tiền mặt thực đếm cuối ca</summary>
    public decimal? TienMatThucDem { get; set; }

    /// <summary>Chênh lệch = TienMatThucDem - (TienDauCa + TienMatHeThong)</summary>
    public decimal? ChenhLechTienMat { get; set; }

    /// <summary>Ghi chú đối soát (bắt buộc nếu có chênh lệch)</summary>
    public string? GhiChuDoiSoat { get; set; }

    public string TrangThaiCaHienThi => TrangThaiCa switch
    {
        "DangMo" => "Đang mở",
        "DaDong" => "Đã đóng",
        _ => TrangThaiCa
    };
}
