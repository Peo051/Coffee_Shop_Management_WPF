using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class CongThucMonService : ICongThucMonService
{
    private readonly ICongThucMonRepository _congThucMonRepository;
    private readonly IMonService _monService;
    private readonly INguyenLieuService _nguyenLieuService;

    public CongThucMonService(
        ICongThucMonRepository congThucMonRepository,
        IMonService monService,
        INguyenLieuService nguyenLieuService)
    {
        _congThucMonRepository = congThucMonRepository;
        _monService = monService;
        _nguyenLieuService = nguyenLieuService;
    }

    public async Task<IReadOnlyList<CongThucMon>> GetByMonAsync(
        int monId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await _congThucMonRepository.GetByMonAsync(monId, activeOnly, cancellationToken);
    }

    public async Task<IReadOnlyList<CongThucMon>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await _congThucMonRepository.GetAllAsync(activeOnly, cancellationToken);
    }

    public async Task<ServiceResult> CreateAsync(
        int monId,
        int nguyenLieuId,
        decimal dinhLuong,
        string? ghiChu = null,
        string? cacBuocThucHien = null,
        decimal tyLeHaoHut = 0,
        CancellationToken cancellationToken = default)
    {
        // === VALIDATION ===

        // Kiểm tra định lượng hợp lệ
        if (dinhLuong <= 0)
        {
            return ServiceResult.Fail("Định lượng phải lớn hơn 0.");
        }

        if (tyLeHaoHut < 0 || tyLeHaoHut >= 100)
        {
            return ServiceResult.Fail("Tỷ lệ hao hụt phải từ 0 đến dưới 100%.");
        }

        try
        {
            // Kiểm tra món tồn tại trong database
            var mon = await _monService.GetByIdAsync(monId, cancellationToken);
            if (mon == null)
            {
                return ServiceResult.Fail($"Không tìm thấy món với ID {monId}.");
            }

            // Kiểm tra nguyên liệu tồn tại trong database
            var nguyenLieu = await _nguyenLieuService.GetByIdAsync(nguyenLieuId, cancellationToken);
            if (nguyenLieu == null)
            {
                return ServiceResult.Fail($"Không tìm thấy nguyên liệu với ID {nguyenLieuId}.");
            }

            // Kiểm tra trùng lặp (món đã có nguyên liệu này chưa?)
            var exists = await _congThucMonRepository.ExistsAsync(monId, nguyenLieuId, cancellationToken);
            if (exists)
            {
                return ServiceResult.Fail($"Món '{mon.TenMon}' đã có nguyên liệu '{nguyenLieu.TenNguyenLieu}' trong công thức.");
            }

            // === TẠO CÔNG THỨC MỚI ===

            var congThucMon = new CongThucMon
            {
                MonId = monId,
                NguyenLieuId = nguyenLieuId,
                DinhLuong = dinhLuong,
                GhiChu = ghiChu?.Trim(),
                IsActive = true,
                CacBuocThucHien = cacBuocThucHien?.Trim(),
                TyLeHaoHut = tyLeHaoHut
            };

            var newId = await _congThucMonRepository.CreateAsync(congThucMon, cancellationToken);

            await _congThucMonRepository.SaveLichSuCapNhatAsync(
                monId,
                Environment.UserName,  // Lấy tên user từ Windows
                $"Thêm nguyên liệu '{nguyenLieu.TenNguyenLieu}' với định lượng {dinhLuong:N2} {nguyenLieu.DonViTinh}",
                cancellationToken);

            return ServiceResult.Success($"Đã thêm '{nguyenLieu.TenNguyenLieu}' vào công thức món '{mon.TenMon}' (ID: {newId}).");
        }
        catch (Exception ex)
        {
            // Bắt lỗi database hoặc lỗi không mong muốn
            return ServiceResult.Fail($"Không thể thêm nguyên liệu vào công thức: {ex.Message}");
        }
    }

    public async Task<ServiceResult> UpdateAsync(
        int congThucMonId,
        decimal dinhLuong,
        string? ghiChu = null,
        string? cacBuocThucHien = null,
        decimal tyLeHaoHut = 0,
        CancellationToken cancellationToken = default)
    {
        // === VALIDATION ===

        if (dinhLuong <= 0)
        {
            return ServiceResult.Fail("Định lượng phải lớn hơn 0.");
        }

        if (tyLeHaoHut < 0 || tyLeHaoHut >= 100)
        {
            return ServiceResult.Fail("Tỷ lệ hao hụt phải từ 0 đến dưới 100%.");
        }

        try
        {
            var existing = await _congThucMonRepository.GetByIdAsync(congThucMonId, cancellationToken);
            if (existing == null)
            {
                return ServiceResult.Fail($"Không tìm thấy công thức với ID {congThucMonId}.");
            }

            existing.DinhLuong = dinhLuong;
            existing.GhiChu = ghiChu?.Trim();
            existing.CacBuocThucHien = cacBuocThucHien?.Trim();
            existing.TyLeHaoHut = tyLeHaoHut;

            // Lưu vào database
            await _congThucMonRepository.UpdateAsync(existing, cancellationToken);

            await _congThucMonRepository.SaveLichSuCapNhatAsync(
                existing.MonId,
                Environment.UserName,
                $"Cập nhật nguyên liệu '{existing.TenNguyenLieu}': định lượng {dinhLuong:N2} {existing.DonViTinh}, hao hụt {tyLeHaoHut}%",
                cancellationToken);

            return ServiceResult.Success($"Đã cập nhật định lượng '{existing.TenNguyenLieu}' thành {dinhLuong:N2} {existing.DonViTinh}.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Không thể cập nhật công thức: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteAsync(
        int congThucMonId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Lấy thông tin công thức trước khi xóa (để log lịch sử)
            var existing = await _congThucMonRepository.GetByIdAsync(congThucMonId, cancellationToken);
            if (existing == null)
            {
                return ServiceResult.Fail($"Không tìm thấy công thức với ID {congThucMonId}.");
            }

            await _congThucMonRepository.SetActiveAsync(congThucMonId, false, cancellationToken);

            await _congThucMonRepository.SaveLichSuCapNhatAsync(
                existing.MonId,
                Environment.UserName,
                $"Xóa nguyên liệu '{existing.TenNguyenLieu}' khỏi công thức",
                cancellationToken);

            return ServiceResult.Success($"Đã xóa '{existing.TenNguyenLieu}' khỏi công thức món '{existing.TenMon}'.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Không thể xóa nguyên liệu khỏi công thức: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<LichSuCapNhatCongThuc>> GetLichSuCapNhatAsync(
        int monId,
        CancellationToken cancellationToken = default)
    {
        return await _congThucMonRepository.GetLichSuCapNhatAsync(monId, cancellationToken);
    }
}
