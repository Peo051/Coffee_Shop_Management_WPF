using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class DanhMucRepository : IDanhMucRepository
{
    public async Task<IReadOnlyList<DanhMuc>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT DanhMucId, TenDanhMuc, MoTa, IsActive, CreatedAt
FROM dbo.DanhMuc
ORDER BY CreatedAt DESC;";

        var result = new List<DanhMuc>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapDanhMuc(reader));
        }

        return result;
    }

    public async Task<DanhMuc?> GetByIdAsync(int danhMucId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT DanhMucId, TenDanhMuc, MoTa, IsActive, CreatedAt
FROM dbo.DanhMuc
WHERE DanhMucId = @DanhMucId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DanhMucId", danhMucId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapDanhMuc(reader);
    }

    public async Task<int> InsertAsync(DanhMuc danhMuc, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.DanhMuc (TenDanhMuc, MoTa, IsActive, CreatedAt)
VALUES (@TenDanhMuc, @MoTa, @IsActive, SYSDATETIME());
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TenDanhMuc", danhMuc.TenDanhMuc);
        command.Parameters.AddWithValue("@MoTa", (object?)danhMuc.MoTa ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", danhMuc.IsActive);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(scalar);
    }

    public async Task UpdateAsync(DanhMuc danhMuc, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.DanhMuc
SET TenDanhMuc = @TenDanhMuc,
    MoTa = @MoTa,
    IsActive = @IsActive
WHERE DanhMucId = @DanhMucId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DanhMucId", danhMuc.DanhMucId);
        command.Parameters.AddWithValue("@TenDanhMuc", danhMuc.TenDanhMuc);
        command.Parameters.AddWithValue("@MoTa", (object?)danhMuc.MoTa ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", danhMuc.IsActive);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(int danhMucId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.DanhMuc
SET IsActive = 0
WHERE DanhMucId = @DanhMucId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DanhMucId", danhMucId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static DanhMuc MapDanhMuc(SqlDataReader reader)
    {
        return new DanhMuc
        {
            DanhMucId = reader.GetInt32(reader.GetOrdinal("DanhMucId")),
            TenDanhMuc = reader.GetString(reader.GetOrdinal("TenDanhMuc")),
            MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}
