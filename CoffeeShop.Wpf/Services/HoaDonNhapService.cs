using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class HoaDonNhapService : IHoaDonNhapService
{
    private readonly IHoaDonNhapRepository _hoaDonNhapRepository;
    private readonly INhaCungCapService _nhaCungCapService;
    private readonly IMonService _monService;
    private readonly IAuditLogService _auditLogService;

    public HoaDonNhapService(
        IHoaDonNhapRepository hoaDonNhapRepository,
        INhaCungCapService nhaCungCapService,
        IMonService monService,
        IAuditLogService auditLogService)
    {
        _hoaDonNhapRepository = hoaDonNhapRepository;
        _nhaCungCapService = nhaCungCapService;
        _monService = monService;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<HoaDonNhap>> CreateAsync(
        int nhaCungCapId,
        int createdByUserId,
        string? ghiChu,
        IReadOnlyList<HoaDonNhapChiTietInputModel> chiTietInputs,
        CancellationToken cancellationToken = default)
    {
        if (nhaCungCapId <= 0)
        {
            return ServiceResult<HoaDonNhap>.Fail("Nhà cung cấp không hợp lệ.");
        }

        if (createdByUserId <= 0)
        {
            return ServiceResult<HoaDonNhap>.Fail("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");
        }

        if (chiTietInputs is null || chiTietInputs.Count == 0)
        {
            return ServiceResult<HoaDonNhap>.Fail("Hóa đơn nhập phải có ít nhất 1 dòng chi tiết.");
        }

        var nhaCungCaps = await _nhaCungCapService.GetAllAsync(cancellationToken: cancellationToken);
        var nhaCungCapExists = nhaCungCaps.Any(x => x.IsActive && x.NhaCungCapId == nhaCungCapId);
        if (!nhaCungCapExists)
        {
            return ServiceResult<HoaDonNhap>.Fail("Nhà cung cấp không tồn tại hoặc đã bị khóa.");
        }

        var monData = await _monService.SearchAsync(null, null, cancellationToken);
        var monMap = monData.ToDictionary(x => x.MonId, x => x);

        var duplicatedMonId = chiTietInputs
            .GroupBy(x => x.MonId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .FirstOrDefault();

        if (duplicatedMonId > 0)
        {
            return ServiceResult<HoaDonNhap>.Fail("Mỗi sản phẩm chỉ được xuất hiện 1 lần trong chi tiết hóa đơn nhập.");
        }

        foreach (var line in chiTietInputs)
        {
            if (line.MonId <= 0 || !monMap.ContainsKey(line.MonId))
            {
                return ServiceResult<HoaDonNhap>.Fail("Có sản phẩm không tồn tại trong chi tiết hóa đơn nhập.");
            }

            if (line.SoLuong <= 0)
            {
                return ServiceResult<HoaDonNhap>.Fail("Số lượng nhập phải lớn hơn 0.");
            }

            if (line.DonGiaNhap <= 0)
            {
                return ServiceResult<HoaDonNhap>.Fail("Đơn giá nhập phải lớn hơn 0.");
            }
        }

        var tongTien = chiTietInputs.Sum(x => x.ThanhTien);
        if (tongTien <= 0)
        {
            return ServiceResult<HoaDonNhap>.Fail("Tổng tiền hóa đơn nhập không hợp lệ.");
        }

        var hoaDonNhap = new HoaDonNhap
        {
            NgayNhap = DateTime.Now,
            NhaCungCapId = nhaCungCapId,
            TongTien = tongTien,
            GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim(),
            CreatedByUserId = createdByUserId
        };

        var chiTietHoaDonNhaps = chiTietInputs
            .Select(x => new ChiTietHoaDonNhap
            {
                MonId = x.MonId,
                SoLuong = x.SoLuong,
                DonGiaNhap = x.DonGiaNhap
            })
            .ToList();

        var newId = await _hoaDonNhapRepository.CreateAsync(hoaDonNhap, chiTietHoaDonNhaps, cancellationToken);
        hoaDonNhap.HoaDonNhapId = newId;

        _ = TryWriteAuditAsync(
            createdByUserId,
            "Tạo hóa đơn nhập",
            "HoaDonNhap",
            $"Hóa đơn nhập #{newId}, số dòng: {chiTietHoaDonNhaps.Count}, tổng tiền: {tongTien:N0}",
            cancellationToken);

        return ServiceResult<HoaDonNhap>.Success(hoaDonNhap, "Lưu hóa đơn nhập thành công.");
    }

    public Task<IReadOnlyList<HoaDonNhap>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return _hoaDonNhapRepository.GetByDateRangeAsync(fromDate, toDate, cancellationToken);
    }

    private async Task TryWriteAuditAsync(
        int nguoiDungId,
        string hanhDong,
        string doiTuong,
        string duLieuTomTat,
        CancellationToken cancellationToken)
    {
        try
        {
            await _auditLogService.GhiLogAsync(
                nguoiDungId,
                hanhDong,
                doiTuong,
                duLieuTomTat,
                Environment.MachineName,
                cancellationToken);
        }
        catch
        {
            // Không chặn luồng lưu hóa đơn nhập khi ghi log lỗi.
        }
    }
}
