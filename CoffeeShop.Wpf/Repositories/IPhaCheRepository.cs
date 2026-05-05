using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IPhaCheRepository
{
    Task<IReadOnlyList<PhaCheDonHangDong>> GetDonCanPhaCheAsync(CancellationToken cancellationToken = default);

    Task<bool> CapNhatTrangThaiPhaCheAsync(int hoaDonBanId, string trangThaiMoi, CancellationToken cancellationToken = default);
}
