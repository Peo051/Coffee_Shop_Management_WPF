using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IUserSecurityService
{
    Task<ServiceResult> DoiMatKhauAsync(
        int nguoiDungId,
        string matKhauCu,
        string matKhauMoi,
        string xacNhanMatKhau,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> KhoaTaiKhoanAsync(
        int adminUserId,
        int targetUserId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> MoKhoaTaiKhoanAsync(
        int adminUserId,
        int targetUserId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<TaiKhoanNguoiDung>>> GetDanhSachTaiKhoanAsync(
        string? keyword,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<VaiTro>>> GetDanhSachVaiTroAsync(
        CancellationToken cancellationToken = default);

    Task<ServiceResult<int>> TaoTaiKhoanMoiAsync(
        int adminUserId,
        string tenDangNhap,
        string matKhau,
        string xacNhanMatKhau,
        string hoTen,
        int vaiTroId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> ResetMatKhauAsync(
        int adminUserId,
        int targetUserId,
        string matKhauMoi,
        CancellationToken cancellationToken = default);
}

