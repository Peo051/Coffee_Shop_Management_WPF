namespace CoffeeShop.Wpf.Models;

/// <summary>
/// Hằng số trạng thái pha chế cho hóa đơn bán.
/// Quản lý quy trình xử lý nước sau khi khách đã thanh toán.
/// </summary>
public static class TrangThaiPhaCheConst
{
    public const string ChoPhaChe = "ChoPhaChe";
    public const string DangPhaChe = "DangPhaChe";
    public const string DaHoanThanh = "DaHoanThanh";
    public const string DaGiaoKhach = "DaGiaoKhach";
    public const string DaHuy = "DaHuy";

    /// <summary>
    /// Chuyển mã trạng thái sang tên hiển thị tiếng Việt.
    /// </summary>
    public static string ToDisplayName(string? trangThai) => trangThai switch
    {
        ChoPhaChe => "Chờ pha chế",
        DangPhaChe => "Đang pha chế",
        DaHoanThanh => "Đã hoàn thành",
        DaGiaoKhach => "Đã giao khách",
        DaHuy => "Đã hủy",
        _ => trangThai ?? "Không xác định"
    };
}
