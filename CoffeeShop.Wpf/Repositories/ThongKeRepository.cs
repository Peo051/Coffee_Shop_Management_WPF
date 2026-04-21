using CoffeeShop.Wpf.Helpers;
using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class ThongKeRepository : IThongKeRepository
{
    public async Task<IReadOnlyList<ThongKeDoanhThu>> GetDoanhThuTheoNgayAsync(
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
ORDER BY Ngay DESC;";

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

    public async Task<IReadOnlyList<ThongKeTopSanPhamDong>> GetTopSanPhamBanChayAsync(
        DateTime fromDate,
        DateTime toDate,
        int topN,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT TOP (@TopN)
       m.MonId,
       m.TenMon,
       SUM(ct.SoLuong) AS TongSoLuongBan,
       COUNT(DISTINCT hb.HoaDonBanId) AS SoHoaDon,
       SUM(ct.ThanhTien) AS TongDoanhThu
FROM dbo.HoaDonBan hb
INNER JOIN dbo.ChiTietHoaDonBan ct ON ct.HoaDonBanId = hb.HoaDonBanId
INNER JOIN dbo.Mon m ON m.MonId = ct.MonId
WHERE hb.NgayBan >= @FromDate
  AND hb.NgayBan < @ToDateExclusive
  AND ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
GROUP BY m.MonId, m.TenMon
ORDER BY SUM(ct.SoLuong) DESC, SUM(ct.ThanhTien) DESC, m.MonId ASC;";

        var result = new List<ThongKeTopSanPhamDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));
        command.Parameters.AddWithValue("@TopN", topN);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ThongKeTopSanPhamDong
            {
                MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
                TenMon = reader.GetString(reader.GetOrdinal("TenMon")),
                TongSoLuongBan = reader.GetInt32(reader.GetOrdinal("TongSoLuongBan")),
                SoHoaDon = reader.GetInt32(reader.GetOrdinal("SoHoaDon")),
                TongDoanhThu = reader.GetDecimal(reader.GetOrdinal("TongDoanhThu"))
            });
        }

        return result;
    }

    /// <summary>
    /// Thống kê doanh thu theo hình thức thanh toán.
    /// Chỉ tính hóa đơn đã thanh toán (loại trừ đã hủy).
    /// </summary>
    public async Task<IReadOnlyList<ThongKeDoanhThuTheoHTTT>> GetDoanhThuTheoHTTTAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT ISNULL(HinhThucThanhToan, N'Tiền mặt') AS HinhThucThanhToan,
       COUNT(1) AS SoHoaDon,
       SUM(ThanhToan) AS TongDoanhThu
FROM dbo.HoaDonBan
WHERE NgayBan >= @FromDate
  AND NgayBan < @ToDateExclusive
  AND ISNULL(TrangThaiThanhToan, N'Đã thanh toán') <> N'Đã hủy'
GROUP BY ISNULL(HinhThucThanhToan, N'Tiền mặt')
ORDER BY TongDoanhThu DESC;";

        var result = new List<ThongKeDoanhThuTheoHTTT>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ThongKeDoanhThuTheoHTTT
            {
                HinhThucThanhToan = TextNormalizationHelper.NormalizeOrEmpty(
                    reader.GetString(reader.GetOrdinal("HinhThucThanhToan"))),
                SoHoaDon = reader.GetInt32(reader.GetOrdinal("SoHoaDon")),
                TongDoanhThu = reader.GetDecimal(reader.GetOrdinal("TongDoanhThu"))
            });
        }

        return result;
    }
}
