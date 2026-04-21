namespace CoffeeShop.Wpf.Models;

public sealed class CanhBaoTonKhoThapDong
{
    public int MonId { get; init; }

    public string TenMon { get; init; } = string.Empty;

    public int DanhMucId { get; init; }

    public string TenDanhMuc { get; init; } = string.Empty;

    public int TonKho { get; init; }

    public int MucCanhBaoTonKho { get; init; }

    public int SoLuongCanBoSung => MucCanhBaoTonKho - TonKho < 0 ? 0 : MucCanhBaoTonKho - TonKho;
}

