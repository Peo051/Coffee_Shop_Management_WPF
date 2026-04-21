using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface ICongThucMonRepository
{
    /// <summary>
    /// Lấy công thức theo món
    /// </summary>
    Task<IReadOnlyList<CongThucMon>> GetByMonAsync(
        int monId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả công thức
    /// </summary>
    Task<IReadOnlyList<CongThucMon>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy công thức theo ID
    /// </summary>
    Task<CongThucMon?> GetByIdAsync(
        int congThucMonId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra món đã có nguyên liệu này chưa
    /// </summary>
    Task<bool> ExistsAsync(
        int monId,
        int nguyenLieuId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo công thức mới
    /// </summary>
    Task<int> CreateAsync(
        CongThucMon congThucMon,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật công thức
    /// </summary>
    Task UpdateAsync(
        CongThucMon congThucMon,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa công thức (set IsActive = false)
    /// </summary>
    Task SetActiveAsync(
        int congThucMonId,
        bool isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy công thức theo món (trong transaction)
    /// </summary>
    Task<IReadOnlyList<CongThucMon>> GetByMonAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        Microsoft.Data.SqlClient.SqlTransaction transaction,
        int monId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);
}
