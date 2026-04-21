using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface ILichSuHoaDonService
{
    Task<ServiceResult<IReadOnlyList<LichSuHoaDonDong>>> GetDanhSachHoaDonAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<LichSuHoaDonDong>>> TimKiemHoaDonAsync(
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

    Task<ServiceResult<IReadOnlyList<LichSuHoaDonChiTietDong>>> GetChiTietHoaDonAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default);

    /// <summary>Hủy hóa đơn (cập nhật trạng thái, lưu lý do, không xóa)</summary>
    Task<ServiceResult> HuyHoaDonAsync(
        int hoaDonBanId,
        string lyDoHuy,
        string? nguoiHuy,
        CancellationToken cancellationToken = default);
}

