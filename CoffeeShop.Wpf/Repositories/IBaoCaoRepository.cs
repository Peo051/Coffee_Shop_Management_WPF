using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IBaoCaoRepository
{
    Task<IReadOnlyList<BaoCaoDonGianDong>> GetBaoCaoDonGianAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BaoCaoNangCaoDong>> GetBaoCaoNangCaoAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
