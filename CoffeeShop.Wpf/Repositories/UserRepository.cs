using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class UserRepository : IUserRepository
{
    private const string NguoiDungCreatedAtColumnName = "CreatedAt";

    public async Task<UserAuthRecord?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT nd.NguoiDungId,
       nd.TenDangNhap,
       nd.MatKhau,
       nd.HoTen,
       nd.IsActive,
       vt.MaVaiTro
FROM dbo.NguoiDung nd
INNER JOIN dbo.VaiTro vt ON vt.VaiTroId = nd.VaiTroId
WHERE nd.TenDangNhap = @TenDangNhap;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TenDangNhap", username.Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserAuthRecord
        {
            UserId = reader.GetInt32(reader.GetOrdinal("NguoiDungId")),
            Username = reader.GetString(reader.GetOrdinal("TenDangNhap")),
            Password = reader.GetString(reader.GetOrdinal("MatKhau")),
            DisplayName = reader.GetString(reader.GetOrdinal("HoTen")),
            Role = reader.GetString(reader.GetOrdinal("MaVaiTro")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }

    public async Task<UserAuthRecord?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT nd.NguoiDungId,
       nd.TenDangNhap,
       nd.MatKhau,
       nd.HoTen,
       nd.IsActive,
       vt.MaVaiTro
FROM dbo.NguoiDung nd
INNER JOIN dbo.VaiTro vt ON vt.VaiTroId = nd.VaiTroId
WHERE nd.NguoiDungId = @NguoiDungId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NguoiDungId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserAuthRecord
        {
            UserId = reader.GetInt32(reader.GetOrdinal("NguoiDungId")),
            Username = reader.GetString(reader.GetOrdinal("TenDangNhap")),
            Password = reader.GetString(reader.GetOrdinal("MatKhau")),
            DisplayName = reader.GetString(reader.GetOrdinal("HoTen")),
            Role = reader.GetString(reader.GetOrdinal("MaVaiTro")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }

    public async Task<IReadOnlyList<TaiKhoanNguoiDung>> GetDanhSachTaiKhoanAsync(
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        var hasCreatedAtColumn = await HasNguoiDungCreatedAtColumnAsync(cancellationToken);
        var sql = hasCreatedAtColumn
            ? @"
SELECT nd.NguoiDungId,
       nd.TenDangNhap,
       nd.HoTen,
       vt.MaVaiTro,
       vt.TenVaiTro,
       nd.IsActive,
       nd.CreatedAt
FROM dbo.NguoiDung nd
INNER JOIN dbo.VaiTro vt ON vt.VaiTroId = nd.VaiTroId
WHERE (
        @Keyword = N''
        OR nd.TenDangNhap LIKE N'%' + @Keyword + N'%'
        OR nd.HoTen LIKE N'%' + @Keyword + N'%'
      )
ORDER BY nd.NguoiDungId ASC"
            : @"
SELECT nd.NguoiDungId,
       nd.TenDangNhap,
       nd.HoTen,
       vt.MaVaiTro,
       vt.TenVaiTro,
       nd.IsActive,
       CAST(SYSDATETIME() AS DATETIME2) AS CreatedAt
FROM dbo.NguoiDung nd
INNER JOIN dbo.VaiTro vt ON vt.VaiTroId = nd.VaiTroId
WHERE (
        @Keyword = N''
        OR nd.TenDangNhap LIKE N'%' + @Keyword + N'%'
        OR nd.HoTen LIKE N'%' + @Keyword + N'%'
      )
ORDER BY nd.NguoiDungId ASC;";

        var result = new List<TaiKhoanNguoiDung>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Keyword", (keyword ?? string.Empty).Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new TaiKhoanNguoiDung
            {
                NguoiDungId = reader.GetInt32(reader.GetOrdinal("NguoiDungId")),
                TenDangNhap = reader.IsDBNull(reader.GetOrdinal("TenDangNhap"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("TenDangNhap")),
                HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("HoTen")),
                MaVaiTro = reader.IsDBNull(reader.GetOrdinal("MaVaiTro"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("MaVaiTro")),
                TenVaiTro = reader.IsDBNull(reader.GetOrdinal("TenVaiTro"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("TenVaiTro")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }

        return result;
    }

    public async Task<bool> UpdatePasswordAsync(
        int userId,
        string passwordHash,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.NguoiDung
SET MatKhau = @MatKhau
WHERE NguoiDungId = @NguoiDungId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NguoiDungId", userId);
        command.Parameters.AddWithValue("@MatKhau", passwordHash);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> SetIsActiveAsync(
        int userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.NguoiDung
SET IsActive = @IsActive
WHERE NguoiDungId = @NguoiDungId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NguoiDungId", userId);
        command.Parameters.AddWithValue("@IsActive", isActive);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> IsTenDangNhapExistsAsync(
        string tenDangNhap,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT COUNT(1)
FROM dbo.NguoiDung
WHERE TenDangNhap = @TenDangNhap;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TenDangNhap", tenDangNhap.Trim());

        var count = (int)await command.ExecuteScalarAsync(cancellationToken);
        return count > 0;
    }

    public async Task<int> CreateTaiKhoanAsync(
        string tenDangNhap,
        string matKhauHash,
        string hoTen,
        int vaiTroId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var hasCreatedAtColumn = await HasNguoiDungCreatedAtColumnAsync(cancellationToken);
        var sql = hasCreatedAtColumn
            ? @"
INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive, CreatedAt)
VALUES (@TenDangNhap, @MatKhau, @HoTen, @VaiTroId, @IsActive, SYSDATETIME());

SELECT CAST(SCOPE_IDENTITY() AS INT);"
            : @"
INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive)
VALUES (@TenDangNhap, @MatKhau, @HoTen, @VaiTroId, @IsActive);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TenDangNhap", tenDangNhap.Trim());
        command.Parameters.AddWithValue("@MatKhau", matKhauHash);
        command.Parameters.AddWithValue("@HoTen", hoTen.Trim());
        command.Parameters.AddWithValue("@VaiTroId", vaiTroId);
        command.Parameters.AddWithValue("@IsActive", isActive);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is int id ? id : 0;
    }

    public async Task<IReadOnlyList<VaiTro>> GetDanhSachVaiTroAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT VaiTroId, MaVaiTro, TenVaiTro
FROM dbo.VaiTro
ORDER BY VaiTroId ASC;";

        var result = new List<VaiTro>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new VaiTro
            {
                VaiTroId = reader.GetInt32(reader.GetOrdinal("VaiTroId")),
                MaVaiTro = reader.GetString(reader.GetOrdinal("MaVaiTro")),
                TenVaiTro = reader.GetString(reader.GetOrdinal("TenVaiTro"))
            });
        }

        return result;
    }

    private static async Task<bool> HasNguoiDungCreatedAtColumnAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT CASE
         WHEN COL_LENGTH(N'dbo.NguoiDung', @ColumnName) IS NULL THEN 0
         ELSE 1
       END;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ColumnName", NguoiDungCreatedAtColumnName);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is int value && value == 1;
    }
}
