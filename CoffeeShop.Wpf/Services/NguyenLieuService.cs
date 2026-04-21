using CoffeeShop.Wpf.Helpers;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class NguyenLieuService : INguyenLieuService
{
    private readonly INguyenLieuRepository _nguyenLieuRepository;

    public NguyenLieuService(INguyenLieuRepository nguyenLieuRepository)
    {
        _nguyenLieuRepository = nguyenLieuRepository;
    }

    public async Task<IReadOnlyList<NguyenLieu>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await _nguyenLieuRepository.GetAllAsync(activeOnly, cancellationToken);
    }

    public async Task<NguyenLieu?> GetByIdAsync(
        int nguyenLieuId,
        CancellationToken cancellationToken = default)
    {
        return await _nguyenLieuRepository.GetByIdAsync(nguyenLieuId, cancellationToken);
    }

    public async Task<IReadOnlyList<NguyenLieu>> SearchAsync(
        string? keyword,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await _nguyenLieuRepository.SearchAsync(keyword, activeOnly, cancellationToken);
    }

    public async Task<ServiceResult> CreateAsync(
        string tenNguyenLieu,
        string donViTinh,
        decimal tonKho,
        decimal tonKhoToiThieu,
        decimal donGiaNhap,
        CancellationToken cancellationToken = default)
    {
        // Validation
        var validationResult = ValidateNguyenLieu(tenNguyenLieu, donViTinh, tonKho, tonKhoToiThieu, donGiaNhap);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        try
        {
            var nguyenLieu = new NguyenLieu
            {
                TenNguyenLieu = tenNguyenLieu.Trim(),
                DonViTinh = donViTinh.Trim(),
                TonKho = tonKho,
                TonKhoToiThieu = tonKhoToiThieu,
                DonGiaNhap = donGiaNhap,
                IsActive = true
            };

            var newId = await _nguyenLieuRepository.CreateAsync(nguyenLieu, cancellationToken);

            return ServiceResult.Success($"Đã tạo nguyên liệu '{nguyenLieu.TenNguyenLieu}' thành công (ID: {newId}).");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Không thể tạo nguyên liệu: {ex.Message}");
        }
    }

    public async Task<ServiceResult> UpdateAsync(
        int nguyenLieuId,
        string tenNguyenLieu,
        string donViTinh,
        decimal tonKho,
        decimal tonKhoToiThieu,
        decimal donGiaNhap,
        CancellationToken cancellationToken = default)
    {
        // Validation
        var validationResult = ValidateNguyenLieu(tenNguyenLieu, donViTinh, tonKho, tonKhoToiThieu, donGiaNhap);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        try
        {
            var existing = await _nguyenLieuRepository.GetByIdAsync(nguyenLieuId, cancellationToken);
            if (existing == null)
            {
                return ServiceResult.Fail($"Không tìm thấy nguyên liệu với ID {nguyenLieuId}.");
            }

            existing.TenNguyenLieu = tenNguyenLieu.Trim();
            existing.DonViTinh = donViTinh.Trim();
            existing.TonKho = tonKho;
            existing.TonKhoToiThieu = tonKhoToiThieu;
            existing.DonGiaNhap = donGiaNhap;

            await _nguyenLieuRepository.UpdateAsync(existing, cancellationToken);

            return ServiceResult.Success($"Đã cập nhật nguyên liệu '{existing.TenNguyenLieu}' thành công.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Không thể cập nhật nguyên liệu: {ex.Message}");
        }
    }

    public async Task<ServiceResult> CapNhatTonKhoAsync(
        int nguyenLieuId,
        decimal tonKhoMoi,
        CancellationToken cancellationToken = default)
    {
        if (tonKhoMoi < 0)
        {
            return ServiceResult.Fail("Tồn kho không được âm.");
        }

        try
        {
            var existing = await _nguyenLieuRepository.GetByIdAsync(nguyenLieuId, cancellationToken);
            if (existing == null)
            {
                return ServiceResult.Fail($"Không tìm thấy nguyên liệu với ID {nguyenLieuId}.");
            }

            await _nguyenLieuRepository.CapNhatTonKhoAsync(nguyenLieuId, tonKhoMoi, cancellationToken);

            return ServiceResult.Success($"Đã cập nhật tồn kho '{existing.TenNguyenLieu}' thành {tonKhoMoi:N2} {existing.DonViTinh}.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Không thể cập nhật tồn kho: {ex.Message}");
        }
    }

    public async Task<ServiceResult> SetActiveAsync(
        int nguyenLieuId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _nguyenLieuRepository.GetByIdAsync(nguyenLieuId, cancellationToken);
            if (existing == null)
            {
                return ServiceResult.Fail($"Không tìm thấy nguyên liệu với ID {nguyenLieuId}.");
            }

            await _nguyenLieuRepository.SetActiveAsync(nguyenLieuId, isActive, cancellationToken);

            var status = isActive ? "kích hoạt" : "vô hiệu hóa";
            return ServiceResult.Success($"Đã {status} nguyên liệu '{existing.TenNguyenLieu}'.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Không thể thay đổi trạng thái nguyên liệu: {ex.Message}");
        }
    }

    private static ServiceResult ValidateNguyenLieu(
        string tenNguyenLieu,
        string donViTinh,
        decimal tonKho,
        decimal tonKhoToiThieu,
        decimal donGiaNhap)
    {
        if (string.IsNullOrWhiteSpace(tenNguyenLieu))
        {
            return ServiceResult.Fail("Tên nguyên liệu không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(donViTinh))
        {
            return ServiceResult.Fail("Đơn vị tính không được để trống.");
        }

        if (tonKho < 0)
        {
            return ServiceResult.Fail("Tồn kho không được âm.");
        }

        if (tonKhoToiThieu < 0)
        {
            return ServiceResult.Fail("Tồn kho tối thiểu không được âm.");
        }

        if (donGiaNhap < 0)
        {
            return ServiceResult.Fail("Đơn giá nhập không được âm.");
        }

        return ServiceResult.Success();
    }
}
