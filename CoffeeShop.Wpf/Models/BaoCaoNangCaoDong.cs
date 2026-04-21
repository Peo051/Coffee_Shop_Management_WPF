namespace CoffeeShop.Wpf.Models;

public sealed class BaoCaoNangCaoDong
{
    public int MonId { get; set; }

    public string TenMon { get; set; } = string.Empty;

    public int SoLuongBan { get; set; }

    public decimal DoanhThuGop { get; set; }

    public decimal GiaBanTrungBinh { get; set; }
}
