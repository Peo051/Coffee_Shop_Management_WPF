using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IMonRepository
{
    Task<IReadOnlyList<Mon>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Mon>> SearchAsync(string keyword, int? danhMucId, CancellationToken cancellationToken = default);

    Task<Mon?> GetByIdAsync(int monId, CancellationToken cancellationToken = default);

    Task<int> InsertAsync(Mon mon, CancellationToken cancellationToken = default);

    Task UpdateAsync(Mon mon, CancellationToken cancellationToken = default);

    Task UpdateTonKhoAsync(int monId, int soLuongThayDoi, CancellationToken cancellationToken = default);
}
