using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class PhaCheService : IPhaCheService
{
    private readonly IPhaCheRepository _phaCheRepository;

    public PhaCheService(IPhaCheRepository phaCheRepository)
    {
        _phaCheRepository = phaCheRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<PhaCheDonHangDong>>> GetDonCanPhaCheAsync(CancellationToken cancellationToken = default)
    {
        var data = await _phaCheRepository.GetDonCanPhaCheAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<PhaCheDonHangDong>>.Success(data, $"Tải {data.Count} đơn pha chế.");
    }

    public async Task<ServiceResult> CapNhatTrangThaiAsync(
        PhaCheDonHangDong? don,
        string trangThaiMoi,
        CancellationToken cancellationToken = default)
    {
        if (don is null)
        {
            return ServiceResult.Fail("Vui lòng chọn đơn cần cập nhật.");
        }

        // Validate chuyển trạng thái hợp lệ
        var hopLe = (don.TrangThaiPhaChe, trangThaiMoi) switch
        {
            (TrangThaiPhaCheConst.ChoPhaChe, TrangThaiPhaCheConst.DangPhaChe) => true,
            (TrangThaiPhaCheConst.DangPhaChe, TrangThaiPhaCheConst.DaHoanThanh) => true,
            (TrangThaiPhaCheConst.DaHoanThanh, TrangThaiPhaCheConst.DaGiaoKhach) => true,
            _ => false
        };

        if (!hopLe)
        {
            return ServiceResult.Fail(
                $"Không thể chuyển từ '{TrangThaiPhaCheConst.ToDisplayName(don.TrangThaiPhaChe)}' sang '{TrangThaiPhaCheConst.ToDisplayName(trangThaiMoi)}'.");
        }

        var updated = await _phaCheRepository.CapNhatTrangThaiPhaCheAsync(don.HoaDonBanId, trangThaiMoi, cancellationToken);

        return updated
            ? ServiceResult.Success($"Đơn #{don.SoGoiMonHienThi} đã chuyển sang: {TrangThaiPhaCheConst.ToDisplayName(trangThaiMoi)}.")
            : ServiceResult.Fail("Không tìm thấy hóa đơn hoặc hóa đơn đã bị hủy.");
    }
}
