using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class DanhMucService : IDanhMucService
{
    private readonly IDanhMucRepository _danhMucRepository;

    public DanhMucService(IDanhMucRepository danhMucRepository)
    {
        _danhMucRepository = danhMucRepository;
    }

    public async Task<IReadOnlyList<DanhMuc>> GetAllAsync(string? keyword = null, CancellationToken cancellationToken = default)
    {
        var danhMucs = await _danhMucRepository.GetAllAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return danhMucs
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        var normalizedKeyword = keyword.Trim();

        return danhMucs
            .Where(x => x.IsActive)
            .Where(x => x.TenDanhMuc.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase)
                        || (x.MoTa?.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task<ServiceResult<DanhMuc>> CreateAsync(string tenDanhMuc, string? moTa, CancellationToken cancellationToken = default)
    {
        var tenDanhMucNormalized = tenDanhMuc.Trim();
        var moTaNormalized = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();

        if (string.IsNullOrWhiteSpace(tenDanhMucNormalized))
        {
            return ServiceResult<DanhMuc>.Fail("Tên danh mục không được để trống.");
        }

        if (tenDanhMucNormalized.Length > 150)
        {
            return ServiceResult<DanhMuc>.Fail("Tên danh mục không vượt quá 150 ký tự.");
        }

        if (moTaNormalized is not null && moTaNormalized.Length > 500)
        {
            return ServiceResult<DanhMuc>.Fail("Mô tả không vượt quá 500 ký tự.");
        }

        var existingDanhMucs = await _danhMucRepository.GetAllAsync(cancellationToken);
        var isDuplicate = existingDanhMucs.Any(x =>
            x.IsActive &&
            string.Equals(x.TenDanhMuc.Trim(), tenDanhMucNormalized, StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
        {
            return ServiceResult<DanhMuc>.Fail("Tên danh mục đã tồn tại.");
        }

        var newDanhMuc = new DanhMuc
        {
            TenDanhMuc = tenDanhMucNormalized,
            MoTa = moTaNormalized,
            IsActive = true
        };

        var newId = await _danhMucRepository.InsertAsync(newDanhMuc, cancellationToken);
        var inserted = await _danhMucRepository.GetByIdAsync(newId, cancellationToken) ?? new DanhMuc
        {
            DanhMucId = newId,
            TenDanhMuc = newDanhMuc.TenDanhMuc,
            MoTa = newDanhMuc.MoTa,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        return ServiceResult<DanhMuc>.Success(inserted, "Tạo danh mục thành công.");
    }
}
