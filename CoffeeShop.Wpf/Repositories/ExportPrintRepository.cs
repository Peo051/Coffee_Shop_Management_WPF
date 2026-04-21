using CoffeeShop.Wpf.Helpers;
using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class ExportPrintRepository : IExportPrintRepository
{
    public async Task<IReadOnlyList<BaoCaoDonGianDong>> GetDuLieuBaoCaoDonGianAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT CAST(h.NgayBan AS DATE) AS Ngay,
       COUNT(1) AS SoHoaDon,
       SUM(h.TongTien) AS TongTien,
       SUM(h.GiamGia) AS TongGiamGia,
       SUM(h.ThanhToan) AS DoanhThuThuan
FROM dbo.HoaDonBan h
WHERE h.NgayBan >= @FromDate
  AND h.NgayBan < @ToDateExclusive
  AND ISNULL(h.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
GROUP BY CAST(h.NgayBan AS DATE)
ORDER BY Ngay DESC;";

        var result = new List<BaoCaoDonGianDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new BaoCaoDonGianDong
            {
                Ngay = reader.GetDateTime(reader.GetOrdinal("Ngay")),
                SoHoaDon = reader.GetInt32(reader.GetOrdinal("SoHoaDon")),
                TongTien = reader.GetDecimal(reader.GetOrdinal("TongTien")),
                TongGiamGia = reader.GetDecimal(reader.GetOrdinal("TongGiamGia")),
                DoanhThuThuan = reader.GetDecimal(reader.GetOrdinal("DoanhThuThuan"))
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<BaoCaoNangCaoDong>> GetDuLieuBaoCaoNangCaoAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT m.MonId,
       m.TenMon,
       SUM(c.SoLuong) AS SoLuongBan,
       SUM(c.ThanhTien) AS DoanhThuGop,
       CASE WHEN SUM(c.SoLuong) = 0 THEN 0 ELSE SUM(c.ThanhTien) / SUM(c.SoLuong) END AS GiaBanTrungBinh
FROM dbo.ChiTietHoaDonBan c
JOIN dbo.HoaDonBan h ON h.HoaDonBanId = c.HoaDonBanId
JOIN dbo.Mon m ON m.MonId = c.MonId
WHERE h.NgayBan >= @FromDate
  AND h.NgayBan < @ToDateExclusive
  AND ISNULL(h.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
GROUP BY m.MonId, m.TenMon
ORDER BY DoanhThuGop DESC, SoLuongBan DESC;";

        var result = new List<BaoCaoNangCaoDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new BaoCaoNangCaoDong
            {
                MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
                TenMon = reader.GetString(reader.GetOrdinal("TenMon")),
                SoLuongBan = reader.GetInt32(reader.GetOrdinal("SoLuongBan")),
                DoanhThuGop = reader.GetDecimal(reader.GetOrdinal("DoanhThuGop")),
                GiaBanTrungBinh = reader.GetDecimal(reader.GetOrdinal("GiaBanTrungBinh"))
            });
        }

        return result;
    }

    public async Task<HoaDonBanInModel?> GetDuLieuHoaDonBanAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default)
    {
        const string headerSql = @"
SELECT hb.HoaDonBanId,
       hb.NgayBan,
       hb.TongTien,
       hb.GiamGia,
       hb.ThanhToan,
       hb.CaLamViecId,
       nd.HoTen AS TenNhanVien,
       b.TenBan,
       kv.TenKhuVuc,
       kh.HoTen AS TenKhachHang,
       kh.SoDienThoai AS SoDienThoaiKhachHang,
       ISNULL(hb.HinhThucThanhToan, N'Tiền mặt') AS HinhThucThanhToan,
       ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') AS TrangThaiThanhToan,
       hb.TienKhachDua,
       hb.TienThoiLai,
       hb.MaGiaoDich,
       hb.GhiChuThanhToan,
       hb.GhiChuHoaDon,
       hb.SoThuTuGoiMon,
       hb.NgaySoThuTu,
       ISNULL(hb.HinhThucPhucVu, N'UongTaiQuan') AS HinhThucPhucVu,
       ISNULL(hb.DiemSuDung, 0) AS DiemSuDung,
       ISNULL(hb.SoTienGiamTuDiem, 0) AS SoTienGiamTuDiem,
       ISNULL(hb.DiemCong, 0) AS DiemCong
FROM dbo.HoaDonBan hb
LEFT JOIN dbo.NguoiDung nd ON nd.NguoiDungId = hb.CreatedByUserId
LEFT JOIN dbo.Ban b ON b.BanId = hb.BanId
LEFT JOIN dbo.KhuVuc kv ON kv.KhuVucId = b.KhuVucId
LEFT JOIN dbo.KhachHang kh ON kh.KhachHangId = hb.KhachHangId
WHERE hb.HoaDonBanId = @HoaDonBanId;";

        const string detailSql = @"
SELECT ct.MonId,
       m.TenMon,
       ct.SoLuong,
       ct.DonGiaBan,
       ct.ThanhTien,
       ct.KichCo,
       ISNULL(ct.PhuThuKichCo, 0) AS PhuThuKichCo,
       ct.GhiChuMon
FROM dbo.ChiTietHoaDonBan ct
JOIN dbo.Mon m ON m.MonId = ct.MonId
WHERE ct.HoaDonBanId = @HoaDonBanId
ORDER BY ct.ChiTietHoaDonBanId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        HoaDonBanInModel? header = null;
        await using (var headerCommand = new SqlCommand(headerSql, connection))
        {
            headerCommand.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
            await using var reader = await headerCommand.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                header = new HoaDonBanInModel
                {
                    HoaDonBanId = reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
                    NgayBan = reader.GetDateTime(reader.GetOrdinal("NgayBan")),
                    TongTien = reader.GetDecimal(reader.GetOrdinal("TongTien")),
                    GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
                    ThanhToan = reader.GetDecimal(reader.GetOrdinal("ThanhToan")),
                    CaLamViecId = reader.IsDBNull(reader.GetOrdinal("CaLamViecId"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("CaLamViecId")),
                    TenNhanVien = reader.IsDBNull(reader.GetOrdinal("TenNhanVien"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("TenNhanVien")),
                    TenBan = reader.IsDBNull(reader.GetOrdinal("TenBan"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("TenBan")),
                    TenKhuVuc = reader.IsDBNull(reader.GetOrdinal("TenKhuVuc"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("TenKhuVuc")),
                    // Thông tin khách hàng
                    TenKhachHang = reader.IsDBNull(reader.GetOrdinal("TenKhachHang"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("TenKhachHang")),
                    SoDienThoaiKhachHang = reader.IsDBNull(reader.GetOrdinal("SoDienThoaiKhachHang"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("SoDienThoaiKhachHang")),
                    // Thanh toán nâng cao
                    // Chuẩn hóa encoding cho dữ liệu cũ
                    HinhThucThanhToan = TextNormalizationHelper.NormalizeOrEmpty(
                        reader.GetString(reader.GetOrdinal("HinhThucThanhToan"))),
                    TrangThaiThanhToan = TextNormalizationHelper.NormalizeOrEmpty(
                        reader.GetString(reader.GetOrdinal("TrangThaiThanhToan"))),
                    TienKhachDua = reader.IsDBNull(reader.GetOrdinal("TienKhachDua"))
                        ? null
                        : reader.GetDecimal(reader.GetOrdinal("TienKhachDua")),
                    TienThoiLai = reader.IsDBNull(reader.GetOrdinal("TienThoiLai"))
                        ? null
                        : reader.GetDecimal(reader.GetOrdinal("TienThoiLai")),
                    MaGiaoDich = reader.IsDBNull(reader.GetOrdinal("MaGiaoDich"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("MaGiaoDich")),
                    GhiChuThanhToan = reader.IsDBNull(reader.GetOrdinal("GhiChuThanhToan"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("GhiChuThanhToan")),
                    GhiChuHoaDon = reader.IsDBNull(reader.GetOrdinal("GhiChuHoaDon"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("GhiChuHoaDon")),
                    // Số gọi món
                    SoThuTuGoiMon = reader.IsDBNull(reader.GetOrdinal("SoThuTuGoiMon"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("SoThuTuGoiMon")),
                    NgaySoThuTu = reader.IsDBNull(reader.GetOrdinal("NgaySoThuTu"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("NgaySoThuTu")),
                    // Hình thức phục vụ
                    HinhThucPhucVu = reader.IsDBNull(reader.GetOrdinal("HinhThucPhucVu"))
                        ? HinhThucPhucVuConst.UongTaiQuan
                        : reader.GetString(reader.GetOrdinal("HinhThucPhucVu")),
                    // Điểm tích lũy
                    DiemSuDung = reader.GetInt32(reader.GetOrdinal("DiemSuDung")),
                    SoTienGiamTuDiem = reader.GetDecimal(reader.GetOrdinal("SoTienGiamTuDiem")),
                    DiemCong = reader.GetInt32(reader.GetOrdinal("DiemCong"))
                };
            }
        }

        if (header is null)
        {
            return null;
        }

        var details = new List<HoaDonBanInChiTietDong>();
        await using (var detailCommand = new SqlCommand(detailSql, connection))
        {
            detailCommand.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
            await using var reader = await detailCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                details.Add(new HoaDonBanInChiTietDong
                {
                    MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
                    TenMon = reader.GetString(reader.GetOrdinal("TenMon")),
                    SoLuong = reader.GetInt32(reader.GetOrdinal("SoLuong")),
                    DonGiaBan = reader.GetDecimal(reader.GetOrdinal("DonGiaBan")),
                    ThanhTien = reader.GetDecimal(reader.GetOrdinal("ThanhTien")),
                    KichCo = reader.IsDBNull(reader.GetOrdinal("KichCo"))
                        ? "Mặc định" : reader.GetString(reader.GetOrdinal("KichCo")),
                    PhuThuKichCo = reader.GetDecimal(reader.GetOrdinal("PhuThuKichCo")),
                    GhiChuMon = reader.IsDBNull(reader.GetOrdinal("GhiChuMon"))
                        ? null : reader.GetString(reader.GetOrdinal("GhiChuMon"))
                });
            }
        }

        header.ChiTiet = details;
        return header;
    }

    public async Task<IReadOnlyList<ThongKeDoanhThu>> GetDuLieuThongKeDoanhThuAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT CAST(NgayBan AS DATE) AS Ngay,
       COUNT(1) AS SoHoaDon,
       SUM(TongTien) AS DoanhThuGop,
       SUM(GiamGia) AS GiamGia,
       SUM(ThanhToan) AS DoanhThuThuan
FROM dbo.HoaDonBan
WHERE NgayBan >= @FromDate
  AND NgayBan < @ToDateExclusive
  AND ISNULL(TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
GROUP BY CAST(NgayBan AS DATE)
ORDER BY Ngay ASC;";

        var result = new List<ThongKeDoanhThu>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ThongKeDoanhThu
            {
                Ngay = reader.GetDateTime(reader.GetOrdinal("Ngay")),
                SoHoaDon = reader.GetInt32(reader.GetOrdinal("SoHoaDon")),
                DoanhThuGop = reader.GetDecimal(reader.GetOrdinal("DoanhThuGop")),
                GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
                DoanhThuThuan = reader.GetDecimal(reader.GetOrdinal("DoanhThuThuan"))
            });
        }

        return result;
    }
}

