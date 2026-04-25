using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class HoaDonBanService : IHoaDonBanService
{
    private readonly IHoaDonBanRepository _hoaDonBanRepository;
    private readonly IMonService _monService;
    // Module Quản lý bàn đã bị gỡ - giữ lại để tránh lỗi build, nhưng không dùng nữa
    // private readonly IBanRepository _banRepository;
    private readonly ICaLamViecRepository _caLamViecRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IKhuyenMaiService _khuyenMaiService;
    private readonly IKhachHangService _khachHangService;

    public HoaDonBanService(
        IHoaDonBanRepository hoaDonBanRepository,
        IMonService monService,
        IBanRepository banRepository, // Giữ lại parameter để không phá App.xaml.cs
        ICaLamViecRepository caLamViecRepository,
        IAuditLogService auditLogService,
        IKhuyenMaiService khuyenMaiService,
        IKhachHangService khachHangService)
    {
        _hoaDonBanRepository = hoaDonBanRepository;
        _monService = monService;
        // Module Quản lý bàn đã bị gỡ - không gán _banRepository nữa
        // _banRepository = banRepository;
        _caLamViecRepository = caLamViecRepository;
        _auditLogService = auditLogService;
        _khuyenMaiService = khuyenMaiService;
        _khachHangService = khachHangService;
    }

    // Danh sách hình thức thanh toán hợp lệ
    private static readonly string[] HinhThucThanhToanHopLe = ["Tiền mặt", "Chuyển khoản", "Thẻ", "Ví điện tử", "QR Payment"];

    public async Task<ServiceResult<HoaDonBan>> CreateAsync(
        int createdByUserId,
        decimal giamGia,
        int? banId,
        int? caLamViecId,
        IReadOnlyList<HoaDonBanChiTietInputModel> chiTietInputs,
        int? khachHangId = null,
        int? khuyenMaiId = null,
        string hinhThucThanhToan = "Tiền mặt",
        decimal? tienKhachDua = null,
        string? maGiaoDich = null,
        string? ghiChuThanhToan = null,
        string? ghiChuHoaDon = null,
        string hinhThucPhucVu = "UongTaiQuan",
        int diemSuDung = 0,
        bool isQrPendingPayment = false,
        CancellationToken cancellationToken = default)
    {
        if (createdByUserId <= 0)
        {
            return ServiceResult<HoaDonBan>.Fail("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");
        }

        if (giamGia < 0)
        {
            return ServiceResult<HoaDonBan>.Fail("Giảm giá không được âm.");
        }

        // === Module Quản lý bàn đã bị gỡ - không kiểm tra bàn nữa ===
        // BanId là tùy chọn — quán bán theo mô hình order tại quầy, không quản lý bàn
        // if (banId.HasValue && banId > 0)
        // {
        //     var ban = await _banRepository.GetByIdAsync(banId.Value, cancellationToken);
        //     if (ban is null || !ban.IsActive)
        //     {
        //         return ServiceResult<HoaDonBan>.Fail("Bàn đã chọn không tồn tại hoặc đã ngừng hoạt động.");
        //     }
        //
        //     if (string.Equals(ban.TrangThaiBan, TrangThaiBanConst.TamKhoa, StringComparison.OrdinalIgnoreCase))
        //     {
        //         return ServiceResult<HoaDonBan>.Fail("Bàn đang ở trạng thái tạm khóa.");
        //     }
        // }
        // else
        // {
        //     banId = null; // Normalize: 0 hoặc negative → null
        // }
        
        // Quán không quản lý bàn - luôn set banId = null
        banId = null;

        if (!caLamViecId.HasValue || caLamViecId <= 0)
        {
            return ServiceResult<HoaDonBan>.Fail("Vui lòng mở ca làm việc trước khi tạo hóa đơn.");
        }


        var caDangMo = await _caLamViecRepository.GetCaDangMoAsync(createdByUserId, cancellationToken);
        if (caDangMo is null || caDangMo.CaLamViecId != caLamViecId.Value)
        {
            return ServiceResult<HoaDonBan>.Fail("Không tìm thấy ca làm việc đang mở hợp lệ.");
        }

        if (chiTietInputs is null || chiTietInputs.Count == 0)
        {
            return ServiceResult<HoaDonBan>.Fail("Hóa đơn bán phải có ít nhất 1 dòng chi tiết.");
        }

        // Fix lỗi trùng món: Sử dụng bộ 3 thuộc tính (MonId, Size, Ghi chú) làm khóa
        // Giải thích: Trước đây chặn theo MonId dẫn đến UI cho thêm món kèm size/ghi chú khác nhau nhưng Service báo lỗi không lưu được
        var hasDuplicates = chiTietInputs
            .GroupBy(x => new { x.MonId, KichCo = x.KichCo ?? "", GhiChuMon = (x.GhiChuMon ?? "").Trim() })
            .Any(g => g.Count() > 1);

        if (hasDuplicates)
        {
            return ServiceResult<HoaDonBan>.Fail("Dòng chi tiết (Món + Size + Ghi chú) bị trùng lặp. Hệ thống cần gộp số lượng các dòng giống hệt nhau.");
        }

        var monData = await _monService.SearchAsync(null, null, cancellationToken);
        var monMap = monData.ToDictionary(x => x.MonId, x => x);

        // Gom nhóm theo MonId để kiểm tra tổng tồn kho theo từng món, tránh trường hợp tách dòng qua vòng check
        var groupedByMonId = chiTietInputs.GroupBy(x => x.MonId);
        foreach (var group in groupedByMonId)
        {
            int monId = group.Key;
            if (monId <= 0 || !monMap.TryGetValue(monId, out var mon))
            {
                return ServiceResult<HoaDonBan>.Fail("Có sản phẩm không tồn tại trong chi tiết hóa đơn bán.");
            }

            var totalSoLuong = group.Sum(x => x.SoLuong);
            if (totalSoLuong > mon.TonKho)
            {
                return ServiceResult<HoaDonBan>.Fail($"Tổng số lượng sản phẩm '{mon.TenMon}' cần bán ({totalSoLuong}) không đủ tồn kho (hiện có: {mon.TonKho}).");
            }

            foreach (var line in group)
            {
                if (line.SoLuong <= 0)
                {
                    return ServiceResult<HoaDonBan>.Fail("Số lượng bán phải lớn hơn 0.");
                }

                if (line.DonGiaBan <= 0)
                {
                    return ServiceResult<HoaDonBan>.Fail("Đơn giá bán phải lớn hơn 0.");
                }
            }
        }

        var tongTien = chiTietInputs.Sum(x => x.ThanhTien);
        if (tongTien <= 0)
        {
            return ServiceResult<HoaDonBan>.Fail("Tổng tiền hóa đơn bán không hợp lệ.");
        }

        if (giamGia > tongTien)
        {
            return ServiceResult<HoaDonBan>.Fail("Giảm giá không được lớn hơn tổng tiền.");
        }

        // === Validate hình thức thanh toán ===
        if (string.IsNullOrWhiteSpace(hinhThucThanhToan))
        {
            return ServiceResult<HoaDonBan>.Fail("Vui lòng chọn hình thức thanh toán.");
        }

        if (!HinhThucThanhToanHopLe.Contains(hinhThucThanhToan))
        {
            return ServiceResult<HoaDonBan>.Fail($"Hình thức thanh toán '{hinhThucThanhToan}' không hợp lệ.");
        }

        if (khachHangId.HasValue && khachHangId.Value > 0)
        {
            var khachHang = await _khachHangService.GetByIdAsync(khachHangId.Value, cancellationToken);
            if (!khachHang.IsSuccess || khachHang.Data is null || !khachHang.Data.IsActive)
            {
                return ServiceResult<HoaDonBan>.Fail("Khách hàng đã chọn không hợp lệ hoặc đã ngừng hoạt động.");
            }

            // Validate điểm sử dụng
            if (diemSuDung < 0)
            {
                return ServiceResult<HoaDonBan>.Fail("Điểm sử dụng không được âm.");
            }

            // Giới hạn tối đa 500 điểm (= 50,000đ) cho mỗi đơn hàng
            if (diemSuDung > 500)
            {
                return ServiceResult<HoaDonBan>.Fail("Chỉ được sử dụng tối đa 500 điểm (50,000đ) cho mỗi đơn hàng.");
            }

            if (diemSuDung > khachHang.Data.DiemTichLuy)
            {
                return ServiceResult<HoaDonBan>.Fail($"Điểm sử dụng ({diemSuDung}) vượt quá điểm tích lũy hiện có ({khachHang.Data.DiemTichLuy}).");
            }
        }
        else if (diemSuDung > 0)
        {
            return ServiceResult<HoaDonBan>.Fail("Chỉ có thể sử dụng điểm khi đã chọn khách hàng.");
        }

        var apDungKhuyenMai = await _khuyenMaiService.ApDungKhuyenMaiAsync(
            khuyenMaiId,
            tongTien,
            chiTietInputs,
            DateTime.Now,
            cancellationToken);

        if (!apDungKhuyenMai.IsSuccess || apDungKhuyenMai.Data is null)
        {
            return ServiceResult<HoaDonBan>.Fail(apDungKhuyenMai.Message);
        }

        var soTienGiamKhuyenMai = apDungKhuyenMai.Data.SoTienGiam;
        
        // Tính số tiền giảm từ điểm (1 điểm = 100đ, tối đa 50,000đ cho mỗi đơn hàng)
        var soTienGiamTuDiem = diemSuDung * 100m;
        
        // Giới hạn tối đa 50,000đ
        soTienGiamTuDiem = Math.Min(soTienGiamTuDiem, 50000m);
        
        var tongGiamGia = giamGia + soTienGiamKhuyenMai + soTienGiamTuDiem;
        if (tongGiamGia > tongTien)
        {
            return ServiceResult<HoaDonBan>.Fail("Tổng giảm giá vượt quá tổng tiền hóa đơn.");
        }

        var thanhToanSauGiam = tongTien - tongGiamGia;

        // Declare tienThoiLai variable
        decimal? tienThoiLai = null;

        // === Validate thanh toán ===
        // Nếu là QR pending payment, không cần validate tiền khách đưa / mã giao dịch
        if (!isQrPendingPayment)
        {
            if (string.Equals(hinhThucThanhToan, "Tiền mặt", StringComparison.OrdinalIgnoreCase))
            {
                if (!tienKhachDua.HasValue || tienKhachDua.Value < thanhToanSauGiam)
                {
                    return ServiceResult<HoaDonBan>.Fail(
                        $"Tiền khách đưa ({tienKhachDua?.ToString("N0") ?? "0"}) không đủ. Cần ít nhất {thanhToanSauGiam:N0} đ.");
                }
                tienThoiLai = tienKhachDua.Value - thanhToanSauGiam;
                maGiaoDich = null; // Tiền mặt không cần mã giao dịch
            }
            else
            {
                // Chuyển khoản / Thẻ / Ví điện tử: bắt buộc mã giao dịch
                if (string.IsNullOrWhiteSpace(maGiaoDich))
                {
                    return ServiceResult<HoaDonBan>.Fail(
                        $"Vui lòng nhập mã giao dịch cho hình thức {hinhThucThanhToan}.");
                }
                maGiaoDich = maGiaoDich.Trim();
                tienKhachDua = null; // Không dùng tiền mặt
                tienThoiLai = null;
            }
        }
        else
        {
            // QR pending payment: không cần tiền khách đưa / mã giao dịch ngay
            tienKhachDua = null;
            tienThoiLai = null;
            maGiaoDich = null;
        }

        var diemCong = (khachHangId.HasValue && khachHangId.Value > 0)
            ? TinhDiemTichLuy(thanhToanSauGiam)
            : 0;

        var hoaDonBan = new HoaDonBan
        {
            NgayBan = DateTime.Now,
            TongTien = tongTien,
            GiamGia = tongGiamGia,
            CreatedByUserId = createdByUserId,
            BanId = banId,
            CaLamViecId = caLamViecId,
            KhachHangId = khachHangId > 0 ? khachHangId : null,
            KhuyenMaiId = apDungKhuyenMai.Data.KhuyenMaiId,
            SoTienGiam = soTienGiamKhuyenMai,
            DiemCong = diemCong,
            DiemSuDung = diemSuDung,
            SoTienGiamTuDiem = soTienGiamTuDiem,
            // Thanh toán nâng cao
            HinhThucThanhToan = hinhThucThanhToan,
            TrangThaiThanhToan = isQrPendingPayment ? "Chờ thanh toán" : "Đã thanh toán",
            TienKhachDua = tienKhachDua,
            TienThoiLai = tienThoiLai,
            MaGiaoDich = maGiaoDich,
            GhiChuThanhToan = ghiChuThanhToan,
            GhiChuHoaDon = ghiChuHoaDon,
            // Trạng thái pha chế mặc định
            TrangThaiPhaChe = TrangThaiPhaCheConst.ChoPhaChe,
            // Hình thức phục vụ
            HinhThucPhucVu = string.IsNullOrWhiteSpace(hinhThucPhucVu)
                ? HinhThucPhucVuConst.UongTaiQuan
                : hinhThucPhucVu
        };

        var chiTietHoaDonBans = chiTietInputs
            .Select(x => new ChiTietHoaDonBan
            {
                MonId = x.MonId,
                SoLuong = x.SoLuong,
                DonGiaBan = x.DonGiaBan,
                KichCo = string.IsNullOrWhiteSpace(x.KichCo) ? "Mặc định" : x.KichCo.Trim(),
                PhuThuKichCo = x.PhuThuKichCo,
                GhiChuMon = string.IsNullOrWhiteSpace(x.GhiChuMon) ? null : x.GhiChuMon.Trim()
            })
            .ToList();

        try
        {
            var newId = await _hoaDonBanRepository.CreateAsync(
                hoaDonBan, 
                chiTietHoaDonBans, 
                isQrPendingPayment, // Skip inventory deduction if QR pending
                cancellationToken);
            hoaDonBan.HoaDonBanId = newId;

            // === Module Quản lý bàn đã bị gỡ - không cập nhật trạng thái bàn nữa ===
            // if (banId.HasValue)
            // {
            //     _ = await _banRepository.CapNhatTrangThaiBanAsync(banId.Value, TrangThaiBanConst.Trong, cancellationToken);
            // }
            
            _ = TryWriteAuditAsync(
                createdByUserId,
                "Tạo hóa đơn bán",
                "HoaDonBan",
                $"Hóa đơn bán #{newId}, bàn: {banId}, ca: {caLamViecId}, tổng tiền: {tongTien:N0}, giảm KM: {soTienGiamKhuyenMai:N0}, khách hàng: {(khachHangId?.ToString() ?? "N/A")}",
                cancellationToken);
            return ServiceResult<HoaDonBan>.Success(hoaDonBan, "Lưu hóa đơn bán thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<HoaDonBan>.Fail(ex.Message);
        }
    }

    public Task<IReadOnlyList<HoaDonBan>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return _hoaDonBanRepository.GetByDateRangeAsync(fromDate, toDate, cancellationToken);
    }

    private async Task TryWriteAuditAsync(
        int nguoiDungId,
        string hanhDong,
        string doiTuong,
        string duLieuTomTat,
        CancellationToken cancellationToken)
    {
        try
        {
            await _auditLogService.GhiLogAsync(
                nguoiDungId,
                hanhDong,
                doiTuong,
                duLieuTomTat,
                Environment.MachineName,
                cancellationToken);
        }
        catch
        {
            // Không chặn luồng tạo hóa đơn bán khi ghi log lỗi.
        }
    }

    private static int TinhDiemTichLuy(decimal thanhToan)
    {
        if (thanhToan <= 0)
        {
            return 0;
        }

        return (int)Math.Floor(thanhToan / 10000m);
    }
}

