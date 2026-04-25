USE CoffeeShopDb;
GO

SELECT nd.TenDangNhap, nd.HoTen, vt.MaVaiTro
FROM dbo.NguoiDung nd
JOIN dbo.VaiTro vt ON nd.VaiTroId = vt.VaiTroId
WHERE nd.TenDangNhap = N'admin' AND nd.IsActive = 1;
GO

DECLARE @Keyword NVARCHAR(100) = N'cà phê';
SELECT m.MonId, m.TenMon, dm.TenDanhMuc, m.DonGia, m.TonKho
FROM dbo.Mon m
JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
WHERE (@Keyword IS NULL OR LTRIM(RTRIM(@Keyword)) = N'' OR m.TenMon LIKE N'%' + @Keyword + N'%');
GO

SELECT dm.TenDanhMuc, SUM(m.TonKho) AS TongTonKho
FROM dbo.Mon m
JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
GROUP BY dm.TenDanhMuc;
GO

SELECT CAST(hdb.NgayBan AS DATE) AS Ngay,
       COUNT(1) AS SoHoaDon,
       SUM(hdb.TongTien) AS DoanhThuGop,
       SUM(hdb.GiamGia) AS TongGiamGia,
       SUM(hdb.ThanhToan) AS DoanhThuThuan
FROM dbo.HoaDonBan hdb
GROUP BY CAST(hdb.NgayBan AS DATE)
ORDER BY Ngay DESC;
GO

-- Trang thai san pham + nguong canh bao ton kho
SELECT m.MonId,
       m.TenMon,
       dm.TenDanhMuc,
       m.IsActive,
       m.TonKho,
       m.MucCanhBaoTonKho
FROM dbo.Mon m
JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
ORDER BY m.IsActive DESC, m.TonKho ASC, m.MonId ASC;
GO

-- Danh sach canh bao ton kho thap
SELECT m.MonId,
       m.TenMon,
       dm.TenDanhMuc,
       m.TonKho,
       m.MucCanhBaoTonKho,
       (m.MucCanhBaoTonKho - m.TonKho) AS SoLuongCanBoSung
FROM dbo.Mon m
JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
WHERE m.IsActive = 1
  AND m.TonKho <= m.MucCanhBaoTonKho
ORDER BY (m.MucCanhBaoTonKho - m.TonKho) DESC, m.MonId ASC;
GO

-- Top san pham ban chay doi chieu UI module moi
DECLARE @FromDate DATE = DATEADD(DAY, -30, CAST(GETDATE() AS DATE));
DECLARE @ToDate DATE = CAST(GETDATE() AS DATE);
DECLARE @TopN INT = 10;

SELECT TOP (@TopN)
       m.MonId,
       m.TenMon,
       SUM(ct.SoLuong) AS TongSoLuongBan,
       COUNT(DISTINCT hb.HoaDonBanId) AS SoHoaDon,
       SUM(ct.ThanhTien) AS TongDoanhThu
FROM dbo.HoaDonBan hb
JOIN dbo.ChiTietHoaDonBan ct ON ct.HoaDonBanId = hb.HoaDonBanId
JOIN dbo.Mon m ON m.MonId = ct.MonId
WHERE hb.NgayBan >= @FromDate
  AND hb.NgayBan < DATEADD(DAY, 1, @ToDate)
GROUP BY m.MonId, m.TenMon
ORDER BY SUM(ct.SoLuong) DESC, SUM(ct.ThanhTien) DESC, m.MonId ASC;
GO

-- Wave 2: Danh sách khu vực và bàn
SELECT kv.KhuVucId,
       kv.TenKhuVuc,
       b.BanId,
       b.TenBan,
       b.TrangThaiBan,
       b.IsActive
FROM dbo.KhuVuc kv
LEFT JOIN dbo.Ban b ON b.KhuVucId = kv.KhuVucId
ORDER BY kv.KhuVucId, b.BanId;
GO

-- Wave 2: Ca đang mở theo người dùng
SELECT c.CaLamViecId,
       c.NguoiDungId,
       nd.HoTen,
       c.ThoiGianMoCa,
       c.ThoiGianDongCa,
       c.TrangThaiCa
FROM dbo.CaLamViec c
JOIN dbo.NguoiDung nd ON nd.NguoiDungId = c.NguoiDungId
WHERE c.TrangThaiCa = N'DangMo'
ORDER BY c.ThoiGianMoCa DESC;
GO

-- Wave 2: Lịch sử hóa đơn có gắn bàn/ca
SELECT hb.HoaDonBanId,
       hb.NgayBan,
       hb.TongTien,
       hb.GiamGia,
       hb.ThanhToan,
       hb.BanId,
       b.TenBan,
       kv.TenKhuVuc,
       hb.CaLamViecId,
       hb.CreatedByUserId,
       nd.HoTen AS NhanVien
FROM dbo.HoaDonBan hb
LEFT JOIN dbo.Ban b ON b.BanId = hb.BanId
LEFT JOIN dbo.KhuVuc kv ON kv.KhuVucId = b.KhuVucId
LEFT JOIN dbo.NguoiDung nd ON nd.NguoiDungId = hb.CreatedByUserId
ORDER BY hb.NgayBan DESC;
GO

