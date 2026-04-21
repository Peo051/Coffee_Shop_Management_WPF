using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<ServiceResult<int>> GhiLogAsync(
        int? nguoiDungId,
        string hanhDong,
        string? doiTuong,
        string? duLieuTomTat,
        string? mayTram = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hanhDong))
        {
            return ServiceResult<int>.Fail("Hành động nhật ký không được để trống.");
        }

        var hanhDongNormalized = hanhDong.Trim();
        if (hanhDongNormalized.Length > 120)
        {
            return ServiceResult<int>.Fail("Hành động nhật ký không vượt quá 120 ký tự.");
        }

        var logId = await _auditLogRepository.GhiLogAsync(
            nguoiDungId,
            hanhDongNormalized,
            doiTuong,
            duLieuTomTat,
            string.IsNullOrWhiteSpace(mayTram) ? Environment.MachineName : mayTram,
            cancellationToken);

        return ServiceResult<int>.Success(logId, "Ghi nhật ký thao tác thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<AuditLogDong>>> GetDanhSachLogAsync(
        DateTime fromDate,
        DateTime toDate,
        int topN = 200,
        CancellationToken cancellationToken = default)
    {
        if (fromDate.Date > toDate.Date)
        {
            return ServiceResult<IReadOnlyList<AuditLogDong>>.Fail("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        if (topN <= 0 || topN > 2000)
        {
            return ServiceResult<IReadOnlyList<AuditLogDong>>.Fail("Giới hạn log phải trong khoảng từ 1 đến 2000.");
        }

        var data = await _auditLogRepository.GetDanhSachLogAsync(fromDate, toDate, topN, cancellationToken);
        return ServiceResult<IReadOnlyList<AuditLogDong>>.Success(data, "Tải nhật ký thao tác thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<AuditLogDong>>> TimKiemLogAsync(
        DateTime fromDate,
        DateTime toDate,
        int? nguoiDungId,
        string? hanhDong,
        string? keyword,
        int topN = 200,
        CancellationToken cancellationToken = default)
    {
        if (fromDate.Date > toDate.Date)
        {
            return ServiceResult<IReadOnlyList<AuditLogDong>>.Fail("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        if (topN <= 0 || topN > 2000)
        {
            return ServiceResult<IReadOnlyList<AuditLogDong>>.Fail("Giới hạn log phải trong khoảng từ 1 đến 2000.");
        }

        if (nguoiDungId.HasValue && nguoiDungId <= 0)
        {
            return ServiceResult<IReadOnlyList<AuditLogDong>>.Fail("Mã người dùng lọc không hợp lệ.");
        }

        var data = await _auditLogRepository.TimKiemLogAsync(
            fromDate,
            toDate,
            nguoiDungId,
            hanhDong,
            keyword,
            topN,
            cancellationToken);

        return ServiceResult<IReadOnlyList<AuditLogDong>>.Success(data, "Tìm kiếm nhật ký thao tác thành công.");
    }
}

