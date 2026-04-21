namespace CoffeeShop.Wpf.Models;

public sealed class BaoCaoTongHopModel
{
    public IReadOnlyList<BaoCaoDonGianDong> BaoCaoDonGian { get; init; } = [];

    public IReadOnlyList<BaoCaoNangCaoDong> BaoCaoNangCao { get; init; } = [];

    public int TongSoHoaDon { get; init; }

    public decimal TongDoanhThuThuan { get; init; }

    public int TongSoLuongBan { get; init; }

    public decimal TongDoanhThuSanPham { get; init; }
}
