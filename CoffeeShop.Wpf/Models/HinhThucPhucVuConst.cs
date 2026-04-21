namespace CoffeeShop.Wpf.Models;

/// <summary>
/// Hằng số hình thức phục vụ: Uống tại quán / Mang đi.
/// </summary>
public static class HinhThucPhucVuConst
{
    public const string UongTaiQuan = "UongTaiQuan";
    public const string MangDi = "MangDi";

    /// <summary>Chuyển mã sang tên hiển thị tiếng Việt.</summary>
    public static string ToDisplayName(string? hinhThuc) => hinhThuc switch
    {
        UongTaiQuan => "Uống tại quán",
        MangDi => "Mang đi",
        _ => "Uống tại quán"
    };
}
