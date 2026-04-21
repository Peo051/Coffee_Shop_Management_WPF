using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IKhachHangService
{
    Task<ServiceResult<IReadOnlyList<KhachHang>>> GetDanhSachKhachHangAsync(
        string? keyword,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<KhachHang>> TaoKhachHangAsync(
        KhachHang khachHang,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> CapNhatKhachHangAsync(
        KhachHang khachHang,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<KhachHang>>> TimKiemKhachHangAsync(
        string? keyword,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<int>> CongDiemSauHoaDonAsync(
        int khachHangId,
        decimal giaTriThanhToan,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<KhachHang>> GetByIdAsync(
        int khachHangId,
        CancellationToken cancellationToken = default);
}

