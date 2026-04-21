using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class CaLamViecService : ICaLamViecService
{
    private readonly ICaLamViecRepository _caLamViecRepository;
    private readonly IAuditLogService _auditLogService;

    public CaLamViecService(ICaLamViecRepository caLamViecRepository, IAuditLogService auditLogService)
    {
        _caLamViecRepository = caLamViecRepository;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<CaLamViec>> MoCaAsync(
        int nguoiDungId,
        decimal tienDauCa,
        string? ghiChu,
        CancellationToken cancellationToken = default)
    {
        if (nguoiDungId <= 0)
        {
            return ServiceResult<CaLamViec>.Fail("Người dùng không hợp lệ.");
        }

        if (tienDauCa < 0)
        {
            return ServiceResult<CaLamViec>.Fail("Tiền đầu ca không được âm.");
        }

        var caDangMo = await _caLamViecRepository.GetCaDangMoAsync(nguoiDungId, cancellationToken);
        if (caDangMo is not null)
        {
            return ServiceResult<CaLamViec>.Fail("Bạn đang có ca làm việc chưa đóng.");
        }

        var newId = await _caLamViecRepository.MoCaAsync(nguoiDungId, tienDauCa, ghiChu, cancellationToken);
        var opened = await _caLamViecRepository.GetCaDangMoAsync(nguoiDungId, cancellationToken);
        if (opened is null || opened.CaLamViecId != newId)
        {
            return ServiceResult<CaLamViec>.Fail("Không thể tải ca làm việc vừa mở.");
        }

        _ = TryWriteAuditAsync(
            nguoiDungId,
            "Mở ca làm việc",
            "CaLamViec",
            $"Ca #{newId} được mở. Tiền đầu ca: {tienDauCa:N0}.",
            cancellationToken);

        return ServiceResult<CaLamViec>.Success(opened, "Mở ca làm việc thành công.");
    }

    public async Task<ServiceResult> DongCaAsync(
        int nguoiDungId,
        decimal tienMatThucDem,
        string? ghiChu,
        string? ghiChuDoiSoat,
        CancellationToken cancellationToken = default)
    {
        if (nguoiDungId <= 0)
        {
            return ServiceResult.Fail("Người dùng không hợp lệ.");
        }

        if (tienMatThucDem < 0)
        {
            return ServiceResult.Fail("Tiền mặt thực đếm không được âm.");
        }

        var caDangMo = await _caLamViecRepository.GetCaDangMoAsync(nguoiDungId, cancellationToken);
        if (caDangMo is null)
        {
            return ServiceResult.Fail("Hiện không có ca làm việc đang mở.");
        }

        // Lấy tổng kết để tính chênh lệch
        var tongKet = await _caLamViecRepository.GetTongKetCaAsync(caDangMo.CaLamViecId, cancellationToken);
        var tienDauCa = tongKet?.TienDauCa ?? caDangMo.TienDauCa;
        var tienMatHeThong = tongKet?.TienMatHeThong ?? 0;
        var tienMatDuKien = tienDauCa + tienMatHeThong;
        var chenhLech = tienMatThucDem - tienMatDuKien;

        // Nếu có chênh lệch, bắt buộc ghi chú
        if (chenhLech != 0 && string.IsNullOrWhiteSpace(ghiChuDoiSoat))
        {
            return ServiceResult.Fail(
                $"Có chênh lệch tiền mặt ({chenhLech:N0} đ). Vui lòng nhập ghi chú đối soát.");
        }

        var closed = await _caLamViecRepository.DongCaAsync(
            caDangMo.CaLamViecId, ghiChu, tienMatThucDem, chenhLech, ghiChuDoiSoat, cancellationToken);
        if (!closed)
        {
            return ServiceResult.Fail("Đóng ca không thành công.");
        }

        _ = TryWriteAuditAsync(
            nguoiDungId,
            "Đóng ca làm việc",
            "CaLamViec",
            $"Ca #{caDangMo.CaLamViecId} đã đóng. Tiền đầu ca: {tienDauCa:N0}. Tiền mặt HT: {tienMatHeThong:N0}. Dự kiến: {tienMatDuKien:N0}. Thực đếm: {tienMatThucDem:N0}. Chênh lệch: {chenhLech:N0}.",
            cancellationToken);

        return ServiceResult.Success("Đóng ca làm việc thành công.");
    }

    public async Task<ServiceResult<CaLamViec?>> GetCaDangMoAsync(
        int nguoiDungId,
        CancellationToken cancellationToken = default)
    {
        if (nguoiDungId <= 0)
        {
            return ServiceResult<CaLamViec?>.Fail("Người dùng không hợp lệ.");
        }

        var data = await _caLamViecRepository.GetCaDangMoAsync(nguoiDungId, cancellationToken);
        return ServiceResult<CaLamViec?>.Success(data, data is null
            ? "Hiện không có ca đang mở."
            : "Đã tải ca đang mở.");
    }

    public async Task<ServiceResult<CaTongKetModel>> GetTongKetCaAsync(
        int caLamViecId,
        CancellationToken cancellationToken = default)
    {
        if (caLamViecId <= 0)
        {
            return ServiceResult<CaTongKetModel>.Fail("Ca làm việc không hợp lệ.");
        }

        var data = await _caLamViecRepository.GetTongKetCaAsync(caLamViecId, cancellationToken);
        if (data is null)
        {
            return ServiceResult<CaTongKetModel>.Fail("Không tìm thấy dữ liệu tổng kết ca.");
        }

        return ServiceResult<CaTongKetModel>.Success(data, "Tải tổng kết ca thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<CaLamViec>>> GetLichSuCaAsync(
        DateTime fromDate,
        DateTime toDate,
        int? nguoiDungId,
        CancellationToken cancellationToken = default)
    {
        if (fromDate.Date > toDate.Date)
        {
            return ServiceResult<IReadOnlyList<CaLamViec>>.Fail("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        var data = await _caLamViecRepository.GetLichSuCaAsync(fromDate, toDate, nguoiDungId, cancellationToken);
        return ServiceResult<IReadOnlyList<CaLamViec>>.Success(data, "Tải lịch sử ca làm việc thành công.");
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
            // Không chặn luồng ca làm việc khi ghi log lỗi.
        }
    }
}
