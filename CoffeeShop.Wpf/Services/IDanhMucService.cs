using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IDanhMucService
{
    Task<IReadOnlyList<DanhMuc>> GetAllAsync(string? keyword = null, CancellationToken cancellationToken = default);

    Task<ServiceResult<DanhMuc>> CreateAsync(string tenDanhMuc, string? moTa, CancellationToken cancellationToken = default);
}
