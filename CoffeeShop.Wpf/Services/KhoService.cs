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

    public async Task<ServiceResult> DieuChinhTonKhoMonAsync(
        int monId,
        int tonKhoMoi,
        string lyDo,
        string loaiPhatSinh,
        CancellationToken cancellationToken = default)
    {
        if (monId <= 0)
        {
            return ServiceResult.Fail("Mã sản phẩm không hợp lệ.");
        }

        if (tonKhoMoi < 0)
        {
            return ServiceResult.Fail("Tồn kho sau điều chỉnh không được âm.");
        }

        if (string.IsNullOrWhiteSpace(lyDo))
        {
            return ServiceResult.Fail("Lý do điều chỉnh không được để trống.");
        }

        if (!string.Equals(loaiPhatSinh, "DieuChinh", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(loaiPhatSinh, "KiemKe", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult.Fail("Loại điều chỉnh không hợp lệ.");
        }

        if (_khoRepository is not KhoRepository khoRepository)
        {
            return ServiceResult.Fail("Không thể điều chỉnh tồn kho do cấu hình repository không tương thích.");
        }

        try
        {
            var updated = await khoRepository.DieuChinhTonKhoVaGhiLichSuAsync(
                monId,
                tonKhoMoi,
                lyDo,
                loaiPhatSinh,
                _sessionService.CurrentUser?.UserId,
                cancellationToken);

            if (updated is null)
            {
                return ServiceResult.Fail("Không tìm thấy sản phẩm để điều chỉnh tồn kho.");
            }

            _ = TryWriteAuditAsync(
                "Điều chỉnh tồn kho món",
                "Mon",
                $"Món #{updated.MonId} ({updated.TenMon}) => Tồn kho mới: {updated.TonKho}; Loại: {loaiPhatSinh}; Lý do: {lyDo.Trim()}",
                cancellationToken);

            return ServiceResult.Success(
                $"Đã cập nhật tồn kho sản phẩm '{updated.TenMon}' thành {updated.TonKho}.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Không thể điều chỉnh tồn kho món: {ex.Message}");
        }
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
