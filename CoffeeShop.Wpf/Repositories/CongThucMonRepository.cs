using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class CongThucMonRepository : ICongThucMonRepository
{
    public async Task<IReadOnlyList<CongThucMon>> GetByMonAsync(
        int monId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                ct.CongThucMonId,
                ct.MonId,
                m.TenMon,
                ct.NguyenLieuId,
                nl.TenNguyenLieu,
                nl.DonViTinh,
                ct.DinhLuong,
                ct.GhiChu,
                ct.IsActive
            FROM dbo.CongThucMon ct
            INNER JOIN dbo.Mon m ON ct.MonId = m.MonId
            INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
            WHERE ct.MonId = @MonId";

        if (activeOnly)
        {
            sql += " AND ct.IsActive = 1";
        }

        sql += " ORDER BY nl.TenNguyenLieu;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@MonId", monId);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<CongThucMon>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapCongThucMon(reader));
        }

        return result;
    }

    public async Task<IReadOnlyList<CongThucMon>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                ct.CongThucMonId,
                ct.MonId,
                m.TenMon,
                ct.NguyenLieuId,
                nl.TenNguyenLieu,
                nl.DonViTinh,
                ct.DinhLuong,
                ct.GhiChu,
                ct.IsActive
            FROM dbo.CongThucMon ct
            INNER JOIN dbo.Mon m ON ct.MonId = m.MonId
            INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId";

        if (activeOnly)
        {
            sql += " WHERE ct.IsActive = 1";
        }

        sql += " ORDER BY m.TenMon, nl.TenNguyenLieu;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<CongThucMon>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapCongThucMon(reader));
        }

        return result;
    }

    public async Task<CongThucMon?> GetByIdAsync(
        int congThucMonId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                ct.CongThucMonId,
                ct.MonId,
                m.TenMon,
                ct.NguyenLieuId,
                nl.TenNguyenLieu,
                nl.DonViTinh,
                ct.DinhLuong,
                ct.GhiChu,
                ct.IsActive
            FROM dbo.CongThucMon ct
            INNER JOIN dbo.Mon m ON ct.MonId = m.MonId
            INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
            WHERE ct.CongThucMonId = @CongThucMonId;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CongThucMonId", congThucMonId);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapCongThucMon(reader);
        }

        return null;
    }

    public async Task<bool> ExistsAsync(
        int monId,
        int nguyenLieuId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM dbo.CongThucMon
            WHERE MonId = @MonId 
              AND NguyenLieuId = @NguyenLieuId
              AND IsActive = 1;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@MonId", monId);
        cmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);

        var count = (int)await cmd.ExecuteScalarAsync(cancellationToken);
        return count > 0;
    }

    public async Task<int> CreateAsync(
        CongThucMon congThucMon,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dbo.CongThucMon 
            (
                MonId,
                NguyenLieuId,
                DinhLuong,
                GhiChu,
                IsActive
            )
            VALUES 
            (
                @MonId,
                @NguyenLieuId,
                @DinhLuong,
                @GhiChu,
                @IsActive
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@MonId", congThucMon.MonId);
        cmd.Parameters.AddWithValue("@NguyenLieuId", congThucMon.NguyenLieuId);
        cmd.Parameters.AddWithValue("@DinhLuong", congThucMon.DinhLuong);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)congThucMon.GhiChu ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive", congThucMon.IsActive);

        var newId = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(newId);
    }

    public async Task UpdateAsync(
        CongThucMon congThucMon,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.CongThucMon
            SET 
                DinhLuong = @DinhLuong,
                GhiChu = @GhiChu,
                IsActive = @IsActive
            WHERE CongThucMonId = @CongThucMonId;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CongThucMonId", congThucMon.CongThucMonId);
        cmd.Parameters.AddWithValue("@DinhLuong", congThucMon.DinhLuong);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)congThucMon.GhiChu ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive", congThucMon.IsActive);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetActiveAsync(
        int congThucMonId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.CongThucMon
            SET IsActive = @IsActive
            WHERE CongThucMonId = @CongThucMonId;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CongThucMonId", congThucMonId);
        cmd.Parameters.AddWithValue("@IsActive", isActive);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CongThucMon>> GetByMonAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int monId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                ct.CongThucMonId,
                ct.MonId,
                m.TenMon,
                ct.NguyenLieuId,
                nl.TenNguyenLieu,
                nl.DonViTinh,
                ct.DinhLuong,
                ct.GhiChu,
                ct.IsActive
            FROM dbo.CongThucMon ct
            INNER JOIN dbo.Mon m ON ct.MonId = m.MonId
            INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
            WHERE ct.MonId = @MonId";

        if (activeOnly)
        {
            sql += " AND ct.IsActive = 1";
        }

        sql += " ORDER BY nl.TenNguyenLieu;";

        await using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@MonId", monId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<CongThucMon>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapCongThucMon(reader));
        }

        return result;
    }

    private static CongThucMon MapCongThucMon(SqlDataReader reader)
    {
        return new CongThucMon
        {
            CongThucMonId = reader.GetInt32(reader.GetOrdinal("CongThucMonId")),
            MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
            TenMon = reader.IsDBNull(reader.GetOrdinal("TenMon"))
                ? null
                : reader.GetString(reader.GetOrdinal("TenMon")),
            NguyenLieuId = reader.GetInt32(reader.GetOrdinal("NguyenLieuId")),
            TenNguyenLieu = reader.IsDBNull(reader.GetOrdinal("TenNguyenLieu"))
                ? null
                : reader.GetString(reader.GetOrdinal("TenNguyenLieu")),
            DonViTinh = reader.IsDBNull(reader.GetOrdinal("DonViTinh"))
                ? null
                : reader.GetString(reader.GetOrdinal("DonViTinh")),
            DinhLuong = reader.GetDecimal(reader.GetOrdinal("DinhLuong")),
            GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu"))
                ? null
                : reader.GetString(reader.GetOrdinal("GhiChu")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }
}
