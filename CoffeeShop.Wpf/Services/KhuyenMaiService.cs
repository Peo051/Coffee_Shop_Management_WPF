using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class KhuyenMaiService : IKhuyenMaiService
{
    private static readonly HashSet<string> SupportedLoaiKhuyenMai =
    [
        "PhanTramHoaDon",
        "SoTienCoDinh",
        "TheoSanPham"
    ];

    private readonly IKhuyenMaiRepository _khuyenMaiRepository;

    public KhuyenMaiService(IKhuyenMaiRepository khuyenMaiRepository)
    {
        _khuyenMaiRepository = khuyenMaiRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<KhuyenMai>>> GetDanhSachKhuyenMaiAsync(
        string? keyword,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var data = await _khuyenMaiRepository.GetDanhSachKhuyenMaiAsync(keyword, isActive, cancellationToken);
        return ServiceResult<IReadOnlyList<KhuyenMai>>.Success(data, "Tải danh sách khuyến mãi thành công.");
    }

    public async Task<ServiceResult<KhuyenMai>> TaoKhuyenMaiAsync(
        KhuyenMai khuyenMai,
        CancellationToken cancellationToken = default)
    {
        var validate = await ValidateKhuyenMaiAsync(khuyenMai, cancellationToken);
        if (!validate.IsSuccess)
        {
            return ServiceResult<KhuyenMai>.Fail(validate.Message);
        }

        var newId = await _khuyenMaiRepository.TaoKhuyenMaiAsync(khuyenMai, cancellationToken);
        var created = await _khuyenMaiRepository.GetByIdAsync(newId, cancellationToken);
        if (created is null)
        {
            return ServiceResult<KhuyenMai>.Fail("Không thể tải lại khuyến mãi vừa tạo.");
        }

        return ServiceResult<KhuyenMai>.Success(created, "Tạo khuyến mãi thành công.");
    }

    public async Task<ServiceResult> CapNhatKhuyenMaiAsync(
        KhuyenMai khuyenMai,
        CancellationToken cancellationToken = default)
    {
        if (khuyenMai.KhuyenMaiId <= 0)
        {
            return ServiceResult.Fail("Mã khuyến mãi không hợp lệ.");
        }

        var validate = await ValidateKhuyenMaiAsync(khuyenMai, cancellationToken);
        if (!validate.IsSuccess)
        {
            return validate;
        }

        var updated = await _khuyenMaiRepository.CapNhatKhuyenMaiAsync(khuyenMai, cancellationToken);
        if (!updated)
        {
            return ServiceResult.Fail("Không tìm thấy khuyến mãi để cập nhật.");
        }

        return ServiceResult.Success("Cập nhật khuyến mãi thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<KhuyenMai>>> GetKhuyenMaiHieuLucAsync(
        DateTime thoiDiem,
        CancellationToken cancellationToken = default)
    {
        var data = await _khuyenMaiRepository.GetKhuyenMaiHieuLucAsync(thoiDiem, cancellationToken);
        return ServiceResult<IReadOnlyList<KhuyenMai>>.Success(data, "Tải khuyến mãi hiệu lực thành công.");
    }

    public async Task<ServiceResult<KhuyenMaiApDungModel>> ApDungKhuyenMaiAsync(
        int? khuyenMaiId,
        decimal tongTienTruocGiam,
        DateTime? thoiDiem = null,
        CancellationToken cancellationToken = default)
    {
        // Overload cũ: gọi overload mới với danh sách rỗng (TheoSanPham sẽ không áp dụng được)
        return await ApDungKhuyenMaiAsync(khuyenMaiId, tongTienTruocGiam, Array.Empty<HoaDonBanChiTietInputModel>(), thoiDiem, cancellationToken);
    }

    public async Task<ServiceResult<KhuyenMaiApDungModel>> ApDungKhuyenMaiAsync(
        int? khuyenMaiId,
        decimal tongTienTruocGiam,
        IReadOnlyList<HoaDonBanChiTietInputModel> chiTietInputs,
        DateTime? thoiDiem = null,
        CancellationToken cancellationToken = default)
    {
        if (tongTienTruocGiam <= 0)
        {
            return ServiceResult<KhuyenMaiApDungModel>.Fail("Tổng tiền hóa đơn không hợp lệ.");
        }

        if (!khuyenMaiId.HasValue || khuyenMaiId <= 0)
        {
            return ServiceResult<KhuyenMaiApDungModel>.Success(
                new KhuyenMaiApDungModel
                {
                    KhuyenMaiId = null,
                    TenKhuyenMai = null,
                    TongTienTruocGiam = tongTienTruocGiam,
                    SoTienGiam = 0
                },
                "Không áp dụng khuyến mãi.");
        }

        var khuyenMai = await _khuyenMaiRepository.GetByIdAsync(khuyenMaiId.Value, cancellationToken);
        if (khuyenMai is null)
        {
            return ServiceResult<KhuyenMaiApDungModel>.Fail("Không tìm thấy khuyến mãi đã chọn.");
        }

        var now = thoiDiem ?? DateTime.Now;
        if (!khuyenMai.IsActive || now < khuyenMai.TuNgay || now > khuyenMai.DenNgay)
        {
            return ServiceResult<KhuyenMaiApDungModel>.Fail("Khuyến mãi đã chọn không còn hiệu lực.");
        }

        // Kiểm tra đơn hàng tối thiểu
        if (khuyenMai.GiaTriDonHangToiThieu.HasValue && tongTienTruocGiam < khuyenMai.GiaTriDonHangToiThieu.Value)
        {
            return ServiceResult<KhuyenMaiApDungModel>.Fail(
                $"Khuyến mãi yêu cầu đơn hàng tối thiểu {khuyenMai.GiaTriDonHangToiThieu.Value:N0} đ. Đơn hiện tại: {tongTienTruocGiam:N0} đ.");
        }

        decimal soTienGiam;
        switch (khuyenMai.LoaiKhuyenMai)
        {
            case "PhanTramHoaDon":
                soTienGiam = Math.Round(tongTienTruocGiam * khuyenMai.GiaTri / 100m, 0, MidpointRounding.AwayFromZero);
                break;
            case "SoTienCoDinh":
                soTienGiam = khuyenMai.GiaTri;
                break;
            case "TheoSanPham":
            {
                // Bắt buộc có MonId
                if (!khuyenMai.MonId.HasValue || khuyenMai.MonId <= 0)
                {
                    return ServiceResult<KhuyenMaiApDungModel>.Fail("Khuyến mãi theo sản phẩm chưa được gắn sản phẩm.");
                }

                // Tìm dòng chi tiết có MonId trùng
                var dongSanPham = chiTietInputs
                    .Where(x => x.MonId == khuyenMai.MonId.Value)
                    .ToList();

                if (dongSanPham.Count == 0)
                {
                    return ServiceResult<KhuyenMaiApDungModel>.Fail(
                        "Khuyến mãi này chỉ áp dụng cho sản phẩm được chọn. Hóa đơn không có sản phẩm đó.");
                }

                // Tính tổng thành tiền của sản phẩm được khuyến mãi
                var thanhTienSanPham = dongSanPham.Sum(x => x.ThanhTien);

                // Giảm = GiaTri, nhưng tối đa = thành tiền sản phẩm
                soTienGiam = khuyenMai.GiaTri;
                if (soTienGiam > thanhTienSanPham)
                {
                    soTienGiam = thanhTienSanPham;
                }
                break;
            }
            default:
                return ServiceResult<KhuyenMaiApDungModel>.Fail("Loại khuyến mãi chưa được hỗ trợ.");
        }

        if (soTienGiam < 0)
        {
            soTienGiam = 0;
        }

        // Áp dụng giới hạn giảm tối đa
        if (khuyenMai.SoTienGiamToiDa.HasValue && soTienGiam > khuyenMai.SoTienGiamToiDa.Value)
        {
            soTienGiam = khuyenMai.SoTienGiamToiDa.Value;
        }

        if (soTienGiam > tongTienTruocGiam)
        {
            soTienGiam = tongTienTruocGiam;
        }

        return ServiceResult<KhuyenMaiApDungModel>.Success(
            new KhuyenMaiApDungModel
            {
                KhuyenMaiId = khuyenMai.KhuyenMaiId,
                TenKhuyenMai = khuyenMai.TenKhuyenMai,
                TongTienTruocGiam = tongTienTruocGiam,
                SoTienGiam = soTienGiam
            },
            "Áp dụng khuyến mãi thành công.");
    }

    public Task<ServiceResult> ValidateKhuyenMaiAsync(
        KhuyenMai khuyenMai,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(khuyenMai.TenKhuyenMai))
        {
            return Task.FromResult(ServiceResult.Fail("Tên khuyến mãi không được để trống."));
        }

        if (!SupportedLoaiKhuyenMai.Contains(khuyenMai.LoaiKhuyenMai))
        {
            return Task.FromResult(ServiceResult.Fail("Loại khuyến mãi không hợp lệ."));
        }

        if (khuyenMai.GiaTri <= 0)
        {
            return Task.FromResult(ServiceResult.Fail("Giá trị khuyến mãi phải lớn hơn 0."));
        }

        if (khuyenMai.LoaiKhuyenMai == "PhanTramHoaDon" && khuyenMai.GiaTri > 100)
        {
            return Task.FromResult(ServiceResult.Fail("Khuyến mãi phần trăm không được lớn hơn 100."));
        }

        if (khuyenMai.DenNgay < khuyenMai.TuNgay)
        {
            return Task.FromResult(ServiceResult.Fail("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu."));
        }

        if (khuyenMai.LoaiKhuyenMai == "TheoSanPham" && (!khuyenMai.MonId.HasValue || khuyenMai.MonId <= 0))
        {
            return Task.FromResult(ServiceResult.Fail("Khuyến mãi theo sản phẩm phải chọn sản phẩm áp dụng."));
        }

        if (khuyenMai.GiaTriDonHangToiThieu.HasValue && khuyenMai.GiaTriDonHangToiThieu.Value < 0)
        {
            return Task.FromResult(ServiceResult.Fail("Giá trị đơn hàng tối thiểu không được âm."));
        }

        if (khuyenMai.SoTienGiamToiDa.HasValue && khuyenMai.SoTienGiamToiDa.Value <= 0)
        {
            return Task.FromResult(ServiceResult.Fail("Số tiền giảm tối đa phải lớn hơn 0."));
        }

        return Task.FromResult(ServiceResult.Success());
    }
}



