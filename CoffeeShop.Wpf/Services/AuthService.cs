using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class AuthService : IAuthService
{
    private const string InvalidCredentialMessage = "Sai tên đăng nhập hoặc mật khẩu.";
    private const string LockedAccountMessage = "Tài khoản đã bị khóa.";
    private const string EmptyCredentialMessage = "Tên đăng nhập và mật khẩu không được để trống.";

    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;

    public AuthService(IUserRepository userRepository, IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _auditLogService = auditLogService;
    }

    public async Task<AuthResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var usernameNormalized = username.Trim();
        if (string.IsNullOrWhiteSpace(usernameNormalized) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Fail(EmptyCredentialMessage);
        }

        var user = await _userRepository.GetByUsernameAsync(usernameNormalized, cancellationToken);
        if (user is null)
        {
            return AuthResult.Fail(InvalidCredentialMessage);
        }

        if (!user.IsActive)
        {
            return AuthResult.Fail(LockedAccountMessage);
        }

        if (!PasswordHasher.VerifyPassword(password, user.Password))
        {
            return AuthResult.Fail(InvalidCredentialMessage);
        }

        if (!PasswordHasher.IsHashedFormat(user.Password))
        {
            var newHash = PasswordHasher.HashPassword(password);
            _ = _userRepository.UpdatePasswordAsync(user.UserId, newHash, cancellationToken);
        }

        _ = TryWriteAuditAsync(
            user.UserId,
            "Đăng nhập thành công",
            "NguoiDung",
            $"Tài khoản: {user.Username}",
            cancellationToken);

        return AuthResult.Success(user.UserId, user.Username, user.DisplayName, user.Role);
    }

    private async Task TryWriteAuditAsync(
        int nguoiDungId,
        string hanhDong,
        string doiTuong,
        string duLieuTomTat,
        CancellationToken cancellationToken)
    {
        try
        {
            await _auditLogService.GhiLogAsync(
                nguoiDungId,
                hanhDong,
                doiTuong,
                duLieuTomTat,
                Environment.MachineName,
                cancellationToken);
        }
        catch
        {
            // Không chặn luồng đăng nhập khi ghi log lỗi.
        }
    }
}

