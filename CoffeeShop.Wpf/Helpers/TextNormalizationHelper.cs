namespace CoffeeShop.Wpf.Helpers;

/// <summary>
/// Helper chuẩn hóa chuỗi tiếng Việt bị lỗi mã hóa (UTF-8 bị đọc sai thành Latin-1).
/// Dùng để hiển thị đúng dữ liệu cũ trong database chưa kịp chạy migration.
/// </summary>
public static class TextNormalizationHelper
{
    /// <summary>
    /// Bảng mapping chuỗi lỗi → chuỗi đúng.
    /// Thứ tự: chuỗi dài trước, ngắn sau để tránh replace nhầm.
    /// </summary>
    private static readonly (string broken, string correct)[] Mappings =
    [
        // Hình thức thanh toán
        ("Tiá»\u0081n máº·t", "Tiền mặt"),
        ("Tiá»n máº·t", "Tiền mặt"),
        ("Chuyá»\u0083n khoáº£n", "Chuyển khoản"),
        ("Chuyá»ƒn khoáº£n", "Chuyển khoản"),
        ("VÃ\u00AD Ä\u0091iá»\u0087n tá»­", "Ví điện tử"),
        ("VÃ Ä'iá»‡n tá»", "Ví điện tử"),
        ("Tháº»", "Thẻ"),
        ("Tháº½", "Thẻ"),

        // Trạng thái thanh toán
        ("Ä\u0090Ã£ thanh toÃ¡n", "Đã thanh toán"),
        ("ÄÃ£ thanh toÃ¡n", "Đã thanh toán"),
        ("Ä\u0090Ã£ há»§y", "Đã hủy"),
        ("ÄÃ£ há»§y", "Đã hủy"),
        ("ChÆ°a thanh toÃ¡n", "Chưa thanh toán"),
    ];

    /// <summary>
    /// Chuẩn hóa chuỗi tiếng Việt bị lỗi encoding.
    /// Nếu input null → trả null. Nếu chuỗi đã đúng → giữ nguyên.
    /// </summary>
    public static string? Normalize(string? value)
    {
        if (value is null)
        {
            return null;
        }

        foreach (var (broken, correct) in Mappings)
        {
            if (value.Contains(broken))
            {
                value = value.Replace(broken, correct);
            }
        }

        return value;
    }

    /// <summary>
    /// Chuẩn hóa chuỗi, nếu null trả về chuỗi rỗng.
    /// </summary>
    public static string NormalizeOrEmpty(string? value)
    {
        return Normalize(value) ?? string.Empty;
    }
}
