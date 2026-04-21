using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class KhachHangRepository : IKhachHangRepository
{
    public async Task<IReadOnlyList<KhachHang>> GetDanhSachKhachHangAsync(
        string? keyword,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT KhachHangId,
       HoTen,
       SoDienThoai,
       Email,
       DiemTichLuy,
       IsActive,
       CreatedAt
FROM dbo.KhachHang
WHERE (
        @Keyword = N''
        OR HoTen LIKE N'%' + @Keyword + N'%'
        OR SoDienThoai LIKE N'%' + @Keyword + N'%'
        OR Email LIKE N'%' + @Keyword + N'%'
      )
  AND (@IsActive IS NULL OR IsActive = @IsActive)
ORDER BY HoTen ASC, KhachHangId DESC;";

        var result = new List<KhachHang>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Keyword", (keyword ?? string.Empty).Trim());
        command.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapKhachHang(reader));
        }

        return result;
    }

    public async Task<int> TaoKhachHangAsync(KhachHang khachHang, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.KhachHang (HoTen, SoDienThoai, Email, DiemTichLuy, IsActive)
VALUES (@HoTen, @SoDienThoai, @Email, @DiemTichLuy, @IsActive);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@HoTen", khachHang.HoTen.Trim());
        command.Parameters.AddWithValue("@SoDienThoai", string.IsNullOrWhiteSpace(khachHang.SoDienThoai) ? DBNull.Value : khachHang.SoDienThoai.Trim());
        command.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(khachHang.Email) ? DBNull.Value : khachHang.Email.Trim());
        command.Parameters.AddWithValue("@DiemTichLuy", khachHang.DiemTichLuy);
        command.Parameters.AddWithValue("@IsActive", khachHang.IsActive);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(scalar);
    }

    public async Task<bool> CapNhatKhachHangAsync(KhachHang khachHang, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.KhachHang
SET HoTen = @HoTen,
    SoDienThoai = @SoDienThoai,
    Email = @Email,
    IsActive = @IsActive
WHERE KhachHangId = @KhachHangId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@KhachHangId", khachHang.KhachHangId);
        command.Parameters.AddWithValue("@HoTen", khachHang.HoTen.Trim());
        command.Parameters.AddWithValue("@SoDienThoai", string.IsNullOrWhiteSpace(khachHang.SoDienThoai) ? DBNull.Value : khachHang.SoDienThoai.Trim());
        command.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(khachHang.Email) ? DBNull.Value : khachHang.Email.Trim());
        command.Parameters.AddWithValue("@IsActive", khachHang.IsActive);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public Task<IReadOnlyList<KhachHang>> TimKiemKhachHangAsync(
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        return GetDanhSachKhachHangAsync(keyword, true, cancellationToken);
    }

    public async Task<bool> CongDiemAsync(int khachHangId, int diemCong, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.KhachHang
SET DiemTichLuy = DiemTichLuy + @DiemCong
WHERE KhachHangId = @KhachHangId
  AND IsActive = 1;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@KhachHangId", khachHangId);
        command.Parameters.AddWithValue("@DiemCong", diemCong);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<KhachHang?> GetByIdAsync(int khachHangId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT KhachHangId,
       HoTen,
       SoDienThoai,
       Email,
       DiemTichLuy,
       IsActive,
       CreatedAt
FROM dbo.KhachHang
WHERE KhachHangId = @KhachHangId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@KhachHangId", khachHangId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapKhachHang(reader);
    }

    private static KhachHang MapKhachHang(SqlDataReader reader)
    {
        return new KhachHang
        {
            KhachHangId = reader.GetInt32(reader.GetOrdinal("KhachHangId")),
            HoTen = reader.GetString(reader.GetOrdinal("HoTen")),
            SoDienThoai = reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? null : reader.GetString(reader.GetOrdinal("SoDienThoai")),
            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
            DiemTichLuy = reader.GetInt32(reader.GetOrdinal("DiemTichLuy")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}

