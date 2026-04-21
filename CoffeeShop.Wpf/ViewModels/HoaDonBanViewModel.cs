using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class HoaDonBanViewModel : BaseViewModel
{
    private readonly IHoaDonBanService _hoaDonBanService;
    private readonly IMonService _monService;
    private readonly IBanService _banService;
    private readonly ICaLamViecService _caLamViecService;
    private readonly IKhachHangService _khachHangService;
    private readonly IKhuyenMaiService _khuyenMaiService;
    private readonly SessionService _sessionService;

    private readonly RelayCommand _themDongCommand;
    private readonly RelayCommand _xoaDongCommand;
    private readonly RelayCommand _luuHoaDonCommand;
    private readonly RelayCommand _lamMoiCommand;
    private readonly RelayCommand<ChiTietHoaDonBanHienThi> _tangSoLuongCommand;
    private readonly RelayCommand<ChiTietHoaDonBanHienThi> _giamSoLuongCommand;
    private readonly RelayCommand<ChiTietHoaDonBanHienThi> _xoaMonCommand;
    private readonly RelayCommand<Mon> _themMonNhanhCommand;

    private string _tuKhoaTimMon = string.Empty;
    private string _selectedDanhMuc = "Tất cả";

    private Mon? _selectedMon;
    private Ban? _selectedBan;
    private KhachHang? _selectedKhachHang;
    private KhuyenMai? _selectedKhuyenMai;

    private string _soLuongBan = "1";
    private string _donGiaBan = string.Empty;
    private string _giamGia = "0";
    private string _thongTinCaLamViec = "Chưa mở ca làm việc.";

    private ChiTietHoaDonBanHienThi? _selectedDongChiTiet;
    private decimal _tongTien;
    private decimal _soTienGiamKhuyenMai;
    private decimal _soTienGiamTuDiem;
    private decimal _thanhToan;
    private int _diemCongDuKien;
    private string _diemSuDungText = "0";

    // === Thanh toán nâng cao ===
    private string _hinhThucThanhToanDuocChon = "Tiền mặt";
    private string _tienKhachDuaText = string.Empty;
    private decimal _tienThoiLai;
    private string _maGiaoDich = string.Empty;
    private string _ghiChuThanhToan = string.Empty;
    private string _ghiChuHoaDon = string.Empty;

    // === Size & Ghi chú món ===
    private string _kichCoDuocChon = "Mặc định";
    private string _ghiChuMon = string.Empty;

    // === Hình thức phục vụ ===
    private string _hinhThucPhucVu = HinhThucPhucVuConst.UongTaiQuan;

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public HoaDonBanViewModel(
        IHoaDonBanService hoaDonBanService,
        IMonService monService,
        IBanService banService,
        ICaLamViecService caLamViecService,
        IKhachHangService khachHangService,
        IKhuyenMaiService khuyenMaiService,
        SessionService sessionService)
    {
        _hoaDonBanService = hoaDonBanService;
        _monService = monService;
        _banService = banService;
        _caLamViecService = caLamViecService;
        _khachHangService = khachHangService;
        _khuyenMaiService = khuyenMaiService;
        _sessionService = sessionService;

        Mons = new ObservableCollection<Mon>();
        MonsHienThi = new ObservableCollection<Mon>();
        DanhMucMons = new ObservableCollection<string>();
        Bans = new ObservableCollection<Ban>();
        KhachHangs = new ObservableCollection<KhachHang>();
        KhuyenMais = new ObservableCollection<KhuyenMai>();
        ChiTietLines = new ObservableCollection<ChiTietHoaDonBanHienThi>();

        _themDongCommand = new RelayCommand(ExecuteThemDong, CanExecuteThemDong);
        _xoaDongCommand = new RelayCommand(ExecuteXoaDong, CanExecuteXoaDong);
        _luuHoaDonCommand = new RelayCommand(ExecuteLuuHoaDon, CanExecuteLuuHoaDon);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
        _tangSoLuongCommand = new RelayCommand<ChiTietHoaDonBanHienThi>(ExecuteTangSoLuong);
        _giamSoLuongCommand = new RelayCommand<ChiTietHoaDonBanHienThi>(ExecuteGiamSoLuong);
        _xoaMonCommand = new RelayCommand<ChiTietHoaDonBanHienThi>(ExecuteXoaMon);
        _themMonNhanhCommand = new RelayCommand<Mon>(ExecuteThemMonNhanh);
    }

    public ObservableCollection<Mon> Mons { get; }

    public ObservableCollection<Mon> MonsHienThi { get; }

    public ObservableCollection<string> DanhMucMons { get; }

    public ObservableCollection<Ban> Bans { get; }

    public ObservableCollection<KhachHang> KhachHangs { get; }

    public ObservableCollection<KhuyenMai> KhuyenMais { get; }

    public ObservableCollection<ChiTietHoaDonBanHienThi> ChiTietLines { get; }

    /// <summary>Danh sách hình thức thanh toán hiển thị trên ComboBox</summary>
    public IReadOnlyList<string> DanhSachHinhThucThanhToan { get; } =
        ["Tiền mặt", "Chuyển khoản", "Thẻ", "Ví điện tử"];

    /// <summary>Danh sách kích cỡ đồ uống</summary>
    public IReadOnlyList<string> DanhSachKichCo { get; } =
        ["Mặc định", "M", "L", "XL"];

    /// <summary>Danh sách hình thức phục vụ (display name)</summary>
    public IReadOnlyList<string> DanhSachHinhThucPhucVu { get; } =
        ["Uống tại quán", "Mang đi"];

    public string HinhThucPhucVu
    {
        get => _hinhThucPhucVu;
        set => SetProperty(ref _hinhThucPhucVu, value);
    }

    public string HinhThucPhucVuHienThi
    {
        get => HinhThucPhucVuConst.ToDisplayName(HinhThucPhucVu);
        set
        {
            // Chuyển từ display name sang mã
            var ma = value switch
            {
                "Mang đi" => HinhThucPhucVuConst.MangDi,
                _ => HinhThucPhucVuConst.UongTaiQuan
            };
            HinhThucPhucVu = ma;
            OnPropertyChanged();
        }
    }

    public bool LaUongTaiQuan => HinhThucPhucVu == HinhThucPhucVuConst.UongTaiQuan;
    public bool LaMangDi => HinhThucPhucVu == HinhThucPhucVuConst.MangDi;

    public string TuKhoaTimMon
    {
        get => _tuKhoaTimMon;
        set
        {
            if (SetProperty(ref _tuKhoaTimMon, value))
            {
                ApplyMonFilter();
            }
        }
    }

    public string SelectedDanhMuc
    {
        get => _selectedDanhMuc;
        set
        {
            if (SetProperty(ref _selectedDanhMuc, value))
            {
                ApplyMonFilter();
            }
        }
    }

    public Mon? SelectedMon
    {
        get => _selectedMon;
        set
        {
            if (SetProperty(ref _selectedMon, value))
            {
                if (value is not null)
                {
                    DonGiaBan = (value.DonGia + GetPhuThuKichCo(KichCoDuocChon)).ToString("0.##", CultureInfo.CurrentCulture);
                }

                _themDongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public Ban? SelectedBan
    {
        get => _selectedBan;
        set
        {
            if (SetProperty(ref _selectedBan, value))
            {
                _luuHoaDonCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public KhachHang? SelectedKhachHang
    {
        get => _selectedKhachHang;
        set
        {
            if (SetProperty(ref _selectedKhachHang, value))
            {
                DiemSuDungText = "0";
                RecalculateTotals();
            }
        }
    }

    public KhuyenMai? SelectedKhuyenMai
    {
        get => _selectedKhuyenMai;
        set
        {
            if (SetProperty(ref _selectedKhuyenMai, value))
            {
                RecalculateTotals();
            }
        }
    }

    public string SoLuongBan
    {
        get => _soLuongBan;
        set
        {
            if (SetProperty(ref _soLuongBan, value))
            {
                _themDongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string DonGiaBan
    {
        get => _donGiaBan;
        set
        {
            if (SetProperty(ref _donGiaBan, value))
            {
                _themDongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string GiamGia
    {
        get => _giamGia;
        set
        {
            if (SetProperty(ref _giamGia, value))
            {
                RecalculateTotals();
            }
        }
    }

    public string ThongTinCaLamViec
    {
        get => _thongTinCaLamViec;
        private set => SetProperty(ref _thongTinCaLamViec, value);
    }

    public ChiTietHoaDonBanHienThi? SelectedDongChiTiet
    {
        get => _selectedDongChiTiet;
        set
        {
            if (SetProperty(ref _selectedDongChiTiet, value))
            {
                _xoaDongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public decimal TongTien
    {
        get => _tongTien;
        private set => SetProperty(ref _tongTien, value);
    }

    public decimal SoTienGiamKhuyenMai
    {
        get => _soTienGiamKhuyenMai;
        private set => SetProperty(ref _soTienGiamKhuyenMai, value);
    }

    public decimal SoTienGiamTuDiem
    {
        get => _soTienGiamTuDiem;
        private set => SetProperty(ref _soTienGiamTuDiem, value);
    }

    public string DiemSuDungText
    {
        get => _diemSuDungText;
        set
        {
            if (SetProperty(ref _diemSuDungText, value))
            {
                RecalculateTotals();
            }
        }
    }

    public decimal ThanhToan
    {
        get => _thanhToan;
        private set => SetProperty(ref _thanhToan, value);
    }

    public int DiemCongDuKien
    {
        get => _diemCongDuKien;
        private set => SetProperty(ref _diemCongDuKien, value);
    }

    // === Properties thanh toán nâng cao ===

    /// <summary>Hình thức thanh toán được chọn (binding ComboBox)</summary>
    public string HinhThucThanhToanDuocChon
    {
        get => _hinhThucThanhToanDuocChon;
        set
        {
            if (SetProperty(ref _hinhThucThanhToanDuocChon, value))
            {
                OnPropertyChanged(nameof(IsThanhToanTienMat));
                OnPropertyChanged(nameof(IsThanhToanKhongDungTienMat));

                if (IsThanhToanTienMat)
                {
                    MaGiaoDich = string.Empty;
                }
                else
                {
                    TienKhachDuaText = string.Empty;
                    TienThoiLai = 0;
                }

                RecalculateTienThoiLai();
            }
        }
    }

    /// <summary>True khi hình thức thanh toán là tiền mặt (dùng cho Visibility binding)</summary>
    public bool IsThanhToanTienMat =>
        string.Equals(_hinhThucThanhToanDuocChon, "Tiền mặt", StringComparison.OrdinalIgnoreCase);

    /// <summary>True khi không phải tiền mặt (dùng cho Visibility binding)</summary>
    public bool IsThanhToanKhongDungTienMat => !IsThanhToanTienMat;

    /// <summary>Tiền khách đưa (text binding)</summary>
    public string TienKhachDuaText
    {
        get => _tienKhachDuaText;
        set
        {
            if (SetProperty(ref _tienKhachDuaText, value))
            {
                RecalculateTienThoiLai();
            }
        }
    }

    /// <summary>Tiền thối lại (tự tính)</summary>
    public decimal TienThoiLai
    {
        get => _tienThoiLai;
        private set => SetProperty(ref _tienThoiLai, value);
    }

    /// <summary>Mã giao dịch (chuyển khoản, thẻ, ví)</summary>
    public string MaGiaoDich
    {
        get => _maGiaoDich;
        set => SetProperty(ref _maGiaoDich, value);
    }

    /// <summary>Ghi chú thanh toán</summary>
    public string GhiChuThanhToan
    {
        get => _ghiChuThanhToan;
        set => SetProperty(ref _ghiChuThanhToan, value);
    }

    /// <summary>Ghi chú hóa đơn</summary>
    public string GhiChuHoaDon
    {
        get => _ghiChuHoaDon;
        set => SetProperty(ref _ghiChuHoaDon, value);
    }

    // === Size & Ghi chú món ===

    /// <summary>Kích cỡ đồ uống được chọn</summary>
    public string KichCoDuocChon
    {
        get => _kichCoDuocChon;
        set
        {
            if (SetProperty(ref _kichCoDuocChon, value) && SelectedMon is not null)
            {
                DonGiaBan = (SelectedMon.DonGia + GetPhuThuKichCo(value)).ToString("0.##", CultureInfo.CurrentCulture);
            }
        }
    }

    /// <summary>Ghi chú riêng cho món (ít đá, không đường...)</summary>
    public string GhiChuMon
    {
        get => _ghiChuMon;
        set => SetProperty(ref _ghiChuMon, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        private set => SetProperty(ref _successMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                _themDongCommand.RaiseCanExecuteChanged();
                _xoaDongCommand.RaiseCanExecuteChanged();
                _luuHoaDonCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand ThemDongCommand => _themDongCommand;

    public ICommand XoaDongCommand => _xoaDongCommand;

    public ICommand LuuHoaDonCommand => _luuHoaDonCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public ICommand TangSoLuongCommand => _tangSoLuongCommand;

    public ICommand GiamSoLuongCommand => _giamSoLuongCommand;

    public ICommand XoaMonCommand => _xoaMonCommand;

    public ICommand ThemMonNhanhCommand => _themMonNhanhCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            await LoadMonAsync(cancellationToken);
            await LoadBanAsync(cancellationToken);
            await LoadKhachHangAsync(cancellationToken);
            await LoadKhuyenMaiAsync(cancellationToken);
            await LoadThongTinCaDangMoAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu hóa đơn bán: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteThemDong()
    {
        return !IsBusy
               && SelectedMon is not null
               && !string.IsNullOrWhiteSpace(SoLuongBan)
               && !string.IsNullOrWhiteSpace(DonGiaBan);
    }

    private bool CanExecuteXoaDong()
    {
        return !IsBusy && SelectedDongChiTiet is not null;
    }

    private bool CanExecuteLuuHoaDon()
    {
        return !IsBusy && ChiTietLines.Count > 0;
    }

    private void ExecuteThemDong()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (SelectedMon is null)
        {
            ErrorMessage = "Vui lòng chọn sản phẩm.";
            return;
        }

        if (!int.TryParse(SoLuongBan, out var soLuongBan) || soLuongBan <= 0)
        {
            ErrorMessage = "Số lượng bán phải là số nguyên lớn hơn 0.";
            return;
        }

        if (!TryParseDecimal(DonGiaBan, out var donGiaBan) || donGiaBan <= 0)
        {
            ErrorMessage = "Đơn giá bán phải là số lớn hơn 0.";
            return;
        }

        var kichCo = KichCoDuocChon ?? "Mặc định";
        var phuThu = GetPhuThuKichCo(kichCo);
        var ghiChu = string.IsNullOrWhiteSpace(GhiChuMon) ? null : GhiChuMon.Trim();

        // Tìm dòng cùng MonId + cùng KichCo + cùng GhiChuMon
        var dongDaTonTai = ChiTietLines.FirstOrDefault(x =>
            x.MonId == SelectedMon.MonId &&
            string.Equals(x.KichCo, kichCo, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.GhiChuMon ?? "", ghiChu ?? "", StringComparison.OrdinalIgnoreCase));

        // Kiểm tra tồn kho theo tổng số lượng cùng MonId
        var tongSoLuongCungMon = ChiTietLines
            .Where(x => x.MonId == SelectedMon.MonId)
            .Sum(x => x.SoLuong);

        if (tongSoLuongCungMon + soLuongBan > SelectedMon.TonKho)
        {
            ErrorMessage = $"Sản phẩm '{SelectedMon.TenMon}' không đủ tồn kho. Tồn kho hiện tại: {SelectedMon.TonKho}, đã chọn: {tongSoLuongCungMon}.";
            return;
        }

        if (dongDaTonTai is not null)
        {
            var index = ChiTietLines.IndexOf(dongDaTonTai);

            ChiTietLines[index] = new ChiTietHoaDonBanHienThi
            {
                MonId = dongDaTonTai.MonId,
                TenMon = dongDaTonTai.TenMon,
                SoLuong = dongDaTonTai.SoLuong + soLuongBan,
                DonGiaBan = dongDaTonTai.DonGiaBan,
                KichCo = dongDaTonTai.KichCo,
                PhuThuKichCo = dongDaTonTai.PhuThuKichCo,
                GhiChuMon = dongDaTonTai.GhiChuMon
            };

            RecalculateTotals();
            SuccessMessage = "Đã tăng số lượng món trong hóa đơn.";
        }
        else
        {
            ChiTietLines.Add(new ChiTietHoaDonBanHienThi
            {
                MonId = SelectedMon.MonId,
                TenMon = SelectedMon.TenMon,
                SoLuong = soLuongBan,
                DonGiaBan = donGiaBan,
                KichCo = kichCo,
                PhuThuKichCo = phuThu,
                GhiChuMon = ghiChu
            });

            RecalculateTotals();
            SuccessMessage = "Đã thêm dòng chi tiết bán.";
        }

        // Reset
        SoLuongBan = "1";
        DonGiaBan = string.Empty;
        KichCoDuocChon = "Mặc định";
        GhiChuMon = string.Empty;
        SelectedMon = null;

        _luuHoaDonCommand.RaiseCanExecuteChanged();
    }

    private void ExecuteXoaDong()
    {
        if (SelectedDongChiTiet is null)
        {
            return;
        }

        ChiTietLines.Remove(SelectedDongChiTiet);
        SelectedDongChiTiet = null;

        RecalculateTotals();

        _luuHoaDonCommand.RaiseCanExecuteChanged();
        SuccessMessage = "Đã xóa dòng chi tiết bán.";
    }

    private async void ExecuteLuuHoaDon()
    {
        await LuuHoaDonAsync();
    }

    private async Task LuuHoaDonAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var currentUserId = _sessionService.CurrentUser?.UserId ?? 0;
        if (currentUserId <= 0)
        {
            ErrorMessage = "Không xác định được phiên đăng nhập. Vui lòng đăng nhập lại.";
            return;
        }

        var caDangMoResult = await _caLamViecService.GetCaDangMoAsync(currentUserId, cancellationToken);
        if (!caDangMoResult.IsSuccess)
        {
            ErrorMessage = caDangMoResult.Message;
            return;
        }

        if (caDangMoResult.Data is null)
        {
            ErrorMessage = "Vui lòng mở ca làm việc trước khi bán hàng.";
            return;
        }

        if (!TryParseDecimal(GiamGia, out var giamGiaValue) || giamGiaValue < 0)
        {
            ErrorMessage = "Giảm giá không hợp lệ.";
            return;
        }

        if (SelectedKhuyenMai is not null)
        {
            var checkKhuyenMai = await _khuyenMaiService.ApDungKhuyenMaiAsync(
                SelectedKhuyenMai.KhuyenMaiId,
                TongTien,
                DateTime.Now,
                cancellationToken);

            if (!checkKhuyenMai.IsSuccess)
            {
                ErrorMessage = checkKhuyenMai.Message;
                return;
            }

            SoTienGiamKhuyenMai = checkKhuyenMai.Data?.SoTienGiam ?? 0;
        }
        else
        {
            SoTienGiamKhuyenMai = 0;
        }

        // === Validate thanh toán ===
        if (ChiTietLines.Count == 0)
        {
            ErrorMessage = "Vui lòng thêm ít nhất 1 món vào hóa đơn.";
            return;
        }

        if (IsThanhToanTienMat)
        {
            if (!TryParseDecimal(TienKhachDuaText, out var tienDua) || tienDua <= 0)
            {
                ErrorMessage = "Vui lòng nhập số tiền khách đưa hợp lệ.";
                return;
            }
            if (tienDua < ThanhToan)
            {
                ErrorMessage = $"Tiền khách đưa ({tienDua:N0}) không đủ thanh toán ({ThanhToan:N0}).";
                return;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(MaGiaoDich))
            {
                ErrorMessage = $"Vui lòng nhập mã giao dịch cho hình thức {HinhThucThanhToanDuocChon}.";
                return;
            }
        }

        var chiTietInputs = ChiTietLines
            .Select(x => new HoaDonBanChiTietInputModel
            {
                MonId = x.MonId,
                SoLuong = x.SoLuong,
                DonGiaBan = x.DonGiaBan,
                KichCo = x.KichCo,
                PhuThuKichCo = x.PhuThuKichCo,
                GhiChuMon = x.GhiChuMon
            })
            .ToList();

        IsBusy = true;

        try
        {
            // Tính điểm sử dụng thực tế
            var diemSuDung = 0;
            if (SelectedKhachHang is not null && int.TryParse(DiemSuDungText, out var diemNhap) && diemNhap > 0)
            {
                diemSuDung = Math.Min(diemNhap, SelectedKhachHang.DiemTichLuy);
            }

            var result = await _hoaDonBanService.CreateAsync(
                currentUserId,
                giamGiaValue,
                null, // Không cần chọn bàn
                caDangMoResult.Data.CaLamViecId,
                chiTietInputs,
                SelectedKhachHang?.KhachHangId,
                SelectedKhuyenMai?.KhuyenMaiId,
                HinhThucThanhToanDuocChon,
                IsThanhToanTienMat && TryParseDecimal(TienKhachDuaText, out var tienKhachDua) ? tienKhachDua : null,
                IsThanhToanKhongDungTienMat && !string.IsNullOrWhiteSpace(MaGiaoDich) ? MaGiaoDich.Trim() : null,
                string.IsNullOrWhiteSpace(GhiChuThanhToan) ? null : GhiChuThanhToan,
                string.IsNullOrWhiteSpace(GhiChuHoaDon) ? null : GhiChuHoaDon,
                HinhThucPhucVu,
                diemSuDung,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            var hoaDon = result.Data;
            var soGoiMon = hoaDon?.SoGoiMonHienThi ?? "---";
            var maHoaDon = $"HD{hoaDon?.HoaDonBanId ?? 0:D5}";

            // Thông báo chi tiết theo hình thức thanh toán
            var msgBuilder = $"Thanh toán thành công. Số gọi món: {soGoiMon}. Mã hóa đơn: {maHoaDon}. Hình thức: {HinhThucThanhToanDuocChon}.";
            if (IsThanhToanTienMat)
            {
                msgBuilder += $" Tiền khách đưa: {hoaDon?.TienKhachDua ?? 0:N0} đ. Tiền thối lại: {hoaDon?.TienThoiLai ?? 0:N0} đ.";
            }
            else
            {
                msgBuilder += $" Mã giao dịch: {hoaDon?.MaGiaoDich ?? "N/A"}.";
            }
            SuccessMessage = msgBuilder;

            await LoadMonAsync(cancellationToken);
            await LoadBanAsync(cancellationToken);
            await LoadKhachHangAsync(cancellationToken);
            await LoadKhuyenMaiAsync(cancellationToken);
            await LoadThongTinCaDangMoAsync(cancellationToken);
            ResetFormAfterSave();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể lưu hóa đơn bán: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ExecuteLamMoi()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        SelectedBan = null;
        SelectedMon = null;
        SelectedKhachHang = null;
        SelectedKhuyenMai = null;

        SoLuongBan = "1";
        DonGiaBan = string.Empty;
        GiamGia = "0";

        // Reset thanh toán
        HinhThucThanhToanDuocChon = "Tiền mặt";
        TienKhachDuaText = string.Empty;
        MaGiaoDich = string.Empty;
        GhiChuThanhToan = string.Empty;
        GhiChuHoaDon = string.Empty;

        // Reset điểm sử dụng
        DiemSuDungText = "0";

        // Reset size & ghi chú món
        KichCoDuocChon = "Mặc định";
        GhiChuMon = string.Empty;

        SelectedDongChiTiet = null;
        ChiTietLines.Clear();
        RecalculateTotals();
    }

    private void ExecuteTangSoLuong(ChiTietHoaDonBanHienThi? chiTiet)
    {
        if (chiTiet is null || IsBusy) return;

        var mon = Mons.FirstOrDefault(m => m.MonId == chiTiet.MonId);
        if (mon is null) return;

        if (chiTiet.SoLuong + 1 > mon.TonKho)
        {
            ErrorMessage = $"Sản phẩm '{mon.TenMon}' không đủ tồn kho. Hiện chỉ còn {mon.TonKho}.";
            return;
        }

        chiTiet.SoLuong++;
        RecalculateTotals();
        ErrorMessage = string.Empty;
    }

    private void ExecuteGiamSoLuong(ChiTietHoaDonBanHienThi? chiTiet)
    {
        if (chiTiet is null || IsBusy) return;

        if (chiTiet.SoLuong > 1)
        {
            chiTiet.SoLuong--;
            RecalculateTotals();
        }
        else
        {
            ChiTietLines.Remove(chiTiet);
            RecalculateTotals();
            _luuHoaDonCommand.RaiseCanExecuteChanged();
        }
        ErrorMessage = string.Empty;
    }

    private void ExecuteXoaMon(ChiTietHoaDonBanHienThi? chiTiet)
    {
        if (chiTiet is null || IsBusy) return;

        ChiTietLines.Remove(chiTiet);
        RecalculateTotals();
        _luuHoaDonCommand.RaiseCanExecuteChanged();
        ErrorMessage = string.Empty;
    }

    private void ExecuteThemMonNhanh(Mon? mon)
    {
        if (mon is null || IsBusy) return;

        ErrorMessage = string.Empty;

        // Kiểm tra tồn kho
        if (mon.TonKho <= 0)
        {
            ErrorMessage = $"Sản phẩm '{mon.TenMon}' đã hết hàng.";
            return;
        }

        var kichCo = string.IsNullOrWhiteSpace(KichCoDuocChon) ? "Mặc định" : KichCoDuocChon;
        var phuThu = GetPhuThuKichCo(kichCo);
        var ghiChu = string.IsNullOrWhiteSpace(GhiChuMon) ? null : GhiChuMon.Trim();
        var donGia = mon.DonGia + phuThu;

        // Tìm dòng cùng MonId + cùng KichCo + cùng GhiChuMon
        var dongDaTonTai = ChiTietLines.FirstOrDefault(x =>
            x.MonId == mon.MonId &&
            string.Equals(x.KichCo, kichCo, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.GhiChuMon ?? "", ghiChu ?? "", StringComparison.OrdinalIgnoreCase));

        // Kiểm tra tồn kho theo tổng cùng MonId
        var tongSoLuongCungMon = ChiTietLines
            .Where(x => x.MonId == mon.MonId)
            .Sum(x => x.SoLuong);

        if (tongSoLuongCungMon + 1 > mon.TonKho)
        {
            ErrorMessage = $"Sản phẩm '{mon.TenMon}' không đủ tồn kho. Hiện chỉ còn {mon.TonKho}, đã chọn: {tongSoLuongCungMon}.";
            return;
        }

        if (dongDaTonTai is not null)
        {
            // Cùng MonId + KichCo + GhiChuMon → tăng số lượng
            dongDaTonTai.SoLuong++;
        }
        else
        {
            // Khác size hoặc khác ghi chú → tạo dòng mới
            ChiTietLines.Add(new ChiTietHoaDonBanHienThi
            {
                MonId = mon.MonId,
                TenMon = mon.TenMon,
                SoLuong = 1,
                DonGiaBan = donGia,
                KichCo = kichCo,
                PhuThuKichCo = phuThu,
                GhiChuMon = ghiChu
            });
        }

        RecalculateTotals();
        _luuHoaDonCommand.RaiseCanExecuteChanged();
        SuccessMessage = $"Đã thêm {mon.TenMon} size {kichCo}.";

        // Chỉ reset ghi chú, giữ nguyên size để thêm nhiều món cùng size
        GhiChuMon = string.Empty;
    }

    private async Task LoadMonAsync(CancellationToken cancellationToken)
    {
        var data = await _monService.SearchAsync(null, null, cancellationToken);

        Mons.Clear();
        foreach (var item in data.Where(x => x.IsActive).OrderBy(x => x.TenMon))
        {
            Mons.Add(item);
        }

        DanhMucMons.Clear();
        var cats = Mons.Select(x => x.TenDanhMuc ?? "Khác").Distinct().OrderBy(x => x).ToList();
        DanhMucMons.Add("Tất cả");
        foreach (var cat in cats)
        {
            DanhMucMons.Add(cat);
        }

        if (string.IsNullOrWhiteSpace(SelectedDanhMuc) || !DanhMucMons.Contains(SelectedDanhMuc))
        {
            SetProperty(ref _selectedDanhMuc, "Tất cả", nameof(SelectedDanhMuc));
        }

        ApplyMonFilter();
    }


    private async Task LoadBanAsync(CancellationToken cancellationToken)
    {
        var banResult = await _banService.GetDanhSachBanAsync(
            null,
            null,
            true,
            null,
            cancellationToken);

        if (!banResult.IsSuccess || banResult.Data is null)
        {
            ErrorMessage = banResult.Message;
            return;
        }

        Bans.Clear();
        // Chỉ hiển thị bàn Trống khi lập hóa đơn bán
        foreach (var ban in banResult.Data
                     .Where(x => string.Equals(x.TrangThaiBan, TrangThaiBanConst.Trong, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(x => x.TenKhuVuc)
                     .ThenBy(x => x.TenBan))
        {
            Bans.Add(ban);
        }

        if (SelectedBan is null && Bans.Count > 0)
        {
            SelectedBan = Bans[0];
        }
    }

    private async Task LoadKhachHangAsync(CancellationToken cancellationToken)
    {
        var result = await _khachHangService.GetDanhSachKhachHangAsync(null, true, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message;
            return;
        }

        KhachHangs.Clear();
        foreach (var item in result.Data.OrderBy(x => x.HoTen))
        {
            KhachHangs.Add(item);
        }
    }

    private async Task LoadKhuyenMaiAsync(CancellationToken cancellationToken)
    {
        var result = await _khuyenMaiService.GetKhuyenMaiHieuLucAsync(DateTime.Now, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.Message;
            return;
        }

        KhuyenMais.Clear();
        foreach (var item in result.Data.OrderByDescending(x => x.CreatedAt))
        {
            KhuyenMais.Add(item);
        }

        if (SelectedKhuyenMai is not null && KhuyenMais.All(x => x.KhuyenMaiId != SelectedKhuyenMai.KhuyenMaiId))
        {
            SelectedKhuyenMai = null;
        }
    }

    private async Task LoadThongTinCaDangMoAsync(CancellationToken cancellationToken)
    {
        var currentUserId = _sessionService.CurrentUser?.UserId ?? 0;
        if (currentUserId <= 0)
        {
            ThongTinCaLamViec = "Chưa có phiên đăng nhập hợp lệ.";
            return;
        }

        var caResult = await _caLamViecService.GetCaDangMoAsync(currentUserId, cancellationToken);
        if (!caResult.IsSuccess)
        {
            ThongTinCaLamViec = caResult.Message;
            return;
        }

        if (caResult.Data is null)
        {
            ThongTinCaLamViec = "Chưa mở ca làm việc.";
            return;
        }

        ThongTinCaLamViec = $"Ca #{caResult.Data.CaLamViecId} mở lúc {caResult.Data.ThoiGianMoCa:dd/MM/yyyy HH:mm}.";
    }

    private void ResetFormAfterSave()
    {
        SelectedMon = null;
        SelectedKhachHang = null;
        SelectedKhuyenMai = null;

        SoLuongBan = "1";
        DonGiaBan = string.Empty;
        GiamGia = "0";

        // Reset thanh toán
        HinhThucThanhToanDuocChon = "Tiền mặt";
        TienKhachDuaText = string.Empty;
        MaGiaoDich = string.Empty;
        GhiChuThanhToan = string.Empty;
        GhiChuHoaDon = string.Empty;

        // Reset điểm sử dụng
        DiemSuDungText = "0";

        // Reset size & ghi chú món
        KichCoDuocChon = "Mặc định";
        GhiChuMon = string.Empty;

        SelectedDongChiTiet = null;
        ChiTietLines.Clear();
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        TongTien = ChiTietLines.Sum(x => x.ThanhTien);

        if (!TryParseDecimal(GiamGia, out var giamGiaValue) || giamGiaValue < 0)
        {
            giamGiaValue = 0;
        }

        SoTienGiamKhuyenMai = TinhSoTienGiamKhuyenMai(TongTien, SelectedKhuyenMai, ChiTietLines);

        // Tính giảm từ điểm
        var diemSuDung = 0;
        if (SelectedKhachHang is not null && int.TryParse(DiemSuDungText, out var diemNhap) && diemNhap > 0)
        {
            // Giới hạn điểm dùng không vượt quá điểm khách có
            diemSuDung = Math.Min(diemNhap, SelectedKhachHang.DiemTichLuy);
            
            // Tính số tiền giảm từ điểm (1 điểm = 1.000đ)
            var soTienGiamTuDiemTamTinh = diemSuDung * 1000m;
            
            // Số tiền sau giảm giá và khuyến mãi
            var soTienSauGiamKhac = TongTien - giamGiaValue - SoTienGiamKhuyenMai;
            
            // Giới hạn số tiền giảm từ điểm không vượt quá số tiền còn phải thanh toán
            SoTienGiamTuDiem = Math.Min(soTienGiamTuDiemTamTinh, soTienSauGiamKhac);
            
            // Cập nhật lại điểm sử dụng thực tế nếu bị giới hạn
            if (SoTienGiamTuDiem < soTienGiamTuDiemTamTinh)
            {
                diemSuDung = (int)Math.Floor(SoTienGiamTuDiem / 1000m);
            }
        }
        else
        {
            SoTienGiamTuDiem = 0;
        }

        var thanhToan = TongTien - giamGiaValue - SoTienGiamKhuyenMai - SoTienGiamTuDiem;
        ThanhToan = thanhToan < 0 ? 0 : thanhToan;

        // Điểm cộng tính trên số tiền cuối cùng khách thật sự thanh toán
        DiemCongDuKien = SelectedKhachHang is null
            ? 0
            : (int)Math.Floor(ThanhToan / 10000m);

        RecalculateTienThoiLai();
    }

    private static decimal TinhSoTienGiamKhuyenMai(
        decimal tongTien,
        KhuyenMai? khuyenMai,
        IEnumerable<ChiTietHoaDonBanHienThi>? chiTietLines = null)
    {
        if (khuyenMai is null || tongTien <= 0 || !khuyenMai.DangHieuLuc)
        {
            return 0;
        }

        // Kiểm tra đơn tối thiểu
        if (khuyenMai.GiaTriDonHangToiThieu.HasValue && tongTien < khuyenMai.GiaTriDonHangToiThieu.Value)
        {
            return 0;
        }

        decimal soTienGiam;
        switch (khuyenMai.LoaiKhuyenMai)
        {
            case "PhanTramHoaDon":
                soTienGiam = Math.Round(tongTien * khuyenMai.GiaTri / 100m, 0, MidpointRounding.AwayFromZero);
                break;
            case "SoTienCoDinh":
                soTienGiam = khuyenMai.GiaTri;
                break;
            case "TheoSanPham":
            {
                if (!khuyenMai.MonId.HasValue || khuyenMai.MonId <= 0)
                    return 0;

                var lines = chiTietLines?.ToList();
                if (lines is null || lines.Count == 0)
                    return 0;

                var dongSP = lines.Where(x => x.MonId == khuyenMai.MonId.Value).ToList();
                if (dongSP.Count == 0)
                    return 0;

                var thanhTienSP = dongSP.Sum(x => x.ThanhTien);
                soTienGiam = khuyenMai.GiaTri;
                if (soTienGiam > thanhTienSP)
                    soTienGiam = thanhTienSP;
                break;
            }
            default:
                return 0;
        }

        if (soTienGiam < 0)
        {
            return 0;
        }

        // Giới hạn giảm tối đa
        if (khuyenMai.SoTienGiamToiDa.HasValue && soTienGiam > khuyenMai.SoTienGiamToiDa.Value)
        {
            soTienGiam = khuyenMai.SoTienGiamToiDa.Value;
        }

        return soTienGiam > tongTien ? tongTien : soTienGiam;
    }

    private static bool TryParseDecimal(string input, out decimal result)
    {
        if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
        {
            return true;
        }

        return decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }

    /// <summary>Tính lại tiền thối lại khi HTTT hoặc tiền khách đưa thay đổi</summary>
    private void RecalculateTienThoiLai()
    {
        if (!IsThanhToanTienMat)
        {
            TienThoiLai = 0;
            return;
        }

        if (TryParseDecimal(TienKhachDuaText, out var tienKhachDua) && tienKhachDua >= ThanhToan)
        {
            TienThoiLai = tienKhachDua - ThanhToan;
        }
        else
        {
            TienThoiLai = 0;
        }
    }

    private void ApplyMonFilter()
    {
        var query = Mons.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(TuKhoaTimMon))
        {
            var keyword = TuKhoaTimMon.ToLowerInvariant();
            query = query.Where(x => 
                x.TenMon.ToLowerInvariant().Contains(keyword) || 
                (x.TenDanhMuc != null && x.TenDanhMuc.ToLowerInvariant().Contains(keyword)) ||
                x.MonId.ToString().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(SelectedDanhMuc) && SelectedDanhMuc != "Tất cả")
        {
            query = query.Where(x => (x.TenDanhMuc ?? "Khác") == SelectedDanhMuc);
        }

        var result = query
            .Where(x => x.IsActive)
            .OrderBy(x => x.TenDanhMuc)
            .ThenBy(x => x.TenMon)
            .ToList();

        MonsHienThi.Clear();
        foreach (var mon in result)
        {
            MonsHienThi.Add(mon);
        }
    }

    /// <summary>Trả phụ thu theo kích cỡ đồ uống</summary>
    private static decimal GetPhuThuKichCo(string? kichCo) => kichCo switch
    {
        "L" => 5000m,
        "XL" => 10000m,
        _ => 0m
    };
}







