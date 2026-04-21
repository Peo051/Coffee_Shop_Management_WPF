using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface INguyenLieuRepository
{
    /// <summary>
    /// Lấy tất cả nguyên liệu
    /// </summary>
    Task<IReadOnlyList<NguyenLieu>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy nguyên liệu theo ID
    /// </summary>
    Task<NguyenLieu?> GetByIdAsync(
        int nguyenLieuId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm kiếm nguyên liệu theo tên
    /// </summary>
    Task<IReadOnlyList<NguyenLieu>> SearchAsync(
        string? keyword,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo nguyên liệu mới
    /// </summary>
    Task<int> CreateAsync(
        NguyenLieu nguyenLieu,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin nguyên liệu
    /// </summary>
    Task UpdateAsync(
        NguyenLieu nguyenLieu,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật tồn kho nguyên liệu
    /// </summary>
    Task CapNhatTonKhoAsync(
        int nguyenLieuId,
        decimal tonKhoMoi,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kích hoạt/vô hiệu hóa nguyên liệu
    /// </summary>
    Task SetActiveAsync(
        int nguyenLieuId,
        bool isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tồn kho nguyên liệu (trong transaction)
    /// </summary>
    Task<decimal> GetTonKhoAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        Microsoft.Data.SqlClient.SqlTransaction transaction,
        int nguyenLieuId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Trừ tồn kho nguyên liệu (trong transaction)
    /// </summary>
    Task<int> TruTonKhoAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        Microsoft.Data.SqlClient.SqlTransaction transaction,
        int nguyenLieuId,
        decimal soLuongTru,
        CancellationToken cancellationToken = default);
}
