using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class CaLamViecRepository : ICaLamViecRepository
{
    private const string CaSelectColumns = @"
        c.CaLamViecId, c.NguoiDungId, nd.HoTen AS TenNhanVien,
        c.ThoiGianMoCa, c.ThoiGianDongCa, c.TrangThaiCa, c.GhiChu,
        ISNULL(c.TienDauCa, 0) AS TienDauCa,
        c.TienMatThucDem, c.ChenhLechTienMat, c.GhiChuDoiSoat";

    public async Task<CaLamViec?> GetCaDangMoAsync(int nguoiDungId, CancellationToken cancellationToken = default)
    {
        string sql = $@"
        SELECT TOP (1)
               {CaSelectColumns},
               COUNT(hb.HoaDonBanId) AS SoHoaDon,
               ISNULL(SUM(hb.ThanhToan), 0) AS TongDoanhThu
        FROM dbo.CaLamViec c
        JOIN dbo.NguoiDung nd ON nd.NguoiDungId = c.NguoiDungId
        LEFT JOIN dbo.HoaDonBan hb ON hb.CaLamViecId = c.CaLamViecId
              AND ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
        WHERE c.NguoiDungId = @NguoiDungId
          AND c.TrangThaiCa = N'DangMo'
        GROUP BY c.CaLamViecId, c.NguoiDungId, nd.HoTen,
                 c.ThoiGianMoCa, c.ThoiGianDongCa, c.TrangThaiCa, c.GhiChu,
                 c.TienDauCa, c.TienMatThucDem, c.ChenhLechTienMat, c.GhiChuDoiSoat
        ORDER BY c.ThoiGianMoCa DESC;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NguoiDungId", nguoiDungId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapCaLamViec(reader);
    }

    public async Task<int> MoCaAsync(int nguoiDungId, decimal tienDauCa, string? ghiChu, CancellationToken cancellationToken = default)
    {
        const string sql = @"
        INSERT INTO dbo.CaLamViec (NguoiDungId, ThoiGianMoCa, TrangThaiCa, GhiChu, TienDauCa)
        VALUES (@NguoiDungId, SYSDATETIME(), N'DangMo', @GhiChu, @TienDauCa);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NguoiDungId", nguoiDungId);
        command.Parameters.AddWithValue("@GhiChu", string.IsNullOrWhiteSpace(ghiChu) ? DBNull.Value : ghiChu.Trim());
        command.Parameters.AddWithValue("@TienDauCa", tienDauCa);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(scalar);
    }

    public async Task<bool> DongCaAsync(int caLamViecId, string? ghiChu,
        decimal tienMatThucDem, decimal chenhLechTienMat, string? ghiChuDoiSoat,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
        UPDATE dbo.CaLamViec
        SET ThoiGianDongCa = SYSDATETIME(),
            TrangThaiCa = N'DaDong',
            GhiChu = CASE
                         WHEN @GhiChu IS NULL OR LTRIM(RTRIM(@GhiChu)) = N'' THEN GhiChu
                         ELSE @GhiChu
                     END,
            TienMatThucDem = @TienMatThucDem,
            ChenhLechTienMat = @ChenhLechTienMat,
            GhiChuDoiSoat = @GhiChuDoiSoat
        WHERE CaLamViecId = @CaLamViecId
          AND TrangThaiCa = N'DangMo';";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CaLamViecId", caLamViecId);
        command.Parameters.AddWithValue("@GhiChu", string.IsNullOrWhiteSpace(ghiChu) ? DBNull.Value : ghiChu.Trim());
        command.Parameters.AddWithValue("@TienMatThucDem", tienMatThucDem);
        command.Parameters.AddWithValue("@ChenhLechTienMat", chenhLechTienMat);
        command.Parameters.AddWithValue("@GhiChuDoiSoat",
            string.IsNullOrWhiteSpace(ghiChuDoiSoat) ? DBNull.Value : ghiChuDoiSoat.Trim());

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IReadOnlyList<CaLamViec>> GetLichSuCaAsync(
        DateTime fromDate,
        DateTime toDate,
        int? nguoiDungId,
        CancellationToken cancellationToken = default)
    {
        string sql = $@"
        SELECT {CaSelectColumns},
               COUNT(hb.HoaDonBanId) AS SoHoaDon,
               ISNULL(SUM(hb.ThanhToan), 0) AS TongDoanhThu
        FROM dbo.CaLamViec c
        JOIN dbo.NguoiDung nd ON nd.NguoiDungId = c.NguoiDungId
        LEFT JOIN dbo.HoaDonBan hb ON hb.CaLamViecId = c.CaLamViecId
              AND ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
        WHERE c.ThoiGianMoCa >= @FromDate
          AND c.ThoiGianMoCa < @ToDateExclusive
          AND (@NguoiDungId IS NULL OR c.NguoiDungId = @NguoiDungId)
        GROUP BY c.CaLamViecId, c.NguoiDungId, nd.HoTen,
                 c.ThoiGianMoCa, c.ThoiGianDongCa, c.TrangThaiCa, c.GhiChu,
                 c.TienDauCa, c.TienMatThucDem, c.ChenhLechTienMat, c.GhiChuDoiSoat
        ORDER BY c.ThoiGianMoCa DESC;";

        var result = new List<CaLamViec>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));
        command.Parameters.AddWithValue("@NguoiDungId", (object?)nguoiDungId ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapCaLamViec(reader));
        }

        return result;
    }

    public async Task<CaTongKetModel?> GetTongKetCaAsync(int caLamViecId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT c.CaLamViecId,
               ISNULL(c.TienDauCa, 0) AS TienDauCa,
               c.TienMatThucDem,
               c.ChenhLechTienMat,
               c.GhiChuDoiSoat,
               -- Hóa đơn đã thanh toán
               COUNT(CASE WHEN ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán' THEN 1 END) AS SoHoaDon,
               ISNULL(SUM(CASE WHEN ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán' THEN hb.ThanhToan END), 0) AS TongDoanhThu,
               -- Hóa đơn hủy
               COUNT(CASE WHEN hb.TrangThaiThanhToan = N'Đã hủy' THEN 1 END) AS SoHoaDonHuy,
               -- Doanh thu theo hình thức thanh toán (chỉ hóa đơn đã thanh toán)
               ISNULL(SUM(CASE WHEN ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
                               AND ISNULL(hb.HinhThucThanhToan, N'Tiền mặt') = N'Tiền mặt' THEN hb.ThanhToan END), 0) AS TienMatHeThong,
               ISNULL(SUM(CASE WHEN ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
                               AND hb.HinhThucThanhToan = N'Chuyển khoản' THEN hb.ThanhToan END), 0) AS ChuyenKhoanHeThong,
               ISNULL(SUM(CASE WHEN ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
                               AND hb.HinhThucThanhToan = N'Thẻ' THEN hb.ThanhToan END), 0) AS TheHeThong,
               ISNULL(SUM(CASE WHEN ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
                               AND hb.HinhThucThanhToan = N'Ví điện tử' THEN hb.ThanhToan END), 0) AS ViDienTuHeThong
        FROM dbo.CaLamViec c
        LEFT JOIN dbo.HoaDonBan hb ON hb.CaLamViecId = c.CaLamViecId
        WHERE c.CaLamViecId = @CaLamViecId
        GROUP BY c.CaLamViecId, c.TienDauCa, c.TienMatThucDem, c.ChenhLechTienMat, c.GhiChuDoiSoat;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CaLamViecId", caLamViecId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new CaTongKetModel
        {
            CaLamViecId = reader.GetInt32(reader.GetOrdinal("CaLamViecId")),
            TienDauCa = reader.GetDecimal(reader.GetOrdinal("TienDauCa")),
            TienMatThucDem = reader.IsDBNull(reader.GetOrdinal("TienMatThucDem"))
                ? null : reader.GetDecimal(reader.GetOrdinal("TienMatThucDem")),
            ChenhLechTienMat = reader.IsDBNull(reader.GetOrdinal("ChenhLechTienMat"))
                ? null : reader.GetDecimal(reader.GetOrdinal("ChenhLechTienMat")),
            GhiChuDoiSoat = reader.IsDBNull(reader.GetOrdinal("GhiChuDoiSoat"))
                ? null : reader.GetString(reader.GetOrdinal("GhiChuDoiSoat")),
            SoHoaDon = reader.GetInt32(reader.GetOrdinal("SoHoaDon")),
            SoHoaDonHuy = reader.GetInt32(reader.GetOrdinal("SoHoaDonHuy")),
            TongDoanhThu = reader.GetDecimal(reader.GetOrdinal("TongDoanhThu")),
            TienMatHeThong = reader.GetDecimal(reader.GetOrdinal("TienMatHeThong")),
            ChuyenKhoanHeThong = reader.GetDecimal(reader.GetOrdinal("ChuyenKhoanHeThong")),
            TheHeThong = reader.GetDecimal(reader.GetOrdinal("TheHeThong")),
            ViDienTuHeThong = reader.GetDecimal(reader.GetOrdinal("ViDienTuHeThong"))
        };
    }

    private static CaLamViec MapCaLamViec(SqlDataReader reader)
    {
        return new CaLamViec
        {
            CaLamViecId = reader.GetInt32(reader.GetOrdinal("CaLamViecId")),
            NguoiDungId = reader.GetInt32(reader.GetOrdinal("NguoiDungId")),
            TenNhanVien = reader.GetString(reader.GetOrdinal("TenNhanVien")),
            ThoiGianMoCa = reader.GetDateTime(reader.GetOrdinal("ThoiGianMoCa")),
            ThoiGianDongCa = reader.IsDBNull(reader.GetOrdinal("ThoiGianDongCa"))
                ? null : reader.GetDateTime(reader.GetOrdinal("ThoiGianDongCa")),
            TrangThaiCa = reader.GetString(reader.GetOrdinal("TrangThaiCa")),
            GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu")) ? null : reader.GetString(reader.GetOrdinal("GhiChu")),
            SoHoaDon = reader.GetInt32(reader.GetOrdinal("SoHoaDon")),
            TongDoanhThu = reader.GetDecimal(reader.GetOrdinal("TongDoanhThu")),
            // Đối soát
            TienDauCa = reader.GetDecimal(reader.GetOrdinal("TienDauCa")),
            TienMatThucDem = reader.IsDBNull(reader.GetOrdinal("TienMatThucDem"))
                ? null : reader.GetDecimal(reader.GetOrdinal("TienMatThucDem")),
            ChenhLechTienMat = reader.IsDBNull(reader.GetOrdinal("ChenhLechTienMat"))
                ? null : reader.GetDecimal(reader.GetOrdinal("ChenhLechTienMat")),
            GhiChuDoiSoat = reader.IsDBNull(reader.GetOrdinal("GhiChuDoiSoat"))
                ? null : reader.GetString(reader.GetOrdinal("GhiChuDoiSoat"))
        };
    }
}
