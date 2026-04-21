using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class LichSuTonKhoRepository : ILichSuTonKhoRepository
{
    public async Task ThemLichSuAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int monId,
        string loaiPhatSinh,
        int soLuongThayDoi,
        int tonTruoc,
        int tonSau,
        int? hoaDonBanId = null,
        int? hoaDonNhapId = null,
        string? ghiChu = null,
        int? nguoiDungId = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dbo.LichSuTonKho 
            (
                MonId, 
                LoaiPhatSinh, 
                SoLuongThayDoi, 
                TonTruoc, 
                TonSau, 
                HoaDonBanId, 
                HoaDonNhapId, 
                GhiChu, 
                NguoiDungId,
                ThoiGian
            )
            VALUES 
            (
                @MonId, 
                @LoaiPhatSinh, 
                @SoLuongThayDoi, 
                @TonTruoc, 
                @TonSau, 
                @HoaDonBanId, 
                @HoaDonNhapId, 
                @GhiChu, 
                @NguoiDungId,
                SYSDATETIME()
            );";

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@MonId", monId);
        cmd.Parameters.AddWithValue("@LoaiPhatSinh", loaiPhatSinh);
        cmd.Parameters.AddWithValue("@SoLuongThayDoi", soLuongThayDoi);
        cmd.Parameters.AddWithValue("@TonTruoc", tonTruoc);
        cmd.Parameters.AddWithValue("@TonSau", tonSau);
        cmd.Parameters.AddWithValue("@HoaDonBanId", (object?)hoaDonBanId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@HoaDonNhapId", (object?)hoaDonNhapId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)ghiChu ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NguoiDungId", (object?)nguoiDungId ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LichSuTonKho>> GetLichSuTheoMonAsync(
        int monId,
        DateTime? tuNgay = null,
        DateTime? denNgay = null,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                ls.LichSuTonKhoId,
                ls.MonId,
                m.TenMon,
                ls.LoaiPhatSinh,
                ls.SoLuongThayDoi,
                ls.TonTruoc,
                ls.TonSau,
                ls.HoaDonBanId,
                ls.HoaDonNhapId,
                ls.GhiChu,
                ls.ThoiGian,
                ls.NguoiDungId,
                nd.HoTen AS TenNguoiDung
            FROM dbo.LichSuTonKho ls
            INNER JOIN dbo.Mon m ON ls.MonId = m.MonId
            LEFT JOIN dbo.TaiKhoanNguoiDung nd ON ls.NguoiDungId = nd.TaiKhoanNguoiDungId
            WHERE ls.MonId = @MonId";

        if (tuNgay.HasValue)
        {
            sql += " AND ls.ThoiGian >= @TuNgay";
        }

        if (denNgay.HasValue)
        {
            sql += " AND ls.ThoiGian <= @DenNgay";
        }

        sql += " ORDER BY ls.ThoiGian DESC;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@MonId", monId);

        if (tuNgay.HasValue)
        {
            cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Value);
        }

        if (denNgay.HasValue)
        {
            cmd.Parameters.AddWithValue("@DenNgay", denNgay.Value);
        }

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<LichSuTonKho>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapLichSuTonKho(reader));
        }

        return result;
    }

    public async Task<IReadOnlyList<LichSuTonKho>> GetLichSuGanDayAsync(
        int soLuong = 100,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TOP (@SoLuong)
                ls.LichSuTonKhoId,
                ls.MonId,
                m.TenMon,
                ls.LoaiPhatSinh,
                ls.SoLuongThayDoi,
                ls.TonTruoc,
                ls.TonSau,
                ls.HoaDonBanId,
                ls.HoaDonNhapId,
                ls.GhiChu,
                ls.ThoiGian,
                ls.NguoiDungId,
                nd.HoTen AS TenNguoiDung
            FROM dbo.LichSuTonKho ls
            INNER JOIN dbo.Mon m ON ls.MonId = m.MonId
            LEFT JOIN dbo.TaiKhoanNguoiDung nd ON ls.NguoiDungId = nd.TaiKhoanNguoiDungId
            ORDER BY ls.ThoiGian DESC;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@SoLuong", soLuong);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<LichSuTonKho>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapLichSuTonKho(reader));
        }

        return result;
    }

    public async Task<IReadOnlyList<LichSuTonKho>> GetLichSuTheoHoaDonBanAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                ls.LichSuTonKhoId,
                ls.MonId,
                m.TenMon,
                ls.LoaiPhatSinh,
                ls.SoLuongThayDoi,
                ls.TonTruoc,
                ls.TonSau,
                ls.HoaDonBanId,
                ls.HoaDonNhapId,
                ls.GhiChu,
                ls.ThoiGian,
                ls.NguoiDungId,
                nd.HoTen AS TenNguoiDung
            FROM dbo.LichSuTonKho ls
            INNER JOIN dbo.Mon m ON ls.MonId = m.MonId
            LEFT JOIN dbo.TaiKhoanNguoiDung nd ON ls.NguoiDungId = nd.TaiKhoanNguoiDungId
            WHERE ls.HoaDonBanId = @HoaDonBanId
            ORDER BY ls.ThoiGian DESC;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<LichSuTonKho>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapLichSuTonKho(reader));
        }

        return result;
    }

    public async Task<IReadOnlyList<LichSuTonKho>> GetLichSuTheoHoaDonNhapAsync(
        int hoaDonNhapId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                ls.LichSuTonKhoId,
                ls.MonId,
                m.TenMon,
                ls.LoaiPhatSinh,
                ls.SoLuongThayDoi,
                ls.TonTruoc,
                ls.TonSau,
                ls.HoaDonBanId,
                ls.HoaDonNhapId,
                ls.GhiChu,
                ls.ThoiGian,
                ls.NguoiDungId,
                nd.HoTen AS TenNguoiDung
            FROM dbo.LichSuTonKho ls
            INNER JOIN dbo.Mon m ON ls.MonId = m.MonId
            LEFT JOIN dbo.TaiKhoanNguoiDung nd ON ls.NguoiDungId = nd.TaiKhoanNguoiDungId
            WHERE ls.HoaDonNhapId = @HoaDonNhapId
            ORDER BY ls.ThoiGian DESC;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@HoaDonNhapId", hoaDonNhapId);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<LichSuTonKho>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapLichSuTonKho(reader));
        }

        return result;
    }

    private static LichSuTonKho MapLichSuTonKho(SqlDataReader reader)
    {
        return new LichSuTonKho
        {
            LichSuTonKhoId = reader.GetInt32(reader.GetOrdinal("LichSuTonKhoId")),
            MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
            TenMon = reader.IsDBNull(reader.GetOrdinal("TenMon")) 
                ? null 
                : reader.GetString(reader.GetOrdinal("TenMon")),
            LoaiPhatSinh = reader.GetString(reader.GetOrdinal("LoaiPhatSinh")),
            SoLuongThayDoi = reader.GetInt32(reader.GetOrdinal("SoLuongThayDoi")),
            TonTruoc = reader.GetInt32(reader.GetOrdinal("TonTruoc")),
            TonSau = reader.GetInt32(reader.GetOrdinal("TonSau")),
            HoaDonBanId = reader.IsDBNull(reader.GetOrdinal("HoaDonBanId")) 
                ? null 
                : reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
            HoaDonNhapId = reader.IsDBNull(reader.GetOrdinal("HoaDonNhapId")) 
                ? null 
                : reader.GetInt32(reader.GetOrdinal("HoaDonNhapId")),
            GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu")) 
                ? null 
                : reader.GetString(reader.GetOrdinal("GhiChu")),
            ThoiGian = reader.GetDateTime(reader.GetOrdinal("ThoiGian")),
            NguoiDungId = reader.IsDBNull(reader.GetOrdinal("NguoiDungId")) 
                ? null 
                : reader.GetInt32(reader.GetOrdinal("NguoiDungId")),
            TenNguoiDung = reader.IsDBNull(reader.GetOrdinal("TenNguoiDung")) 
                ? null 
                : reader.GetString(reader.GetOrdinal("TenNguoiDung"))
        };
    }
}
