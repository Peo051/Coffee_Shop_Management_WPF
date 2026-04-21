using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IThongKeRepository
{
    Task<IReadOnlyList<ThongKeDoanhThu>> GetDoanhThuTheoNgayAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ThongKeTopSanPhamDong>> GetTopSanPhamBanChayAsync(
        DateTime fromDate,
        DateTime toDate,
        int topN,
        CancellationToken cancellationToken = default);

    /// <summary>Thống kê doanh thu theo hình thức thanh toán</summary>
    Task<IReadOnlyList<ThongKeDoanhThuTheoHTTT>> GetDoanhThuTheoHTTTAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
