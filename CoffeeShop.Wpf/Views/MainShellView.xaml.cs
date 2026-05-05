using System.Windows;
using System.Windows.Controls;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.ViewModels;

namespace CoffeeShop.Wpf.Views;

public partial class MainShellView : UserControl
{
    public MainShellView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Xử lý click vào group header để toggle expand/collapse
    /// </summary>
    private void MenuGroupHeader_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MenuGroupModel group)
        {
            // Toggle trạng thái expand/collapse
            group.IsExpanded = !group.IsExpanded;

            // Tùy chọn: Đóng các nhóm khác (chỉ mở 1 nhóm tại một thời điểm)
            if (group.IsExpanded && DataContext is MainShellViewModel viewModel)
            {
                foreach (var otherGroup in viewModel.MenuGroups)
                {
                    if (otherGroup != group)
                    {
                        otherGroup.IsExpanded = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Xử lý click vào menu item để navigate
    /// </summary>
    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && 
            button.DataContext is MenuItemModel menuItem &&
            DataContext is MainShellViewModel viewModel)
        {
            viewModel.SelectedMenuItem = menuItem;
        }
    }
}
