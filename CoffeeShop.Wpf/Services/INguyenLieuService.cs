using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface INguyenLieuService
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
    Task<ServiceResult> CreateAsync(
        string tenNguyenLieu,
        string donViTinh,
        decimal tonKho,
        decimal tonKhoToiThieu,
        decimal donGiaNhap,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin nguyên liệu
    /// </summary>
    Task<ServiceResult> UpdateAsync(
        int nguyenLieuId,
        string tenNguyenLieu,
        string donViTinh,
        decimal tonKho,
        decimal tonKhoToiThieu,
        decimal donGiaNhap,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật tồn kho nguyên liệu
    /// </summary>
    Task<ServiceResult> CapNhatTonKhoAsync(
        int nguyenLieuId,
        decimal tonKhoMoi,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kích hoạt/vô hiệu hóa nguyên liệu
    /// </summary>
    Task<ServiceResult> SetActiveAsync(
        int nguyenLieuId,
        bool isActive,
        CancellationToken cancellationToken = default);
}
