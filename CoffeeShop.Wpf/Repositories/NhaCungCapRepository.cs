using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class NhaCungCapRepository : INhaCungCapRepository
{
    public async Task<IReadOnlyList<NhaCungCap>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT NhaCungCapId, TenNhaCungCap, SoDienThoai, DiaChi, IsActive, CreatedAt
FROM dbo.NhaCungCap
ORDER BY CreatedAt DESC;";

        var result = new List<NhaCungCap>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapNhaCungCap(reader));
        }

        return result;
    }

    public async Task<NhaCungCap?> GetByIdAsync(int nhaCungCapId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT NhaCungCapId, TenNhaCungCap, SoDienThoai, DiaChi, IsActive, CreatedAt
FROM dbo.NhaCungCap
WHERE NhaCungCapId = @NhaCungCapId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NhaCungCapId", nhaCungCapId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapNhaCungCap(reader);
    }

    public async Task<int> InsertAsync(NhaCungCap nhaCungCap, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.NhaCungCap (TenNhaCungCap, SoDienThoai, DiaChi, IsActive, CreatedAt)
VALUES (@TenNhaCungCap, @SoDienThoai, @DiaChi, @IsActive, SYSDATETIME());
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TenNhaCungCap", nhaCungCap.TenNhaCungCap);
        command.Parameters.AddWithValue("@SoDienThoai", (object?)nhaCungCap.SoDienThoai ?? DBNull.Value);
        command.Parameters.AddWithValue("@DiaChi", (object?)nhaCungCap.DiaChi ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", nhaCungCap.IsActive);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(scalar);
    }

    public async Task UpdateAsync(NhaCungCap nhaCungCap, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.NhaCungCap
SET TenNhaCungCap = @TenNhaCungCap,
    SoDienThoai = @SoDienThoai,
    DiaChi = @DiaChi,
    IsActive = @IsActive
WHERE NhaCungCapId = @NhaCungCapId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NhaCungCapId", nhaCungCap.NhaCungCapId);
        command.Parameters.AddWithValue("@TenNhaCungCap", nhaCungCap.TenNhaCungCap);
        command.Parameters.AddWithValue("@SoDienThoai", (object?)nhaCungCap.SoDienThoai ?? DBNull.Value);
        command.Parameters.AddWithValue("@DiaChi", (object?)nhaCungCap.DiaChi ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", nhaCungCap.IsActive);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(int nhaCungCapId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.NhaCungCap
SET IsActive = 0
WHERE NhaCungCapId = @NhaCungCapId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NhaCungCapId", nhaCungCapId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static NhaCungCap MapNhaCungCap(SqlDataReader reader)
    {
        return new NhaCungCap
        {
            NhaCungCapId = reader.GetInt32(reader.GetOrdinal("NhaCungCapId")),
            TenNhaCungCap = reader.GetString(reader.GetOrdinal("TenNhaCungCap")),
            SoDienThoai = reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? null : reader.GetString(reader.GetOrdinal("SoDienThoai")),
            DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? null : reader.GetString(reader.GetOrdinal("DiaChi")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}
