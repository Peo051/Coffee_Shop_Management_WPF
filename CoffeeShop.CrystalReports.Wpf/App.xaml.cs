using System;
using System.Windows;
using CoffeeShop.CrystalReports.Wpf.Infrastructure;

namespace CoffeeShop.CrystalReports.Wpf
{
    /// <summary>
    /// Khởi tạo ứng dụng WPF (Crystal Reports module).
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Đọc connection string một lần lúc khởi động.
            // Nếu thiếu sẽ ném lỗi rõ ràng để người dùng biết phải sửa App.config.
            try
            {
                AppConfig.LoadConnectionString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không tải được cấu hình kết nối CSDL.\n\nChi tiết: " + ex.Message,
                    "Lỗi cấu hình",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(-1);
            }
        }
    }
}
