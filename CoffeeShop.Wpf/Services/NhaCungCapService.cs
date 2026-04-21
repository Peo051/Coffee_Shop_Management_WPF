using System.Text.RegularExpressions;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class NhaCungCapService : INhaCungCapService
{
    private static readonly Regex PhoneRegex = new(@"^[0-9]{8,15}$", RegexOptions.Compiled);

    private readonly INhaCungCapRepository _nhaCungCapRepository;

    public NhaCungCapService(INhaCungCapRepository nhaCungCapRepository)
    {
        _nhaCungCapRepository = nhaCungCapRepository;
    }

    public async Task<IReadOnlyList<NhaCungCap>> GetAllAsync(string? keyword = null, CancellationToken cancellationToken = default)
    {
        var nhaCungCaps = await _nhaCungCapRepository.GetAllAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return nhaCungCaps
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        var normalizedKeyword = keyword.Trim();

        return nhaCungCaps
            .Where(x => x.IsActive)
            .Where(x => x.TenNhaCungCap.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase)
                        || (x.SoDienThoai?.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase) ?? false)
                        || (x.DiaChi?.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task<ServiceResult<NhaCungCap>> CreateAsync(
        string tenNhaCungCap,
        string? soDienThoai,
        string? diaChi,
        CancellationToken cancellationToken = default)
    {
        var tenNhaCungCapNormalized = tenNhaCungCap.Trim();
        var soDienThoaiNormalized = string.IsNullOrWhiteSpace(soDienThoai) ? null : soDienThoai.Trim();
        var diaChiNormalized = string.IsNullOrWhiteSpace(diaChi) ? null : diaChi.Trim();

        if (string.IsNullOrWhiteSpace(tenNhaCungCapNormalized))
        {
            return ServiceResult<NhaCungCap>.Fail("Tên nhà cung cấp không được để trống.");
        }

        if (tenNhaCungCapNormalized.Length > 150)
        {
            return ServiceResult<NhaCungCap>.Fail("Tên nhà cung cấp không vượt quá 150 ký tự.");
        }

        if (soDienThoaiNormalized is not null && !PhoneRegex.IsMatch(soDienThoaiNormalized))
        {
            return ServiceResult<NhaCungCap>.Fail("Số điện thoại phải gồm 8-15 chữ số.");
        }

        if (diaChiNormalized is not null && diaChiNormalized.Length > 300)
        {
            return ServiceResult<NhaCungCap>.Fail("Địa chỉ không vượt quá 300 ký tự.");
        }

        var existingNhaCungCaps = await _nhaCungCapRepository.GetAllAsync(cancellationToken);
        var isDuplicate = existingNhaCungCaps.Any(x =>
            x.IsActive &&
            string.Equals(x.TenNhaCungCap.Trim(), tenNhaCungCapNormalized, StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
        {
            return ServiceResult<NhaCungCap>.Fail("Tên nhà cung cấp đã tồn tại.");
        }

        var newNhaCungCap = new NhaCungCap
        {
            TenNhaCungCap = tenNhaCungCapNormalized,
            SoDienThoai = soDienThoaiNormalized,
            DiaChi = diaChiNormalized,
            IsActive = true
        };

        var newId = await _nhaCungCapRepository.InsertAsync(newNhaCungCap, cancellationToken);
        var inserted = await _nhaCungCapRepository.GetByIdAsync(newId, cancellationToken) ?? new NhaCungCap
        {
            NhaCungCapId = newId,
            TenNhaCungCap = newNhaCungCap.TenNhaCungCap,
            SoDienThoai = newNhaCungCap.SoDienThoai,
            DiaChi = newNhaCungCap.DiaChi,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        return ServiceResult<NhaCungCap>.Success(inserted, "Tạo nhà cung cấp thành công.");
    }
}
