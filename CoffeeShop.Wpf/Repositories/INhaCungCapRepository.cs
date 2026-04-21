using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface INhaCungCapRepository
{
    Task<IReadOnlyList<NhaCungCap>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<NhaCungCap?> GetByIdAsync(int nhaCungCapId, CancellationToken cancellationToken = default);

    Task<int> InsertAsync(NhaCungCap nhaCungCap, CancellationToken cancellationToken = default);

    Task UpdateAsync(NhaCungCap nhaCungCap, CancellationToken cancellationToken = default);

    Task DeleteAsync(int nhaCungCapId, CancellationToken cancellationToken = default);
}
