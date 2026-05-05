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
        // SQL query join 3 bảng: CongThucMon, Mon, NguyenLieu
        var sql = @"
            SELECT
                ct.CongThucMonId,
                ct.MonId,
                m.TenMon,
                ct.NguyenLieuId,
                nl.TenNguyenLieu,
                nl.DonViTinh,                              -- YÊU CẦU 1: Đơn vị tính
                ct.DinhLuong,
                ct.GhiChu,
                ct.IsActive,
                nl.DonGiaNhap AS GiaVonDonVi,              -- YÊU CẦU 4: Giá vốn đơn vị
                ISNULL(ct.CacBuocThucHien, '') AS CacBuocThucHien,  -- YÊU CẦU 5
                ISNULL(ct.TyLeHaoHut, 0) AS TyLeHaoHut     -- YÊU CẦU 6
            FROM dbo.CongThucMon ct
            INNER JOIN dbo.Mon m ON ct.MonId = m.MonId
            INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
            WHERE ct.MonId = @MonId";

        // Lọc chỉ lấy công thức đang hoạt động nếu cần
        if (activeOnly)
        {
            sql += " AND ct.IsActive = 1";
        }

        // Sắp xếp theo tên nguyên liệu để dễ đọc
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
                ct.IsActive,
                nl.DonGiaNhap AS GiaVonDonVi,
                ISNULL(ct.CacBuocThucHien, '') AS CacBuocThucHien,
                ISNULL(ct.TyLeHaoHut, 0) AS TyLeHaoHut
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
                ct.IsActive,
                nl.DonGiaNhap AS GiaVonDonVi,
                ISNULL(ct.CacBuocThucHien, '') AS CacBuocThucHien,
                ISNULL(ct.TyLeHaoHut, 0) AS TyLeHaoHut
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
                IsActive,
                CacBuocThucHien,
                TyLeHaoHut
            )
            VALUES
            (
                @MonId,
                @NguyenLieuId,
                @DinhLuong,
                @GhiChu,
                @IsActive,
                @CacBuocThucHien,
                @TyLeHaoHut
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
        cmd.Parameters.AddWithValue("@CacBuocThucHien", (object?)congThucMon.CacBuocThucHien ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TyLeHaoHut", congThucMon.TyLeHaoHut);

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
                DinhLuong = @DinhLuong,                    -- Cập nhật định lượng mới
                GhiChu = @GhiChu,                          -- Cập nhật ghi chú
                IsActive = @IsActive,                      -- Có thể kích hoạt lại
                CacBuocThucHien = @CacBuocThucHien,
                TyLeHaoHut = @TyLeHaoHut
            WHERE CongThucMonId = @CongThucMonId;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CongThucMonId", congThucMon.CongThucMonId);
        cmd.Parameters.AddWithValue("@DinhLuong", congThucMon.DinhLuong);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)congThucMon.GhiChu ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive", congThucMon.IsActive);
        cmd.Parameters.AddWithValue("@CacBuocThucHien", (object?)congThucMon.CacBuocThucHien ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TyLeHaoHut", congThucMon.TyLeHaoHut);

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
                ct.IsActive,
                nl.DonGiaNhap AS GiaVonDonVi,
                ISNULL(ct.CacBuocThucHien, '') AS CacBuocThucHien,
                ISNULL(ct.TyLeHaoHut, 0) AS TyLeHaoHut
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
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            // YÊU CẦU 4: Giá vốn đơn vị từ NguyenLieu.DonGiaNhap
            GiaVonDonVi = reader.GetDecimal(reader.GetOrdinal("GiaVonDonVi")),
            // YÊU CẦU 5: Các bước thực hiện (có thể null)
            CacBuocThucHien = reader.IsDBNull(reader.GetOrdinal("CacBuocThucHien"))
                ? null
                : reader.GetString(reader.GetOrdinal("CacBuocThucHien")),
            // YÊU CẦU 6: Tỷ lệ hao hụt (mặc định 0 nếu null)
            TyLeHaoHut = reader.GetDecimal(reader.GetOrdinal("TyLeHaoHut"))
        };
    }

    /// <summary>
    /// YÊU CẦU 7: Lưu lịch sử cập nhật công thức
    /// Tự động gọi mỗi khi có thao tác thêm/sửa/xóa nguyên liệu
    /// </summary>
    public async Task SaveLichSuCapNhatAsync(
        int monId,
        string nguoiCapNhat,
        string noiDungCapNhat,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dbo.LichSuCapNhatCongThuc
            (
                MonId,
                NgayCapNhat,
                NguoiCapNhat,
                NoiDungCapNhat
            )
            VALUES
            (
                @MonId,
                @NgayCapNhat,        -- Lấy thời gian hiện tại
                @NguoiCapNhat,       -- Tên người thực hiện (từ Environment.UserName)
                @NoiDungCapNhat      -- Mô tả ngắn gọn về thay đổi
            );";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@MonId", monId);
        cmd.Parameters.AddWithValue("@NgayCapNhat", DateTime.Now);
        cmd.Parameters.AddWithValue("@NguoiCapNhat", nguoiCapNhat);
        cmd.Parameters.AddWithValue("@NoiDungCapNhat", noiDungCapNhat);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// YÊU CẦU 7: Lấy lịch sử cập nhật công thức theo món
    /// Sắp xếp theo thời gian mới nhất trước (DESC)
    /// </summary>
    public async Task<IReadOnlyList<LichSuCapNhatCongThuc>> GetLichSuCapNhatAsync(
        int monId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                ls.LichSuId,
                ls.MonId,
                m.TenMon,
                ls.NgayCapNhat,
                ls.NguoiCapNhat,
                ls.NoiDungCapNhat
            FROM dbo.LichSuCapNhatCongThuc ls
            INNER JOIN dbo.Mon m ON ls.MonId = m.MonId
            WHERE ls.MonId = @MonId
            ORDER BY ls.NgayCapNhat DESC;  -- Mới nhất trước";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@MonId", monId);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<LichSuCapNhatCongThuc>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new LichSuCapNhatCongThuc
            {
                LichSuId = reader.GetInt32(reader.GetOrdinal("LichSuId")),
                MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
                TenMon = reader.IsDBNull(reader.GetOrdinal("TenMon"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("TenMon")),
                NgayCapNhat = reader.GetDateTime(reader.GetOrdinal("NgayCapNhat")),
                NguoiCapNhat = reader.IsDBNull(reader.GetOrdinal("NguoiCapNhat"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("NguoiCapNhat")),
                NoiDungCapNhat = reader.IsDBNull(reader.GetOrdinal("NoiDungCapNhat"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("NoiDungCapNhat"))
            });
        }

        return result;
    }
}
