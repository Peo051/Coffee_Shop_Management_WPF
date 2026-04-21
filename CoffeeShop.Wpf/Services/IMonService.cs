using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IMonService
{
    Task<IReadOnlyList<Mon>> SearchAsync(string? keyword, int? danhMucId, CancellationToken cancellationToken = default);

    Task<Mon?> GetByIdAsync(int monId, CancellationToken cancellationToken = default);

    Task<ServiceResult<Mon>> CreateAsync(
        string tenMon,
        int danhMucId,
        decimal donGia,
        int tonKhoBanDau,
        int tonKhoToiThieu,
        string hinhAnhPath,
        CancellationToken cancellationToken = default);
}
