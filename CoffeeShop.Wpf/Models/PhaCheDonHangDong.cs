using CoffeeShop.Wpf.ViewModels;

namespace CoffeeShop.Wpf.Models;

public sealed class PhaCheDonHangDong : BaseViewModel
{
    private int _hoaDonBanId;
    private int? _soThuTuGoiMon;
    private DateTime _ngayBan;
    private string _tenNhanVien = string.Empty;
    private string _tenKhachHang = "Khách lẻ";
    private string? _tenBan;
    private string? _tenKhuVuc;
    private decimal _thanhToan;
    private string _trangThaiPhaChe = TrangThaiPhaCheConst.ChoPhaChe;
    private string _danhSachMonTomTat = string.Empty;
    private DateTime? _thoiGianBatDauPhaChe;
    private DateTime? _thoiGianHoanThanhPhaChe;
    private DateTime? _thoiGianGiaoKhach;

    public int HoaDonBanId
    {
        get => _hoaDonBanId;
        set
        {
            if (SetProperty(ref _hoaDonBanId, value))
            {
                OnPropertyChanged(nameof(MaHoaDonHienThi));
                OnPropertyChanged(nameof(SoGoiMonHienThi));
            }
        }
    }

    public string MaHoaDonHienThi => $"HD{HoaDonBanId:D5}";

    public int? SoThuTuGoiMon
    {
        get => _soThuTuGoiMon;
        set
        {
            if (SetProperty(ref _soThuTuGoiMon, value))
            {
                OnPropertyChanged(nameof(SoGoiMonHienThi));
            }
        }
    }

    public string SoGoiMonHienThi => SoThuTuGoiMon.HasValue
        ? SoThuTuGoiMon.Value.ToString("D3")
        : HoaDonBanId.ToString("D3");

    public DateTime NgayBan
    {
        get => _ngayBan;
        set => SetProperty(ref _ngayBan, value);
    }

    public string TenNhanVien
    {
        get => _tenNhanVien;
        set => SetProperty(ref _tenNhanVien, value);
    }

    public string TenKhachHang
    {
        get => _tenKhachHang;
        set => SetProperty(ref _tenKhachHang, value);
    }

    public string? TenBan
    {
        get => _tenBan;
        set
        {
            if (SetProperty(ref _tenBan, value))
            {
                OnPropertyChanged(nameof(ViTriHienThi));
            }
        }
    }

    public string? TenKhuVuc
    {
        get => _tenKhuVuc;
        set
        {
            if (SetProperty(ref _tenKhuVuc, value))
            {
                OnPropertyChanged(nameof(ViTriHienThi));
            }
        }
    }

    public string ViTriHienThi =>
        $"{(string.IsNullOrWhiteSpace(TenBan) ? "N/A" : TenBan)} / {(string.IsNullOrWhiteSpace(TenKhuVuc) ? "N/A" : TenKhuVuc)}";

    public decimal ThanhToan
    {
        get => _thanhToan;
        set => SetProperty(ref _thanhToan, value);
    }

    public string TrangThaiPhaChe
    {
        get => _trangThaiPhaChe;
        set
        {
            if (SetProperty(ref _trangThaiPhaChe, value))
            {
                OnPropertyChanged(nameof(TrangThaiPhaCheHienThi));
            }
        }
    }

    public string TrangThaiPhaCheHienThi => TrangThaiPhaCheConst.ToDisplayName(TrangThaiPhaChe);

    public string DanhSachMonTomTat
    {
        get => _danhSachMonTomTat;
        set => SetProperty(ref _danhSachMonTomTat, value);
    }

    public DateTime? ThoiGianBatDauPhaChe
    {
        get => _thoiGianBatDauPhaChe;
        set => SetProperty(ref _thoiGianBatDauPhaChe, value);
    }

    public DateTime? ThoiGianHoanThanhPhaChe
    {
        get => _thoiGianHoanThanhPhaChe;
        set => SetProperty(ref _thoiGianHoanThanhPhaChe, value);
    }

    public DateTime? ThoiGianGiaoKhach
    {
        get => _thoiGianGiaoKhach;
        set => SetProperty(ref _thoiGianGiaoKhach, value);
    }
}
