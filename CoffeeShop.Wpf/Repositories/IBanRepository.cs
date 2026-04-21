using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IBanRepository
{
    Task<IReadOnlyList<Ban>> GetDanhSachBanAsync(
        string? keyword,
        int? khuVucId,
        bool? isActive,
        string? trangThaiBan,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KhuVuc>> GetDanhSachKhuVucAsync(
        bool onlyActive = true,
        CancellationToken cancellationToken = default);

    Task<Ban?> GetByIdAsync(int banId, CancellationToken cancellationToken = default);

    Task<int> TaoBanAsync(
        int khuVucId,
        string tenBan,
        string trangThaiBan,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<bool> CapNhatTrangThaiBanAsync(
        int banId,
        string trangThaiBan,
        CancellationToken cancellationToken = default);

    Task<bool> ChuyenBanAsync(
        int fromBanId,
        int toBanId,
        CancellationToken cancellationToken = default);

    Task<bool> GopBanAsync(
        int sourceBanId,
        int targetBanId,
        CancellationToken cancellationToken = default);
}

