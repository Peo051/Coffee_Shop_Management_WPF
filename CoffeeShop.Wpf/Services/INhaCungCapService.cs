using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface INhaCungCapService
{
    Task<IReadOnlyList<NhaCungCap>> GetAllAsync(string? keyword = null, CancellationToken cancellationToken = default);

    Task<ServiceResult<NhaCungCap>> CreateAsync(
        string tenNhaCungCap,
        string? soDienThoai,
        string? diaChi,
        CancellationToken cancellationToken = default);
}
