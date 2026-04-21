using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IKhoRepository
{
    Task<IReadOnlyList<TrangThaiSanPhamDong>> GetTrangThaiSanPhamAsync(
        string? keyword,
        int? danhMucId,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CanhBaoTonKhoThapDong>> GetCanhBaoTonKhoThapAsync(
        string? keyword,
        int? danhMucId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateTrangThaiKinhDoanhAsync(
        int monId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateMucCanhBaoTonKhoAsync(
        int monId,
        int mucCanhBaoTonKho,
        CancellationToken cancellationToken = default);
}

