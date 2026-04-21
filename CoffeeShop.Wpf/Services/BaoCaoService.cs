using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class BaoCaoService : IBaoCaoService
{
    private readonly IBaoCaoRepository _baoCaoRepository;

    public BaoCaoService(IBaoCaoRepository baoCaoRepository)
    {
        _baoCaoRepository = baoCaoRepository;
    }

    public async Task<ServiceResult<BaoCaoTongHopModel>> GetTongHopAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        if (fromDate.Date > toDate.Date)
        {
            return ServiceResult<BaoCaoTongHopModel>.Fail("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        if ((toDate.Date - fromDate.Date).TotalDays > 366)
        {
            return ServiceResult<BaoCaoTongHopModel>.Fail("Khoảng thời gian báo cáo tối đa là 366 ngày.");
        }

        var baoCaoDonGian = await _baoCaoRepository.GetBaoCaoDonGianAsync(fromDate, toDate, cancellationToken);
        var baoCaoNangCao = await _baoCaoRepository.GetBaoCaoNangCaoAsync(fromDate, toDate, cancellationToken);

        var summary = new BaoCaoTongHopModel
        {
            BaoCaoDonGian = baoCaoDonGian,
            BaoCaoNangCao = baoCaoNangCao,
            TongSoHoaDon = baoCaoDonGian.Sum(x => x.SoHoaDon),
            TongDoanhThuThuan = baoCaoDonGian.Sum(x => x.DoanhThuThuan),
            TongSoLuongBan = baoCaoNangCao.Sum(x => x.SoLuongBan),
            TongDoanhThuSanPham = baoCaoNangCao.Sum(x => x.DoanhThuGop)
        };

        return ServiceResult<BaoCaoTongHopModel>.Success(summary, "Tải báo cáo thành công.");
    }
}
