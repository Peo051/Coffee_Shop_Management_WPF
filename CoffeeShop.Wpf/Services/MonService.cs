using System.IO;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class MonService : IMonService
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private readonly IMonRepository _monRepository;
    private readonly IDanhMucService _danhMucService;

    public MonService(IMonRepository monRepository, IDanhMucService danhMucService)
    {
        _monRepository = monRepository;
        _danhMucService = danhMucService;
    }

    public async Task<IReadOnlyList<Mon>> SearchAsync(string? keyword, int? danhMucId, CancellationToken cancellationToken = default)
    {
        var data = await _monRepository.SearchAsync(keyword ?? string.Empty, danhMucId, cancellationToken);
        return data.Where(x => x.IsActive).ToList();
    }

    public async Task<Mon?> GetByIdAsync(int monId, CancellationToken cancellationToken = default)
    {
        return await _monRepository.GetByIdAsync(monId, cancellationToken);
    }

    public async Task<ServiceResult<Mon>> CreateAsync(
        string tenMon,
        int danhMucId,
        decimal donGia,
        int tonKhoBanDau,
        int tonKhoToiThieu,
        string hinhAnhPath,
        CancellationToken cancellationToken = default)
    {
        var tenMonNormalized = tenMon.Trim();
        var hinhAnhPathNormalized = hinhAnhPath.Trim();

        if (string.IsNullOrWhiteSpace(tenMonNormalized))
        {
            return ServiceResult<Mon>.Fail("Tên sản phẩm không được để trống.");
        }

        if (tenMonNormalized.Length > 150)
        {
            return ServiceResult<Mon>.Fail("Tên sản phẩm không vượt quá 150 ký tự.");
        }

        if (danhMucId <= 0)
        {
            return ServiceResult<Mon>.Fail("Vui lòng chọn danh mục hợp lệ.");
        }

        var danhMucs = await _danhMucService.GetAllAsync(cancellationToken: cancellationToken);
        var danhMucExists = danhMucs.Any(x => x.DanhMucId == danhMucId);
        if (!danhMucExists)
        {
            return ServiceResult<Mon>.Fail("Danh mục không tồn tại hoặc đã bị khóa.");
        }

        if (donGia <= 0)
        {
            return ServiceResult<Mon>.Fail("Đơn giá phải lớn hơn 0.");
        }

        if (tonKhoBanDau < 0)
        {
            return ServiceResult<Mon>.Fail("Tồn kho ban đầu không được âm.");
        }

        if (tonKhoToiThieu < 0)
        {
            return ServiceResult<Mon>.Fail("Tồn kho tối thiểu không được âm.");
        }

        if (string.IsNullOrWhiteSpace(hinhAnhPathNormalized))
        {
            return ServiceResult<Mon>.Fail("Đường dẫn hình ảnh không được để trống.");
        }

        var extension = Path.GetExtension(hinhAnhPathNormalized);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return ServiceResult<Mon>.Fail("Hình ảnh phải có đuôi .jpg, .jpeg, .png hoặc .webp.");
        }

        var allMons = await _monRepository.GetAllAsync(cancellationToken);
        var duplicate = allMons.Any(x =>
            x.IsActive
            && x.DanhMucId == danhMucId
            && string.Equals(x.TenMon.Trim(), tenMonNormalized, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            return ServiceResult<Mon>.Fail("Sản phẩm đã tồn tại trong danh mục đã chọn.");
        }

        var newMon = new Mon
        {
            TenMon = tenMonNormalized,
            DanhMucId = danhMucId,
            DonGia = donGia,
            TonKho = tonKhoBanDau,
            TonKhoToiThieu = tonKhoToiThieu,
            HinhAnhPath = hinhAnhPathNormalized,
            IsActive = true
        };

        var newId = await _monRepository.InsertAsync(newMon, cancellationToken);
        var inserted = await _monRepository.GetByIdAsync(newId, cancellationToken) ?? new Mon
        {
            MonId = newId,
            TenMon = newMon.TenMon,
            DanhMucId = newMon.DanhMucId,
            DonGia = newMon.DonGia,
            TonKho = newMon.TonKho,
            TonKhoToiThieu = newMon.TonKhoToiThieu,
            HinhAnhPath = newMon.HinhAnhPath,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        return ServiceResult<Mon>.Success(inserted, "Tạo sản phẩm thành công.");
    }
}

