using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IBanService
{
    Task<ServiceResult<IReadOnlyList<Ban>>> GetDanhSachBanAsync(
        string? keyword,
        int? khuVucId,
        bool? isActive,
        string? trangThaiBan,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<KhuVuc>>> GetDanhSachKhuVucAsync(
        bool onlyActive = true,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<Ban>> TaoBanAsync(
        int khuVucId,
        string tenBan,
        string trangThaiBan,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> CapNhatTrangThaiBanAsync(
        int banId,
        string trangThaiBan,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> ChuyenBanAsync(
        int fromBanId,
        int toBanId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> GopBanAsync(
        int sourceBanId,
        int targetBanId,
        CancellationToken cancellationToken = default);
}

