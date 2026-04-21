namespace CoffeeShop.Wpf.Models;

/// <summary>Thống kê doanh thu theo hình thức thanh toán</summary>
public sealed class ThongKeDoanhThuTheoHTTT
{
    public string HinhThucThanhToan { get; set; } = string.Empty;

    public int SoHoaDon { get; set; }

    public decimal TongDoanhThu { get; set; }
}

