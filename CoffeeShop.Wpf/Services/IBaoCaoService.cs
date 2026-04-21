using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IBaoCaoService
{
    Task<ServiceResult<BaoCaoTongHopModel>> GetTongHopAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
