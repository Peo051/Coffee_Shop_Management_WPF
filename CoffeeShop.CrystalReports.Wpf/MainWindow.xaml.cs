using System.Windows;

namespace CoffeeShop.CrystalReports.Wpf
{
    /// <summary>
    /// Cửa sổ chính, chứa duy nhất <see cref="Views.CrystalReportDoanhThuView"/>.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (Application.Current != null && Application.Current.MainWindow == null)
            {
                Application.Current.MainWindow = this;
            }

            InitializeComponent();
        }
    }
}
