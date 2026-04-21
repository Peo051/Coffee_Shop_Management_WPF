using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IPhaCheRepository
{
    /// <summary>
    /// Lấy danh sách đơn cần pha chế (ChoPhaChe, DangPhaChe, DaHoanThanh).
    /// </summary>
    Task<IReadOnlyList<PhaCheDonHangDong>> GetDonCanPhaCheAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật trạng thái pha chế và thời gian tương ứng.
    /// </summary>
    Task<bool> CapNhatTrangThaiPhaCheAsync(int hoaDonBanId, string trangThaiMoi, CancellationToken cancellationToken = default);
}
