namespace CoffeeShop.Wpf.Models;

public sealed class HoaDonBan
{
    public int HoaDonBanId { get; set; }

    public DateTime NgayBan { get; set; }

    public decimal TongTien { get; set; }

    public decimal GiamGia { get; set; }

    public decimal ThanhToan => TongTien - GiamGia;

    public int CreatedByUserId { get; set; }

    public int? BanId { get; set; }

    public int? CaLamViecId { get; set; }

    public int? KhachHangId { get; set; }

    public int? KhuyenMaiId { get; set; }

    public decimal SoTienGiam { get; set; }

    public int DiemCong { get; set; }

    public int DiemSuDung { get; set; }

    public decimal SoTienGiamTuDiem { get; set; }

    // === Thông tin thanh toán nâng cao ===

    /// <summary>Hình thức thanh toán: Tiền mặt, Chuyển khoản, Thẻ, Ví điện tử</summary>
    public string HinhThucThanhToan { get; set; } = "Tiền mặt";

    /// <summary>Trạng thái thanh toán: Đã thanh toán, Chưa thanh toán, Đã hủy</summary>
    public string TrangThaiThanhToan { get; set; } = "Đã thanh toán";

    /// <summary>Tiền khách đưa (chỉ áp dụng khi thanh toán tiền mặt)</summary>
    public decimal? TienKhachDua { get; set; }

    /// <summary>Tiền thối lại = TienKhachDua - ThanhToan</summary>
    public decimal? TienThoiLai { get; set; }

    /// <summary>Mã giao dịch (chuyển khoản, thẻ, ví điện tử)</summary>
    public string? MaGiaoDich { get; set; }

    /// <summary>Ghi chú thanh toán</summary>
    public string? GhiChuThanhToan { get; set; }

    /// <summary>Ghi chú hóa đơn</summary>
    public string? GhiChuHoaDon { get; set; }

    // === Thông tin hủy hóa đơn ===

    /// <summary>Lý do hủy hóa đơn</summary>
    public string? LyDoHuy { get; set; }

    /// <summary>Tên người thực hiện hủy</summary>
    public string? NguoiHuy { get; set; }

    /// <summary>Ngày hủy hóa đơn</summary>
    public DateTime? NgayHuy { get; set; }

    // === Số gọi món reset theo ngày ===

    /// <summary>Số thứ tự gọi món trong ngày (001, 002, ...)</summary>
    public int? SoThuTuGoiMon { get; set; }

    /// <summary>Ngày tương ứng của số thứ tự</summary>
    public DateTime? NgaySoThuTu { get; set; }

    /// <summary>Số gọi món hiển thị: ưu tiên SoThuTuGoiMon, fallback HoaDonBanId</summary>
    public string SoGoiMonHienThi => SoThuTuGoiMon.HasValue
        ? SoThuTuGoiMon.Value.ToString("D3")
        : HoaDonBanId.ToString("D3");

    // === Trạng thái pha chế ===

    /// <summary>Trạng thái pha chế: ChoPhaChe, DangPhaChe, DaHoanThanh, DaGiaoKhach, DaHuy</summary>
    public string TrangThaiPhaChe { get; set; } = TrangThaiPhaCheConst.ChoPhaChe;

    /// <summary>Thời gian bắt đầu pha chế</summary>
    public DateTime? ThoiGianBatDauPhaChe { get; set; }

    /// <summary>Thời gian hoàn thành pha chế</summary>
    public DateTime? ThoiGianHoanThanhPhaChe { get; set; }

    /// <summary>Thời gian giao khách</summary>
    public DateTime? ThoiGianGiaoKhach { get; set; }

    /// <summary>Trạng thái pha chế hiển thị tiếng Việt</summary>
    public string TrangThaiPhaCheHienThi => TrangThaiPhaCheConst.ToDisplayName(TrangThaiPhaChe);

    // === Hình thức phục vụ ===

    /// <summary>Hình thức phục vụ: UongTaiQuan, MangDi</summary>
    public string HinhThucPhucVu { get; set; } = HinhThucPhucVuConst.UongTaiQuan;

    /// <summary>Hình thức phục vụ hiển thị tiếng Việt</summary>
    public string HinhThucPhucVuHienThi => HinhThucPhucVuConst.ToDisplayName(HinhThucPhucVu);
}

