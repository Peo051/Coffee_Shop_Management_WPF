namespace CoffeeShop.Wpf.Models;

public sealed class CanhBaoTonKhoThapDong
{
    public int MonId { get; init; }

    public string TenMon { get; init; } = string.Empty;

    public int DanhMucId { get; init; }

    public string TenDanhMuc { get; init; } = string.Empty;

    public int TonKho { get; init; }

    public int MucCanhBaoTonKho { get; init; }

    /// <summary>
    /// Loại: "Món" hoặc "Nguyên liệu"
    /// </summary>
    public string LoaiHangHoa { get; init; } = "Món";

    /// <summary>
    /// Đơn vị tính (cho nguyên liệu: kg, lít, v.v.; cho món: phần/ly)
    /// </summary>
    public string DonViTinh { get; init; } = "phần";

    public int SoLuongCanBoSung => MucCanhBaoTonKho - TonKho < 0 ? 0 : MucCanhBaoTonKho - TonKho;
}

