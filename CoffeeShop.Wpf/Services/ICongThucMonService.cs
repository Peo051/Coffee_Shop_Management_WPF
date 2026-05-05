using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface ICongThucMonService
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
    /// Thêm nguyên liệu vào công thức món
    /// </summary>
    Task<ServiceResult> CreateAsync(
        int monId,
        int nguyenLieuId,
        decimal dinhLuong,
        string? ghiChu = null,
        string? cacBuocThucHien = null,
        decimal tyLeHaoHut = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật định lượng nguyên liệu trong công thức
    /// </summary>
    Task<ServiceResult> UpdateAsync(
        int congThucMonId,
        decimal dinhLuong,
        string? ghiChu = null,
        string? cacBuocThucHien = null,
        decimal tyLeHaoHut = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa nguyên liệu khỏi công thức món
    /// </summary>
    Task<ServiceResult> DeleteAsync(
        int congThucMonId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy lịch sử cập nhật công thức theo món
    /// </summary>
    Task<IReadOnlyList<LichSuCapNhatCongThuc>> GetLichSuCapNhatAsync(
        int monId,
        CancellationToken cancellationToken = default);
}
