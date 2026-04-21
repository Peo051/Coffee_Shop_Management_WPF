using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoffeeShop.Wpf.Models;

public sealed class ChiTietHoaDonBanHienThi : INotifyPropertyChanged
{
    private int _soLuong;
    private decimal _donGiaBan;

    public int MonId { get; set; }

    public string TenMon { get; set; } = string.Empty;

    public int SoLuong
    {
        get => _soLuong;
        set
        {
            if (_soLuong != value)
            {
                _soLuong = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThanhTien));
            }
        }
    }

    public decimal DonGiaBan
    {
        get => _donGiaBan;
        set
        {
            if (_donGiaBan != value)
            {
                _donGiaBan = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThanhTien));
            }
        }
    }

    public decimal ThanhTien => SoLuong * DonGiaBan;

    // === Size & Ghi chú ===

    /// <summary>Kích cỡ: Nhỏ, Vừa, Lớn</summary>
    public string KichCo { get; set; } = "Mặc định";

    /// <summary>Phụ thu theo kích cỡ</summary>
    public decimal PhuThuKichCo { get; set; }

    /// <summary>Ghi chú riêng cho món (ít đá, không đường...)</summary>
    public string? GhiChuMon { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
