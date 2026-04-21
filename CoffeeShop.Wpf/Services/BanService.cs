using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class BanService : IBanService
{
    private static readonly HashSet<string> AllowedTrangThaiBan =
    [
        TrangThaiBanConst.Trong,
        TrangThaiBanConst.DangCoKhach,
        TrangThaiBanConst.TamKhoa
    ];

    private readonly IBanRepository _banRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly SessionService _sessionService;

    public BanService(
        IBanRepository banRepository,
        IAuditLogService auditLogService,
        SessionService sessionService)
    {
        _banRepository = banRepository;
        _auditLogService = auditLogService;
        _sessionService = sessionService;
    }

    public async Task<ServiceResult<IReadOnlyList<Ban>>> GetDanhSachBanAsync(
        string? keyword,
        int? khuVucId,
        bool? isActive,
        string? trangThaiBan,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(trangThaiBan) && !AllowedTrangThaiBan.Contains(trangThaiBan))
        {
            return ServiceResult<IReadOnlyList<Ban>>.Fail("Trạng thái bàn không hợp lệ.");
        }

        var data = await _banRepository.GetDanhSachBanAsync(keyword, khuVucId, isActive, trangThaiBan, cancellationToken);
        return ServiceResult<IReadOnlyList<Ban>>.Success(data, "Tải danh sách bàn thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<KhuVuc>>> GetDanhSachKhuVucAsync(
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var data = await _banRepository.GetDanhSachKhuVucAsync(onlyActive, cancellationToken);
        return ServiceResult<IReadOnlyList<KhuVuc>>.Success(data, "Tải danh sách khu vực thành công.");
    }

    public async Task<ServiceResult<Ban>> TaoBanAsync(
        int khuVucId,
        string tenBan,
        string trangThaiBan,
        CancellationToken cancellationToken = default)
    {
        var tenBanNormalized = tenBan.Trim();
        if (khuVucId <= 0)
        {
            return ServiceResult<Ban>.Fail("Khu vực không hợp lệ.");
        }

        if (string.IsNullOrWhiteSpace(tenBanNormalized))
        {
            return ServiceResult<Ban>.Fail("Tên bàn không được để trống.");
        }

        if (tenBanNormalized.Length > 100)
        {
            return ServiceResult<Ban>.Fail("Tên bàn không được vượt quá 100 ký tự.");
        }

        if (!AllowedTrangThaiBan.Contains(trangThaiBan))
        {
            return ServiceResult<Ban>.Fail("Trạng thái bàn không hợp lệ.");
        }

        var newId = await _banRepository.TaoBanAsync(khuVucId, tenBanNormalized, trangThaiBan, true, cancellationToken);
        var created = await _banRepository.GetByIdAsync(newId, cancellationToken);
        if (created is null)
        {
            return ServiceResult<Ban>.Fail("Không thể tải lại bàn vừa tạo.");
        }

        return ServiceResult<Ban>.Success(created, "Tạo bàn mới thành công.");
    }

    public async Task<ServiceResult> CapNhatTrangThaiBanAsync(
        int banId,
        string trangThaiBan,
        CancellationToken cancellationToken = default)
    {
        if (banId <= 0)
        {
            return ServiceResult.Fail("Mã bàn không hợp lệ.");
        }

        if (!AllowedTrangThaiBan.Contains(trangThaiBan))
        {
            return ServiceResult.Fail("Trạng thái bàn không hợp lệ.");
        }

        var updated = await _banRepository.CapNhatTrangThaiBanAsync(banId, trangThaiBan, cancellationToken);
        if (!updated)
        {
            return ServiceResult.Fail("Không tìm thấy bàn để cập nhật trạng thái.");
        }

        _ = TryWriteAuditAsync(
            "Đổi trạng thái bàn",
            "Ban",
            $"Bàn #{banId} chuyển trạng thái thành {trangThaiBan}.",
            cancellationToken);

        return ServiceResult.Success("Cập nhật trạng thái bàn thành công.");
    }

    public async Task<ServiceResult> ChuyenBanAsync(
        int fromBanId,
        int toBanId,
        CancellationToken cancellationToken = default)
    {
        if (fromBanId <= 0 || toBanId <= 0 || fromBanId == toBanId)
        {
            return ServiceResult.Fail("Thông tin bàn chuyển không hợp lệ.");
        }

        var success = await _banRepository.ChuyenBanAsync(fromBanId, toBanId, cancellationToken);
        if (!success)
        {
            return ServiceResult.Fail("Chuyển bàn không thành công. Vui lòng kiểm tra trạng thái bàn.");
        }

        _ = TryWriteAuditAsync(
            "Chuyển bàn",
            "Ban",
            $"Chuyển từ bàn #{fromBanId} sang bàn #{toBanId}.",
            cancellationToken);

        return ServiceResult.Success("Chuyển bàn thành công.");
    }

    public async Task<ServiceResult> GopBanAsync(
        int sourceBanId,
        int targetBanId,
        CancellationToken cancellationToken = default)
    {
        if (sourceBanId <= 0 || targetBanId <= 0 || sourceBanId == targetBanId)
        {
            return ServiceResult.Fail("Thông tin bàn gộp không hợp lệ.");
        }

        var success = await _banRepository.GopBanAsync(sourceBanId, targetBanId, cancellationToken);
        if (!success)
        {
            return ServiceResult.Fail("Gộp bàn không thành công. Vui lòng kiểm tra trạng thái bàn.");
        }

        _ = TryWriteAuditAsync(
            "Gộp bàn",
            "Ban",
            $"Gộp bàn nguồn #{sourceBanId} vào bàn đích #{targetBanId}.",
            cancellationToken);

        return ServiceResult.Success("Gộp bàn thành công.");
    }

    private async Task TryWriteAuditAsync(
        string hanhDong,
        string doiTuong,
        string duLieuTomTat,
        CancellationToken cancellationToken)
    {
        try
        {
            await _auditLogService.GhiLogAsync(
                _sessionService.CurrentUser?.UserId,
                hanhDong,
                doiTuong,
                duLieuTomTat,
                Environment.MachineName,
                cancellationToken);
        }
        catch
        {
            // Không chặn luồng quản lý bàn khi ghi log lỗi.
        }
    }
}
