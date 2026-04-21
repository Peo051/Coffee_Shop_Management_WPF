using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class KhoService : IKhoService
{
    private readonly IKhoRepository _khoRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly SessionService _sessionService;

    public KhoService(
        IKhoRepository khoRepository,
        IAuditLogService auditLogService,
        SessionService sessionService)
    {
        _khoRepository = khoRepository;
        _auditLogService = auditLogService;
        _sessionService = sessionService;
    }

    public async Task<ServiceResult<IReadOnlyList<TrangThaiSanPhamDong>>> GetTrangThaiSanPhamAsync(
        string? keyword,
        int? danhMucId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        if (danhMucId <= 0)
        {
            danhMucId = null;
        }

        var data = await _khoRepository.GetTrangThaiSanPhamAsync(keyword, danhMucId, isActive, cancellationToken);
        return ServiceResult<IReadOnlyList<TrangThaiSanPhamDong>>.Success(data, "Tải dữ liệu trạng thái sản phẩm thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<CanhBaoTonKhoThapDong>>> GetCanhBaoTonKhoThapAsync(
        string? keyword,
        int? danhMucId,
        CancellationToken cancellationToken = default)
    {
        if (danhMucId <= 0)
        {
            danhMucId = null;
        }

        var data = await _khoRepository.GetCanhBaoTonKhoThapAsync(keyword, danhMucId, cancellationToken);
        return ServiceResult<IReadOnlyList<CanhBaoTonKhoThapDong>>.Success(data, "Tải danh sách cảnh báo tồn kho thành công.");
    }

    public async Task<ServiceResult> CapNhatTrangThaiKinhDoanhAsync(
        int monId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (monId <= 0)
        {
            return ServiceResult.Fail("Mã sản phẩm không hợp lệ.");
        }

        var updated = await _khoRepository.UpdateTrangThaiKinhDoanhAsync(monId, isActive, cancellationToken);
        if (!updated)
        {
            return ServiceResult.Fail("Không tìm thấy sản phẩm để cập nhật trạng thái.");
        }

        _ = TryWriteAuditAsync(
            "Đổi trạng thái kinh doanh sản phẩm",
            "Mon",
            $"Món #{monId} => {(isActive ? "mở kinh doanh" : "khóa kinh doanh")}.",
            cancellationToken);

        return ServiceResult.Success(isActive
            ? "Đã mở kinh doanh sản phẩm."
            : "Đã khóa kinh doanh sản phẩm.");
    }

    public async Task<ServiceResult> CapNhatMucCanhBaoTonKhoAsync(
        int monId,
        int mucCanhBaoTonKho,
        CancellationToken cancellationToken = default)
    {
        if (monId <= 0)
        {
            return ServiceResult.Fail("Mã sản phẩm không hợp lệ.");
        }

        if (mucCanhBaoTonKho < 0)
        {
            return ServiceResult.Fail("Mức cảnh báo tồn kho phải lớn hơn hoặc bằng 0.");
        }

        var updated = await _khoRepository.UpdateMucCanhBaoTonKhoAsync(monId, mucCanhBaoTonKho, cancellationToken);
        if (!updated)
        {
            return ServiceResult.Fail("Không tìm thấy sản phẩm để cập nhật mức cảnh báo.");
        }

        return ServiceResult.Success("Cập nhật mức cảnh báo tồn kho thành công.");
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
            // Không chặn luồng kho khi ghi log lỗi.
        }
    }
}
