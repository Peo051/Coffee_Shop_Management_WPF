using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface ILichSuHoaDonRepository
{
    Task<IReadOnlyList<LichSuHoaDonDong>> GetDanhSachHoaDonAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LichSuHoaDonDong>> TimKiemHoaDonAsync(
        DateTime fromDate,
        DateTime toDate,
        int? hoaDonBanId,
        int? createdByUserId,
        int? banId,
        int? caLamViecId,
        string? tenKhachHang = null,
        string? soDienThoai = null,
        string? hinhThucThanhToan = null,
        string? trangThaiThanhToan = null,
        string? trangThaiPhaChe = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LichSuHoaDonChiTietDong>> GetChiTietHoaDonAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default);

    /// <summary>Hủy hóa đơn (cập nhật trạng thái, không xóa)</summary>
    Task<bool> HuyHoaDonAsync(
        int hoaDonBanId,
        string lyDoHuy,
        string? nguoiHuy,
        CancellationToken cancellationToken = default);
}

