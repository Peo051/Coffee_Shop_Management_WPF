using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class CauHinhHeThongRepository : ICauHinhHeThongRepository
{
    private async Task EnsurePayOsColumnsExistAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = @"
        IF COL_LENGTH(N'dbo.CauHinhHeThong', N'PayOsClientId') IS NULL
        BEGIN
            ALTER TABLE dbo.CauHinhHeThong ADD PayOsClientId NVARCHAR(150) NULL;
        END
        IF COL_LENGTH(N'dbo.CauHinhHeThong', N'PayOsApiKey') IS NULL
        BEGIN
            ALTER TABLE dbo.CauHinhHeThong ADD PayOsApiKey NVARCHAR(150) NULL;
        END
        IF COL_LENGTH(N'dbo.CauHinhHeThong', N'PayOsChecksumKey') IS NULL
        BEGIN
            ALTER TABLE dbo.CauHinhHeThong ADD PayOsChecksumKey NVARCHAR(200) NULL;
        END";

        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<CauHinhHeThong?> GetCauHinhAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT TOP (1)
               CauHinhHeThongId,
               TenQuan,
               DiaChi,
               SoDienThoai,
               FooterHoaDon,
               LogoPath,
               PayOsClientId,
               PayOsApiKey,
               PayOsChecksumKey,
               UpdatedAt
        FROM dbo.CauHinhHeThong
        ORDER BY CauHinhHeThongId DESC;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsurePayOsColumnsExistAsync(connection, cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapCauHinh(reader);
    }

    public async Task<int> LuuCauHinhAsync(CauHinhHeThong cauHinh, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsurePayOsColumnsExistAsync(connection, cancellationToken);

        var current = await GetCauHinhAsync(cancellationToken);

        if (current is null)
        {
            const string insertSql = @"
INSERT INTO dbo.CauHinhHeThong
(
    TenQuan,
    DiaChi,
    SoDienThoai,
    FooterHoaDon,
    LogoPath,
    PayOsClientId,
    PayOsApiKey,
    PayOsChecksumKey,
    UpdatedAt
)
VALUES
(
    @TenQuan,
    @DiaChi,
    @SoDienThoai,
    @FooterHoaDon,
    @LogoPath,
    @PayOsClientId,
    @PayOsApiKey,
    @PayOsChecksumKey,
    SYSDATETIME()
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var insertCommand = new SqlCommand(insertSql, connection);
            BindParameters(insertCommand, cauHinh);
            var scalar = await insertCommand.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(scalar);
        }

        const string updateSql = @"
UPDATE dbo.CauHinhHeThong
SET TenQuan = @TenQuan,
    DiaChi = @DiaChi,
    SoDienThoai = @SoDienThoai,
    FooterHoaDon = @FooterHoaDon,
    LogoPath = @LogoPath,
    PayOsClientId = @PayOsClientId,
    PayOsApiKey = @PayOsApiKey,
    PayOsChecksumKey = @PayOsChecksumKey,
    UpdatedAt = SYSDATETIME()
WHERE CauHinhHeThongId = @CauHinhHeThongId;";

        await using var updateCommand = new SqlCommand(updateSql, connection);
        updateCommand.Parameters.AddWithValue("@CauHinhHeThongId", current.CauHinhHeThongId);
        BindParameters(updateCommand, cauHinh);

        await updateCommand.ExecuteNonQueryAsync(cancellationToken);
        return current.CauHinhHeThongId;
    }

    private static void BindParameters(SqlCommand command, CauHinhHeThong cauHinh)
    {
        command.Parameters.AddWithValue("@TenQuan", cauHinh.TenQuan.Trim());
        command.Parameters.AddWithValue("@DiaChi", string.IsNullOrWhiteSpace(cauHinh.DiaChi) ? DBNull.Value : cauHinh.DiaChi.Trim());
        command.Parameters.AddWithValue("@SoDienThoai", string.IsNullOrWhiteSpace(cauHinh.SoDienThoai) ? DBNull.Value : cauHinh.SoDienThoai.Trim());
        command.Parameters.AddWithValue("@FooterHoaDon", string.IsNullOrWhiteSpace(cauHinh.FooterHoaDon) ? DBNull.Value : cauHinh.FooterHoaDon.Trim());
        command.Parameters.AddWithValue("@LogoPath", string.IsNullOrWhiteSpace(cauHinh.LogoPath) ? DBNull.Value : cauHinh.LogoPath.Trim());
        command.Parameters.AddWithValue("@PayOsClientId", string.IsNullOrWhiteSpace(cauHinh.PayOsClientId) ? DBNull.Value : cauHinh.PayOsClientId.Trim());
        command.Parameters.AddWithValue("@PayOsApiKey", string.IsNullOrWhiteSpace(cauHinh.PayOsApiKey) ? DBNull.Value : cauHinh.PayOsApiKey.Trim());
        command.Parameters.AddWithValue("@PayOsChecksumKey", string.IsNullOrWhiteSpace(cauHinh.PayOsChecksumKey) ? DBNull.Value : cauHinh.PayOsChecksumKey.Trim());
    }

    private static CauHinhHeThong MapCauHinh(SqlDataReader reader)
    {
        return new CauHinhHeThong
        {
            CauHinhHeThongId = reader.GetInt32(reader.GetOrdinal("CauHinhHeThongId")),
            TenQuan = reader.GetString(reader.GetOrdinal("TenQuan")),
            DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? null : reader.GetString(reader.GetOrdinal("DiaChi")),
            SoDienThoai = reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? null : reader.GetString(reader.GetOrdinal("SoDienThoai")),
            FooterHoaDon = reader.IsDBNull(reader.GetOrdinal("FooterHoaDon")) ? null : reader.GetString(reader.GetOrdinal("FooterHoaDon")),
            LogoPath = reader.IsDBNull(reader.GetOrdinal("LogoPath")) ? null : reader.GetString(reader.GetOrdinal("LogoPath")),
            PayOsClientId = reader.IsDBNull(reader.GetOrdinal("PayOsClientId")) ? null : reader.GetString(reader.GetOrdinal("PayOsClientId")),
            PayOsApiKey = reader.IsDBNull(reader.GetOrdinal("PayOsApiKey")) ? null : reader.GetString(reader.GetOrdinal("PayOsApiKey")),
            PayOsChecksumKey = reader.IsDBNull(reader.GetOrdinal("PayOsChecksumKey")) ? null : reader.GetString(reader.GetOrdinal("PayOsChecksumKey")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}

