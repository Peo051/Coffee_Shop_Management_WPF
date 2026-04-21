using System.Windows;
using System.Windows.Controls;

namespace CoffeeShop.Wpf.Helpers
{
    /// <summary>
    /// Lớp hỗ trợ hiển thị thông báo cho người dùng.
    /// Message ngắn dùng MessageBox, message dài dùng custom dialog có scroll.
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// Hiển thị thông báo lỗi (có scroll nếu nội dung dài)
        /// </summary>
        /// <param name="message">Nội dung thông báo lỗi</param>
        public static void ShowError(string message)
        {
            ShowScrollableMessage(message, "Lỗi", MessageBoxImage.Error);
        }

        /// <summary>
        /// Hiển thị thông báo thành công (có scroll nếu nội dung dài)
        /// </summary>
        /// <param name="message">Nội dung thông báo thành công</param>
        public static void ShowSuccess(string message)
        {
            ShowScrollableMessage(message, "Thành công", MessageBoxImage.Information);
        }

        /// <summary>
        /// Hiển thị thông báo thông tin (có scroll nếu nội dung dài)
        /// </summary>
        /// <param name="message">Nội dung thông báo</param>
        public static void ShowInfo(string message)
        {
            ShowScrollableMessage(message, "Thông báo", MessageBoxImage.Information);
        }

        /// <summary>
        /// Hiển thị thông báo cảnh báo (có scroll nếu nội dung dài)
        /// </summary>
        /// <param name="message">Nội dung cảnh báo</param>
        public static void ShowWarning(string message)
        {
            ShowScrollableMessage(message, "Cảnh báo", MessageBoxImage.Warning);
        }

        /// <summary>
        /// Hiển thị hộp thoại xác nhận (có scroll nếu nội dung dài)
        /// </summary>
        /// <param name="message">Nội dung câu hỏi xác nhận</param>
        /// <returns>True nếu người dùng chọn Yes, False nếu chọn No</returns>
        public static bool ShowConfirm(string message)
        {
            // Message ngắn → dùng MessageBox cho nhanh
            if (message.Length < 150 && !message.Contains('\n'))
            {
                MessageBoxResult result = MessageBox.Show(
                    message,
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );
                return result == MessageBoxResult.Yes;
            }

            // Message dài → custom dialog có scroll
            var dialog = new Window
            {
                Title = "Xác nhận",
                Width = 460,
                MaxHeight = 500,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var mainPanel = new DockPanel { Margin = new Thickness(20) };

            // Nút cố định bên dưới
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };
            DockPanel.SetDock(buttonPanel, Dock.Bottom);

            var yesButton = new Button { Content = "Có", Width = 90, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
            var noButton = new Button { Content = "Không", Width = 90, IsCancel = true };

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);
            mainPanel.Children.Add(buttonPanel);

            // Nội dung cuộn được
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 400
            };

            scrollViewer.Content = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13
            };

            mainPanel.Children.Add(scrollViewer);

            yesButton.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
            noButton.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };

            dialog.Content = mainPanel;
            return dialog.ShowDialog() == true;
        }

        /// <summary>
        /// Hiển thị message: ngắn dùng MessageBox, dài dùng custom dialog có scroll
        /// </summary>
        private static void ShowScrollableMessage(string message, string title, MessageBoxImage icon)
        {
            // Message ngắn → dùng MessageBox cho nhẹ
            if (message.Length < 200 && !message.Contains('\n'))
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, icon);
                return;
            }

            // Message dài → custom dialog có ScrollViewer
            var dialog = new Window
            {
                Title = title,
                Width = 460,
                MaxHeight = 500,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var mainPanel = new DockPanel { Margin = new Thickness(20) };

            // Nút OK cố định bên dưới, không bị cuộn
            var okButton = new Button
            {
                Content = "OK",
                Width = 90,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0),
                IsDefault = true,
                IsCancel = true
            };
            DockPanel.SetDock(okButton, Dock.Bottom);
            mainPanel.Children.Add(okButton);

            // Nội dung cuộn được khi dài
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 420
            };

            scrollViewer.Content = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13
            };

            mainPanel.Children.Add(scrollViewer);

            okButton.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }
    }
}
