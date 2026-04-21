using CoffeeShop.Wpf.Infrastructure;
using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public sealed class HoaDonBanRepository : IHoaDonBanRepository
{
    private readonly ILichSuTonKhoRepository _lichSuTonKhoRepository;
    private readonly ICongThucMonRepository _congThucMonRepository;
    private readonly INguyenLieuRepository _nguyenLieuRepository;
    private readonly ILichSuNguyenLieuRepository _lichSuNguyenLieuRepository;

    public HoaDonBanRepository(
        ILichSuTonKhoRepository lichSuTonKhoRepository,
        ICongThucMonRepository congThucMonRepository,
        INguyenLieuRepository nguyenLieuRepository,
        ILichSuNguyenLieuRepository lichSuNguyenLieuRepository)
    {
        _lichSuTonKhoRepository = lichSuTonKhoRepository;
        _congThucMonRepository = congThucMonRepository;
        _nguyenLieuRepository = nguyenLieuRepository;
        _lichSuNguyenLieuRepository = lichSuNguyenLieuRepository;
    }

    public async Task<int> CreateAsync(
        HoaDonBan hoaDonBan,
        IReadOnlyList<ChiTietHoaDonBan> chiTietHoaDonBans,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string insertHoaDonBanSql = @"
INSERT INTO dbo.HoaDonBan
(
    NgayBan,
    TongTien,
    GiamGia,
    CreatedByUserId,
    BanId,
    CaLamViecId,
    KhachHangId,
    KhuyenMaiId,
    SoTienGiam,
    DiemCong,
    DiemSuDung,
    SoTienGiamTuDiem,
    HinhThucThanhToan,
    TrangThaiThanhToan,
    TienKhachDua,
    TienThoiLai,
    MaGiaoDich,
    GhiChuThanhToan,
    GhiChuHoaDon,
    TrangThaiPhaChe,
    HinhThucPhucVu
)
VALUES
(
    @NgayBan,
    @TongTien,
    @GiamGia,
    @CreatedByUserId,
    @BanId,
    @CaLamViecId,
    @KhachHangId,
    @KhuyenMaiId,
    @SoTienGiam,
    @DiemCong,
    @DiemSuDung,
    @SoTienGiamTuDiem,
    @HinhThucThanhToan,
    @TrangThaiThanhToan,
    @TienKhachDua,
    @TienThoiLai,
    @MaGiaoDich,
    @GhiChuThanhToan,
    @GhiChuHoaDon,
    @TrangThaiPhaChe,
    @HinhThucPhucVu
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var insertHoaDonBanCommand = new SqlCommand(insertHoaDonBanSql, connection, (SqlTransaction)transaction);
            insertHoaDonBanCommand.Parameters.AddWithValue("@NgayBan", hoaDonBan.NgayBan);
            insertHoaDonBanCommand.Parameters.AddWithValue("@TongTien", hoaDonBan.TongTien);
            insertHoaDonBanCommand.Parameters.AddWithValue("@GiamGia", hoaDonBan.GiamGia);
            insertHoaDonBanCommand.Parameters.AddWithValue("@CreatedByUserId", hoaDonBan.CreatedByUserId);
            insertHoaDonBanCommand.Parameters.AddWithValue("@BanId", (object?)hoaDonBan.BanId ?? DBNull.Value);
            insertHoaDonBanCommand.Parameters.AddWithValue("@CaLamViecId", (object?)hoaDonBan.CaLamViecId ?? DBNull.Value);
            insertHoaDonBanCommand.Parameters.AddWithValue("@KhachHangId", (object?)hoaDonBan.KhachHangId ?? DBNull.Value);
            insertHoaDonBanCommand.Parameters.AddWithValue("@KhuyenMaiId", (object?)hoaDonBan.KhuyenMaiId ?? DBNull.Value);
            insertHoaDonBanCommand.Parameters.AddWithValue("@SoTienGiam", hoaDonBan.SoTienGiam);
            insertHoaDonBanCommand.Parameters.AddWithValue("@DiemCong", hoaDonBan.DiemCong);
            insertHoaDonBanCommand.Parameters.AddWithValue("@DiemSuDung", hoaDonBan.DiemSuDung);
            insertHoaDonBanCommand.Parameters.AddWithValue("@SoTienGiamTuDiem", hoaDonBan.SoTienGiamTuDiem);
            // Thanh toán nâng cao
            insertHoaDonBanCommand.Parameters.AddWithValue("@HinhThucThanhToan",
                string.IsNullOrWhiteSpace(hoaDonBan.HinhThucThanhToan) ? "Tiền mặt" : hoaDonBan.HinhThucThanhToan);
            insertHoaDonBanCommand.Parameters.AddWithValue("@TrangThaiThanhToan",
                string.IsNullOrWhiteSpace(hoaDonBan.TrangThaiThanhToan) ? "Đã thanh toán" : hoaDonBan.TrangThaiThanhToan);
            insertHoaDonBanCommand.Parameters.AddWithValue("@TienKhachDua", (object?)hoaDonBan.TienKhachDua ?? DBNull.Value);
            insertHoaDonBanCommand.Parameters.AddWithValue("@TienThoiLai", (object?)hoaDonBan.TienThoiLai ?? DBNull.Value);
            insertHoaDonBanCommand.Parameters.AddWithValue("@MaGiaoDich",
                string.IsNullOrWhiteSpace(hoaDonBan.MaGiaoDich) ? DBNull.Value : hoaDonBan.MaGiaoDich.Trim());
            insertHoaDonBanCommand.Parameters.AddWithValue("@GhiChuThanhToan",
                string.IsNullOrWhiteSpace(hoaDonBan.GhiChuThanhToan) ? DBNull.Value : hoaDonBan.GhiChuThanhToan.Trim());
            insertHoaDonBanCommand.Parameters.AddWithValue("@GhiChuHoaDon",
                string.IsNullOrWhiteSpace(hoaDonBan.GhiChuHoaDon) ? DBNull.Value : hoaDonBan.GhiChuHoaDon.Trim());
            insertHoaDonBanCommand.Parameters.AddWithValue("@TrangThaiPhaChe",
                string.IsNullOrWhiteSpace(hoaDonBan.TrangThaiPhaChe) ? TrangThaiPhaCheConst.ChoPhaChe : hoaDonBan.TrangThaiPhaChe);
            insertHoaDonBanCommand.Parameters.AddWithValue("@HinhThucPhucVu",
                string.IsNullOrWhiteSpace(hoaDonBan.HinhThucPhucVu) ? HinhThucPhucVuConst.UongTaiQuan : hoaDonBan.HinhThucPhucVu);

            var newHoaDonBanId = Convert.ToInt32(await insertHoaDonBanCommand.ExecuteScalarAsync(cancellationToken));

            // === Tính SoThuTuGoiMon reset theo ngày ===
            const string updateSoGoiMonSql = @"
DECLARE @ngay DATE = CAST(@NgayBanParam AS DATE);
DECLARE @soTiep INT;

SELECT @soTiep = ISNULL(MAX(SoThuTuGoiMon), 0) + 1
FROM dbo.HoaDonBan WITH (UPDLOCK, HOLDLOCK)
WHERE NgaySoThuTu = @ngay
  AND SoThuTuGoiMon IS NOT NULL;

UPDATE dbo.HoaDonBan
SET SoThuTuGoiMon = @soTiep,
    NgaySoThuTu = @ngay
WHERE HoaDonBanId = @HoaDonBanId;

SELECT @soTiep AS SoThuTuGoiMon, @ngay AS NgaySoThuTu;";

            await using var soGoiMonCommand = new SqlCommand(updateSoGoiMonSql, connection, (SqlTransaction)transaction);
            soGoiMonCommand.Parameters.AddWithValue("@NgayBanParam", hoaDonBan.NgayBan);
            soGoiMonCommand.Parameters.AddWithValue("@HoaDonBanId", newHoaDonBanId);

            await using var soGoiMonReader = await soGoiMonCommand.ExecuteReaderAsync(cancellationToken);
            if (await soGoiMonReader.ReadAsync(cancellationToken))
            {
                hoaDonBan.SoThuTuGoiMon = soGoiMonReader.GetInt32(0);
                hoaDonBan.NgaySoThuTu = soGoiMonReader.GetDateTime(1);
            }
            await soGoiMonReader.CloseAsync();

            const string updateTonKhoSql = @"
UPDATE dbo.Mon
SET TonKho = TonKho - @SoLuongBan
WHERE MonId = @MonId
  AND TonKho >= @SoLuongBan;";

            const string getTonKhoSql = @"
SELECT TonKho FROM dbo.Mon WHERE MonId = @MonId;";

            const string insertChiTietSql = @"
INSERT INTO dbo.ChiTietHoaDonBan (HoaDonBanId, MonId, DonGiaBan, SoLuong, KichCo, PhuThuKichCo, GhiChuMon)
VALUES (@HoaDonBanId, @MonId, @DonGiaBan, @SoLuong, @KichCo, @PhuThuKichCo, @GhiChuMon);";

            foreach (var chiTiet in chiTietHoaDonBans)
            {
                // === 1. Lấy công thức món ===
                var congThucMons = await _congThucMonRepository.GetByMonAsync(
                    connection,
                    (SqlTransaction)transaction,
                    chiTiet.MonId,
                    activeOnly: true,
                    cancellationToken);

                // === 2. Kiểm tra tồn kho nguyên liệu trước khi trừ ===
                var nguyenLieuThieuList = new List<string>();

                foreach (var congThuc in congThucMons)
                {
                    var soLuongCanDung = congThuc.DinhLuong * chiTiet.SoLuong;
                    var tonKhoHienTai = await _nguyenLieuRepository.GetTonKhoAsync(
                        connection,
                        (SqlTransaction)transaction,
                        congThuc.NguyenLieuId,
                        cancellationToken);

                    if (tonKhoHienTai < soLuongCanDung)
                    {
                        nguyenLieuThieuList.Add($"{congThuc.TenNguyenLieu} (cần {soLuongCanDung:N2} {congThuc.DonViTinh}, còn {tonKhoHienTai:N2} {congThuc.DonViTinh})");
                    }
                }

                if (nguyenLieuThieuList.Count > 0)
                {
                    throw new InvalidOperationException($"Không đủ nguyên liệu cho món '{chiTiet.TenMon}':\n- {string.Join("\n- ", nguyenLieuThieuList)}");
                }

                // === 3. Lấy tồn kho món trước khi trừ ===
                int tonTruoc;
                await using (var getTonKhoCommand = new SqlCommand(getTonKhoSql, connection, (SqlTransaction)transaction))
                {
                    getTonKhoCommand.Parameters.AddWithValue("@MonId", chiTiet.MonId);
                    var tonKhoObj = await getTonKhoCommand.ExecuteScalarAsync(cancellationToken);
                    tonTruoc = tonKhoObj != null ? Convert.ToInt32(tonKhoObj) : 0;
                }

                // === 4. Trừ tồn kho món ===
                await using var updateTonKhoCommand = new SqlCommand(updateTonKhoSql, connection, (SqlTransaction)transaction);
                updateTonKhoCommand.Parameters.AddWithValue("@MonId", chiTiet.MonId);
                updateTonKhoCommand.Parameters.AddWithValue("@SoLuongBan", chiTiet.SoLuong);

                var affectedRows = await updateTonKhoCommand.ExecuteNonQueryAsync(cancellationToken);
                if (affectedRows == 0)
                {
                    throw new InvalidOperationException($"Tồn kho không đủ cho sản phẩm mã {chiTiet.MonId}.");
                }

                // === 5. Tính tồn sau món ===
                int tonSau = tonTruoc - chiTiet.SoLuong;

                // === 6. Ghi lịch sử tồn kho món ===
                await _lichSuTonKhoRepository.ThemLichSuAsync(
                    connection,
                    (SqlTransaction)transaction,
                    monId: chiTiet.MonId,
                    loaiPhatSinh: "BanHang",
                    soLuongThayDoi: -chiTiet.SoLuong,
                    tonTruoc: tonTruoc,
                    tonSau: tonSau,
                    hoaDonBanId: newHoaDonBanId,
                    hoaDonNhapId: null,
                    ghiChu: $"Bán hàng - Hóa đơn #{newHoaDonBanId}",
                    nguoiDungId: hoaDonBan.CreatedByUserId,
                    cancellationToken: cancellationToken);

                // === 7. Trừ nguyên liệu theo công thức ===
                foreach (var congThuc in congThucMons)
                {
                    var soLuongCanDung = congThuc.DinhLuong * chiTiet.SoLuong;

                    // Lấy tồn trước nguyên liệu
                    var tonTruocNguyenLieu = await _nguyenLieuRepository.GetTonKhoAsync(
                        connection,
                        (SqlTransaction)transaction,
                        congThuc.NguyenLieuId,
                        cancellationToken);

                    // Trừ tồn kho nguyên liệu
                    var rowsAffected = await _nguyenLieuRepository.TruTonKhoAsync(
                        connection,
                        (SqlTransaction)transaction,
                        congThuc.NguyenLieuId,
                        soLuongCanDung,
                        cancellationToken);

                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException($"Không đủ nguyên liệu '{congThuc.TenNguyenLieu}' (cần {soLuongCanDung:N2} {congThuc.DonViTinh}).");
                    }

                    // Tính tồn sau nguyên liệu
                    var tonSauNguyenLieu = tonTruocNguyenLieu - soLuongCanDung;

                    // Ghi lịch sử nguyên liệu
                    await _lichSuNguyenLieuRepository.ThemLichSuAsync(
                        connection,
                        (SqlTransaction)transaction,
                        nguyenLieuId: congThuc.NguyenLieuId,
                        loaiPhatSinh: "XuatKho",
                        soLuongThayDoi: -soLuongCanDung,
                        tonTruoc: tonTruocNguyenLieu,
                        tonSau: tonSauNguyenLieu,
                        hoaDonBanId: newHoaDonBanId,
                        hoaDonNhapId: null,
                        ghiChu: $"Xuất kho cho món '{chiTiet.TenMon}' - HĐ #{newHoaDonBanId}",
                        nguoiDungId: hoaDonBan.CreatedByUserId,
                        cancellationToken: cancellationToken);
                }

                // === 8. Insert chi tiết hóa đơn ===
                await using var insertChiTietCommand = new SqlCommand(insertChiTietSql, connection, (SqlTransaction)transaction);
                insertChiTietCommand.Parameters.AddWithValue("@HoaDonBanId", newHoaDonBanId);
                insertChiTietCommand.Parameters.AddWithValue("@MonId", chiTiet.MonId);
                insertChiTietCommand.Parameters.AddWithValue("@DonGiaBan", chiTiet.DonGiaBan);
                insertChiTietCommand.Parameters.AddWithValue("@SoLuong", chiTiet.SoLuong);
                insertChiTietCommand.Parameters.AddWithValue("@KichCo",
                    string.IsNullOrWhiteSpace(chiTiet.KichCo) ? (object)DBNull.Value : chiTiet.KichCo);
                insertChiTietCommand.Parameters.AddWithValue("@PhuThuKichCo", chiTiet.PhuThuKichCo);
                insertChiTietCommand.Parameters.AddWithValue("@GhiChuMon",
                    string.IsNullOrWhiteSpace(chiTiet.GhiChuMon) ? (object)DBNull.Value : chiTiet.GhiChuMon);
                await insertChiTietCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            // Cập nhật điểm tích lũy khách hàng: trừ điểm dùng, cộng điểm mới
            if (hoaDonBan.KhachHangId.HasValue && (hoaDonBan.DiemSuDung > 0 || hoaDonBan.DiemCong > 0))
            {
                const string updateDiemSql = @"
UPDATE dbo.KhachHang
SET DiemTichLuy = DiemTichLuy - @DiemSuDung + @DiemCong
WHERE KhachHangId = @KhachHangId
  AND IsActive = 1
  AND DiemTichLuy >= @DiemSuDung;

SELECT @@ROWCOUNT AS RowsAffected;";

                await using var updateDiemCommand = new SqlCommand(updateDiemSql, connection, (SqlTransaction)transaction);
                updateDiemCommand.Parameters.AddWithValue("@KhachHangId", hoaDonBan.KhachHangId.Value);
                updateDiemCommand.Parameters.AddWithValue("@DiemSuDung", hoaDonBan.DiemSuDung);
                updateDiemCommand.Parameters.AddWithValue("@DiemCong", hoaDonBan.DiemCong);
                
                var rowsAffected = Convert.ToInt32(await updateDiemCommand.ExecuteScalarAsync(cancellationToken));
                if (rowsAffected == 0 && hoaDonBan.DiemSuDung > 0)
                {
                    throw new InvalidOperationException($"Khách hàng không đủ điểm tích lũy. Yêu cầu: {hoaDonBan.DiemSuDung} điểm.");
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return newHoaDonBanId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<HoaDonBan>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT HoaDonBanId,
       NgayBan,
       TongTien,
       GiamGia,
       CreatedByUserId,
       BanId,
       CaLamViecId,
       KhachHangId,
       KhuyenMaiId,
       SoTienGiam,
       DiemCong
FROM dbo.HoaDonBan
WHERE NgayBan >= @FromDate
  AND NgayBan < @ToDateExclusive
ORDER BY NgayBan DESC;";

        var result = new List<HoaDonBan>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new HoaDonBan
            {
                HoaDonBanId = reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
                NgayBan = reader.GetDateTime(reader.GetOrdinal("NgayBan")),
                TongTien = reader.GetDecimal(reader.GetOrdinal("TongTien")),
                GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
                CreatedByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId")),
                BanId = reader.IsDBNull(reader.GetOrdinal("BanId")) ? null : reader.GetInt32(reader.GetOrdinal("BanId")),
                CaLamViecId = reader.IsDBNull(reader.GetOrdinal("CaLamViecId")) ? null : reader.GetInt32(reader.GetOrdinal("CaLamViecId")),
                KhachHangId = reader.IsDBNull(reader.GetOrdinal("KhachHangId")) ? null : reader.GetInt32(reader.GetOrdinal("KhachHangId")),
                KhuyenMaiId = reader.IsDBNull(reader.GetOrdinal("KhuyenMaiId")) ? null : reader.GetInt32(reader.GetOrdinal("KhuyenMaiId")),
                SoTienGiam = reader.GetDecimal(reader.GetOrdinal("SoTienGiam")),
                DiemCong = reader.GetInt32(reader.GetOrdinal("DiemCong"))
            });
        }

        return result;
    }

    /// <summary>
    /// Lấy hóa đơn đã thanh toán trong khoảng thời gian (dùng cho thống kê).
    /// Loại trừ hóa đơn đã hủy, dữ liệu cũ NULL xem như đã thanh toán.
    /// </summary>
    public async Task<IReadOnlyList<HoaDonBan>> GetPaidByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT HoaDonBanId,
       NgayBan,
       TongTien,
       GiamGia,
       CreatedByUserId,
       BanId,
       CaLamViecId,
       KhachHangId,
       KhuyenMaiId,
       SoTienGiam,
       DiemCong
FROM dbo.HoaDonBan
WHERE NgayBan >= @FromDate
  AND NgayBan < @ToDateExclusive
  AND ISNULL(TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'
ORDER BY NgayBan DESC;";

        var result = new List<HoaDonBan>();

        await using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.Date);
        command.Parameters.AddWithValue("@ToDateExclusive", toDate.Date.AddDays(1));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new HoaDonBan
            {
                HoaDonBanId = reader.GetInt32(reader.GetOrdinal("HoaDonBanId")),
                NgayBan = reader.GetDateTime(reader.GetOrdinal("NgayBan")),
                TongTien = reader.GetDecimal(reader.GetOrdinal("TongTien")),
                GiamGia = reader.GetDecimal(reader.GetOrdinal("GiamGia")),
                CreatedByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId")),
                BanId = reader.IsDBNull(reader.GetOrdinal("BanId")) ? null : reader.GetInt32(reader.GetOrdinal("BanId")),
                CaLamViecId = reader.IsDBNull(reader.GetOrdinal("CaLamViecId")) ? null : reader.GetInt32(reader.GetOrdinal("CaLamViecId")),
                KhachHangId = reader.IsDBNull(reader.GetOrdinal("KhachHangId")) ? null : reader.GetInt32(reader.GetOrdinal("KhachHangId")),
                KhuyenMaiId = reader.IsDBNull(reader.GetOrdinal("KhuyenMaiId")) ? null : reader.GetInt32(reader.GetOrdinal("KhuyenMaiId")),
                SoTienGiam = reader.GetDecimal(reader.GetOrdinal("SoTienGiam")),
                DiemCong = reader.GetInt32(reader.GetOrdinal("DiemCong"))
            });
        }

        return result;
    }
}

