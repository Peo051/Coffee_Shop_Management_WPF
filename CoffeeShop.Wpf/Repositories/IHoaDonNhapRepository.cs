using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IHoaDonNhapRepository
{
    Task<int> CreateAsync(
        HoaDonNhap hoaDonNhap,
        IReadOnlyList<ChiTietHoaDonNhap> chiTietHoaDonNhaps,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HoaDonNhap>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
