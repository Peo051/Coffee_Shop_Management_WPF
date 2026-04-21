using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class LichSuNguyenLieuRepository : ILichSuNguyenLieuRepository
{
    public async Task ThemLichSuAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int nguyenLieuId,
        string loaiPhatSinh,
        decimal soLuongThayDoi,
        decimal tonTruoc,
        decimal tonSau,
        int? hoaDonBanId,
        int? hoaDonNhapId,
        string? ghiChu,
        int? nguoiDungId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.LichSuNguyenLieu
(NguyenLieuId, LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, HoaDonBanId, HoaDonNhapId, GhiChu, NguoiDungId)
VALUES
(@NguyenLieuId, @LoaiPhatSinh, @SoLuongThayDoi, @TonTruoc, @TonSau, @HoaDonBanId, @HoaDonNhapId, @GhiChu, @NguoiDungId);";

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);
        command.Parameters.AddWithValue("@LoaiPhatSinh", loaiPhatSinh);
        command.Parameters.AddWithValue("@SoLuongThayDoi", soLuongThayDoi);
        command.Parameters.AddWithValue("@TonTruoc", tonTruoc);
        command.Parameters.AddWithValue("@TonSau", tonSau);
        command.Parameters.AddWithValue("@HoaDonBanId", (object?)hoaDonBanId ?? DBNull.Value);
        command.Parameters.AddWithValue("@HoaDonNhapId", (object?)hoaDonNhapId ?? DBNull.Value);
        command.Parameters.AddWithValue("@GhiChu", string.IsNullOrWhiteSpace(ghiChu) ? DBNull.Value : ghiChu);
        command.Parameters.AddWithValue("@NguoiDungId", (object?)nguoiDungId ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LichSuNguyenLieu>> GetLichSuTheoNguyenLieuAsync(
        int nguyenLieuId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
SELECT 
    ls.LichSuNguyenLieuId,
    ls.NguyenLieuId,
    nl.TenNguyenLieu,
    nl.DonViTinh,
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
FROM dbo.LichSuNguyenLieu ls
INNER JOIN dbo.NguyenLieu nl ON ls.NguyenLieuId = nl.NguyenLieuId
LEFT JOIN dbo.NguoiDung nd ON ls.NguoiDungId = nd.NguoiDungId
WHERE ls.NguyenLieuId = @NguyenLieuId";

        if (fromDate.HasValue)
        {
            sql += " AND ls.ThoiGian >= @FromDate";
        }

        if (toDate.HasValue)
        {
            sql += " AND ls.ThoiGian < @ToDate";
        }

        sql += " ORDER BY ls.ThoiGian DESC;";

        var result = new List<LichSuNguyenLieu>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);

        if (fromDate.HasValue)
        {
            command.Parameters.AddWithValue("@FromDate", fromDate.Value);
        }

        if (toDate.HasValue)
        {
            command.Parameters.AddWithValue("@ToDate", toDate.Value);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new LichSuNguyenLieu
            {
                LichSuNguyenLieuId = reader.GetInt32(reader.GetOrdinal("LichSuNguyenLieuId")),
                NguyenLieuId = reader.GetInt32(reader.GetOrdinal("NguyenLieuId")),
                TenNguyenLieu = reader.GetString(reader.GetOrdinal("TenNguyenLieu")),
                DonViTinh = reader.GetString(reader.GetOrdinal("DonViTinh")),
                LoaiPhatSinh = reader.GetString(reader.GetOrdinal("LoaiPhatSinh")),
                SoLuongThayDoi = reader.GetDecimal(reader.GetOrdinal("SoLuongThayDoi")),
                TonTruoc = reader.GetDecimal(reader.GetOrdinal("TonTruoc")),
                TonSau = reader.GetDecimal(reader.GetOrdinal("TonSau")),
                HoaDonBanId = reader.IsDBNull(reader.GetOrdinal("HoaDonBanId")) ? null : reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
                HoaDonNhapId = reader.IsDBNull(reader.GetOrdinal("HoaDonNhapId")) ? null : reader.GetInt32(reader.GetOrdinal("HoaDonNhapId")),
                GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu")) ? null : reader.GetString(reader.GetOrdinal("GhiChu")),
                ThoiGian = reader.GetDateTime(reader.GetOrdinal("ThoiGian")),
                NguoiDungId = reader.IsDBNull(reader.GetOrdinal("NguoiDungId")) ? null : reader.GetInt32(reader.GetOrdinal("NguoiDungId")),
                TenNguoiDung = reader.IsDBNull(reader.GetOrdinal("TenNguoiDung")) ? null : reader.GetString(reader.GetOrdinal("TenNguoiDung"))
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<LichSuNguyenLieu>> GetLichSuGanDayAsync(
        int top = 100,
        CancellationToken cancellationToken = default)
    {
        var sql = $@"
SELECT TOP (@Top)
    ls.LichSuNguyenLieuId,
    ls.NguyenLieuId,
    nl.TenNguyenLieu,
    nl.DonViTinh,
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
FROM dbo.LichSuNguyenLieu ls
INNER JOIN dbo.NguyenLieu nl ON ls.NguyenLieuId = nl.NguyenLieuId
LEFT JOIN dbo.NguoiDung nd ON ls.NguoiDungId = nd.NguoiDungId
ORDER BY ls.ThoiGian DESC;";

        var result = new List<LichSuNguyenLieu>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Top", top);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new LichSuNguyenLieu
            {
                LichSuNguyenLieuId = reader.GetInt32(reader.GetOrdinal("LichSuNguyenLieuId")),
                NguyenLieuId = reader.GetInt32(reader.GetOrdinal("NguyenLieuId")),
                TenNguyenLieu = reader.GetString(reader.GetOrdinal("TenNguyenLieu")),
                DonViTinh = reader.GetString(reader.GetOrdinal("DonViTinh")),
                LoaiPhatSinh = reader.GetString(reader.GetOrdinal("LoaiPhatSinh")),
                SoLuongThayDoi = reader.GetDecimal(reader.GetOrdinal("SoLuongThayDoi")),
                TonTruoc = reader.GetDecimal(reader.GetOrdinal("TonTruoc")),
                TonSau = reader.GetDecimal(reader.GetOrdinal("TonSau")),
                HoaDonBanId = reader.IsDBNull(reader.GetOrdinal("HoaDonBanId")) ? null : reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
                HoaDonNhapId = reader.IsDBNull(reader.GetOrdinal("HoaDonNhapId")) ? null : reader.GetInt32(reader.GetOrdinal("HoaDonNhapId")),
                GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu")) ? null : reader.GetString(reader.GetOrdinal("GhiChu")),
                ThoiGian = reader.GetDateTime(reader.GetOrdinal("ThoiGian")),
                NguoiDungId = reader.IsDBNull(reader.GetOrdinal("NguoiDungId")) ? null : reader.GetInt32(reader.GetOrdinal("NguoiDungId")),
                TenNguoiDung = reader.IsDBNull(reader.GetOrdinal("TenNguoiDung")) ? null : reader.GetString(reader.GetOrdinal("TenNguoiDung"))
            });
        }

        return result;
    }
}
