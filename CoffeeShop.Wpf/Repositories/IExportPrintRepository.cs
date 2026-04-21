using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IExportPrintRepository
{
    Task<IReadOnlyList<BaoCaoDonGianDong>> GetDuLieuBaoCaoDonGianAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BaoCaoNangCaoDong>> GetDuLieuBaoCaoNangCaoAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<HoaDonBanInModel?> GetDuLieuHoaDonBanAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ThongKeDoanhThu>> GetDuLieuThongKeDoanhThuAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}

