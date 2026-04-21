using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IPhaCheService
{
    Task<ServiceResult<IReadOnlyList<PhaCheDonHangDong>>> GetDonCanPhaCheAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult> CapNhatTrangThaiAsync(PhaCheDonHangDong? don, string trangThaiMoi, CancellationToken cancellationToken = default);
}
