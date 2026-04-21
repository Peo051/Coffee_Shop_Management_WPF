using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IHoaDonNhapService
{
    Task<ServiceResult<HoaDonNhap>> CreateAsync(
        int nhaCungCapId,
        int createdByUserId,
        string? ghiChu,
        IReadOnlyList<HoaDonNhapChiTietInputModel> chiTietInputs,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HoaDonNhap>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
