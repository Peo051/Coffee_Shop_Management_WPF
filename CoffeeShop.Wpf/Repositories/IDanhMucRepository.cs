using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IDanhMucRepository
{
    Task<IReadOnlyList<DanhMuc>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<DanhMuc?> GetByIdAsync(int danhMucId, CancellationToken cancellationToken = default);

    Task<int> InsertAsync(DanhMuc danhMuc, CancellationToken cancellationToken = default);

    Task UpdateAsync(DanhMuc danhMuc, CancellationToken cancellationToken = default);

    Task DeleteAsync(int danhMucId, CancellationToken cancellationToken = default);
}
