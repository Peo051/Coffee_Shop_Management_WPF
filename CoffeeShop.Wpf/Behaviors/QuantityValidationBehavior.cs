using System.Windows;
using System.Windows.Controls;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace CoffeeShop.Wpf.Behaviors;

/// <summary>
/// Behavior để validate số lượng món trong hóa đơn
/// Kiểm tra số lượng > 0 và không vượt tồn kho
/// </summary>
public sealed class QuantityValidationBehavior : Behavior<TextBox>
{
    private int _previousValue;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.LostFocus += OnLostFocus;
        AssociatedObject.Loaded += OnLoaded;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.LostFocus -= OnLostFocus;
        AssociatedObject.Loaded -= OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Lưu giá trị ban đầu
        if (AssociatedObject.DataContext is ChiTietHoaDonBanHienThi chiTiet)
        {
            _previousValue = chiTiet.SoLuong;
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject.DataContext is not ChiTietHoaDonBanHienThi chiTiet)
        {
            return;
        }

        // Lưu giá trị cũ trước khi validate
        _previousValue = chiTiet.SoLuong;

        // Validate số lượng
        if (string.IsNullOrWhiteSpace(AssociatedObject.Text) || 
            !int.TryParse(AssociatedObject.Text, out var soLuong) || 
            soLuong <= 0)
        {
            // Nếu không hợp lệ, đặt lại về giá trị cũ
            AssociatedObject.Text = _previousValue.ToString();
            ShowWarning("Số lượng phải là số nguyên lớn hơn 0.");
            return;
        }

        // Tìm ViewModel để kiểm tra tồn kho
        var viewModel = FindViewModel();
        if (viewModel == null)
        {
            // Không tìm thấy ViewModel, chấp nhận giá trị mới
            chiTiet.SoLuong = soLuong;
            return;
        }

        // Kiểm tra tồn kho
        var mon = viewModel.Mons.FirstOrDefault(m => m.MonId == chiTiet.MonId);
        if (mon != null)
        {
            // Tính tổng số lượng cùng MonId trong hóa đơn (không bao gồm dòng hiện tại)
            var tongSoLuongCungMonKhac = viewModel.ChiTietLines
                .Where(x => x.MonId == chiTiet.MonId && x != chiTiet)
                .Sum(x => x.SoLuong);

            if (tongSoLuongCungMonKhac + soLuong > mon.TonKho)
            {
                // Vượt tồn kho, đặt lại về giá trị cũ
                AssociatedObject.Text = _previousValue.ToString();
                ShowWarning($"Sản phẩm '{mon.TenMon}' không đủ tồn kho.\n" +
                           $"Tồn kho hiện tại: {mon.TonKho}\n" +
                           $"Đã chọn (các dòng khác): {tongSoLuongCungMonKhac}");
                return;
            }
        }

        // Cập nhật số lượng mới
        chiTiet.SoLuong = soLuong;
        _previousValue = soLuong;
    }

    private HoaDonBanViewModel? FindViewModel()
    {
        // Tìm ViewModel từ cây visual
        DependencyObject? current = AssociatedObject;
        while (current != null)
        {
            if (current is FrameworkElement element && element.DataContext is HoaDonBanViewModel vm)
            {
                return vm;
            }
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static void ShowWarning(string message)
    {
        MessageBox.Show(
            message,
            "Cảnh báo",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}
