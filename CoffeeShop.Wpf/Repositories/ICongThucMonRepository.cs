using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface ICongThucMonRepository
{
        Task<IReadOnlyList<CongThucMon>> GetByMonAsync(
        int monId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CongThucMon>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<CongThucMon?> GetByIdAsync(
        int congThucMonId,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(
        int monId,
        int nguyenLieuId,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        CongThucMon congThucMon,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        CongThucMon congThucMon,
        CancellationToken cancellationToken = default);


    Task SetActiveAsync(
        int congThucMonId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CongThucMon>> GetByMonAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        Microsoft.Data.SqlClient.SqlTransaction transaction,
        int monId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task SaveLichSuCapNhatAsync(
        int monId,
        string nguoiCapNhat,
        string noiDungCapNhat,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LichSuCapNhatCongThuc>> GetLichSuCapNhatAsync(
        int monId,
        CancellationToken cancellationToken = default);
}
