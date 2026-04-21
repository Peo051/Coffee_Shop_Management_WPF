using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class BanRepository : IBanRepository
{
    public async Task<IReadOnlyList<Ban>> GetDanhSachBanAsync(
        string? keyword,
        int? khuVucId,
        bool? isActive,
        string? trangThaiBan,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT b.BanId,
       b.KhuVucId,
       kv.TenKhuVuc,
       b.TenBan,
       b.TrangThaiBan,
       b.IsActive,
       b.CreatedAt
FROM dbo.Ban b
INNER JOIN dbo.KhuVuc kv ON kv.KhuVucId = b.KhuVucId
WHERE (@Keyword = N'' OR b.TenBan LIKE N'%' + @Keyword + N'%')
  AND (@KhuVucId IS NULL OR b.KhuVucId = @KhuVucId)
  AND (@IsActive IS NULL OR b.IsActive = @IsActive)
  AND (@TrangThaiBan IS NULL OR b.TrangThaiBan = @TrangThaiBan)
ORDER BY kv.TenKhuVuc, b.TenBan;";

        var result = new List<Ban>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Keyword", (keyword ?? string.Empty).Trim());
        command.Parameters.AddWithValue("@KhuVucId", (object?)khuVucId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);
        command.Parameters.AddWithValue("@TrangThaiBan", string.IsNullOrWhiteSpace(trangThaiBan) ? DBNull.Value : trangThaiBan.Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapBan(reader));
        }

        return result;
    }

    public async Task<IReadOnlyList<KhuVuc>> GetDanhSachKhuVucAsync(
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT KhuVucId, TenKhuVuc, MoTa, IsActive, CreatedAt
FROM dbo.KhuVuc
WHERE (@OnlyActive = 0 OR IsActive = 1)
ORDER BY TenKhuVuc;";

        var result = new List<KhuVuc>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@OnlyActive", onlyActive);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new KhuVuc
            {
                KhuVucId = reader.GetInt32(reader.GetOrdinal("KhuVucId")),
                TenKhuVuc = reader.GetString(reader.GetOrdinal("TenKhuVuc")),
                MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }

        return result;
    }

    public async Task<Ban?> GetByIdAsync(int banId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT b.BanId,
       b.KhuVucId,
       kv.TenKhuVuc,
       b.TenBan,
       b.TrangThaiBan,
       b.IsActive,
       b.CreatedAt
FROM dbo.Ban b
INNER JOIN dbo.KhuVuc kv ON kv.KhuVucId = b.KhuVucId
WHERE b.BanId = @BanId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@BanId", banId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapBan(reader);
    }

    public async Task<int> TaoBanAsync(
        int khuVucId,
        string tenBan,
        string trangThaiBan,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.Ban (KhuVucId, TenBan, TrangThaiBan, IsActive)
VALUES (@KhuVucId, @TenBan, @TrangThaiBan, @IsActive);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@KhuVucId", khuVucId);
        command.Parameters.AddWithValue("@TenBan", tenBan.Trim());
        command.Parameters.AddWithValue("@TrangThaiBan", trangThaiBan);
        command.Parameters.AddWithValue("@IsActive", isActive);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(scalar);
    }

    public async Task<bool> CapNhatTrangThaiBanAsync(
        int banId,
        string trangThaiBan,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.Ban
SET TrangThaiBan = @TrangThaiBan
WHERE BanId = @BanId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@BanId", banId);
        command.Parameters.AddWithValue("@TrangThaiBan", trangThaiBan);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> ChuyenBanAsync(
        int fromBanId,
        int toBanId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string updateToBanSql = @"
UPDATE dbo.Ban
SET TrangThaiBan = @TrangThaiMoi
WHERE BanId = @BanId
  AND IsActive = 1
  AND TrangThaiBan <> @TrangThaiTamKhoa;";

            const string updateFromBanSql = @"
UPDATE dbo.Ban
SET TrangThaiBan = @TrangThaiMoi
WHERE BanId = @BanId
  AND IsActive = 1;";

            await using var updateTo = new SqlCommand(updateToBanSql, connection, (SqlTransaction)transaction);
            updateTo.Parameters.AddWithValue("@BanId", toBanId);
            updateTo.Parameters.AddWithValue("@TrangThaiMoi", TrangThaiBanConst.DangCoKhach);
            updateTo.Parameters.AddWithValue("@TrangThaiTamKhoa", TrangThaiBanConst.TamKhoa);
            var toAffected = await updateTo.ExecuteNonQueryAsync(cancellationToken);

            await using var updateFrom = new SqlCommand(updateFromBanSql, connection, (SqlTransaction)transaction);
            updateFrom.Parameters.AddWithValue("@BanId", fromBanId);
            updateFrom.Parameters.AddWithValue("@TrangThaiMoi", TrangThaiBanConst.Trong);
            var fromAffected = await updateFrom.ExecuteNonQueryAsync(cancellationToken);

            if (toAffected == 0 || fromAffected == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> GopBanAsync(
        int sourceBanId,
        int targetBanId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string updateTargetSql = @"
UPDATE dbo.Ban
SET TrangThaiBan = @TrangThaiMoi
WHERE BanId = @BanId
  AND IsActive = 1
  AND TrangThaiBan <> @TrangThaiTamKhoa;";

            const string updateSourceSql = @"
UPDATE dbo.Ban
SET TrangThaiBan = @TrangThaiMoi
WHERE BanId = @BanId
  AND IsActive = 1;";

            await using var updateTarget = new SqlCommand(updateTargetSql, connection, (SqlTransaction)transaction);
            updateTarget.Parameters.AddWithValue("@BanId", targetBanId);
            updateTarget.Parameters.AddWithValue("@TrangThaiMoi", TrangThaiBanConst.DangCoKhach);
            updateTarget.Parameters.AddWithValue("@TrangThaiTamKhoa", TrangThaiBanConst.TamKhoa);
            var targetAffected = await updateTarget.ExecuteNonQueryAsync(cancellationToken);

            await using var updateSource = new SqlCommand(updateSourceSql, connection, (SqlTransaction)transaction);
            updateSource.Parameters.AddWithValue("@BanId", sourceBanId);
            updateSource.Parameters.AddWithValue("@TrangThaiMoi", TrangThaiBanConst.TamKhoa);
            var sourceAffected = await updateSource.ExecuteNonQueryAsync(cancellationToken);

            if (targetAffected == 0 || sourceAffected == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static Ban MapBan(SqlDataReader reader)
    {
        return new Ban
        {
            BanId = reader.GetInt32(reader.GetOrdinal("BanId")),
            KhuVucId = reader.GetInt32(reader.GetOrdinal("KhuVucId")),
            TenKhuVuc = reader.GetString(reader.GetOrdinal("TenKhuVuc")),
            TenBan = reader.GetString(reader.GetOrdinal("TenBan")),
            TrangThaiBan = reader.GetString(reader.GetOrdinal("TrangThaiBan")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}

