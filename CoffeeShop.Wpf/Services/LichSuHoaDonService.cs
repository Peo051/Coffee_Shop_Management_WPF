using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class LichSuHoaDonService : ILichSuHoaDonService
{
    private readonly ILichSuHoaDonRepository _lichSuHoaDonRepository;
    private readonly IAuditLogService _auditLogService;

    public LichSuHoaDonService(
        ILichSuHoaDonRepository lichSuHoaDonRepository,
        IAuditLogService auditLogService)
    {
        _lichSuHoaDonRepository = lichSuHoaDonRepository;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<IReadOnlyList<LichSuHoaDonDong>>> GetDanhSachHoaDonAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        if (fromDate.Date > toDate.Date)
        {
            return ServiceResult<IReadOnlyList<LichSuHoaDonDong>>.Fail("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        var data = await _lichSuHoaDonRepository.GetDanhSachHoaDonAsync(fromDate, toDate, cancellationToken);
        return ServiceResult<IReadOnlyList<LichSuHoaDonDong>>.Success(data, "Tải danh sách hóa đơn thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<LichSuHoaDonDong>>> TimKiemHoaDonAsync(
        DateTime fromDate,
        DateTime toDate,
        int? hoaDonBanId,
        int? createdByUserId,
        int? banId,
        int? caLamViecId,
        string? tenKhachHang = null,
        string? soDienThoai = null,
        string? hinhThucThanhToan = null,
        string? trangThaiThanhToan = null,
        string? trangThaiPhaChe = null,
        CancellationToken cancellationToken = default)
    {
        if (fromDate.Date > toDate.Date)
        {
            return ServiceResult<IReadOnlyList<LichSuHoaDonDong>>.Fail("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        var data = await _lichSuHoaDonRepository.TimKiemHoaDonAsync(
            fromDate,
            toDate,
            hoaDonBanId,
            createdByUserId,
            banId,
            caLamViecId,
            tenKhachHang,
            soDienThoai,
            hinhThucThanhToan,
            trangThaiThanhToan,
            trangThaiPhaChe,
            cancellationToken);

        return ServiceResult<IReadOnlyList<LichSuHoaDonDong>>.Success(data, "Tìm kiếm hóa đơn thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<LichSuHoaDonChiTietDong>>> GetChiTietHoaDonAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default)
    {
        if (hoaDonBanId <= 0)
        {
            return ServiceResult<IReadOnlyList<LichSuHoaDonChiTietDong>>.Fail("Mã hóa đơn không hợp lệ.");
        }

        var data = await _lichSuHoaDonRepository.GetChiTietHoaDonAsync(hoaDonBanId, cancellationToken);
        return ServiceResult<IReadOnlyList<LichSuHoaDonChiTietDong>>.Success(data, "Tải chi tiết hóa đơn thành công.");
    }

    /// <summary>
    /// Hủy hóa đơn: validate rồi gọi repository cập nhật trạng thái.
    /// Sau khi hủy thành công, ghi audit log.
    /// Không xóa dữ liệu.
    /// </summary>
    public async Task<ServiceResult> HuyHoaDonAsync(
        int hoaDonBanId,
        string lyDoHuy,
        string? nguoiHuy,
        CancellationToken cancellationToken = default)
    {
        if (hoaDonBanId <= 0)
        {
            return ServiceResult.Fail("Mã hóa đơn không hợp lệ.");
        }

        if (string.IsNullOrWhiteSpace(lyDoHuy))
        {
            return ServiceResult.Fail("Vui lòng nhập lý do hủy hóa đơn.");
        }

        var success = await _lichSuHoaDonRepository.HuyHoaDonAsync(hoaDonBanId, lyDoHuy, nguoiHuy, cancellationToken);
        if (!success)
        {
            return ServiceResult.Fail("Không thể hủy hóa đơn. Hóa đơn có thể đã bị hủy trước đó.");
        }

        // Ghi audit log sau khi hủy thành công (không rollback nếu log lỗi)
        await TryWriteAuditLogAsync(hoaDonBanId, lyDoHuy, nguoiHuy, cancellationToken);

        return ServiceResult.Success("Hủy hóa đơn thành công.");
    }

    /// <summary>
    /// Ghi audit log hủy hóa đơn. Nếu thất bại chỉ nuốt lỗi,
    /// không làm rollback nghiệp vụ hủy đã thành công.
    /// </summary>
    private async Task TryWriteAuditLogAsync(
        int hoaDonBanId,
        string lyDoHuy,
        string? nguoiHuy,
        CancellationToken cancellationToken)
    {
        try
        {
            var moTa = $"Nhân viên [{nguoiHuy ?? "N/A"}] đã hủy hóa đơn bán #{hoaDonBanId}. Lý do: {lyDoHuy.Trim()}.";

            await _auditLogService.GhiLogAsync(
                nguoiDungId: null, // Không có userId int ở đây, dùng null
                hanhDong: "Hủy hóa đơn bán",
                doiTuong: "HoaDonBan",
                duLieuTomTat: moTa,
                mayTram: Environment.MachineName,
                cancellationToken: cancellationToken);
        }
        catch
        {
            // Không chặn luồng hủy hóa đơn khi ghi audit log thất bại
        }
    }
}

