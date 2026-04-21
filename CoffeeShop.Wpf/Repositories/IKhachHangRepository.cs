using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IKhachHangRepository
{
    Task<IReadOnlyList<KhachHang>> GetDanhSachKhachHangAsync(
        string? keyword,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<int> TaoKhachHangAsync(KhachHang khachHang, CancellationToken cancellationToken = default);

    Task<bool> CapNhatKhachHangAsync(KhachHang khachHang, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KhachHang>> TimKiemKhachHangAsync(
        string? keyword,
        CancellationToken cancellationToken = default);

    Task<bool> CongDiemAsync(int khachHangId, int diemCong, CancellationToken cancellationToken = default);

    Task<KhachHang?> GetByIdAsync(int khachHangId, CancellationToken cancellationToken = default);
}

