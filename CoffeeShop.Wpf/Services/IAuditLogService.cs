using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IAuditLogService
{
    Task<ServiceResult<int>> GhiLogAsync(
        int? nguoiDungId,
        string hanhDong,
        string? doiTuong,
        string? duLieuTomTat,
        string? mayTram = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<AuditLogDong>>> GetDanhSachLogAsync(
        DateTime fromDate,
        DateTime toDate,
        int topN = 200,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<AuditLogDong>>> TimKiemLogAsync(
        DateTime fromDate,
        DateTime toDate,
        int? nguoiDungId,
        string? hanhDong,
        string? keyword,
        int topN = 200,
        CancellationToken cancellationToken = default);
}

