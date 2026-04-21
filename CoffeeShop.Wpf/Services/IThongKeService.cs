using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IThongKeService
{
    Task<ServiceResult<ThongKeTongHopModel>> GetTongHopAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
