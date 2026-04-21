using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    public async Task<int> GhiLogAsync(
        int? nguoiDungId,
        string hanhDong,
        string? doiTuong,
        string? duLieuTomTat,
        string? mayTram,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.AuditLog (NguoiDungId, HanhDong, DoiTuong, DuLieuTomTat, ThoiGianTao, MayTram)
VALUES (@NguoiDungId, @HanhDong, @DoiTuong, @DuLieuTomTat, SYSDATETIME(), @MayTram);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NguoiDungId", (object?)nguoiDungId ?? DBNull.Value);
        command.Parameters.AddWithValue("@HanhDong", hanhDong.Trim());
        command.Parameters.AddWithValue("@DoiTuong", string.IsNullOrWhiteSpace(doiTuong) ? DBNull.Value : doiTuong.Trim());
        command.Parameters.AddWithValue("@DuLieuTomTat", string.IsNullOrWhiteSpace(duLieuTomTat) ? DBNull.Value : duLieuTomTat.Trim());
        command.Parameters.AddWithValue("@MayTram", string.IsNullOrWhiteSpace(mayTram) ? DBNull.Value : mayTram.Trim());

        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public Task<IReadOnlyList<AuditLogDong>> GetDanhSachLogAsync(
        DateTime fromDate,
        DateTime toDate,
        int topN,
        CancellationToken cancellationToken = default)
    {
        return TimKiemLogAsync(fromDate, toDate, null, null, null, topN, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDong>> TimKiemLogAsync(
        DateTime fromDate,
        DateTime toDate,
        int? nguoiDungId,
        string? hanhDong,
        string? keyword,
        int topN,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT TOP (@TopN)
       al.AuditLogId,
       al.NguoiDungId,
       nd.TenDangNhap,
       al.HanhDong,
       al.DoiTuong,
       al.DuLieuTomTat,
       al.ThoiGianTao,
       al.MayTram
FROM dbo.AuditLog al
LEFT JOIN dbo.NguoiDung nd ON nd.NguoiDungId = al.NguoiDungId
WHERE al.ThoiGianTao >= @FromDate
  AND al.ThoiGianTao < @ToDateExclusive
  AND (@NguoiDungId IS NULL OR al.NguoiDungId = @NguoiDungId)
  AND (@HanhDong IS NULL OR al.HanhDong = @HanhDong)
  AND (
        @Keyword IS NULL
        OR al.HanhDong LIKE N'%' + @Keyword + N'%'
        OR al.DoiTuong LIKE N'%' + @Keyword + N'%'
        OR al.DuLieuTomTat LIKE N'%' + @Keyword + N'%'
      )
ORDER BY al.ThoiGianTao DESC, al.AuditLogId DESC;";

        var result = new List<AuditLogDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TopN", topN);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));
        command.Parameters.AddWithValue("@NguoiDungId", (object?)nguoiDungId ?? DBNull.Value);
        command.Parameters.AddWithValue("@HanhDong", string.IsNullOrWhiteSpace(hanhDong) ? DBNull.Value : hanhDong.Trim());
        command.Parameters.AddWithValue("@Keyword", string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : keyword.Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AuditLogDong
            {
                AuditLogId = reader.GetInt32(reader.GetOrdinal("AuditLogId")),
                NguoiDungId = reader.IsDBNull(reader.GetOrdinal("NguoiDungId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("NguoiDungId")),
                TenDangNhap = reader.IsDBNull(reader.GetOrdinal("TenDangNhap"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("TenDangNhap")),
                HanhDong = reader.GetString(reader.GetOrdinal("HanhDong")),
                DoiTuong = reader.IsDBNull(reader.GetOrdinal("DoiTuong"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("DoiTuong")),
                DuLieuTomTat = reader.IsDBNull(reader.GetOrdinal("DuLieuTomTat"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("DuLieuTomTat")),
                ThoiGianTao = reader.GetDateTime(reader.GetOrdinal("ThoiGianTao")),
                MayTram = reader.IsDBNull(reader.GetOrdinal("MayTram"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("MayTram"))
            });
        }

        return result;
    }
}

