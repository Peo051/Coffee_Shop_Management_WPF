using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface ICauHinhHeThongService
{
    Task<ServiceResult<CauHinhHeThong>> GetCauHinhAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult> LuuCauHinhAsync(CauHinhHeThong cauHinh, CancellationToken cancellationToken = default);
}

