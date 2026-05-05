using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;
using QRCoder;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class HoaDonBanViewModel : BaseViewModel
{
    private readonly IHoaDonBanService _hoaDonBanService;
    private readonly IMonService _monService;
    private readonly ICaLamViecService _caLamViecService;
    private readonly IKhachHangService _khachHangService;
    private readonly IKhuyenMaiService _khuyenMaiService;
    private readonly SessionService _sessionService;
    private readonly IExportPrintService _exportPrintService;

    private readonly RelayCommand _themDongCommand;
    private readonly RelayCommand _xoaDongCommand;
    private readonly RelayCommand _luuHoaDonCommand;
    private readonly RelayCommand _lamMoiCommand;
    private readonly RelayCommand<ChiTietHoaDonBanHienThi> _tangSoLuongCommand;
    private readonly RelayCommand<ChiTietHoaDonBanHienThi> _giamSoLuongCommand;
    private readonly RelayCommand<ChiTietHoaDonBanHienThi> _xoaMonCommand;
    private readonly RelayCommand<MonHienThiViewModel> _themMonNhanhCommand;
    private readonly RelayCommand<KhachHang> _chonKhachHangCommand;
    private readonly RelayCommand _xoaKhachHangCommand;
    private readonly RelayCommand _inPhieuPhaCheCommand;

    private string _tuKhoaTimMon = string.Empty;
    private string _selectedDanhMuc = "Tất cả";

    private Mon? _selectedMon;
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
    
    // === Tìm kiếm ===
    private string _timKiemKhachHangText = string.Empty;

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public HoaDonBanViewModel(
        IHoaDonBanService hoaDonBanService,
        IMonService monService,
        ICaLamViecService caLamViecService,
        IKhachHangService khachHangService,
        IKhuyenMaiService khuyenMaiService,
        SessionService sessionService,
        IExportPrintService exportPrintService)
    {
        _hoaDonBanService = hoaDonBanService;
        _monService = monService;
        _caLamViecService = caLamViecService;
        _khachHangService = khachHangService;
        _khuyenMaiService = khuyenMaiService;
        _sessionService = sessionService;
        _exportPrintService = exportPrintService;

        Mons = new ObservableCollection<Mon>();
        MonsHienThi = new ObservableCollection<MonHienThiViewModel>();
        DanhMucMons = new ObservableCollection<string>();
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
        _themMonNhanhCommand = new RelayCommand<MonHienThiViewModel>(ExecuteThemMonNhanh);
        _chonKhachHangCommand = new RelayCommand<KhachHang>(ExecuteChonKhachHang);
        _xoaKhachHangCommand = new RelayCommand(ExecuteXoaKhachHang);
        _inPhieuPhaCheCommand = new RelayCommand(ExecuteInPhieuPhaChe, () => !IsBusy && HoaDonBanIdDaTao > 0);

        InitializeQrPaymentCommands();
    }

    public ObservableCollection<Mon> Mons { get; }

    public ObservableCollection<MonHienThiViewModel> MonsHienThi { get; }

    public ObservableCollection<string> DanhMucMons { get; }

    public ObservableCollection<KhachHang> KhachHangs { get; }
    
    public ObservableCollection<KhachHang> KhachHangsFiltered { get; } = new();

    public ObservableCollection<KhuyenMai> KhuyenMais { get; }

    public ObservableCollection<ChiTietHoaDonBanHienThi> ChiTietLines { get; }

    /// <summary>Danh sách hình thức thanh toán hiển thị trên ComboBox</summary>
    public IReadOnlyList<string> DanhSachHinhThucThanhToan { get; } =
        ["Tiền mặt", "Chuyển khoản", "Thẻ", "Ví điện tử", "QR Payment"];

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

    public string TimKiemKhachHangText
    {
        get => _timKiemKhachHangText;
        set
        {
            if (SetProperty(ref _timKiemKhachHangText, value))
            {
                ApplyKhachHangFilter();
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
                OnPropertyChanged(nameof(IsQrPaymentSelected));

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
    public bool IsThanhToanKhongDungTienMat => !IsThanhToanTienMat && !IsQrPaymentSelected;

    /// <summary>True khi chọn QR Payment</summary>
    public bool IsQrPaymentSelected =>
        string.Equals(_hinhThucThanhToanDuocChon, "QR Payment", StringComparison.OrdinalIgnoreCase);

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
            if (SetProperty(ref _kichCoDuocChon, value))
            {
                // Cập nhật giá món đang chọn
                if (SelectedMon is not null)
                {
                    DonGiaBan = (SelectedMon.DonGia + GetPhuThuKichCo(value)).ToString("0.##", CultureInfo.CurrentCulture);
                }
                
                // Cập nhật giá tất cả món trong danh sách hiển thị
                foreach (var monVM in MonsHienThi)
                {
                    monVM.UpdateKichCo(value);
                }
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
    
    public ICommand ChonKhachHangCommand => _chonKhachHangCommand;
    
    public ICommand XoaKhachHangCommand => _xoaKhachHangCommand;

    public ICommand InPhieuPhaCheCommand => _inPhieuPhaCheCommand;

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

        if (ChiTietLines.Count == 0)
        {
            ErrorMessage = "Vui lòng thêm ít nhất 1 món vào hóa đơn.";
            return;
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

        if (SelectedKhuyenMai is not null)
        {
            var checkKhuyenMai = await _khuyenMaiService.ApDungKhuyenMaiAsync(
                SelectedKhuyenMai.KhuyenMaiId,
                TongTien,
                chiTietInputs,
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

        // === Validate thanh toán theo hình thức ===
        bool isQrPayment = IsQrPaymentSelected;

        if (!isQrPayment)
        {
            // Thanh toán thường: validate tiền mặt hoặc mã giao dịch
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
        }

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
                isQrPayment, // Đánh dấu là QR pending payment
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message;
                return;
            }

            var hoaDon = result.Data;
            var soGoiMon = hoaDon?.SoGoiMonHienThi ?? "---";
            var maHoaDon = $"HD{hoaDon?.HoaDonBanId ?? 0:D5}";

            // Lưu ID hóa đơn
            HoaDonBanIdDaTao = hoaDon?.HoaDonBanId ?? 0;
            
            // Refresh commands để enable nút xác nhận
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            System.Diagnostics.Debug.WriteLine($"[TaoHoaDon] HoaDonBanIdDaTao set to {HoaDonBanIdDaTao}");

            if (isQrPayment)
            {
                // QR Payment: Tự động tạo QR sau khi tạo hóa đơn pending
                SuccessMessage = $"✅ Đã tạo hóa đơn chờ thanh toán. Số gọi món: {soGoiMon}. Mã: {maHoaDon}. Đang tạo QR...";
                
                // Tự động tạo QR
                try
                {
                    await TaoQrThanhToanAsync(cancellationToken);
                    
                    // Nếu tạo QR thành công, message đã được set trong TaoQrThanhToanAsync
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        SuccessMessage = $"✅ Đã tạo hóa đơn và QR thanh toán. Số gọi món: {soGoiMon}. Mã: {maHoaDon}.";
                    }
                }
                catch (Exception qrEx)
                {
                    ErrorMessage = $"Tạo hóa đơn thành công nhưng tạo QR thất bại: {qrEx.Message}";
                    // Log error (logger not available in ViewModel)
                }
            }
            else
            {
                // Thanh toán thường: Thông báo thành công
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
                await LoadKhachHangAsync(cancellationToken);
                await LoadKhuyenMaiAsync(cancellationToken);
                await LoadThongTinCaDangMoAsync(cancellationToken);
                ResetFormAfterSave();
            }
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

        // Reset QR payment state khi làm mới
        ResetQrPaymentState();
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

    private void ExecuteThemMonNhanh(MonHienThiViewModel? monVM)
    {
        if (monVM is null || IsBusy) return;
        
        var mon = monVM.Mon;

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
        
        ApplyKhachHangFilter();
    }
    
    private void ApplyKhachHangFilter()
    {
        KhachHangsFiltered.Clear();
        
        var keyword = TimKiemKhachHangText?.Trim().ToLower() ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(keyword))
        {
            // Hiển thị tất cả
            foreach (var kh in KhachHangs)
            {
                KhachHangsFiltered.Add(kh);
            }
        }
        else
        {
            // Lọc theo SĐT hoặc tên
            foreach (var kh in KhachHangs)
            {
                var sdt = kh.SoDienThoai?.ToLower() ?? string.Empty;
                var ten = kh.HoTen?.ToLower() ?? string.Empty;
                
                if (sdt.Contains(keyword) || ten.Contains(keyword))
                {
                    KhachHangsFiltered.Add(kh);
                }
            }
        }
    }
    
    private void ExecuteChonKhachHang(KhachHang? khachHang)
    {
        if (khachHang is null) return;
        
        SelectedKhachHang = khachHang;
        TimKiemKhachHangText = string.Empty; // Xóa text tìm kiếm sau khi chọn
    }
    
    private void ExecuteXoaKhachHang()
    {
        SelectedKhachHang = null;
        TimKiemKhachHangText = string.Empty;
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

        // KHÔNG reset QR payment state - để user có thể tạo QR sau khi lưu hóa đơn
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
            
            // Tính số tiền giảm từ điểm (1 điểm = 100đ, tối đa 50,000đ)
            var soTienGiamTuDiemTamTinh = diemSuDung * 100m;
            
            // Giới hạn tối đa 50,000đ cho mỗi đơn hàng
            soTienGiamTuDiemTamTinh = Math.Min(soTienGiamTuDiemTamTinh, 50000m);
            
            // Số tiền sau giảm giá và khuyến mãi
            var soTienSauGiamKhac = TongTien - giamGiaValue - SoTienGiamKhuyenMai;
            
            // Giới hạn số tiền giảm từ điểm không vượt quá số tiền còn phải thanh toán
            SoTienGiamTuDiem = Math.Min(soTienGiamTuDiemTamTinh, soTienSauGiamKhac);
            
            // Cập nhật lại điểm sử dụng thực tế nếu bị giới hạn
            if (SoTienGiamTuDiem < soTienGiamTuDiemTamTinh)
            {
                diemSuDung = (int)Math.Floor(SoTienGiamTuDiem / 100m);
            }
        }
        else
        {
            SoTienGiamTuDiem = 0;
        }

        var thanhToan = TongTien - giamGiaValue - SoTienGiamKhuyenMai - SoTienGiamTuDiem;
        ThanhToan = thanhToan < 0 ? 0 : thanhToan;

        // Điểm cộng tính trên số tiền cuối cùng khách thật sự thanh toán (10,000đ = 1 điểm)
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
            var monVM = new MonHienThiViewModel(mon);
            monVM.UpdateKichCo(KichCoDuocChon);
            MonsHienThi.Add(monVM);
        }
    }

    /// <summary>Trả phụ thu theo kích cỡ đồ uống</summary>
    private static decimal GetPhuThuKichCo(string? kichCo) => kichCo switch
    {
        "L" => 5000m,
        "XL" => 10000m,
        _ => 0m
    };

    private async void ExecuteInPhieuPhaChe()
    {
        if (IsBusy || HoaDonBanIdDaTao <= 0)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var currentUserId = _sessionService.CurrentUser?.UserId;
            var result = await _exportPrintService.InPhieuPhaCheAsync(
                HoaDonBanIdDaTao,
                null,
                currentUserId);

            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi in phiếu pha chế: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ==================== QR PAYMENT ====================

    private readonly PaymentApiClient _paymentApiClient = new();
    private RelayCommand? _taoQrThanhToanCommand;
    private RelayCommand? _kiemTraTrangThaiThanhToanCommand;
    private RelayCommand? _huyQrThanhToanCommand;
    private RelayCommand? _xacNhanThanhToanCommand;

    private int _hoaDonBanIdDaTao;
    private string? _qrCodeUrl;
    private string? _checkoutUrl;
    private string? _paymentStatus;
    private bool _isWaitingQrPayment;
    private DateTime? _qrExpiredAt;
    private BitmapImage? _qrCodeImageSource;

    /// <summary>ID hóa đơn đã tạo (sau khi lưu thành công)</summary>
    public int HoaDonBanIdDaTao
    {
        get => _hoaDonBanIdDaTao;
        private set
        {
            if (SetProperty(ref _hoaDonBanIdDaTao, value))
            {
                _inPhieuPhaCheCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>URL QR code để hiển thị</summary>
    public string? QrCodeUrl
    {
        get => _qrCodeUrl;
        private set => SetProperty(ref _qrCodeUrl, value);
    }

    /// <summary>QR Code image source để bind vào Image.Source</summary>
    public BitmapImage? QrCodeImageSource
    {
        get => _qrCodeImageSource;
        private set => SetProperty(ref _qrCodeImageSource, value);
    }

    /// <summary>URL checkout trên web</summary>
    public string? CheckoutUrl
    {
        get => _checkoutUrl;
        private set => SetProperty(ref _checkoutUrl, value);
    }

    /// <summary>Trạng thái payment (PENDING, PAID, CANCELLED)</summary>
    public string? PaymentStatus
    {
        get => _paymentStatus;
        private set
        {
            if (SetProperty(ref _paymentStatus, value))
            {
                OnPropertyChanged(nameof(IsPaymentPending));
                OnPropertyChanged(nameof(IsPaymentPaid));
                OnPropertyChanged(nameof(PaymentStatusDisplay));
            }
        }
    }

    /// <summary>Đang chờ thanh toán QR</summary>
    public bool IsWaitingQrPayment
    {
        get => _isWaitingQrPayment;
        private set
        {
            if (SetProperty(ref _isWaitingQrPayment, value))
            {
                _taoQrThanhToanCommand?.RaiseCanExecuteChanged();
                _kiemTraTrangThaiThanhToanCommand?.RaiseCanExecuteChanged();
                _huyQrThanhToanCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Thời gian hết hạn QR</summary>
    public DateTime? QrExpiredAt
    {
        get => _qrExpiredAt;
        private set => SetProperty(ref _qrExpiredAt, value);
    }

    public bool IsPaymentPending => PaymentStatus == "PENDING";
    public bool IsPaymentPaid => PaymentStatus == "PAID";

    public string PaymentStatusDisplay => PaymentStatus switch
    {
        "PENDING" => "⏳ Chờ thanh toán",
        "PAID" => "✅ Đã thanh toán",
        "CANCELLED" => "❌ Đã hủy",
        _ => ""
    };

    public ICommand TaoQrThanhToanCommand => _taoQrThanhToanCommand ?? throw new InvalidOperationException("Command not initialized");
    public ICommand KiemTraTrangThaiThanhToanCommand => _kiemTraTrangThaiThanhToanCommand ?? throw new InvalidOperationException("Command not initialized");
    public ICommand HuyQrThanhToanCommand => _huyQrThanhToanCommand ?? throw new InvalidOperationException("Command not initialized");
    public ICommand XacNhanThanhToanCommand => _xacNhanThanhToanCommand ?? throw new InvalidOperationException("Command not initialized");

    private void InitializeQrPaymentCommands()
    {
        _taoQrThanhToanCommand = new RelayCommand(ExecuteTaoQrThanhToan, CanExecuteTaoQrThanhToan);
        _kiemTraTrangThaiThanhToanCommand = new RelayCommand(ExecuteKiemTraTrangThaiThanhToan, CanExecuteKiemTraTrangThaiThanhToan);
        _huyQrThanhToanCommand = new RelayCommand(ExecuteHuyQrThanhToan, CanExecuteHuyQrThanhToan);
        _xacNhanThanhToanCommand = new RelayCommand(ExecuteXacNhanThanhToan, CanExecuteXacNhanThanhToan);
    }

    private bool CanExecuteTaoQrThanhToan()
    {
        return !IsBusy && HoaDonBanIdDaTao > 0 && !IsWaitingQrPayment && PaymentStatus != "PAID";
    }

    private bool CanExecuteKiemTraTrangThaiThanhToan()
    {
        // Cho phép kiểm tra khi có HoaDonBanId, không cần IsWaitingQrPayment
        // Vì có thể reload app hoặc chuyển hóa đơn khác
        return !IsBusy && HoaDonBanIdDaTao > 0;
    }

    private bool CanExecuteHuyQrThanhToan()
    {
        return !IsBusy && IsWaitingQrPayment && HoaDonBanIdDaTao > 0 && PaymentStatus == "PENDING";
    }

    private async void ExecuteTaoQrThanhToan()
    {
        await TaoQrThanhToanAsync();
    }

    private async Task TaoQrThanhToanAsync(CancellationToken cancellationToken = default)
    {
        if (HoaDonBanIdDaTao <= 0)
        {
            ErrorMessage = "Chưa có hóa đơn để tạo QR thanh toán.";
            return;
        }

        // Không check IsBusy để cho phép auto-call sau khi tạo hóa đơn
        var wasBusy = IsBusy;
        if (!wasBusy)
        {
            IsBusy = true;
        }

        try
        {
            // Creating QR payment for HoaDonBanId

            var response = await _paymentApiClient.CreateQrPaymentAsync(HoaDonBanIdDaTao, cancellationToken);

            if (response == null)
            {
                ErrorMessage = "Không thể tạo QR thanh toán. Vui lòng kiểm tra kết nối backend API.";
                // CreateQrPaymentAsync returned null
                return;
            }

            // Set QR data
            QrCodeUrl = response.QRCodeRaw;
            CheckoutUrl = response.CheckoutUrl;
            PaymentStatus = response.PaymentStatus;
            QrExpiredAt = response.QRExpiredAt;
            IsWaitingQrPayment = true;

            // Generate QR code image từ qrCodeRaw
            if (!string.IsNullOrWhiteSpace(response.QRCodeRaw))
            {
                QrCodeImageSource = GenerateQrCodeImageSource(response.QRCodeRaw);
            }

            // Refresh commands để enable nút xác nhận
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            // QR payment created successfully

            // Chỉ set success message nếu không phải auto-call
            if (!wasBusy)
            {
                SuccessMessage = $"✅ Đã tạo QR thanh toán. Vui lòng quét mã QR để thanh toán {response.Amount:N0} đ.";
            }
        }
        catch (HttpRequestException httpEx)
        {
            var errorMsg = $"Lỗi kết nối API: {httpEx.Message}";
            ErrorMessage = errorMsg;
            // HTTP error creating QR
        }
        catch (Exception ex)
        {
            var errorMsg = $"Lỗi tạo QR thanh toán: {ex.Message}";
            ErrorMessage = errorMsg;
            // Error creating QR
        }
        finally
        {
            if (!wasBusy)
            {
                IsBusy = false;
            }
        }
    }

    private async void ExecuteKiemTraTrangThaiThanhToan()
    {
        await KiemTraTrangThaiThanhToanAsync();
    }

    private async Task KiemTraTrangThaiThanhToanAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || HoaDonBanIdDaTao <= 0)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsBusy = true;

        try
        {
            var status = await _paymentApiClient.GetPaymentStatusAsync(HoaDonBanIdDaTao, cancellationToken);

            if (status == null)
            {
                ErrorMessage = "Không thể kiểm tra trạng thái thanh toán.";
                return;
            }

            PaymentStatus = status.PaymentStatus;
            QrExpiredAt = status.QRExpiredAt;

            if (status.PaymentStatus == "PAID")
            {
                IsWaitingQrPayment = false;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                SuccessMessage = $"🎉 Thanh toán thành công! Mã giao dịch: {status.MaGiaoDich ?? "N/A"}. Có thể in bill và chuyển pha chế.";
                
                // Reload data để cập nhật trạng thái
                await LoadMonAsync(cancellationToken);
                await LoadKhachHangAsync(cancellationToken);
                await LoadKhuyenMaiAsync(cancellationToken);
                await LoadThongTinCaDangMoAsync(cancellationToken);
            }
            else if (status.PaymentStatus == "PENDING")
            {
                SuccessMessage = "⏳ Đang chờ khách thanh toán. Vui lòng kiểm tra lại sau.";
            }
            else if (status.PaymentStatus == "EXPIRED")
            {
                IsWaitingQrPayment = false;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                ErrorMessage = "⏰ QR đã hết hạn. Vui lòng tạo lại QR mới.";
            }
            else if (status.PaymentStatus == "CANCELLED")
            {
                IsWaitingQrPayment = false;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                ErrorMessage = "❌ QR payment đã bị hủy.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi kiểm tra trạng thái: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteHuyQrThanhToan()
    {
        await HuyQrThanhToanAsync();
    }

    private async Task HuyQrThanhToanAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || HoaDonBanIdDaTao <= 0)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsBusy = true;

        try
        {
            var cancelled = await _paymentApiClient.CancelPaymentAsync(HoaDonBanIdDaTao, cancellationToken);

            if (cancelled)
            {
                PaymentStatus = "CANCELLED";
                IsWaitingQrPayment = false;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                QrCodeUrl = null;
                CheckoutUrl = null;
                SuccessMessage = "✅ Đã hủy QR payment.";
            }
            else
            {
                ErrorMessage = "Không thể hủy QR payment. QR có thể đã hết hạn hoặc đã thanh toán.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi hủy QR payment: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ===== XÁC NHẬN THANH TOÁN (TEST) =====

    private bool CanExecuteXacNhanThanhToan()
    {
        // Cho phép xác nhận khi có hóa đơn đã tạo (bỏ hết điều kiện khác)
        var canExecute = HoaDonBanIdDaTao > 0;
        
        // Debug logging
        System.Diagnostics.Debug.WriteLine($"[XacNhanThanhToan] CanExecute={canExecute}, HoaDonBanIdDaTao={HoaDonBanIdDaTao}, IsBusy={IsBusy}");
        
        return canExecute;
    }

    private async void ExecuteXacNhanThanhToan()
    {
        await XacNhanThanhToanAsync();
    }

    private async Task XacNhanThanhToanAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || HoaDonBanIdDaTao <= 0)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsBusy = true;

        try
        {
            // Gọi API để xác nhận thanh toán thủ công (simulate webhook)
            var testWebhookUrl = $"{_paymentApiClient.BaseUrl}/api/payments/test/confirm/{HoaDonBanIdDaTao}";
            
            using var httpClient = new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });

            var response = await httpClient.PostAsync(testWebhookUrl, null, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "✅ Đã xác nhận thanh toán thành công! Hóa đơn đang được xử lý...";
                
                // Tự động kiểm tra trạng thái sau 1 giây
                await Task.Delay(1000, cancellationToken);
                await KiemTraTrangThaiThanhToanAsync(cancellationToken);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                ErrorMessage = $"❌ Lỗi xác nhận thanh toán: {error}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"❌ Lỗi xác nhận thanh toán: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetQrPaymentState()
    {
        HoaDonBanIdDaTao = 0;
        QrCodeUrl = null;
        QrCodeImageSource = null;
        CheckoutUrl = null;
        PaymentStatus = null;
        IsWaitingQrPayment = false;
        QrExpiredAt = null;
    }

    /// <summary>
    /// Generate QR code image từ chuỗi qrCodeRaw
    /// </summary>
    private BitmapImage? GenerateQrCodeImageSource(string qrCodeRaw)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(qrCodeRaw))
            {
                return null;
            }

            // Kiểm tra nếu là URL (VietQR hoặc PayOS trả về URL ảnh)
            if (qrCodeRaw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                qrCodeRaw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // Load ảnh từ URL
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(qrCodeRaw, UriKind.Absolute);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                
                return bitmapImage;
            }

            // Nếu không phải URL, tạo QR code bằng QRCoder (cho text thuần)
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrCodeRaw, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
            // Generate PNG byte array với pixel size 20
            var qrCodeBytes = qrCode.GetGraphic(20);

            // Convert byte[] thành BitmapImage
            var bitmapImage2 = new BitmapImage();
            using var stream = new MemoryStream(qrCodeBytes);
            
            bitmapImage2.BeginInit();
            bitmapImage2.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage2.StreamSource = stream;
            bitmapImage2.EndInit();
            bitmapImage2.Freeze(); // Freeze để có thể dùng cross-thread

            return bitmapImage2;
        }
        catch (Exception ex)
        {
            // Log error (không có logger trong ViewModel)
            ErrorMessage = $"Lỗi tạo ảnh QR: {ex.Message}";
            return null;
        }
    }
}







