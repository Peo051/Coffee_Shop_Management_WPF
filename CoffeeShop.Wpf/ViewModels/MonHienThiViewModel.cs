using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.ViewModels;

/// <summary>
/// Wrapper class cho Mon để hiển thị giá theo size đã chọn
/// </summary>
public sealed class MonHienThiViewModel : BaseViewModel
{
    private readonly Mon _mon;
    private string _kichCoHienTai = "Mặc định";

    public MonHienThiViewModel(Mon mon)
    {
        _mon = mon;
    }

    public Mon Mon => _mon;

    // Expose các properties của Mon
    public int MonId => _mon.MonId;
    public string TenMon => _mon.TenMon;
    public int DanhMucId => _mon.DanhMucId;
    public string? TenDanhMuc => _mon.TenDanhMuc;
    public int TonKho => _mon.TonKho;
    public int TonKhoToiThieu => _mon.TonKhoToiThieu;
    public string? HinhAnhPath => _mon.HinhAnhPath;
    public bool IsActive => _mon.IsActive;
    public DateTime CreatedAt => _mon.CreatedAt;
    public bool SapHetHang => _mon.SapHetHang;
    public string TrangThaiTonKhoHienThi => _mon.TrangThaiTonKhoHienThi;

    /// <summary>
    /// Giá hiển thị theo size đã chọn
    /// </summary>
    public decimal DonGia => _mon.DonGia + GetPhuThuKichCo(_kichCoHienTai);

    /// <summary>
    /// Cập nhật size hiện tại và notify thay đổi giá
    /// </summary>
    public void UpdateKichCo(string kichCo)
    {
        if (_kichCoHienTai != kichCo)
        {
            _kichCoHienTai = kichCo;
            OnPropertyChanged(nameof(DonGia));
        }
    }

    private static decimal GetPhuThuKichCo(string? kichCo) => kichCo switch
    {
        "L" => 5000m,
        "XL" => 10000m,
        _ => 0m
    };
}
