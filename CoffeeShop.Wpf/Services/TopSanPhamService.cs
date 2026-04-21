using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class TopSanPhamService : ITopSanPhamService
{
    private readonly IThongKeRepository _thongKeRepository;

    public TopSanPhamService(IThongKeRepository thongKeRepository)
    {
        _thongKeRepository = thongKeRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<ThongKeTopSanPhamDong>>> GetTopSanPhamBanChayAsync(
        DateTime fromDate,
        DateTime toDate,
        int topN,
        CancellationToken cancellationToken = default)
    {
        if (fromDate.Date > toDate.Date)
        {
            return ServiceResult<IReadOnlyList<ThongKeTopSanPhamDong>>.Fail("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        if (topN <= 0)
        {
            return ServiceResult<IReadOnlyList<ThongKeTopSanPhamDong>>.Fail("Top N phải lớn hơn 0.");
        }

        if (topN > 100)
        {
            return ServiceResult<IReadOnlyList<ThongKeTopSanPhamDong>>.Fail("Top N tối đa là 100.");
        }

        var data = await _thongKeRepository.GetTopSanPhamBanChayAsync(fromDate, toDate, topN, cancellationToken);
        return ServiceResult<IReadOnlyList<ThongKeTopSanPhamDong>>.Success(data, "Tải danh sách top sản phẩm bán chạy thành công.");
    }
}
