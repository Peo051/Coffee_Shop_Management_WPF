using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class MonRepository : IMonRepository
{
    public async Task<IReadOnlyList<Mon>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT m.MonId,
       m.TenMon,
       m.DanhMucId,
       dm.TenDanhMuc,
       m.DonGia,
       m.TonKho,
       ISNULL(m.TonKhoToiThieu, 0) AS TonKhoToiThieu,
       m.HinhAnhPath,
       m.IsActive,
       m.CreatedAt
FROM dbo.Mon m
LEFT JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
ORDER BY m.CreatedAt DESC;";

        var result = new List<Mon>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapMon(reader));
        }

        return result;
    }

    public async Task<IReadOnlyList<Mon>> SearchAsync(string keyword, int? danhMucId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT m.MonId,
       m.TenMon,
       m.DanhMucId,
       dm.TenDanhMuc,
       m.DonGia,
       m.TonKho,
       ISNULL(m.TonKhoToiThieu, 0) AS TonKhoToiThieu,
       m.HinhAnhPath,
       m.IsActive,
       m.CreatedAt
FROM dbo.Mon m
LEFT JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
WHERE (@Keyword = N'' OR m.TenMon LIKE N'%' + @Keyword + N'%')
  AND (@DanhMucId IS NULL OR m.DanhMucId = @DanhMucId)
ORDER BY m.CreatedAt DESC;";

        var result = new List<Mon>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Keyword", keyword.Trim());
        command.Parameters.AddWithValue("@DanhMucId", (object?)danhMucId ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapMon(reader));
        }

        return result;
    }

    public async Task<Mon?> GetByIdAsync(int monId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT m.MonId,
       m.TenMon,
       m.DanhMucId,
       dm.TenDanhMuc,
       m.DonGia,
       m.TonKho,
       ISNULL(m.TonKhoToiThieu, 0) AS TonKhoToiThieu,
       m.HinhAnhPath,
       m.IsActive,
       m.CreatedAt
FROM dbo.Mon m
LEFT JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
WHERE m.MonId = @MonId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MonId", monId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapMon(reader);
    }

    public async Task<int> InsertAsync(Mon mon, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.Mon (TenMon, DanhMucId, DonGia, TonKho, TonKhoToiThieu, HinhAnhPath, IsActive, CreatedAt)
VALUES (@TenMon, @DanhMucId, @DonGia, @TonKho, @TonKhoToiThieu, @HinhAnhPath, @IsActive, SYSDATETIME());
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TenMon", mon.TenMon);
        command.Parameters.AddWithValue("@DanhMucId", mon.DanhMucId);
        command.Parameters.AddWithValue("@DonGia", mon.DonGia);
        command.Parameters.AddWithValue("@TonKho", mon.TonKho);
        command.Parameters.AddWithValue("@TonKhoToiThieu", mon.TonKhoToiThieu);
        command.Parameters.AddWithValue("@HinhAnhPath", (object?)mon.HinhAnhPath ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", mon.IsActive);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(scalar);
    }

    public async Task UpdateAsync(Mon mon, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.Mon
SET TenMon = @TenMon,
    DanhMucId = @DanhMucId,
    DonGia = @DonGia,
    TonKho = @TonKho,
    TonKhoToiThieu = @TonKhoToiThieu,
    HinhAnhPath = @HinhAnhPath,
    IsActive = @IsActive
WHERE MonId = @MonId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MonId", mon.MonId);
        command.Parameters.AddWithValue("@TenMon", mon.TenMon);
        command.Parameters.AddWithValue("@DanhMucId", mon.DanhMucId);
        command.Parameters.AddWithValue("@DonGia", mon.DonGia);
        command.Parameters.AddWithValue("@TonKho", mon.TonKho);
        command.Parameters.AddWithValue("@TonKhoToiThieu", mon.TonKhoToiThieu);
        command.Parameters.AddWithValue("@HinhAnhPath", (object?)mon.HinhAnhPath ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", mon.IsActive);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateTonKhoAsync(int monId, int soLuongThayDoi, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.Mon
SET TonKho = CASE
                 WHEN (TonKho + @SoLuongThayDoi) < 0 THEN 0
                 ELSE TonKho + @SoLuongThayDoi
             END
WHERE MonId = @MonId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MonId", monId);
        command.Parameters.AddWithValue("@SoLuongThayDoi", soLuongThayDoi);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Mon MapMon(SqlDataReader reader)
    {
        return new Mon
        {
            MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
            TenMon = reader.GetString(reader.GetOrdinal("TenMon")),
            DanhMucId = reader.GetInt32(reader.GetOrdinal("DanhMucId")),
            TenDanhMuc = reader.IsDBNull(reader.GetOrdinal("TenDanhMuc")) ? null : reader.GetString(reader.GetOrdinal("TenDanhMuc")),
            DonGia = reader.GetDecimal(reader.GetOrdinal("DonGia")),
            TonKho = reader.GetInt32(reader.GetOrdinal("TonKho")),
            TonKhoToiThieu = reader.GetInt32(reader.GetOrdinal("TonKhoToiThieu")),
            HinhAnhPath = reader.IsDBNull(reader.GetOrdinal("HinhAnhPath")) ? null : reader.GetString(reader.GetOrdinal("HinhAnhPath")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}
