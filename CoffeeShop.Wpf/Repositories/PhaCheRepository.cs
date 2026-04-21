using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class PhaCheRepository : IPhaCheRepository
{
    public async Task<IReadOnlyList<PhaCheDonHangDong>> GetDonCanPhaCheAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT hb.HoaDonBanId,
       hb.NgayBan,
       hb.TrangThaiPhaChe,
       hb.ThoiGianBatDauPhaChe,
       hb.ThoiGianHoanThanhPhaChe,
       hb.ThoiGianGiaoKhach,
       hb.SoThuTuGoiMon,
       (hb.TongTien - hb.GiamGia) AS ThanhToan,
       ISNULL(nd.HoTen, N'') AS TenNhanVien,
       b.TenBan,
       kv.TenKhuVuc,
       ISNULL(kh.HoTen, N'Khách lẻ') AS TenKhachHang
FROM dbo.HoaDonBan hb
LEFT JOIN dbo.NguoiDung nd ON nd.NguoiDungId = hb.CreatedByUserId
LEFT JOIN dbo.Ban b ON b.BanId = hb.BanId
LEFT JOIN dbo.KhuVuc kv ON kv.KhuVucId = b.KhuVucId
LEFT JOIN dbo.KhachHang kh ON kh.KhachHangId = hb.KhachHangId
WHERE ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
  AND hb.TrangThaiPhaChe IN (N'ChoPhaChe', N'DangPhaChe', N'DaHoanThanh')
ORDER BY hb.NgayBan ASC;";

        const string chiTietSql = @"
SELECT ct.HoaDonBanId,
       m.TenMon,
       ct.SoLuong
FROM dbo.ChiTietHoaDonBan ct
JOIN dbo.Mon m ON m.MonId = ct.MonId
WHERE ct.HoaDonBanId IN (
    SELECT hb2.HoaDonBanId
    FROM dbo.HoaDonBan hb2
    WHERE ISNULL(hb2.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
      AND hb2.TrangThaiPhaChe IN (N'ChoPhaChe', N'DangPhaChe', N'DaHoanThanh')
)
ORDER BY ct.HoaDonBanId, ct.ChiTietHoaDonBanId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        // 1. Lấy danh sách đơn
        var dons = new List<PhaCheDonHangDong>();
        await using (var cmd = new SqlCommand(sql, connection))
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                dons.Add(new PhaCheDonHangDong
                {
                    HoaDonBanId = reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
                    NgayBan = reader.GetDateTime(reader.GetOrdinal("NgayBan")),
                    TrangThaiPhaChe = reader.GetString(reader.GetOrdinal("TrangThaiPhaChe")),
                    ThoiGianBatDauPhaChe = reader.IsDBNull(reader.GetOrdinal("ThoiGianBatDauPhaChe"))
                        ? null : reader.GetDateTime(reader.GetOrdinal("ThoiGianBatDauPhaChe")),
                    ThoiGianHoanThanhPhaChe = reader.IsDBNull(reader.GetOrdinal("ThoiGianHoanThanhPhaChe"))
                        ? null : reader.GetDateTime(reader.GetOrdinal("ThoiGianHoanThanhPhaChe")),
                    ThoiGianGiaoKhach = reader.IsDBNull(reader.GetOrdinal("ThoiGianGiaoKhach"))
                        ? null : reader.GetDateTime(reader.GetOrdinal("ThoiGianGiaoKhach")),
                    SoThuTuGoiMon = reader.IsDBNull(reader.GetOrdinal("SoThuTuGoiMon"))
                        ? null : reader.GetInt32(reader.GetOrdinal("SoThuTuGoiMon")),
                    ThanhToan = reader.GetDecimal(reader.GetOrdinal("ThanhToan")),
                    TenNhanVien = reader.GetString(reader.GetOrdinal("TenNhanVien")),
                    TenBan = reader.IsDBNull(reader.GetOrdinal("TenBan"))
                        ? null : reader.GetString(reader.GetOrdinal("TenBan")),
                    TenKhuVuc = reader.IsDBNull(reader.GetOrdinal("TenKhuVuc"))
                        ? null : reader.GetString(reader.GetOrdinal("TenKhuVuc")),
                    TenKhachHang = reader.GetString(reader.GetOrdinal("TenKhachHang"))
                });
            }
        }

        // 2. Lấy chi tiết món cho tất cả đơn
        var monMap = new Dictionary<int, List<string>>();
        await using (var cmd = new SqlCommand(chiTietSql, connection))
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var hdId = reader.GetInt32(0);
                var tenMon = reader.GetString(1);
                var soLuong = reader.GetInt32(2);

                if (!monMap.TryGetValue(hdId, out var list))
                {
                    list = [];
                    monMap[hdId] = list;
                }
                list.Add($"{tenMon} x{soLuong}");
            }
        }

        // 3. Gán DanhSachMonTomTat
        foreach (var don in dons)
        {
            if (monMap.TryGetValue(don.HoaDonBanId, out var items))
            {
                don.DanhSachMonTomTat = string.Join(", ", items);
            }
        }

        return dons;
    }

    public async Task<bool> CapNhatTrangThaiPhaCheAsync(
        int hoaDonBanId,
        string trangThaiMoi,
        CancellationToken cancellationToken = default)
    {
        // Xác định cột thời gian cần cập nhật
        var timeColumn = trangThaiMoi switch
        {
            TrangThaiPhaCheConst.DangPhaChe => "ThoiGianBatDauPhaChe",
            TrangThaiPhaCheConst.DaHoanThanh => "ThoiGianHoanThanhPhaChe",
            TrangThaiPhaCheConst.DaGiaoKhach => "ThoiGianGiaoKhach",
            _ => null
        };

        var sql = timeColumn is not null
            ? $@"
UPDATE dbo.HoaDonBan
SET TrangThaiPhaChe = @TrangThaiMoi,
    {timeColumn} = GETDATE()
WHERE HoaDonBanId = @HoaDonBanId
  AND TrangThaiPhaChe <> N'DaHuy';"
            : @"
UPDATE dbo.HoaDonBan
SET TrangThaiPhaChe = @TrangThaiMoi
WHERE HoaDonBanId = @HoaDonBanId
  AND TrangThaiPhaChe <> N'DaHuy';";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@TrangThaiMoi", trangThaiMoi);
        cmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);

        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }
}
