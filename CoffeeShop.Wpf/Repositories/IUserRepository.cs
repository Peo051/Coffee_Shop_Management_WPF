using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface IUserRepository
{
    Task<UserAuthRecord?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<UserAuthRecord?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaiKhoanNguoiDung>> GetDanhSachTaiKhoanAsync(
        string? keyword,
        CancellationToken cancellationToken = default);

    Task<bool> UpdatePasswordAsync(
        int userId,
        string passwordHash,
        CancellationToken cancellationToken = default);

    Task<bool> SetIsActiveAsync(
        int userId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<bool> IsTenDangNhapExistsAsync(
        string tenDangNhap,
        CancellationToken cancellationToken = default);

    Task<int> CreateTaiKhoanAsync(
        string tenDangNhap,
        string matKhauHash,
        string hoTen,
        int vaiTroId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VaiTro>> GetDanhSachVaiTroAsync(
        CancellationToken cancellationToken = default);
}

