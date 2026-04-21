using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IKhuyenMaiRepository
{
    Task<IReadOnlyList<KhuyenMai>> GetDanhSachKhuyenMaiAsync(
        string? keyword,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<int> TaoKhuyenMaiAsync(KhuyenMai khuyenMai, CancellationToken cancellationToken = default);

    Task<bool> CapNhatKhuyenMaiAsync(KhuyenMai khuyenMai, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KhuyenMai>> GetKhuyenMaiHieuLucAsync(
        DateTime thoiDiem,
        CancellationToken cancellationToken = default);

    Task<KhuyenMai?> GetByIdAsync(int khuyenMaiId, CancellationToken cancellationToken = default);
}

