namespace CoffeeShop.Wpf.Models;

/// <summary>
/// YÊU CẦU 7: Model lưu lịch sử cập nhật công thức món
/// Mỗi record là một log ghi lại thao tác thêm/sửa/xóa nguyên liệu trong công thức
/// Giúp theo dõi ai đã thay đổi gì, khi nào
/// </summary>
public sealed class LichSuCapNhatCongThuc
{
    /// <summary>
    /// ID duy nhất của log lịch sử (Primary Key)
    /// </summary>
    public int LichSuId { get; set; }

    /// <summary>
    /// ID của món được cập nhật (Foreign Key đến bảng Mon)
    /// </summary>
    public int MonId { get; set; }

    /// <summary>
    /// Tên món (join từ bảng Mon)
    /// </summary>
    public string? TenMon { get; set; }

    /// <summary>
    /// Thời điểm thực hiện cập nhật
    /// Tự động lấy từ DateTime.Now khi insert
    /// </summary>
    public DateTime NgayCapNhat { get; set; }

    /// <summary>
    /// Tên người thực hiện cập nhật
    /// Lấy từ Environment.UserName (Windows username)
    /// Có thể thay bằng user đăng nhập nếu có authentication
    /// </summary>
    public string? NguoiCapNhat { get; set; }

    /// <summary>
    /// Mô tả ngắn gọn về thay đổi
    /// Ví dụ: "Thêm nguyên liệu 'Cà phê' với định lượng 0.02 kg"
    ///        "Cập nhật nguyên liệu 'Sữa': định lượng 0.15 lít, hao hụt 10%"
    ///        "Xóa nguyên liệu 'Đường' khỏi công thức"
    /// </summary>
    public string? NoiDungCapNhat { get; set; }

    // ========================================
    // === Computed properties (read-only) ===
    // ========================================

    /// <summary>
    /// Hiển thị ngày giờ cập nhật theo định dạng Việt Nam
    /// Ví dụ: "03/05/2026 14:30:45"
    /// </summary>
    public string NgayCapNhatHienThi => NgayCapNhat.ToString("dd/MM/yyyy HH:mm:ss");

    /// <summary>
    /// Hiển thị thông tin đầy đủ trong một dòng
    /// Ví dụ: "[03/05/2026 14:30:45] Admin: Thêm nguyên liệu 'Cà phê' với định lượng 0.02 kg"
    /// </summary>
    public string ThongTinDayDu => $"[{NgayCapNhatHienThi}] {NguoiCapNhat}: {NoiDungCapNhat}";
}
