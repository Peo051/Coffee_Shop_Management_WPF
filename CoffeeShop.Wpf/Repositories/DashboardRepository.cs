using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
    public async Task<DashboardTongQuanModel> GetTongQuanHomNayAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
DECLARE @Today DATE = CAST(GETDATE() AS DATE);

SELECT ISNULL(SUM(hb.ThanhToan), 0) AS DoanhThuHomNay,
       COUNT(1) AS SoHoaDonHomNay
FROM dbo.HoaDonBan hb
WHERE hb.NgayBan >= @Today
  AND hb.NgayBan < DATEADD(DAY, 1, @Today)
  AND ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán';

SELECT COUNT(1) AS SoSanPhamDangKinhDoanh
FROM dbo.Mon
WHERE IsActive = 1;

SELECT COUNT(1) AS SoSanPhamSapHet
FROM dbo.Mon
WHERE IsActive = 1
  AND TonKho <= MucCanhBaoTonKho;

SELECT
    COUNT(1) AS TongSoSanPham,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS SoSanPhamDangKinhDoanh
FROM dbo.Mon;";

        var result = new DashboardTongQuanModel();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            result.DoanhThuHomNay = reader.GetDecimal(reader.GetOrdinal("DoanhThuHomNay"));
            result.SoHoaDonHomNay = reader.GetInt32(reader.GetOrdinal("SoHoaDonHomNay"));
        }

        if (await reader.NextResultAsync(cancellationToken)
            && await reader.ReadAsync(cancellationToken))
        {
            result.SoSanPhamDangKinhDoanh = reader.GetInt32(reader.GetOrdinal("SoSanPhamDangKinhDoanh"));
        }

        if (await reader.NextResultAsync(cancellationToken)
            && await reader.ReadAsync(cancellationToken))
        {
            result.SoSanPhamSapHet = reader.GetInt32(reader.GetOrdinal("SoSanPhamSapHet"));
        }

        if (await reader.NextResultAsync(cancellationToken)
            && await reader.ReadAsync(cancellationToken))
        {
            var tongSoSanPham = reader.GetInt32(reader.GetOrdinal("TongSoSanPham"));
            var soDangKinhDoanh = reader.GetInt32(reader.GetOrdinal("SoSanPhamDangKinhDoanh"));
            result.TyLeSanPhamDangKinhDoanh = tongSoSanPham == 0
                ? 0
                : Math.Round((decimal)soDangKinhDoanh / tongSoSanPham * 100, 2);
        }

        return result;
    }

    public async Task<IReadOnlyList<DashboardDoanhThuNgayDong>> GetDoanhThu7NgayAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT CAST(hb.NgayBan AS DATE) AS Ngay,
       COUNT(1) AS SoHoaDon,
       SUM(hb.ThanhToan) AS DoanhThuThuan
FROM dbo.HoaDonBan hb
WHERE hb.NgayBan >= DATEADD(DAY, -6, CAST(GETDATE() AS DATE))
  AND hb.NgayBan < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
  AND ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
GROUP BY CAST(hb.NgayBan AS DATE)
ORDER BY Ngay ASC;";

        var map = new Dictionary<DateTime, DashboardDoanhThuNgayDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var ngay = reader.GetDateTime(reader.GetOrdinal("Ngay")).Date;
            map[ngay] = new DashboardDoanhThuNgayDong
            {
                Ngay = ngay,
                SoHoaDon = reader.GetInt32(reader.GetOrdinal("SoHoaDon")),
                DoanhThuThuan = reader.GetDecimal(reader.GetOrdinal("DoanhThuThuan"))
            };
        }

        var result = new List<DashboardDoanhThuNgayDong>();
        var start = DateTime.Today.AddDays(-6);
        for (var i = 0; i < 7; i++)
        {
            var ngay = start.AddDays(i).Date;
            if (map.TryGetValue(ngay, out var item))
            {
                result.Add(item);
            }
            else
            {
                result.Add(new DashboardDoanhThuNgayDong
                {
                    Ngay = ngay,
                    SoHoaDon = 0,
                    DoanhThuThuan = 0
                });
            }
        }

        return result;
    }

    public async Task<IReadOnlyList<ThongKeTopSanPhamDong>> GetTopSanPhamHomNayAsync(
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
JOIN dbo.ChiTietHoaDonBan ct ON ct.HoaDonBanId = hb.HoaDonBanId
JOIN dbo.Mon m ON m.MonId = ct.MonId
WHERE hb.NgayBan >= CAST(GETDATE() AS DATE)
  AND hb.NgayBan < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
  AND ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
GROUP BY m.MonId, m.TenMon
ORDER BY SUM(ct.SoLuong) DESC, SUM(ct.ThanhTien) DESC, m.MonId ASC;";

        var result = new List<ThongKeTopSanPhamDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
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

    public async Task<IReadOnlyList<CanhBaoTonKhoThapDong>> GetSanPhamTonKhoThapAsync(
        int topN,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT TOP (@TopN)
       m.MonId,
       m.TenMon,
       dm.TenDanhMuc,
       m.TonKho,
       m.MucCanhBaoTonKho,
       (m.MucCanhBaoTonKho - m.TonKho) AS SoLuongCanBoSung
FROM dbo.Mon m
JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
WHERE m.IsActive = 1
  AND m.TonKho <= m.MucCanhBaoTonKho
ORDER BY (m.MucCanhBaoTonKho - m.TonKho) DESC, m.MonId ASC;";

        var result = new List<CanhBaoTonKhoThapDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TopN", topN);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new CanhBaoTonKhoThapDong
            {
                MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
                TenMon = reader.GetString(reader.GetOrdinal("TenMon")),
                TenDanhMuc = reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                TonKho = reader.GetInt32(reader.GetOrdinal("TonKho")),
                MucCanhBaoTonKho = reader.GetInt32(reader.GetOrdinal("MucCanhBaoTonKho"))
            });
        }

        return result;
    }

    public async Task<int> GetSoHoaDonHomNayAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT COUNT(1)
FROM dbo.HoaDonBan
WHERE NgayBan >= CAST(GETDATE() AS DATE)
  AND NgayBan < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
  AND ISNULL(TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán';";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> GetSoSanPhamDangKinhDoanhAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT COUNT(1)
FROM dbo.Mon
WHERE IsActive = 1;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }
}
