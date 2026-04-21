namespace CoffeeShop.Wpf.Models;

public sealed class HoaDonBanInModel
{
    public int HoaDonBanId { get; set; }

    public DateTime NgayBan { get; set; }

    public string TenNhanVien { get; set; } = string.Empty;

    public string? TenBan { get; set; }

    public string? TenKhuVuc { get; set; }

    public decimal TongTien { get; set; }

    public decimal GiamGia { get; set; }

    public decimal ThanhToan { get; set; }

    public int? CaLamViecId { get; set; }

    // === Thông tin khách hàng ===
    public string? TenKhachHang { get; set; }
    public string? SoDienThoaiKhachHang { get; set; }

    // === Thông tin thanh toán ===
    public string HinhThucThanhToan { get; set; } = "Tiền mặt";
    public string TrangThaiThanhToan { get; set; } = "Đã thanh toán";
    public decimal? TienKhachDua { get; set; }
    public decimal? TienThoiLai { get; set; }
    public string? MaGiaoDich { get; set; }
    public string? GhiChuThanhToan { get; set; }
    public string? GhiChuHoaDon { get; set; }

    public IReadOnlyList<HoaDonBanInChiTietDong> ChiTiet { get; set; } = [];

    // === Số gọi món reset theo ngày ===
    public int? SoThuTuGoiMon { get; set; }
    public DateTime? NgaySoThuTu { get; set; }

    /// <summary>Số gọi món hiển thị: ưu tiên SoThuTuGoiMon, fallback HoaDonBanId</summary>
    public string SoGoiMonHienThi => SoThuTuGoiMon.HasValue
        ? SoThuTuGoiMon.Value.ToString("D3")
        : HoaDonBanId.ToString("D3");

    public string MaHoaDonHienThi => $"HD{HoaDonBanId:D5}";

    // === Hình thức phục vụ ===
    public string HinhThucPhucVu { get; set; } = HinhThucPhucVuConst.UongTaiQuan;
    public string HinhThucPhucVuHienThi => HinhThucPhucVuConst.ToDisplayName(HinhThucPhucVu);

    // === Điểm tích lũy ===
    public int DiemSuDung { get; set; }
    public decimal SoTienGiamTuDiem { get; set; }
    public int DiemCong { get; set; }
}

