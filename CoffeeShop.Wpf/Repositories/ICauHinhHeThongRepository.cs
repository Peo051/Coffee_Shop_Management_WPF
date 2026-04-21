using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface ICauHinhHeThongRepository
{
    Task<CauHinhHeThong?> GetCauHinhAsync(CancellationToken cancellationToken = default);

    Task<int> LuuCauHinhAsync(CauHinhHeThong cauHinh, CancellationToken cancellationToken = default);
}

