using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class KhoRepository : IKhoRepository
{
    public async Task<IReadOnlyList<TrangThaiSanPhamDong>> GetTrangThaiSanPhamAsync(
        string? keyword,
        int? danhMucId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT m.MonId,
       m.TenMon,
       m.DanhMucId,
       dm.TenDanhMuc,
       m.TonKho,
       m.MucCanhBaoTonKho,
       m.IsActive,
       m.CreatedAt
FROM dbo.Mon m
INNER JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
WHERE (@Keyword = N'' OR m.TenMon LIKE N'%' + @Keyword + N'%')
  AND (@DanhMucId IS NULL OR m.DanhMucId = @DanhMucId)
  AND (@IsActive IS NULL OR m.IsActive = @IsActive)
ORDER BY m.IsActive DESC, m.TonKho ASC, m.CreatedAt DESC, m.MonId DESC;";

        var result = new List<TrangThaiSanPhamDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Keyword", (keyword ?? string.Empty).Trim());
        command.Parameters.AddWithValue("@DanhMucId", (object?)danhMucId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new TrangThaiSanPhamDong
            {
                MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
                TenMon = reader.GetString(reader.GetOrdinal("TenMon")),
                DanhMucId = reader.GetInt32(reader.GetOrdinal("DanhMucId")),
                TenDanhMuc = reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                TonKho = reader.GetInt32(reader.GetOrdinal("TonKho")),
                MucCanhBaoTonKho = reader.GetInt32(reader.GetOrdinal("MucCanhBaoTonKho")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<CanhBaoTonKhoThapDong>> GetCanhBaoTonKhoThapAsync(
        string? keyword,
        int? danhMucId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT m.MonId,
       m.TenMon,
       m.DanhMucId,
       dm.TenDanhMuc,
       m.TonKho,
       m.MucCanhBaoTonKho
FROM dbo.Mon m
INNER JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
WHERE m.IsActive = 1
  AND m.TonKho <= m.MucCanhBaoTonKho
  AND (@Keyword = N'' OR m.TenMon LIKE N'%' + @Keyword + N'%')
  AND (@DanhMucId IS NULL OR m.DanhMucId = @DanhMucId)
ORDER BY (m.MucCanhBaoTonKho - m.TonKho) DESC, m.TonKho ASC, m.MonId ASC;";

        var result = new List<CanhBaoTonKhoThapDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Keyword", (keyword ?? string.Empty).Trim());
        command.Parameters.AddWithValue("@DanhMucId", (object?)danhMucId ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new CanhBaoTonKhoThapDong
            {
                MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
                TenMon = reader.GetString(reader.GetOrdinal("TenMon")),
                DanhMucId = reader.GetInt32(reader.GetOrdinal("DanhMucId")),
                TenDanhMuc = reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                TonKho = reader.GetInt32(reader.GetOrdinal("TonKho")),
                MucCanhBaoTonKho = reader.GetInt32(reader.GetOrdinal("MucCanhBaoTonKho"))
            });
        }

        return result;
    }

    public async Task<bool> UpdateTrangThaiKinhDoanhAsync(
        int monId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.Mon
SET IsActive = @IsActive
WHERE MonId = @MonId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MonId", monId);
        command.Parameters.AddWithValue("@IsActive", isActive);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> UpdateMucCanhBaoTonKhoAsync(
        int monId,
        int mucCanhBaoTonKho,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.Mon
SET MucCanhBaoTonKho = @MucCanhBaoTonKho
WHERE MonId = @MonId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MonId", monId);
        command.Parameters.AddWithValue("@MucCanhBaoTonKho", mucCanhBaoTonKho);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }
}

