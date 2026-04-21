using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class CauHinhHeThongService : ICauHinhHeThongService
{
    private readonly ICauHinhHeThongRepository _cauHinhRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly SessionService _sessionService;

    public CauHinhHeThongService(
        ICauHinhHeThongRepository cauHinhRepository,
        IAuditLogService auditLogService,
        SessionService sessionService)
    {
        _cauHinhRepository = cauHinhRepository;
        _auditLogService = auditLogService;
        _sessionService = sessionService;
    }

    public async Task<ServiceResult<CauHinhHeThong>> GetCauHinhAsync(CancellationToken cancellationToken = default)
    {
        var data = await _cauHinhRepository.GetCauHinhAsync(cancellationToken);
        if (data is null)
        {
            return ServiceResult<CauHinhHeThong>.Fail("Chưa có cấu hình hệ thống.");
        }

        return ServiceResult<CauHinhHeThong>.Success(data, "Tải cấu hình hệ thống thành công.");
    }

    public async Task<ServiceResult> LuuCauHinhAsync(CauHinhHeThong cauHinh, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cauHinh.TenQuan))
        {
            return ServiceResult.Fail("Tên quán không được để trống.");
        }

        if (cauHinh.TenQuan.Trim().Length > 200)
        {
            return ServiceResult.Fail("Tên quán không được vượt quá 200 ký tự.");
        }

        if (!string.IsNullOrWhiteSpace(cauHinh.DiaChi) && cauHinh.DiaChi.Trim().Length > 300)
        {
            return ServiceResult.Fail("Địa chỉ không được vượt quá 300 ký tự.");
        }

        if (!string.IsNullOrWhiteSpace(cauHinh.SoDienThoai) && cauHinh.SoDienThoai.Trim().Length > 20)
        {
            return ServiceResult.Fail("Số điện thoại không được vượt quá 20 ký tự.");
        }

        if (!string.IsNullOrWhiteSpace(cauHinh.FooterHoaDon) && cauHinh.FooterHoaDon.Trim().Length > 500)
        {
            return ServiceResult.Fail("Footer hóa đơn không được vượt quá 500 ký tự.");
        }

        await _cauHinhRepository.LuuCauHinhAsync(cauHinh, cancellationToken);

        _ = TryWriteAuditAsync(
            "Cập nhật cấu hình hệ thống",
            "CauHinhHeThong",
            $"Tên quán: {cauHinh.TenQuan}",
            cancellationToken);

        return ServiceResult.Success("Lưu cấu hình hệ thống thành công.");
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
            // Không chặn lưu cấu hình nếu ghi log lỗi.
        }
    }
}



