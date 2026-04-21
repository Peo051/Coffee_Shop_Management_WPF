using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class KhachHangService : IKhachHangService
{
    private const decimal DiemQuyDoiDonViTien = 10000m;

    private readonly IKhachHangRepository _khachHangRepository;

    public KhachHangService(IKhachHangRepository khachHangRepository)
    {
        _khachHangRepository = khachHangRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<KhachHang>>> GetDanhSachKhachHangAsync(
        string? keyword,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var data = await _khachHangRepository.GetDanhSachKhachHangAsync(keyword, isActive, cancellationToken);
        return ServiceResult<IReadOnlyList<KhachHang>>.Success(data, "Tải danh sách khách hàng thành công.");
    }

    public async Task<ServiceResult<KhachHang>> TaoKhachHangAsync(
        KhachHang khachHang,
        CancellationToken cancellationToken = default)
    {
        var validate = ValidateThongTinKhachHang(khachHang, isCreate: true);
        if (!validate.IsSuccess)
        {
            return ServiceResult<KhachHang>.Fail(validate.Message);
        }

        var newId = await _khachHangRepository.TaoKhachHangAsync(khachHang, cancellationToken);
        var created = await _khachHangRepository.GetByIdAsync(newId, cancellationToken);
        if (created is null)
        {
            return ServiceResult<KhachHang>.Fail("Không thể tải lại khách hàng vừa tạo.");
        }

        return ServiceResult<KhachHang>.Success(created, "Tạo khách hàng thành công.");
    }

    public async Task<ServiceResult> CapNhatKhachHangAsync(
        KhachHang khachHang,
        CancellationToken cancellationToken = default)
    {
        if (khachHang.KhachHangId <= 0)
        {
            return ServiceResult.Fail("Mã khách hàng không hợp lệ.");
        }

        var validate = ValidateThongTinKhachHang(khachHang, isCreate: false);
        if (!validate.IsSuccess)
        {
            return validate;
        }

        var updated = await _khachHangRepository.CapNhatKhachHangAsync(khachHang, cancellationToken);
        if (!updated)
        {
            return ServiceResult.Fail("Không tìm thấy khách hàng để cập nhật.");
        }

        return ServiceResult.Success("Cập nhật khách hàng thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<KhachHang>>> TimKiemKhachHangAsync(
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        var data = await _khachHangRepository.TimKiemKhachHangAsync(keyword, cancellationToken);
        return ServiceResult<IReadOnlyList<KhachHang>>.Success(data, "Tìm kiếm khách hàng thành công.");
    }

    public async Task<ServiceResult<int>> CongDiemSauHoaDonAsync(
        int khachHangId,
        decimal giaTriThanhToan,
        CancellationToken cancellationToken = default)
    {
        if (khachHangId <= 0)
        {
            return ServiceResult<int>.Fail("Mã khách hàng không hợp lệ.");
        }

        if (giaTriThanhToan <= 0)
        {
            return ServiceResult<int>.Success(0, "Không phát sinh điểm tích lũy.");
        }

        var khachHang = await _khachHangRepository.GetByIdAsync(khachHangId, cancellationToken);
        if (khachHang is null || !khachHang.IsActive)
        {
            return ServiceResult<int>.Fail("Khách hàng không tồn tại hoặc đã ngừng hoạt động.");
        }

        var diemCong = (int)Math.Floor(giaTriThanhToan / DiemQuyDoiDonViTien);
        if (diemCong <= 0)
        {
            return ServiceResult<int>.Success(0, "Hóa đơn chưa đủ điều kiện cộng điểm.");
        }

        var updated = await _khachHangRepository.CongDiemAsync(khachHangId, diemCong, cancellationToken);
        if (!updated)
        {
            return ServiceResult<int>.Fail("Không thể cập nhật điểm tích lũy cho khách hàng.");
        }

        return ServiceResult<int>.Success(diemCong, $"Đã cộng {diemCong} điểm cho khách hàng.");
    }

    public async Task<ServiceResult<KhachHang>> GetByIdAsync(
        int khachHangId,
        CancellationToken cancellationToken = default)
    {
        if (khachHangId <= 0)
        {
            return ServiceResult<KhachHang>.Fail("Mã khách hàng không hợp lệ.");
        }

        var data = await _khachHangRepository.GetByIdAsync(khachHangId, cancellationToken);
        if (data is null)
        {
            return ServiceResult<KhachHang>.Fail("Không tìm thấy khách hàng.");
        }

        return ServiceResult<KhachHang>.Success(data, "Tải khách hàng thành công.");
    }

    private static ServiceResult ValidateThongTinKhachHang(KhachHang khachHang, bool isCreate)
    {
        if (!isCreate && khachHang.KhachHangId <= 0)
        {
            return ServiceResult.Fail("Mã khách hàng không hợp lệ.");
        }

        if (string.IsNullOrWhiteSpace(khachHang.HoTen))
        {
            return ServiceResult.Fail("Họ tên khách hàng không được để trống.");
        }

        if (khachHang.HoTen.Trim().Length > 150)
        {
            return ServiceResult.Fail("Họ tên khách hàng không được vượt quá 150 ký tự.");
        }

        if (!string.IsNullOrWhiteSpace(khachHang.SoDienThoai) && khachHang.SoDienThoai.Trim().Length > 20)
        {
            return ServiceResult.Fail("Số điện thoại không được vượt quá 20 ký tự.");
        }

        if (!string.IsNullOrWhiteSpace(khachHang.Email) && khachHang.Email.Trim().Length > 150)
        {
            return ServiceResult.Fail("Email không được vượt quá 150 ký tự.");
        }

        if (khachHang.DiemTichLuy < 0)
        {
            return ServiceResult.Fail("Điểm tích lũy không được âm.");
        }

        return ServiceResult.Success();
    }
}



