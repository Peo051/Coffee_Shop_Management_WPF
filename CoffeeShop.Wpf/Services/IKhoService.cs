using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IKhoService
{
    Task<ServiceResult<IReadOnlyList<TrangThaiSanPhamDong>>> GetTrangThaiSanPhamAsync(
        string? keyword,
        int? danhMucId,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CanhBaoTonKhoThapDong>>> GetCanhBaoTonKhoThapAsync(
        string? keyword,
        int? danhMucId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> CapNhatTrangThaiKinhDoanhAsync(
        int monId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> CapNhatMucCanhBaoTonKhoAsync(
        int monId,
        int mucCanhBaoTonKho,
        CancellationToken cancellationToken = default);
}

