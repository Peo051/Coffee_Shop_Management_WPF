using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class KhuyenMaiRepository : IKhuyenMaiRepository
{
    private const string SelectColumns = @"
KhuyenMaiId, TenKhuyenMai, LoaiKhuyenMai, GiaTri,
TuNgay, DenNgay, IsActive, MonId, MoTa,
ISNULL(GiaTriDonHangToiThieu, NULL) AS GiaTriDonHangToiThieu,
ISNULL(SoTienGiamToiDa, NULL) AS SoTienGiamToiDa,
CreatedAt";

    public async Task<IReadOnlyList<KhuyenMai>> GetDanhSachKhuyenMaiAsync(
        string? keyword,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        string sql = $@"
SELECT {SelectColumns}
FROM dbo.KhuyenMai
WHERE (@Keyword = N'' OR TenKhuyenMai LIKE N'%' + @Keyword + N'%')
  AND (@IsActive IS NULL OR IsActive = @IsActive)
ORDER BY CreatedAt DESC, KhuyenMaiId DESC;";

        var result = new List<KhuyenMai>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Keyword", (keyword ?? string.Empty).Trim());
        command.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapKhuyenMai(reader));
        }

        return result;
    }

    public async Task<int> TaoKhuyenMaiAsync(KhuyenMai khuyenMai, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.KhuyenMai
(
    TenKhuyenMai, LoaiKhuyenMai, GiaTri,
    TuNgay, DenNgay, IsActive, MonId, MoTa,
    GiaTriDonHangToiThieu, SoTienGiamToiDa
)
VALUES
(
    @TenKhuyenMai, @LoaiKhuyenMai, @GiaTri,
    @TuNgay, @DenNgay, @IsActive, @MonId, @MoTa,
    @GiaTriDonHangToiThieu, @SoTienGiamToiDa
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        AddKhuyenMaiParams(command, khuyenMai);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(scalar);
    }

    public async Task<bool> CapNhatKhuyenMaiAsync(KhuyenMai khuyenMai, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.KhuyenMai
SET TenKhuyenMai = @TenKhuyenMai,
    LoaiKhuyenMai = @LoaiKhuyenMai,
    GiaTri = @GiaTri,
    TuNgay = @TuNgay,
    DenNgay = @DenNgay,
    IsActive = @IsActive,
    MonId = @MonId,
    MoTa = @MoTa,
    GiaTriDonHangToiThieu = @GiaTriDonHangToiThieu,
    SoTienGiamToiDa = @SoTienGiamToiDa
WHERE KhuyenMaiId = @KhuyenMaiId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@KhuyenMaiId", khuyenMai.KhuyenMaiId);
        AddKhuyenMaiParams(command, khuyenMai);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IReadOnlyList<KhuyenMai>> GetKhuyenMaiHieuLucAsync(
        DateTime thoiDiem,
        CancellationToken cancellationToken = default)
    {
        string sql = $@"
SELECT {SelectColumns}
FROM dbo.KhuyenMai
WHERE IsActive = 1
  AND @ThoiDiem >= TuNgay
  AND @ThoiDiem <= DenNgay
ORDER BY CreatedAt DESC, KhuyenMaiId DESC;";

        var result = new List<KhuyenMai>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ThoiDiem", thoiDiem);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapKhuyenMai(reader));
        }

        return result;
    }

    public async Task<KhuyenMai?> GetByIdAsync(int khuyenMaiId, CancellationToken cancellationToken = default)
    {
        string sql = $@"
SELECT {SelectColumns}
FROM dbo.KhuyenMai
WHERE KhuyenMaiId = @KhuyenMaiId;";

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@KhuyenMaiId", khuyenMaiId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapKhuyenMai(reader);
    }

    private static void AddKhuyenMaiParams(SqlCommand command, KhuyenMai km)
    {
        command.Parameters.AddWithValue("@TenKhuyenMai", km.TenKhuyenMai.Trim());
        command.Parameters.AddWithValue("@LoaiKhuyenMai", km.LoaiKhuyenMai.Trim());
        command.Parameters.AddWithValue("@GiaTri", km.GiaTri);
        command.Parameters.AddWithValue("@TuNgay", km.TuNgay);
        command.Parameters.AddWithValue("@DenNgay", km.DenNgay);
        command.Parameters.AddWithValue("@IsActive", km.IsActive);
        command.Parameters.AddWithValue("@MonId", (object?)km.MonId ?? DBNull.Value);
        command.Parameters.AddWithValue("@MoTa", string.IsNullOrWhiteSpace(km.MoTa) ? DBNull.Value : km.MoTa.Trim());
        command.Parameters.AddWithValue("@GiaTriDonHangToiThieu", (object?)km.GiaTriDonHangToiThieu ?? DBNull.Value);
        command.Parameters.AddWithValue("@SoTienGiamToiDa", (object?)km.SoTienGiamToiDa ?? DBNull.Value);
    }

    private static KhuyenMai MapKhuyenMai(SqlDataReader reader)
    {
        return new KhuyenMai
        {
            KhuyenMaiId = reader.GetInt32(reader.GetOrdinal("KhuyenMaiId")),
            TenKhuyenMai = reader.GetString(reader.GetOrdinal("TenKhuyenMai")),
            LoaiKhuyenMai = reader.GetString(reader.GetOrdinal("LoaiKhuyenMai")),
            GiaTri = reader.GetDecimal(reader.GetOrdinal("GiaTri")),
            TuNgay = reader.GetDateTime(reader.GetOrdinal("TuNgay")),
            DenNgay = reader.GetDateTime(reader.GetOrdinal("DenNgay")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            MonId = reader.IsDBNull(reader.GetOrdinal("MonId")) ? null : reader.GetInt32(reader.GetOrdinal("MonId")),
            MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
            GiaTriDonHangToiThieu = reader.IsDBNull(reader.GetOrdinal("GiaTriDonHangToiThieu"))
                ? null : reader.GetDecimal(reader.GetOrdinal("GiaTriDonHangToiThieu")),
            SoTienGiamToiDa = reader.IsDBNull(reader.GetOrdinal("SoTienGiamToiDa"))
                ? null : reader.GetDecimal(reader.GetOrdinal("SoTienGiamToiDa")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}
