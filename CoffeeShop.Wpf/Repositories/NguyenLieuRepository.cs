using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class NguyenLieuRepository : INguyenLieuRepository
{
    private readonly LichSuNguyenLieuRepository _lichSuNguyenLieuRepository = new();

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
        cmd.Parameters.AddWithValue("@TonKhoToiThieu", nguyenLieu.TonKhoToiThieu);
        cmd.Parameters.AddWithValue("@DonGiaNhap", nguyenLieu.DonGiaNhap);
        cmd.Parameters.AddWithValue("@IsActive", nguyenLieu.IsActive);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task CapNhatTonKhoAsync(
        int nguyenLieuId,
        decimal tonKhoMoi,
        CancellationToken cancellationToken = default)
    {
        _ = nguyenLieuId;
        _ = tonKhoMoi;
        _ = cancellationToken;
        throw new InvalidOperationException(
            "Không cho phép cập nhật tồn kho trực tiếp. Vui lòng dùng nghiệp vụ nhập thêm hoặc điều chỉnh tồn kho có lý do.");
    }

    public async Task<NguyenLieu?> NhapThemVaGhiLichSuAsync(
        int nguyenLieuId,
        decimal soLuongNhap,
        decimal? donGiaNhapMoi,
        string? ghiChu,
        int? nguoiDungId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var snapshot = await GetNguyenLieuSnapshotAsync(connection, (SqlTransaction)transaction, nguyenLieuId, cancellationToken);
            if (snapshot is null)
            {
                throw new InvalidOperationException($"Không tìm thấy nguyên liệu với ID {nguyenLieuId}.");
            }

            var tonSau = snapshot.Value.TonKho + soLuongNhap;

            const string updateSql = @"
UPDATE dbo.NguyenLieu
SET TonKho = @TonSau,
    DonGiaNhap = ISNULL(@DonGiaNhapMoi, DonGiaNhap),
    UpdatedAt = SYSDATETIME()
WHERE NguyenLieuId = @NguyenLieuId;";

            await using (var updateCmd = new SqlCommand(updateSql, connection, (SqlTransaction)transaction))
            {
                updateCmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);
                updateCmd.Parameters.AddWithValue("@TonSau", tonSau);
                updateCmd.Parameters.AddWithValue("@DonGiaNhapMoi", (object?)donGiaNhapMoi ?? DBNull.Value);
                await updateCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            var noiDungGhiChu = string.IsNullOrWhiteSpace(ghiChu)
                ? $"Nhập nguyên liệu {snapshot.Value.TenNguyenLieu}: +{soLuongNhap:N2} {snapshot.Value.DonViTinh}."
                : $"Nhập nguyên liệu {snapshot.Value.TenNguyenLieu}: +{soLuongNhap:N2} {snapshot.Value.DonViTinh}. {ghiChu.Trim()}";

            await _lichSuNguyenLieuRepository.ThemLichSuAsync(
                connection,
                (SqlTransaction)transaction,
                nguyenLieuId,
                "NhapNguyenLieu",
                soLuongNhap,
                snapshot.Value.TonKho,
                tonSau,
                null,
                null,
                noiDungGhiChu,
                nguoiDungId,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return await GetByIdAsync(nguyenLieuId, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<NguyenLieu?> DieuChinhTonKhoVaGhiLichSuAsync(
        int nguyenLieuId,
        decimal tonKhoMoi,
        string lyDo,
        int? nguoiDungId,
        string loaiPhatSinh = "DieuChinh",
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var snapshot = await GetNguyenLieuSnapshotAsync(connection, (SqlTransaction)transaction, nguyenLieuId, cancellationToken);
            if (snapshot is null)
            {
                throw new InvalidOperationException($"Không tìm thấy nguyên liệu với ID {nguyenLieuId}.");
            }

            if (tonKhoMoi < 0)
            {
                throw new InvalidOperationException("Tồn kho sau điều chỉnh không được âm.");
            }

            const string updateSql = @"
UPDATE dbo.NguyenLieu
SET TonKho = @TonKhoMoi,
    UpdatedAt = SYSDATETIME()
WHERE NguyenLieuId = @NguyenLieuId;";

            await using (var updateCmd = new SqlCommand(updateSql, connection, (SqlTransaction)transaction))
            {
                updateCmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);
                updateCmd.Parameters.AddWithValue("@TonKhoMoi", tonKhoMoi);
                await updateCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            var soLuongThayDoi = tonKhoMoi - snapshot.Value.TonKho;

            await _lichSuNguyenLieuRepository.ThemLichSuAsync(
                connection,
                (SqlTransaction)transaction,
                nguyenLieuId,
                loaiPhatSinh,
                soLuongThayDoi,
                snapshot.Value.TonKho,
                tonKhoMoi,
                null,
                null,
                lyDo.Trim(),
                nguoiDungId,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return await GetByIdAsync(nguyenLieuId, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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

    private static async Task<(string TenNguyenLieu, string DonViTinh, decimal TonKho)?> GetNguyenLieuSnapshotAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int nguyenLieuId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TenNguyenLieu, DonViTinh, TonKho
FROM dbo.NguyenLieu WITH (UPDLOCK, ROWLOCK)
WHERE NguyenLieuId = @NguyenLieuId;";

        await using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@NguyenLieuId", nguyenLieuId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return (
            reader.GetString(reader.GetOrdinal("TenNguyenLieu")),
            reader.GetString(reader.GetOrdinal("DonViTinh")),
            reader.GetDecimal(reader.GetOrdinal("TonKho")));
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
