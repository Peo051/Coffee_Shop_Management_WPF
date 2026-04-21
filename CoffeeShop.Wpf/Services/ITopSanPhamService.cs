using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface ITopSanPhamService
{
    Task<ServiceResult<IReadOnlyList<ThongKeTopSanPhamDong>>> GetTopSanPhamBanChayAsync(
        DateTime fromDate,
        DateTime toDate,
        int topN,
        CancellationToken cancellationToken = default);
}

