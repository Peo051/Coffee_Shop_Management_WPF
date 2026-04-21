using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IDashboardRepository
{
    Task<DashboardTongQuanModel> GetTongQuanHomNayAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DashboardDoanhThuNgayDong>> GetDoanhThu7NgayAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ThongKeTopSanPhamDong>> GetTopSanPhamHomNayAsync(
        int topN,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CanhBaoTonKhoThapDong>> GetSanPhamTonKhoThapAsync(
        int topN,
        CancellationToken cancellationToken = default);

    Task<int> GetSoHoaDonHomNayAsync(CancellationToken cancellationToken = default);

    Task<int> GetSoSanPhamDangKinhDoanhAsync(CancellationToken cancellationToken = default);
}

