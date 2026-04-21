namespace CoffeeShop.Wpf.Models;

public sealed class CaTongKetModel
{
    public int CaLamViecId { get; init; }

    // === Số lượng hóa đơn ===
    public int SoHoaDon { get; init; }
    public int SoHoaDonHuy { get; init; }

    // === Doanh thu tổng ===
    public decimal TongDoanhThu { get; init; }

    // === Doanh thu theo hình thức thanh toán ===
    public decimal TienMatHeThong { get; init; }
    public decimal ChuyenKhoanHeThong { get; init; }
    public decimal TheHeThong { get; init; }
    public decimal ViDienTuHeThong { get; init; }

    // === Đối soát tiền mặt ===
    public decimal TienDauCa { get; init; }

    /// <summary>Tiền mặt dự kiến cuối ca = TienDauCa + TienMatHeThong</summary>
    public decimal TienMatDuKienCuoiCa => TienDauCa + TienMatHeThong;

    public decimal? TienMatThucDem { get; init; }
    public decimal? ChenhLechTienMat { get; init; }
    public string? GhiChuDoiSoat { get; init; }
}
