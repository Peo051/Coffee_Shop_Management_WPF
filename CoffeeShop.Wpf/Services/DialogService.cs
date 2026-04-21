using System.Windows;
using System.Windows.Controls;

namespace CoffeeShop.Wpf.Services;

public sealed class DialogService : IDialogService
{
    /// <summary>
    /// Hiển thị popup xác nhận có ScrollViewer cho nội dung dài.
    /// Nút Xác nhận / Hủy luôn nhìn thấy, không bị cuộn theo.
    /// </summary>
    public bool ShowConfirmation(string message, string title)
    {
        // Message ngắn → dùng MessageBox cho nhanh
        if (message.Length < 150 && !message.Contains('\n'))
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        // Message dài → custom dialog có scroll
        var dialog = new Window
        {
            Title = title,
            Width = 480,
            MaxHeight = 500,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };

        var mainPanel = new DockPanel { Margin = new Thickness(20) };

        // Nút xác nhận / hủy cố định bên dưới
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 15, 0, 0)
        };
        DockPanel.SetDock(buttonPanel, Dock.Bottom);

        var yesButton = new Button
        {
            Content = "Xác nhận",
            Width = 100,
            Margin = new Thickness(0, 0, 10, 0),
            IsDefault = true
        };

        var noButton = new Button
        {
            Content = "Hủy",
            Width = 100,
            IsCancel = true
        };

        buttonPanel.Children.Add(yesButton);
        buttonPanel.Children.Add(noButton);
        mainPanel.Children.Add(buttonPanel);

        // Nội dung cuộn được khi dài
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

        yesButton.Click += (s, e) =>
        {
            dialog.DialogResult = true;
            dialog.Close();
        };

        noButton.Click += (s, e) =>
        {
            dialog.DialogResult = false;
            dialog.Close();
        };

        dialog.Content = mainPanel;
        return dialog.ShowDialog() == true;
    }

    public string? ShowPasswordInput(string message, string title)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            MaxHeight = 350,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };

        var mainPanel = new DockPanel { Margin = new Thickness(20) };

        // Nút nằm dưới cùng, ngoài vùng scroll
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 15, 0, 0)
        };
        DockPanel.SetDock(buttonPanel, Dock.Bottom);

        var okButton = new Button
        {
            Content = "Xác nhận",
            Width = 100,
            Margin = new Thickness(0, 0, 10, 0),
            IsDefault = true
        };

        var cancelButton = new Button
        {
            Content = "Hủy",
            Width = 100,
            IsCancel = true
        };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        mainPanel.Children.Add(buttonPanel);

        // Nội dung cuộn được
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            MaxHeight = 200
        };

        var contentPanel = new StackPanel();
        contentPanel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        });

        var passwordBox = new PasswordBox();
        contentPanel.Children.Add(passwordBox);

        scrollViewer.Content = contentPanel;
        mainPanel.Children.Add(scrollViewer);

        okButton.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                MessageBox.Show("Mật khẩu không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            dialog.Tag = passwordBox.Password;
            dialog.DialogResult = true;
            dialog.Close();
        };

        cancelButton.Click += (s, e) =>
        {
            dialog.DialogResult = false;
            dialog.Close();
        };

        dialog.Content = mainPanel;

        return dialog.ShowDialog() == true ? dialog.Tag as string : null;
    }

    /// <summary>
    /// Hiển thị thông báo lỗi bằng custom dialog có ScrollViewer cho nội dung dài.
    /// Nút OK luôn nhìn thấy, không bị cuộn theo.
    /// </summary>
    public void ShowError(string message, string title = "Lỗi")
    {
        ShowScrollableDialog(message, title, MessageBoxImage.Error);
    }

    /// <summary>
    /// Hiển thị thông báo thông tin bằng custom dialog có ScrollViewer cho nội dung dài.
    /// </summary>
    public void ShowInformation(string message, string title = "Thông báo")
    {
        ShowScrollableDialog(message, title, MessageBoxImage.Information);
    }

    /// <summary>
    /// Custom dialog với ScrollViewer cho nội dung và nút OK cố định bên dưới.
    /// </summary>
    private static void ShowScrollableDialog(string message, string title, MessageBoxImage icon)
    {
        // Nếu message ngắn, dùng MessageBox bình thường cho nhẹ
        if (message.Length < 200 && !message.Contains('\n'))
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
            return;
        }

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

        // Nút OK cố định bên dưới
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

        // Nội dung cuộn được
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

        okButton.Click += (s, e) =>
        {
            dialog.DialogResult = true;
            dialog.Close();
        };

        dialog.Content = mainPanel;
        dialog.ShowDialog();
    }
}
