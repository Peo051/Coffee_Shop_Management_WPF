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
        CancellationToken cancellationToken = default)
    {
        // Validation
        if (dinhLuong <= 0)
        {
            return ServiceResult.Fail("Định lượng phải lớn hơn 0.");
        }

        try
        {
            // Kiểm tra món tồn tại
            var mon = await _monService.GetByIdAsync(monId, cancellationToken);
            if (mon == null)
            {
                return ServiceResult.Fail($"Không tìm thấy món với ID {monId}.");
            }

            // Kiểm tra nguyên liệu tồn tại
            var nguyenLieu = await _nguyenLieuService.GetByIdAsync(nguyenLieuId, cancellationToken);
            if (nguyenLieu == null)
            {
                return ServiceResult.Fail($"Không tìm thấy nguyên liệu với ID {nguyenLieuId}.");
            }

            // Kiểm tra trùng
            var exists = await _congThucMonRepository.ExistsAsync(monId, nguyenLieuId, cancellationToken);
            if (exists)
            {
                return ServiceResult.Fail($"Món '{mon.TenMon}' đã có nguyên liệu '{nguyenLieu.TenNguyenLieu}' trong công thức.");
            }

            var congThucMon = new CongThucMon
            {
                MonId = monId,
                NguyenLieuId = nguyenLieuId,
                DinhLuong = dinhLuong,
                GhiChu = ghiChu?.Trim(),
                IsActive = true
            };

            var newId = await _congThucMonRepository.CreateAsync(congThucMon, cancellationToken);

            return ServiceResult.Success($"Đã thêm '{nguyenLieu.TenNguyenLieu}' vào công thức món '{mon.TenMon}' (ID: {newId}).");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Không thể thêm nguyên liệu vào công thức: {ex.Message}");
        }
    }

    public async Task<ServiceResult> UpdateAsync(
        int congThucMonId,
        decimal dinhLuong,
        string? ghiChu = null,
        CancellationToken cancellationToken = default)
    {
        // Validation
        if (dinhLuong <= 0)
        {
            return ServiceResult.Fail("Định lượng phải lớn hơn 0.");
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

            await _congThucMonRepository.UpdateAsync(existing, cancellationToken);

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
            var existing = await _congThucMonRepository.GetByIdAsync(congThucMonId, cancellationToken);
            if (existing == null)
            {
                return ServiceResult.Fail($"Không tìm thấy công thức với ID {congThucMonId}.");
            }

            await _congThucMonRepository.SetActiveAsync(congThucMonId, false, cancellationToken);

            return ServiceResult.Success($"Đã xóa '{existing.TenNguyenLieu}' khỏi công thức món '{existing.TenMon}'.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Không thể xóa nguyên liệu khỏi công thức: {ex.Message}");
        }
    }
}
