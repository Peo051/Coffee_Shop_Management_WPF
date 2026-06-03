
SELECT
    hdb.HoaDonBanId      AS MaHoaDon,
    hdb.NgayBan          AS NgayLap,
    nd.HoTen             AS TenNhanVien,
    ISNULL(kh.HoTen, b.TenBan) AS TenKhachHangHoacBan,
    m.TenMon             AS TenMon,
    dm.TenDanhMuc        AS TenDanhMuc,
    ct.SoLuong           AS SoLuong,
    ct.DonGiaBan         AS DonGia,
    ct.ThanhTien         AS ThanhTien,
    hdb.HinhThucPhucVu   AS HinhThucPhucVu,
    hdb.HinhThucThanhToan AS HinhThucThanhToan
FROM dbo.HoaDonBan hdb
INNER JOIN dbo.ChiTietHoaDonBan ct ON hdb.HoaDonBanId = ct.HoaDonBanId
INNER JOIN dbo.Mon m               ON ct.MonId = m.MonId
LEFT  JOIN dbo.DanhMuc dm          ON m.DanhMucId = dm.DanhMucId
LEFT  JOIN dbo.NguoiDung nd        ON hdb.CreatedByUserId = nd.NguoiDungId
LEFT  JOIN dbo.KhachHang kh        ON hdb.KhachHangId = kh.KhachHangId
LEFT  JOIN dbo.Ban b               ON hdb.BanId = b.BanId
WHERE hdb.NgayBan >= {?TuNgay}
  AND hdb.NgayBan <  DATEADD(DAY, 1, {?DenNgay})
  AND ISNULL(hdb.TrangThaiThanhToan, N'Đã thanh toán') <> N'Đã hủy'
ORDER BY hdb.NgayBan, hdb.HoaDonBanId;
