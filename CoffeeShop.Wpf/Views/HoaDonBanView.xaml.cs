using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CoffeeShop.Wpf.Views;

public partial class HoaDonBanView : UserControl
{
    public HoaDonBanView()
    {
        InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            // Mở link trong browser mặc định
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch
        {
            // Ignore errors opening browser
        }
    }
}