-- Wave 3: Dashboard tong quan hom nay
DECLARE @HomNay DATE = CAST(GETDATE() AS DATE);
SELECT SUM(hb.ThanhToan) AS DoanhThuHomNay,
       COUNT(1) AS SoHoaDonHomNay
FROM dbo.HoaDonBan hb
WHERE hb.NgayBan >= @HomNay
  AND hb.NgayBan < DATEADD(DAY, 1, @HomNay);
GO

-- Wave 3: Top 5 san pham ban chay hom nay
DECLARE @TopNWave3 INT = 5;
DECLARE @TuNgayWave3 DATE = CAST(GETDATE() AS DATE);
SELECT TOP (@TopNWave3)
       m.MonId,
       m.TenMon,
       SUM(ct.SoLuong) AS TongSoLuongBan,
       SUM(ct.ThanhTien) AS TongDoanhThu
FROM dbo.HoaDonBan hb
JOIN dbo.ChiTietHoaDonBan ct ON ct.HoaDonBanId = hb.HoaDonBanId
JOIN dbo.Mon m ON m.MonId = ct.MonId
WHERE hb.NgayBan >= @TuNgayWave3
  AND hb.NgayBan < DATEADD(DAY, 1, @TuNgayWave3)
GROUP BY m.MonId, m.TenMon
ORDER BY SUM(ct.SoLuong) DESC, SUM(ct.ThanhTien) DESC;
GO

-- Wave 3: Ton kho thap (theo nguong canh bao)
SELECT m.MonId,
       m.TenMon,
       dm.TenDanhMuc,
       m.TonKho,
       m.MucCanhBaoTonKho,
       (m.MucCanhBaoTonKho - m.TonKho) AS SoLuongCanBoSung
FROM dbo.Mon m
JOIN dbo.DanhMuc dm ON dm.DanhMucId = m.DanhMucId
WHERE m.IsActive = 1
  AND m.TonKho <= m.MucCanhBaoTonKho
ORDER BY (m.MucCanhBaoTonKho - m.TonKho) DESC, m.MonId ASC;
GO

-- Wave 3: Dashboard 7 ngay gan nhat
SELECT CAST(hb.NgayBan AS DATE) AS Ngay,
       COUNT(1) AS SoHoaDon,
       SUM(hb.ThanhToan) AS DoanhThuThuan
FROM dbo.HoaDonBan hb
WHERE hb.NgayBan >= DATEADD(DAY, -6, CAST(GETDATE() AS DATE))
  AND hb.NgayBan < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
GROUP BY CAST(hb.NgayBan AS DATE)
ORDER BY Ngay ASC;
GO

-- Wave 3: Audit log moi nhat
SELECT TOP 50
       al.AuditLogId,
       al.ThoiGianTao,
       nd.TenDangNhap,
       al.HanhDong,
       al.DoiTuong,
       al.DuLieuTomTat,
       al.MayTram
FROM dbo.AuditLog al
LEFT JOIN dbo.NguoiDung nd ON nd.NguoiDungId = al.NguoiDungId
ORDER BY al.ThoiGianTao DESC, al.AuditLogId DESC;
GO

-- Wave 4: Khuyen mai dang hieu luc
SELECT km.KhuyenMaiId,
       km.TenKhuyenMai,
       km.LoaiKhuyenMai,
       km.GiaTri,
       km.TuNgay,
       km.DenNgay,
       km.IsActive
FROM dbo.KhuyenMai km
WHERE km.IsActive = 1
  AND SYSDATETIME() >= km.TuNgay
  AND SYSDATETIME() <= km.DenNgay
ORDER BY km.KhuyenMaiId DESC;
GO

-- Wave 4: Danh sach khach hang than thiet
SELECT kh.KhachHangId,
       kh.HoTen,
       kh.SoDienThoai,
       kh.Email,
       kh.DiemTichLuy,
       kh.IsActive
FROM dbo.KhachHang kh
ORDER BY kh.DiemTichLuy DESC, kh.KhachHangId DESC;
GO

-- Wave 4: Hoa don ban co lien ket khach hang + khuyen mai
SELECT TOP 50
       hb.HoaDonBanId,
       hb.NgayBan,
       hb.TongTien,
       hb.GiamGia,
       hb.SoTienGiam,
       hb.DiemCong,
       hb.ThanhToan,
       kh.HoTen AS TenKhachHang,
       km.TenKhuyenMai
FROM dbo.HoaDonBan hb
LEFT JOIN dbo.KhachHang kh ON kh.KhachHangId = hb.KhachHangId
LEFT JOIN dbo.KhuyenMai km ON km.KhuyenMaiId = hb.KhuyenMaiId
ORDER BY hb.HoaDonBanId DESC;
GO

-- Wave 4: Cau hinh he thong hien tai
SELECT TOP 1
       ch.CauHinhHeThongId,
       ch.TenQuan,
       ch.DiaChi,
       ch.SoDienThoai,
       ch.FooterHoaDon,
       ch.LogoPath,
       ch.UpdatedAt
FROM dbo.CauHinhHeThong ch
ORDER BY ch.CauHinhHeThongId DESC;
GO


