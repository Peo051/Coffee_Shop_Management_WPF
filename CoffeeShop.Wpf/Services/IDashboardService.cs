using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IDashboardService
{
    Task<ServiceResult<DashboardTongQuanModel>> GetTongQuanAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<DashboardDoanhThuNgayDong>>> GetDoanhThu7NgayAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<ThongKeTopSanPhamDong>>> GetTopSanPhamHomNayAsync(
        int topN,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CanhBaoTonKhoThapDong>>> GetTonKhoThapAsync(
        int topN,
        CancellationToken cancellationToken = default);
}

