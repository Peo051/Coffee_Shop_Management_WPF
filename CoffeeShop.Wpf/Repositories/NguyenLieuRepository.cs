using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class NguyenLieuRepository : INguyenLieuRepository
{
    public async Task<IReadOnlyList<NguyenLieu>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                NguyenLieuId,
                TenNguyenLieu,
                DonViTinh,
                TonKho,
                TonKhoToiThieu,
                DonGiaNhap,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM dbo.NguyenLieu";

        if (activeOnly)
        {
            sql += " WHERE IsActive = 1";
        }

        sql += " ORDER BY TenNguyenLieu;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<NguyenLieu>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapNguyenLieu(reader));
        }

        return result;
    }

    public async Task<NguyenLieu?> GetByIdAsync(
        int nguyenLieuId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                NguyenLieuId,
                TenNguyenLieu,
                DonViTinh,
                TonKho,
                TonKhoToiThieu,
                DonGiaNhap,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM dbo.NguyenLieu
            WHERE NguyenLieuId = @NguyenLieuId;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapNguyenLieu(reader);
        }

        return null;
    }

    public async Task<IReadOnlyList<NguyenLieu>> SearchAsync(
        string? keyword,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                NguyenLieuId,
                TenNguyenLieu,
                DonViTinh,
                TonKho,
                TonKhoToiThieu,
                DonGiaNhap,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM dbo.NguyenLieu
            WHERE 1=1";

        if (activeOnly)
        {
            sql += " AND IsActive = 1";
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            sql += " AND TenNguyenLieu LIKE @Keyword";
        }

        sql += " ORDER BY TenNguyenLieu;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
        }

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<NguyenLieu>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapNguyenLieu(reader));
        }

        return result;
    }

    public async Task<int> CreateAsync(
        NguyenLieu nguyenLieu,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dbo.NguyenLieu 
            (
                TenNguyenLieu,
                DonViTinh,
                TonKho,
                TonKhoToiThieu,
                DonGiaNhap,
                IsActive,
                CreatedAt
            )
            VALUES 
            (
                @TenNguyenLieu,
                @DonViTinh,
                @TonKho,
                @TonKhoToiThieu,
                @DonGiaNhap,
                @IsActive,
                SYSDATETIME()
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@TenNguyenLieu", nguyenLieu.TenNguyenLieu);
        cmd.Parameters.AddWithValue("@DonViTinh", nguyenLieu.DonViTinh);
        cmd.Parameters.AddWithValue("@TonKho", nguyenLieu.TonKho);
        cmd.Parameters.AddWithValue("@TonKhoToiThieu", nguyenLieu.TonKhoToiThieu);
        cmd.Parameters.AddWithValue("@DonGiaNhap", nguyenLieu.DonGiaNhap);
        cmd.Parameters.AddWithValue("@IsActive", nguyenLieu.IsActive);

        var newId = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(newId);
    }

    public async Task UpdateAsync(
        NguyenLieu nguyenLieu,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.NguyenLieu
            SET 
                TenNguyenLieu = @TenNguyenLieu,
                DonViTinh = @DonViTinh,
                TonKho = @TonKho,
                TonKhoToiThieu = @TonKhoToiThieu,
                DonGiaNhap = @DonGiaNhap,
                IsActive = @IsActive,
                UpdatedAt = SYSDATETIME()
            WHERE NguyenLieuId = @NguyenLieuId;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieu.NguyenLieuId);
        cmd.Parameters.AddWithValue("@TenNguyenLieu", nguyenLieu.TenNguyenLieu);
        cmd.Parameters.AddWithValue("@DonViTinh", nguyenLieu.DonViTinh);
        cmd.Parameters.AddWithValue("@TonKho", nguyenLieu.TonKho);
        cmd.Parameters.AddWithValue("@TonKhoToiThieu", nguyenLieu.TonKhoToiThieu);
        cmd.Parameters.AddWithValue("@DonGiaNhap", nguyenLieu.DonGiaNhap);
        cmd.Parameters.AddWithValue("@IsActive", nguyenLieu.IsActive);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task CapNhatTonKhoAsync(
        int nguyenLieuId,
        decimal tonKhoMoi,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.NguyenLieu
            SET 
                TonKho = @TonKhoMoi,
                UpdatedAt = SYSDATETIME()
            WHERE NguyenLieuId = @NguyenLieuId;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);
        cmd.Parameters.AddWithValue("@TonKhoMoi", tonKhoMoi);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetActiveAsync(
        int nguyenLieuId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.NguyenLieu
            SET 
                IsActive = @IsActive,
                UpdatedAt = SYSDATETIME()
            WHERE NguyenLieuId = @NguyenLieuId;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);
        cmd.Parameters.AddWithValue("@IsActive", isActive);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<decimal> GetTonKhoAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int nguyenLieuId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TonKho 
            FROM dbo.NguyenLieu 
            WHERE NguyenLieuId = @NguyenLieuId;";

        await using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result != null ? Convert.ToDecimal(result) : 0;
    }

    public async Task<int> TruTonKhoAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int nguyenLieuId,
        decimal soLuongTru,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.NguyenLieu
            SET 
                TonKho = TonKho - @SoLuongTru,
                UpdatedAt = SYSDATETIME()
            WHERE NguyenLieuId = @NguyenLieuId
              AND TonKho >= @SoLuongTru;";

        await using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);
        cmd.Parameters.AddWithValue("@SoLuongTru", soLuongTru);

        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static NguyenLieu MapNguyenLieu(SqlDataReader reader)
    {
        return new NguyenLieu
        {
            NguyenLieuId = reader.GetInt32(reader.GetOrdinal("NguyenLieuId")),
            TenNguyenLieu = reader.GetString(reader.GetOrdinal("TenNguyenLieu")),
            DonViTinh = reader.GetString(reader.GetOrdinal("DonViTinh")),
            TonKho = reader.GetDecimal(reader.GetOrdinal("TonKho")),
            TonKhoToiThieu = reader.GetDecimal(reader.GetOrdinal("TonKhoToiThieu")),
            DonGiaNhap = reader.GetDecimal(reader.GetOrdinal("DonGiaNhap")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
