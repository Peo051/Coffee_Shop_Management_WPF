using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IHoaDonBanRepository
{
    Task<int> CreateAsync(
        HoaDonBan hoaDonBan,
        IReadOnlyList<ChiTietHoaDonBan> chiTietHoaDonBans,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HoaDonBan>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy hóa đơn đã thanh toán trong khoảng thời gian (dùng cho thống kê).
    /// Loại trừ hóa đơn đã hủy.
    /// </summary>
    Task<IReadOnlyList<HoaDonBan>> GetPaidByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
