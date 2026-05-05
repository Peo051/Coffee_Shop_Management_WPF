using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface INguyenLieuRepository
{

    Task<IReadOnlyList<NguyenLieu>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<NguyenLieu?> GetByIdAsync(
        int nguyenLieuId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NguyenLieu>> SearchAsync(
        string? keyword,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        NguyenLieu nguyenLieu,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        NguyenLieu nguyenLieu,
        CancellationToken cancellationToken = default);

    Task CapNhatTonKhoAsync(
        int nguyenLieuId,
        decimal tonKhoMoi,
        CancellationToken cancellationToken = default);

    Task SetActiveAsync(
        int nguyenLieuId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<decimal> GetTonKhoAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        Microsoft.Data.SqlClient.SqlTransaction transaction,
        int nguyenLieuId,
        CancellationToken cancellationToken = default);

    Task<int> TruTonKhoAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        Microsoft.Data.SqlClient.SqlTransaction transaction,
        int nguyenLieuId,
        decimal soLuongTru,
        CancellationToken cancellationToken = default);
}
