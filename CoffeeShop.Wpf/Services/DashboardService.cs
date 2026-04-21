using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<ServiceResult<DashboardTongQuanModel>> GetTongQuanAsync(CancellationToken cancellationToken = default)
    {
        var data = await _dashboardRepository.GetTongQuanHomNayAsync(cancellationToken);
        return ServiceResult<DashboardTongQuanModel>.Success(data, "Tải dữ liệu tổng quan thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<DashboardDoanhThuNgayDong>>> GetDoanhThu7NgayAsync(CancellationToken cancellationToken = default)
    {
        var data = await _dashboardRepository.GetDoanhThu7NgayAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<DashboardDoanhThuNgayDong>>.Success(data, "Tải doanh thu 7 ngày thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<ThongKeTopSanPhamDong>>> GetTopSanPhamHomNayAsync(
        int topN,
        CancellationToken cancellationToken = default)
    {
        if (topN <= 0 || topN > 100)
        {
            return ServiceResult<IReadOnlyList<ThongKeTopSanPhamDong>>.Fail("Top N phải trong khoảng từ 1 đến 100.");
        }

        var data = await _dashboardRepository.GetTopSanPhamHomNayAsync(topN, cancellationToken);
        return ServiceResult<IReadOnlyList<ThongKeTopSanPhamDong>>.Success(data, "Tải top sản phẩm hôm nay thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<CanhBaoTonKhoThapDong>>> GetTonKhoThapAsync(
        int topN,
        CancellationToken cancellationToken = default)
    {
        if (topN <= 0 || topN > 200)
        {
            return ServiceResult<IReadOnlyList<CanhBaoTonKhoThapDong>>.Fail("Giới hạn tồn kho thấp phải trong khoảng từ 1 đến 200.");
        }

        var data = await _dashboardRepository.GetSanPhamTonKhoThapAsync(topN, cancellationToken);
        return ServiceResult<IReadOnlyList<CanhBaoTonKhoThapDong>>.Success(data, "Tải danh sách tồn kho thấp thành công.");
    }
}

