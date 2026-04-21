using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Repositories;

namespace CoffeeShop.Wpf.Services;

public sealed class ThongKeService : IThongKeService
{
    private readonly IThongKeRepository _thongKeRepository;
    private readonly IHoaDonBanRepository _hoaDonBanRepository;

    public ThongKeService(IThongKeRepository thongKeRepository, IHoaDonBanRepository hoaDonBanRepository)
    {
        _thongKeRepository = thongKeRepository;
        _hoaDonBanRepository = hoaDonBanRepository;
    }

    public async Task<ServiceResult<ThongKeTongHopModel>> GetTongHopAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        if (fromDate.Date > toDate.Date)
        {
            return ServiceResult<ThongKeTongHopModel>.Fail("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        if ((toDate.Date - fromDate.Date).TotalDays > 366)
        {
            return ServiceResult<ThongKeTongHopModel>.Fail("Khoảng thời gian thống kê tối đa là 366 ngày.");
        }

        var doanhThuTheoNgay = await _thongKeRepository.GetDoanhThuTheoNgayAsync(fromDate, toDate, cancellationToken);
        var topSanPhamBanChay = await _thongKeRepository.GetTopSanPhamBanChayAsync(fromDate, toDate, 10, cancellationToken);
        var doanhThuTheoHTTT = await _thongKeRepository.GetDoanhThuTheoHTTTAsync(fromDate, toDate, cancellationToken);
        // Chỉ lấy hóa đơn đã thanh toán cho thống kê (loại trừ đã hủy)
        var hoaDonBans = await _hoaDonBanRepository.GetPaidByDateRangeAsync(fromDate, toDate, cancellationToken);

        var danhSachHoaDon = hoaDonBans
            .Select(x => new HoaDonBanTimKiemDong
            {
                HoaDonBanId = x.HoaDonBanId,
                NgayBan = x.NgayBan,
                TongTien = x.TongTien,
                GiamGia = x.GiamGia,
                CreatedByUserId = x.CreatedByUserId,
                BanId = x.BanId,
                CaLamViecId = x.CaLamViecId
            })
            .OrderByDescending(x => x.NgayBan)
            .ToList();

        var summary = new ThongKeTongHopModel
        {
            DoanhThuTheoNgay = doanhThuTheoNgay.OrderByDescending(x => x.Ngay).ToList(),
            TopSanPhamBanChay = topSanPhamBanChay,
            DoanhThuTheoHTTT = doanhThuTheoHTTT,
            DanhSachHoaDon = danhSachHoaDon,
            TongSoHoaDon = danhSachHoaDon.Count,
            TongDoanhThuGop = danhSachHoaDon.Sum(x => x.TongTien),
            TongGiamGia = danhSachHoaDon.Sum(x => x.GiamGia),
            TongDoanhThuThuan = danhSachHoaDon.Sum(x => x.ThanhToan)
        };

        return ServiceResult<ThongKeTongHopModel>.Success(summary, "Tải thống kê thành công.");
    }
}
