using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class HoaDonNhapRepository : IHoaDonNhapRepository
{
    private readonly ILichSuTonKhoRepository _lichSuTonKhoRepository;

    public HoaDonNhapRepository(ILichSuTonKhoRepository lichSuTonKhoRepository)
    {
        _lichSuTonKhoRepository = lichSuTonKhoRepository;
    }

    public async Task<int> CreateAsync(
        HoaDonNhap hoaDonNhap,
        IReadOnlyList<ChiTietHoaDonNhap> chiTietHoaDonNhaps,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string insertHoaDonNhapSql = @"
INSERT INTO dbo.HoaDonNhap (NgayNhap, NhaCungCapId, TongTien, GhiChu, CreatedByUserId)
VALUES (@NgayNhap, @NhaCungCapId, @TongTien, @GhiChu, @CreatedByUserId);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var insertHoaDonNhapCommand = new SqlCommand(insertHoaDonNhapSql, connection, (SqlTransaction)transaction);
            insertHoaDonNhapCommand.Parameters.AddWithValue("@NgayNhap", hoaDonNhap.NgayNhap);
            insertHoaDonNhapCommand.Parameters.AddWithValue("@NhaCungCapId", hoaDonNhap.NhaCungCapId);
            insertHoaDonNhapCommand.Parameters.AddWithValue("@TongTien", hoaDonNhap.TongTien);
            insertHoaDonNhapCommand.Parameters.AddWithValue("@GhiChu", (object?)hoaDonNhap.GhiChu ?? DBNull.Value);
            insertHoaDonNhapCommand.Parameters.AddWithValue("@CreatedByUserId", hoaDonNhap.CreatedByUserId);

            var newHoaDonNhapId = Convert.ToInt32(await insertHoaDonNhapCommand.ExecuteScalarAsync(cancellationToken));

            const string insertChiTietSql = @"
INSERT INTO dbo.ChiTietHoaDonNhap (HoaDonNhapId, MonId, DonGiaNhap, SoLuong)
VALUES (@HoaDonNhapId, @MonId, @DonGiaNhap, @SoLuong);";

            const string getTonKhoSql = @"
SELECT TonKho FROM dbo.Mon WHERE MonId = @MonId;";

            const string updateTonKhoSql = @"
UPDATE dbo.Mon
SET TonKho = TonKho + @SoLuongTang
WHERE MonId = @MonId;";

            foreach (var chiTiet in chiTietHoaDonNhaps)
            {
                await using var insertChiTietCommand = new SqlCommand(insertChiTietSql, connection, (SqlTransaction)transaction);
                insertChiTietCommand.Parameters.AddWithValue("@HoaDonNhapId", newHoaDonNhapId);
                insertChiTietCommand.Parameters.AddWithValue("@MonId", chiTiet.MonId);
                insertChiTietCommand.Parameters.AddWithValue("@DonGiaNhap", chiTiet.DonGiaNhap);
                insertChiTietCommand.Parameters.AddWithValue("@SoLuong", chiTiet.SoLuong);
                await insertChiTietCommand.ExecuteNonQueryAsync(cancellationToken);

                // Lấy tồn kho trước khi tăng
                int tonTruoc;
                await using (var getTonKhoCommand = new SqlCommand(getTonKhoSql, connection, (SqlTransaction)transaction))
                {
                    getTonKhoCommand.Parameters.AddWithValue("@MonId", chiTiet.MonId);
                    var tonKhoObj = await getTonKhoCommand.ExecuteScalarAsync(cancellationToken);
                    tonTruoc = tonKhoObj != null ? Convert.ToInt32(tonKhoObj) : 0;
                }

                // Tăng tồn kho
                await using var updateTonKhoCommand = new SqlCommand(updateTonKhoSql, connection, (SqlTransaction)transaction);
                updateTonKhoCommand.Parameters.AddWithValue("@MonId", chiTiet.MonId);
                updateTonKhoCommand.Parameters.AddWithValue("@SoLuongTang", chiTiet.SoLuong);
                await updateTonKhoCommand.ExecuteNonQueryAsync(cancellationToken);

                // Tính tồn sau
                int tonSau = tonTruoc + chiTiet.SoLuong;

                // Ghi lịch sử tồn kho
                await _lichSuTonKhoRepository.ThemLichSuAsync(
                    connection,
                    (SqlTransaction)transaction,
                    monId: chiTiet.MonId,
                    loaiPhatSinh: "NhapHang",
                    soLuongThayDoi: chiTiet.SoLuong,
                    tonTruoc: tonTruoc,
                    tonSau: tonSau,
                    hoaDonBanId: null,
                    hoaDonNhapId: newHoaDonNhapId,
                    ghiChu: $"Nhập hàng - Hóa đơn #{newHoaDonNhapId}",
                    nguoiDungId: hoaDonNhap.CreatedByUserId,
                    cancellationToken: cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return newHoaDonNhapId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<HoaDonNhap>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT HoaDonNhapId, NgayNhap, NhaCungCapId, TongTien, GhiChu, CreatedByUserId
FROM dbo.HoaDonNhap
WHERE NgayNhap >= @FromDate
  AND NgayNhap < @ToDateExclusive
ORDER BY NgayNhap DESC;";

        var result = new List<HoaDonNhap>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new HoaDonNhap
            {
                HoaDonNhapId = reader.GetInt32(reader.GetOrdinal("HoaDonNhapId")),
                NgayNhap = reader.GetDateTime(reader.GetOrdinal("NgayNhap")),
                NhaCungCapId = reader.GetInt32(reader.GetOrdinal("NhaCungCapId")),
                TongTien = reader.GetDecimal(reader.GetOrdinal("TongTien")),
                GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu")) ? null : reader.GetString(reader.GetOrdinal("GhiChu")),
                CreatedByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId"))
            });
        }

        return result;
    }
}
