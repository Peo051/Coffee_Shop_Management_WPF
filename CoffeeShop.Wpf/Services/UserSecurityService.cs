using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class UserSecurityService : IUserSecurityService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;

    public UserSecurityService(IUserRepository userRepository, IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult> DoiMatKhauAsync(
        int nguoiDungId,
        string matKhauCu,
        string matKhauMoi,
        string xacNhanMatKhau,
        CancellationToken cancellationToken = default)
    {
        if (nguoiDungId <= 0)
        {
            return ServiceResult.Fail("Phiên đăng nhập không hợp lệ.");
        }

        if (string.IsNullOrWhiteSpace(matKhauCu)
            || string.IsNullOrWhiteSpace(matKhauMoi)
            || string.IsNullOrWhiteSpace(xacNhanMatKhau))
        {
            return ServiceResult.Fail("Vui lòng nhập đầy đủ mật khẩu cũ, mật khẩu mới và xác nhận mật khẩu.");
        }

        if (!string.Equals(matKhauMoi, xacNhanMatKhau, StringComparison.Ordinal))
        {
            return ServiceResult.Fail("Xác nhận mật khẩu mới không khớp.");
        }

        if (matKhauMoi.Length < 6)
        {
            return ServiceResult.Fail("Mật khẩu mới phải có ít nhất 6 ký tự.");
        }

        var user = await _userRepository.GetByUserIdAsync(nguoiDungId, cancellationToken);
        if (user is null)
        {
            return ServiceResult.Fail("Không tìm thấy tài khoản cần đổi mật khẩu.");
        }

        if (!PasswordHasher.VerifyPassword(matKhauCu, user.Password))
        {
            return ServiceResult.Fail("Mật khẩu cũ không chính xác.");
        }

        if (PasswordHasher.VerifyPassword(matKhauMoi, user.Password))
        {
            return ServiceResult.Fail("Mật khẩu mới không được trùng với mật khẩu cũ.");
        }

        var hash = PasswordHasher.HashPassword(matKhauMoi);
        var updated = await _userRepository.UpdatePasswordAsync(nguoiDungId, hash, cancellationToken);
        if (!updated)
        {
            return ServiceResult.Fail("Không thể cập nhật mật khẩu.");
        }

        _ = TryWriteAuditAsync(
            nguoiDungId,
            "Đổi mật khẩu",
            "NguoiDung",
            $"Tài khoản {user.Username} đã đổi mật khẩu.",
            cancellationToken);

        return ServiceResult.Success("Đổi mật khẩu thành công.");
    }

    public async Task<ServiceResult> KhoaTaiKhoanAsync(
        int adminUserId,
        int targetUserId,
        CancellationToken cancellationToken = default)
    {
        var validateAdmin = await ValidateAdminPermissionAsync(adminUserId, cancellationToken);
        if (!validateAdmin.IsSuccess)
        {
            return validateAdmin;
        }

        if (targetUserId <= 0)
        {
            return ServiceResult.Fail("Tài khoản cần khóa không hợp lệ.");
        }

        if (adminUserId == targetUserId)
        {
            return ServiceResult.Fail("Không thể tự khóa tài khoản đang đăng nhập.");
        }

        var user = await _userRepository.GetByUserIdAsync(targetUserId, cancellationToken);
        if (user is null)
        {
            return ServiceResult.Fail("Không tìm thấy tài khoản cần khóa.");
        }

        if (!user.IsActive)
        {
            return ServiceResult.Success("Tài khoản đã ở trạng thái khóa.");
        }

        var updated = await _userRepository.SetIsActiveAsync(targetUserId, false, cancellationToken);
        if (!updated)
        {
            return ServiceResult.Fail("Khóa tài khoản không thành công.");
        }

        _ = TryWriteAuditAsync(
            adminUserId,
            "Khóa tài khoản",
            "NguoiDung",
            $"Khóa tài khoản: {user.Username} (ID={targetUserId}).",
            cancellationToken);

        return ServiceResult.Success("Khóa tài khoản thành công.");
    }

    public async Task<ServiceResult> MoKhoaTaiKhoanAsync(
        int adminUserId,
        int targetUserId,
        CancellationToken cancellationToken = default)
    {
        var validateAdmin = await ValidateAdminPermissionAsync(adminUserId, cancellationToken);
        if (!validateAdmin.IsSuccess)
        {
            return validateAdmin;
        }

        if (targetUserId <= 0)
        {
            return ServiceResult.Fail("Tài khoản cần mở khóa không hợp lệ.");
        }

        var user = await _userRepository.GetByUserIdAsync(targetUserId, cancellationToken);
        if (user is null)
        {
            return ServiceResult.Fail("Không tìm thấy tài khoản cần mở khóa.");
        }

        if (user.IsActive)
        {
            return ServiceResult.Success("Tài khoản đã ở trạng thái hoạt động.");
        }

        var updated = await _userRepository.SetIsActiveAsync(targetUserId, true, cancellationToken);
        if (!updated)
        {
            return ServiceResult.Fail("Mở khóa tài khoản không thành công.");
        }

        _ = TryWriteAuditAsync(
            adminUserId,
            "Mở khóa tài khoản",
            "NguoiDung",
            $"Mở khóa tài khoản: {user.Username} (ID={targetUserId}).",
            cancellationToken);

        return ServiceResult.Success("Mở khóa tài khoản thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<TaiKhoanNguoiDung>>> GetDanhSachTaiKhoanAsync(
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        var data = await _userRepository.GetDanhSachTaiKhoanAsync(keyword, cancellationToken);
        return ServiceResult<IReadOnlyList<TaiKhoanNguoiDung>>.Success(data, "Tải danh sách tài khoản thành công.");
    }

    public async Task<ServiceResult<IReadOnlyList<VaiTro>>> GetDanhSachVaiTroAsync(
        CancellationToken cancellationToken = default)
    {
        var data = await _userRepository.GetDanhSachVaiTroAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<VaiTro>>.Success(data, "Tải danh sách vai trò thành công.");
    }

    public async Task<ServiceResult<int>> TaoTaiKhoanMoiAsync(
        int adminUserId,
        string tenDangNhap,
        string matKhau,
        string xacNhanMatKhau,
        string hoTen,
        int vaiTroId,
        CancellationToken cancellationToken = default)
    {
        var validateAdmin = await ValidateAdminPermissionAsync(adminUserId, cancellationToken);
        if (!validateAdmin.IsSuccess)
        {
            return ServiceResult<int>.Fail(validateAdmin.Message);
        }

        if (string.IsNullOrWhiteSpace(tenDangNhap))
        {
            return ServiceResult<int>.Fail("Tên đăng nhập không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(matKhau))
        {
            return ServiceResult<int>.Fail("Mật khẩu không được để trống.");
        }

        if (matKhau.Length < 6)
        {
            return ServiceResult<int>.Fail("Mật khẩu phải có ít nhất 6 ký tự.");
        }

        if (!string.Equals(matKhau, xacNhanMatKhau, StringComparison.Ordinal))
        {
            return ServiceResult<int>.Fail("Xác nhận mật khẩu không khớp.");
        }

        if (string.IsNullOrWhiteSpace(hoTen))
        {
            return ServiceResult<int>.Fail("Họ tên không được để trống.");
        }

        if (vaiTroId <= 0)
        {
            return ServiceResult<int>.Fail("Vui lòng chọn vai trò.");
        }

        var exists = await _userRepository.IsTenDangNhapExistsAsync(tenDangNhap, cancellationToken);
        if (exists)
        {
            return ServiceResult<int>.Fail("Tên đăng nhập đã tồn tại.");
        }

        var hash = PasswordHasher.HashPassword(matKhau);
        var newUserId = await _userRepository.CreateTaiKhoanAsync(
            tenDangNhap,
            hash,
            hoTen,
            vaiTroId,
            true,
            cancellationToken);

        if (newUserId <= 0)
        {
            return ServiceResult<int>.Fail("Không thể tạo tài khoản mới.");
        }

        _ = TryWriteAuditAsync(
            adminUserId,
            "Tạo tài khoản mới",
            "NguoiDung",
            $"Tạo tài khoản: {tenDangNhap} (ID={newUserId}).",
            cancellationToken);

        return ServiceResult<int>.Success(newUserId, "Tạo tài khoản mới thành công.");
    }

    public async Task<ServiceResult> ResetMatKhauAsync(
        int adminUserId,
        int targetUserId,
        string matKhauMoi,
        CancellationToken cancellationToken = default)
    {
        var validateAdmin = await ValidateAdminPermissionAsync(adminUserId, cancellationToken);
        if (!validateAdmin.IsSuccess)
        {
            return validateAdmin;
        }

        if (targetUserId <= 0)
        {
            return ServiceResult.Fail("Tài khoản cần reset mật khẩu không hợp lệ.");
        }

        if (string.IsNullOrWhiteSpace(matKhauMoi))
        {
            return ServiceResult.Fail("Mật khẩu mới không được để trống.");
        }

        if (matKhauMoi.Length < 6)
        {
            return ServiceResult.Fail("Mật khẩu mới phải có ít nhất 6 ký tự.");
        }

        var user = await _userRepository.GetByUserIdAsync(targetUserId, cancellationToken);
        if (user is null)
        {
            return ServiceResult.Fail("Không tìm thấy tài khoản cần reset mật khẩu.");
        }

        var hash = PasswordHasher.HashPassword(matKhauMoi);
        var updated = await _userRepository.UpdatePasswordAsync(targetUserId, hash, cancellationToken);
        if (!updated)
        {
            return ServiceResult.Fail("Không thể reset mật khẩu.");
        }

        _ = TryWriteAuditAsync(
            adminUserId,
            "Reset mật khẩu",
            "NguoiDung",
            $"Admin reset mật khẩu cho tài khoản: {user.Username} (ID={targetUserId}).",
            cancellationToken);

        return ServiceResult.Success("Reset mật khẩu thành công.");
    }

    private async Task<ServiceResult> ValidateAdminPermissionAsync(int adminUserId, CancellationToken cancellationToken)
    {
        if (adminUserId <= 0)
        {
            return ServiceResult.Fail("Phiên đăng nhập không hợp lệ.");
        }

        var admin = await _userRepository.GetByUserIdAsync(adminUserId, cancellationToken);
        if (admin is null || !admin.IsActive)
        {
            return ServiceResult.Fail("Không tìm thấy tài khoản quản trị hợp lệ.");
        }

        if (!string.Equals(admin.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult.Fail("Bạn không có quyền thực hiện thao tác này.");
        }

        return ServiceResult.Success();
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
            // Không chặn luồng nghiệp vụ khi ghi log lỗi.
        }
    }
}

