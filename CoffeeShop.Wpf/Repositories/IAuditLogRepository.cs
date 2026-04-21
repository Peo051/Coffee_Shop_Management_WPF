using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IAuditLogRepository
{
    Task<int> GhiLogAsync(
        int? nguoiDungId,
        string hanhDong,
        string? doiTuong,
        string? duLieuTomTat,
        string? mayTram,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDong>> GetDanhSachLogAsync(
        DateTime fromDate,
        DateTime toDate,
        int topN,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDong>> TimKiemLogAsync(
        DateTime fromDate,
        DateTime toDate,
        int? nguoiDungId,
        string? hanhDong,
        string? keyword,
        int topN,
        CancellationToken cancellationToken = default);
}

