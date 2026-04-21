using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IKhuyenMaiService
{
    Task<ServiceResult<IReadOnlyList<KhuyenMai>>> GetDanhSachKhuyenMaiAsync(
        string? keyword,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<KhuyenMai>> TaoKhuyenMaiAsync(
        KhuyenMai khuyenMai,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> CapNhatKhuyenMaiAsync(
        KhuyenMai khuyenMai,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<KhuyenMai>>> GetKhuyenMaiHieuLucAsync(
        DateTime thoiDiem,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<KhuyenMaiApDungModel>> ApDungKhuyenMaiAsync(
        int? khuyenMaiId,
        decimal tongTienTruocGiam,
        DateTime? thoiDiem = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<KhuyenMaiApDungModel>> ApDungKhuyenMaiAsync(
        int? khuyenMaiId,
        decimal tongTienTruocGiam,
        IReadOnlyList<HoaDonBanChiTietInputModel> chiTietInputs,
        DateTime? thoiDiem = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> ValidateKhuyenMaiAsync(
        KhuyenMai khuyenMai,
        CancellationToken cancellationToken = default);
}

