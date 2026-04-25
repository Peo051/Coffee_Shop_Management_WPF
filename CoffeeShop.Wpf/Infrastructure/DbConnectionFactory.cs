using System.Configuration;

namespace CoffeeShop.Wpf.Infrastructure;

public static class DbConnectionFactory
{
    public static string ConnectionString { get; private set; } = string.Empty;

    public static void InitializeFromConfig()
    {
        // Đọc connection string từ App.config thay vì hard-code trong mã nguồn
        // Cách này giúp khi đổi máy tính, chỉ cần đổi chuỗi kết nối trong App.config là chạy được.
        var connectionString = ConfigurationManager.ConnectionStrings["CoffeeShopDb"]?.ConnectionString;
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Không tìm thấy chuỗi kết nối 'CoffeeShopDb' trong App.config. Vui lòng kiểm tra lại cấu hình.");
        }

        ConnectionString = connectionString;
    }

    public static void Configure(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string không được để trống.", nameof(connectionString));
        }

        ConnectionString = connectionString;
    }
}
