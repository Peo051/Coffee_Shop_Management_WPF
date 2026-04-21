using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class BaoCaoRepository : IBaoCaoRepository
{
    public async Task<IReadOnlyList<BaoCaoDonGianDong>> GetBaoCaoDonGianAsync(
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

    public async Task<IReadOnlyList<BaoCaoNangCaoDong>> GetBaoCaoNangCaoAsync(
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
}
