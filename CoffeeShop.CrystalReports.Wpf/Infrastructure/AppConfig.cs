using System;
using System.Configuration;
using System.Data.SqlClient;

namespace CoffeeShop.CrystalReports.Wpf.Infrastructure
{
    /// <summary>
    /// Bộ đọc cấu hình ứng dụng.
    /// Lấy connection string CoffeeShopDb từ App.config (dùng chung CSDL với project chính).
    /// Cho phép code phía sau lấy ra Server / Database / User / Password để feed vào
    /// Crystal Reports thông qua TableLogOnInfo.
    /// </summary>
    internal static class AppConfig
    {
        /// <summary>Connection string đầy đủ.</summary>
        public static string ConnectionString { get; private set; } = string.Empty;

        /// <summary>Server name hoặc địa chỉ SQL.</summary>
        public static string Server { get; private set; } = string.Empty;

        /// <summary>Tên database.</summary>
        public static string Database { get; private set; } = string.Empty;

        /// <summary>User SQL Server (rỗng nếu dùng Windows Authentication).</summary>
        public static string UserId { get; private set; } = string.Empty;

        /// <summary>Mật khẩu SQL Server (rỗng nếu dùng Windows Authentication).</summary>
        public static string Password { get; private set; } = string.Empty;

        /// <summary>True nếu connection string đang dùng Windows Authentication.</summary>
        public static bool IntegratedSecurity { get; private set; }

        /// <summary>
        /// Đọc connection string "CoffeeShopDb" và phân giải các thành phần.
        /// Gọi 1 lần lúc App khởi động.
        /// </summary>
        public static void LoadConnectionString()
        {
            var settings = ConfigurationManager.ConnectionStrings["CoffeeShopDb"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Không tìm thấy connection string 'CoffeeShopDb' trong App.config. " +
                    "Kiểm tra file App.config của project CoffeeShop.CrystalReports.Wpf.");
            }

            ConnectionString = settings.ConnectionString;

            var builder = new SqlConnectionStringBuilder(settings.ConnectionString);
            Server = builder.DataSource ?? string.Empty;
            Database = builder.InitialCatalog ?? string.Empty;
            IntegratedSecurity = builder.IntegratedSecurity;
            UserId = builder.UserID ?? string.Empty;
            Password = builder.Password ?? string.Empty;
        }
    }
}
