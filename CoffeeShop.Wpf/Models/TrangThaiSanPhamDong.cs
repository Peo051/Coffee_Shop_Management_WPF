namespace CoffeeShop.Wpf.Models;

public sealed class TrangThaiSanPhamDong
{
    public int MonId { get; init; }

    public string TenMon { get; init; } = string.Empty;

    public int DanhMucId { get; init; }

    public string TenDanhMuc { get; init; } = string.Empty;

    public int TonKho { get; init; }

    public int MucCanhBaoTonKho { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public bool IsTonKhoThap => TonKho <= MucCanhBaoTonKho;
}

