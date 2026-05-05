using CoffeeShop.Wpf.Helpers;
using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class LichSuHoaDonRepository : ILichSuHoaDonRepository
{
    public Task<IReadOnlyList<LichSuHoaDonDong>> GetDanhSachHoaDonAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        // Gọi TimKiemHoaDonAsync với tất cả filter = null → lấy toàn bộ
        return TimKiemHoaDonAsync(fromDate, toDate, null, null, null, null,
            null, null, null, null, null, cancellationToken);
    }

    public async Task<IReadOnlyList<LichSuHoaDonDong>> TimKiemHoaDonAsync(
        DateTime fromDate,
        DateTime toDate,
        int? hoaDonBanId,
        int? createdByUserId,
        int? banId,
        int? caLamViecId,
        string? tenKhachHang = null,
        string? soDienThoai = null,
        string? hinhThucThanhToan = null,
        string? trangThaiThanhToan = null,
        string? trangThaiPhaChe = null,
        CancellationToken cancellationToken = default)
    {
        // SQL query mở rộng: JOIN bảng KhachHang lấy thông tin KH,
        // đọc thêm các cột thanh toán nâng cao
        const string sql = @"
SELECT hb.HoaDonBanId,
       hb.NgayBan,
       hb.TongTien,
       hb.GiamGia,
       hb.CreatedByUserId,
       nd.HoTen AS TenNhanVien,
       hb.BanId,
       b.TenBan,
       kv.TenKhuVuc,
       hb.CaLamViecId,
       hb.KhachHangId,
       kh.HoTen AS TenKhachHang,
       kh.SoDienThoai,
       ISNULL(hb.HinhThucThanhToan, N'Tiền mặt') AS HinhThucThanhToan,
       ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') AS TrangThaiThanhToan,
       ISNULL(hb.TrangThaiPhaChe, N'ChoPhaChe') AS TrangThaiPhaChe,
       ISNULL(hb.HinhThucPhucVu, N'UongTaiQuan') AS HinhThucPhucVu
FROM dbo.HoaDonBan hb
LEFT JOIN dbo.NguoiDung nd ON nd.NguoiDungId = hb.CreatedByUserId
LEFT JOIN dbo.Ban b ON b.BanId = hb.BanId
LEFT JOIN dbo.KhuVuc kv ON kv.KhuVucId = b.KhuVucId
LEFT JOIN dbo.KhachHang kh ON kh.KhachHangId = hb.KhachHangId
WHERE hb.NgayBan >= @FromDate
  AND hb.NgayBan < @ToDateExclusive
  AND (@HoaDonBanId IS NULL OR hb.HoaDonBanId = @HoaDonBanId)
  AND (@CreatedByUserId IS NULL OR hb.CreatedByUserId = @CreatedByUserId)
  AND (@BanId IS NULL OR hb.BanId = @BanId)
  AND (@CaLamViecId IS NULL OR hb.CaLamViecId = @CaLamViecId)
  AND (@TenKhachHang IS NULL OR kh.HoTen LIKE N'%' + @TenKhachHang + N'%')
  AND (@SoDienThoai IS NULL OR kh.SoDienThoai LIKE N'%' + @SoDienThoai + N'%')
  AND (@HinhThucThanhToan IS NULL OR hb.HinhThucThanhToan = @HinhThucThanhToan)
  AND (@TrangThaiThanhToan IS NULL OR ISNULL(hb.TrangThaiThanhToan, N'Đã thanh toán') = @TrangThaiThanhToan)
  AND (@TrangThaiPhaChe IS NULL OR ISNULL(hb.TrangThaiPhaChe, N'ChoPhaChe') = @TrangThaiPhaChe)
ORDER BY hb.NgayBan DESC;";

        var result = new List<LichSuHoaDonDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));
        command.Parameters.AddWithValue("@HoaDonBanId", (object?)hoaDonBanId ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedByUserId", (object?)createdByUserId ?? DBNull.Value);
        command.Parameters.AddWithValue("@BanId", (object?)banId ?? DBNull.Value);
        command.Parameters.AddWithValue("@CaLamViecId", (object?)caLamViecId ?? DBNull.Value);
        command.Parameters.AddWithValue("@TenKhachHang",
            string.IsNullOrWhiteSpace(tenKhachHang) ? DBNull.Value : tenKhachHang.Trim());
        command.Parameters.AddWithValue("@SoDienThoai",
            string.IsNullOrWhiteSpace(soDienThoai) ? DBNull.Value : soDienThoai.Trim());
        command.Parameters.AddWithValue("@HinhThucThanhToan",
            string.IsNullOrWhiteSpace(hinhThucThanhToan) ? DBNull.Value : hinhThucThanhToan.Trim());
        command.Parameters.AddWithValue("@TrangThaiThanhToan",
            string.IsNullOrWhiteSpace(trangThaiThanhToan) ? DBNull.Value : trangThaiThanhToan.Trim());
        command.Parameters.AddWithValue("@TrangThaiPhaChe",
            string.IsNullOrWhiteSpace(trangThaiPhaChe) ? DBNull.Value : trangThaiPhaChe.Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new LichSuHoaDonDong
            {
                HoaDonBanId = reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
                NgayBan = reader.GetDateTime(reader.GetOrdinal("NgayBan")),
                TongTien = reader.GetDecimal(reader.GetOrdinal("TongTien")),
                GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
                CreatedByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId")),
                TenNhanVien = reader.IsDBNull(reader.GetOrdinal("TenNhanVien"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("TenNhanVien")),
                BanId = reader.IsDBNull(reader.GetOrdinal("BanId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("BanId")),
                TenBan = reader.IsDBNull(reader.GetOrdinal("TenBan"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("TenBan")),
                TenKhuVuc = reader.IsDBNull(reader.GetOrdinal("TenKhuVuc"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("TenKhuVuc")),
                CaLamViecId = reader.IsDBNull(reader.GetOrdinal("CaLamViecId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("CaLamViecId")),
                // Thông tin khách hàng
                TenKhachHang = reader.IsDBNull(reader.GetOrdinal("TenKhachHang"))
                    ? "Khách lẻ"
                    : reader.GetString(reader.GetOrdinal("TenKhachHang")),
                SoDienThoai = reader.IsDBNull(reader.GetOrdinal("SoDienThoai"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                // Thông tin thanh toán (chuẩn hóa encoding nếu dữ liệu cũ bị lỗi)
                HinhThucThanhToan = TextNormalizationHelper.NormalizeOrEmpty(
                    reader.GetString(reader.GetOrdinal("HinhThucThanhToan"))),
                TrangThaiThanhToan = TextNormalizationHelper.NormalizeOrEmpty(
                    reader.GetString(reader.GetOrdinal("TrangThaiThanhToan"))),
                // Trạng thái pha chế
                TrangThaiPhaChe = reader.GetString(reader.GetOrdinal("TrangThaiPhaChe")),
                // Hình thức phục vụ
                HinhThucPhucVu = reader.GetString(reader.GetOrdinal("HinhThucPhucVu"))
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<LichSuHoaDonChiTietDong>> GetChiTietHoaDonAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT ct.MonId,
       m.TenMon,
       ct.SoLuong,
       ct.DonGiaBan,
       ct.ThanhTien,
       ct.KichCo,
       ISNULL(ct.PhuThuKichCo, 0) AS PhuThuKichCo,
       ct.GhiChuMon
FROM dbo.ChiTietHoaDonBan ct
JOIN dbo.Mon m ON m.MonId = ct.MonId
WHERE ct.HoaDonBanId = @HoaDonBanId
ORDER BY ct.ChiTietHoaDonBanId;";

        var result = new List<LichSuHoaDonChiTietDong>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new LichSuHoaDonChiTietDong
            {
                MonId = reader.GetInt32(reader.GetOrdinal("MonId")),
                TenMon = reader.GetString(reader.GetOrdinal("TenMon")),
                SoLuong = reader.GetInt32(reader.GetOrdinal("SoLuong")),
                DonGiaBan = reader.GetDecimal(reader.GetOrdinal("DonGiaBan")),
                ThanhTien = reader.GetDecimal(reader.GetOrdinal("ThanhTien")),
                KichCo = reader.IsDBNull(reader.GetOrdinal("KichCo"))
                    ? "Mặc định" : reader.GetString(reader.GetOrdinal("KichCo")),
                PhuThuKichCo = reader.GetDecimal(reader.GetOrdinal("PhuThuKichCo")),
                GhiChuMon = reader.IsDBNull(reader.GetOrdinal("GhiChuMon"))
                    ? null : reader.GetString(reader.GetOrdinal("GhiChuMon"))
            });
        }

        return result;
    }

    /// <summary>
    /// Hủy hóa đơn hoàn chỉnh trong transaction:
    /// 1. Kiểm tra hóa đơn tồn tại và chưa hủy
    /// 2. Nếu hóa đơn đã từng trừ kho (đã thanh toán) thì hoàn kho món + nguyên liệu và ghi lịch sử
    /// 3. Đổi trạng thái = 'Đã hủy', ghi lý do, người hủy, ngày hủy
    /// 4. Hoàn điểm khách hàng nếu trước đó đã cộng điểm
    /// </summary>
    public async Task<bool> HuyHoaDonAsync(
        int hoaDonBanId,
        string lyDoHuy,
        string? nguoiHuy,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Bước 1: Kiểm tra hóa đơn tồn tại và chưa hủy, lấy thông tin cần thiết
            const string checkSql = @"
SELECT HoaDonBanId, KhachHangId, DiemCong, CreatedByUserId,
       ISNULL(TrangThaiThanhToan, N'Đã thanh toán') AS TrangThaiThanhToan
FROM dbo.HoaDonBan WITH (UPDLOCK, HOLDLOCK)
WHERE HoaDonBanId = @HoaDonBanId;";

            int? khachHangId = null;
            int diemCong = 0;
            int? createdByUserId = null;
            string trangThai;

            await using (var checkCmd = new SqlCommand(checkSql, connection, (SqlTransaction)transaction))
            {
                checkCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                await using var reader = await checkCmd.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    // Hóa đơn không tồn tại
                    await transaction.RollbackAsync(cancellationToken);
                    return false;
                }

                trangThai = reader.GetString(reader.GetOrdinal("TrangThaiThanhToan"));
                khachHangId = reader.IsDBNull(reader.GetOrdinal("KhachHangId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("KhachHangId"));
                diemCong = reader.GetInt32(reader.GetOrdinal("DiemCong"));
                createdByUserId = reader.IsDBNull(reader.GetOrdinal("CreatedByUserId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("CreatedByUserId"));
            }

            // Đã hủy rồi thì không cho hủy lại
            if (string.Equals(trangThai, "Đã hủy", StringComparison.OrdinalIgnoreCase))
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            var daTruKho = !string.Equals(trangThai, "Chờ thanh toán", StringComparison.OrdinalIgnoreCase)
                           && !string.Equals(trangThai, "Cho thanh toan", StringComparison.OrdinalIgnoreCase);

            // Bước 2: Hoàn kho (chỉ khi đã từng trừ kho)
            if (daTruKho)
            {
                const string chiTietSql = @"
SELECT ct.MonId, m.TenMon, ct.SoLuong
FROM dbo.ChiTietHoaDonBan ct
INNER JOIN dbo.Mon m ON m.MonId = ct.MonId
WHERE ct.HoaDonBanId = @HoaDonBanId
ORDER BY ct.ChiTietHoaDonBanId;";

                var chiTiets = new List<(int MonId, string TenMon, int SoLuong)>();
                await using (var chiTietCmd = new SqlCommand(chiTietSql, connection, (SqlTransaction)transaction))
                {
                    chiTietCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                    await using var reader = await chiTietCmd.ExecuteReaderAsync(cancellationToken);
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        chiTiets.Add((
                            reader.GetInt32(reader.GetOrdinal("MonId")),
                            reader.GetString(reader.GetOrdinal("TenMon")),
                            reader.GetInt32(reader.GetOrdinal("SoLuong"))));
                    }
                }

                foreach (var chiTiet in chiTiets)
                {
                    const string getTonMonSql = @"SELECT TonKho FROM dbo.Mon WHERE MonId = @MonId;";
                    int tonTruocMon;
                    await using (var getTonMonCmd = new SqlCommand(getTonMonSql, connection, (SqlTransaction)transaction))
                    {
                        getTonMonCmd.Parameters.AddWithValue("@MonId", chiTiet.MonId);
                        var tonObj = await getTonMonCmd.ExecuteScalarAsync(cancellationToken);
                        tonTruocMon = tonObj is null ? 0 : Convert.ToInt32(tonObj);
                    }

                    var tonSauMon = tonTruocMon + chiTiet.SoLuong;

                    const string updateMonSql = @"
UPDATE dbo.Mon
SET TonKho = TonKho + @SoLuongHoan
WHERE MonId = @MonId;";
                    await using (var updateMonCmd = new SqlCommand(updateMonSql, connection, (SqlTransaction)transaction))
                    {
                        updateMonCmd.Parameters.AddWithValue("@MonId", chiTiet.MonId);
                        updateMonCmd.Parameters.AddWithValue("@SoLuongHoan", chiTiet.SoLuong);
                        await updateMonCmd.ExecuteNonQueryAsync(cancellationToken);
                    }

                    const string insertLichSuTonKhoSql = @"
INSERT INTO dbo.LichSuTonKho
(
    MonId, LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, HoaDonBanId, HoaDonNhapId, GhiChu, NguoiDungId, ThoiGian
)
VALUES
(
    @MonId, N'HoanKhoHuyHoaDon', @SoLuongThayDoi, @TonTruoc, @TonSau, @HoaDonBanId, NULL, @GhiChu, @NguoiDungId, SYSDATETIME()
);";
                    await using (var lichSuMonCmd = new SqlCommand(insertLichSuTonKhoSql, connection, (SqlTransaction)transaction))
                    {
                        lichSuMonCmd.Parameters.AddWithValue("@MonId", chiTiet.MonId);
                        lichSuMonCmd.Parameters.AddWithValue("@SoLuongThayDoi", chiTiet.SoLuong);
                        lichSuMonCmd.Parameters.AddWithValue("@TonTruoc", tonTruocMon);
                        lichSuMonCmd.Parameters.AddWithValue("@TonSau", tonSauMon);
                        lichSuMonCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                        lichSuMonCmd.Parameters.AddWithValue("@GhiChu", $"Hoàn kho do hủy hóa đơn #{hoaDonBanId}. Lý do: {lyDoHuy.Trim()}");
                        lichSuMonCmd.Parameters.AddWithValue("@NguoiDungId", (object?)createdByUserId ?? DBNull.Value);
                        await lichSuMonCmd.ExecuteNonQueryAsync(cancellationToken);
                    }

                    const string congThucSql = @"
SELECT ct.NguyenLieuId,
       nl.TenNguyenLieu,
       nl.DonViTinh,
       ct.DinhLuong
FROM dbo.CongThucMon ct
INNER JOIN dbo.NguyenLieu nl ON nl.NguyenLieuId = ct.NguyenLieuId
WHERE ct.MonId = @MonId
  AND ct.IsActive = 1;";

                    var congThucs = new List<(int NguyenLieuId, string TenNguyenLieu, string DonViTinh, decimal DinhLuong)>();
                    await using (var congThucCmd = new SqlCommand(congThucSql, connection, (SqlTransaction)transaction))
                    {
                        congThucCmd.Parameters.AddWithValue("@MonId", chiTiet.MonId);
                        await using var reader = await congThucCmd.ExecuteReaderAsync(cancellationToken);
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            congThucs.Add((
                                reader.GetInt32(reader.GetOrdinal("NguyenLieuId")),
                                reader.GetString(reader.GetOrdinal("TenNguyenLieu")),
                                reader.GetString(reader.GetOrdinal("DonViTinh")),
                                reader.GetDecimal(reader.GetOrdinal("DinhLuong"))));
                        }
                    }

                    foreach (var congThuc in congThucs)
                    {
                        var soLuongHoan = congThuc.DinhLuong * chiTiet.SoLuong;

                        const string getTonNlSql = @"SELECT TonKho FROM dbo.NguyenLieu WHERE NguyenLieuId = @NguyenLieuId;";
                        decimal tonTruocNl;
                        await using (var getTonNlCmd = new SqlCommand(getTonNlSql, connection, (SqlTransaction)transaction))
                        {
                            getTonNlCmd.Parameters.AddWithValue("@NguyenLieuId", congThuc.NguyenLieuId);
                            var tonObj = await getTonNlCmd.ExecuteScalarAsync(cancellationToken);
                            tonTruocNl = tonObj is null ? 0 : Convert.ToDecimal(tonObj);
                        }

                        var tonSauNl = tonTruocNl + soLuongHoan;

                        const string updateNlSql = @"
UPDATE dbo.NguyenLieu
SET TonKho = TonKho + @SoLuongHoan,
    UpdatedAt = SYSDATETIME()
WHERE NguyenLieuId = @NguyenLieuId;";
                        await using (var updateNlCmd = new SqlCommand(updateNlSql, connection, (SqlTransaction)transaction))
                        {
                            updateNlCmd.Parameters.AddWithValue("@NguyenLieuId", congThuc.NguyenLieuId);
                            updateNlCmd.Parameters.AddWithValue("@SoLuongHoan", soLuongHoan);
                            await updateNlCmd.ExecuteNonQueryAsync(cancellationToken);
                        }

                        const string insertLichSuNlSql = @"
INSERT INTO dbo.LichSuNguyenLieu
(
    NguyenLieuId, LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, HoaDonBanId, HoaDonNhapId, GhiChu, NguoiDungId
)
VALUES
(
    @NguyenLieuId, N'HoanKhoHuyHoaDon', @SoLuongThayDoi, @TonTruoc, @TonSau, @HoaDonBanId, NULL, @GhiChu, @NguoiDungId
);";
                        await using (var lichSuNlCmd = new SqlCommand(insertLichSuNlSql, connection, (SqlTransaction)transaction))
                        {
                            lichSuNlCmd.Parameters.AddWithValue("@NguyenLieuId", congThuc.NguyenLieuId);
                            lichSuNlCmd.Parameters.AddWithValue("@SoLuongThayDoi", soLuongHoan);
                            lichSuNlCmd.Parameters.AddWithValue("@TonTruoc", tonTruocNl);
                            lichSuNlCmd.Parameters.AddWithValue("@TonSau", tonSauNl);
                            lichSuNlCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                            lichSuNlCmd.Parameters.AddWithValue("@GhiChu", $"Hoàn kho nguyên liệu do hủy hóa đơn #{hoaDonBanId} cho món '{chiTiet.TenMon}'. Lý do: {lyDoHuy.Trim()}");
                            lichSuNlCmd.Parameters.AddWithValue("@NguoiDungId", (object?)createdByUserId ?? DBNull.Value);
                            await lichSuNlCmd.ExecuteNonQueryAsync(cancellationToken);
                        }
                    }
                }
            }

            // Bước 3: Đổi trạng thái hóa đơn
            const string updateStatusSql = @"
UPDATE dbo.HoaDonBan
SET TrangThaiThanhToan = N'Đã hủy',
    TrangThaiPhaChe = N'DaHuy',
    LyDoHuy = @LyDoHuy,
    NguoiHuy = @NguoiHuy,
    NgayHuy = SYSDATETIME()
WHERE HoaDonBanId = @HoaDonBanId
  AND ISNULL(TrangThaiThanhToan, N'Đã thanh toán') <> N'Đã hủy';";

            await using (var updateCmd = new SqlCommand(updateStatusSql, connection, (SqlTransaction)transaction))
            {
                updateCmd.Parameters.AddWithValue("@HoaDonBanId", hoaDonBanId);
                updateCmd.Parameters.AddWithValue("@LyDoHuy", string.IsNullOrWhiteSpace(lyDoHuy) ? DBNull.Value : lyDoHuy.Trim());
                updateCmd.Parameters.AddWithValue("@NguoiHuy", string.IsNullOrWhiteSpace(nguoiHuy) ? DBNull.Value : nguoiHuy.Trim());

                var affected = await updateCmd.ExecuteNonQueryAsync(cancellationToken);
                if (affected == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return false;
                }
            }

            // Bước 4: Hoàn điểm khách hàng nếu trước đó đã cộng (chỉ khi từng trừ kho)
            if (daTruKho && khachHangId.HasValue && diemCong > 0)
            {
                const string deductPointsSql = @"
UPDATE dbo.KhachHang
SET DiemTichLuy = CASE
    WHEN DiemTichLuy >= @DiemCong THEN DiemTichLuy - @DiemCong
    ELSE 0
END
WHERE KhachHangId = @KhachHangId;";

                await using var deductCmd = new SqlCommand(deductPointsSql, connection, (SqlTransaction)transaction);
                deductCmd.Parameters.AddWithValue("@KhachHangId", khachHangId.Value);
                deductCmd.Parameters.AddWithValue("@DiemCong", diemCong);
                await deductCmd.ExecuteNonQueryAsync(cancellationToken);
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
}

