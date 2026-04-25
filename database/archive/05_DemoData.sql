USE CoffeeShopDb;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @AdminId INT = (
    SELECT TOP 1 nd.NguoiDungId
    FROM dbo.NguoiDung nd
    JOIN dbo.VaiTro vt ON vt.VaiTroId = nd.VaiTroId
    WHERE nd.TenDangNhap = N'admin' AND vt.MaVaiTro = N'Admin'
);

IF @AdminId IS NULL
BEGIN
    THROW 51000, N'Không tìm thấy tài khoản admin trong dữ liệu seed.', 1;
END
GO

-- 1) Dữ liệu danh mục/ncc/sản phẩm mở rộng cho demo
IF NOT EXISTS (SELECT 1 FROM dbo.DanhMuc WHERE TenDanhMuc = N'Sinh tố')
BEGIN
    INSERT INTO dbo.DanhMuc (TenDanhMuc, MoTa)
    VALUES (N'Sinh tố', N'Nhóm đồ uống trái cây xay.');
END

IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE TenNhaCungCap = N'Demo Supplier')
BEGIN
    INSERT INTO dbo.NhaCungCap (TenNhaCungCap, SoDienThoai, DiaChi)
    VALUES (N'Demo Supplier', N'0909000999', N'TP.HCM');
END
GO

DECLARE @DanhMucCaPhe INT = (SELECT TOP 1 DanhMucId FROM dbo.DanhMuc WHERE TenDanhMuc = N'Cà phê');
DECLARE @DanhMucSinhTo INT = (SELECT TOP 1 DanhMucId FROM dbo.DanhMuc WHERE TenDanhMuc = N'Sinh tố');

IF NOT EXISTS (SELECT 1 FROM dbo.Mon WHERE TenMon = N'Espresso Demo')
BEGIN
    INSERT INTO dbo.Mon (TenMon, DanhMucId, DonGia, TonKho, HinhAnhPath)
    VALUES (N'Espresso Demo', @DanhMucCaPhe, 45000, 40, N'images/espresso-demo.jpg');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Mon WHERE TenMon = N'Sinh tố dâu Demo')
BEGIN
    INSERT INTO dbo.Mon (TenMon, DanhMucId, DonGia, TonKho, HinhAnhPath)
    VALUES (N'Sinh tố dâu Demo', @DanhMucSinhTo, 52000, 35, N'images/sinhto-dau-demo.jpg');
END
GO

-- 2) Tạo 1 hóa đơn nhập mẫu có kiểm tra idempotent
DECLARE @NccDemoId INT = (SELECT TOP 1 NhaCungCapId FROM dbo.NhaCungCap WHERE TenNhaCungCap = N'Demo Supplier');
DECLARE @MonEspresso INT = (SELECT TOP 1 MonId FROM dbo.Mon WHERE TenMon = N'Espresso Demo');
DECLARE @MonSinhTo INT = (SELECT TOP 1 MonId FROM dbo.Mon WHERE TenMon = N'Sinh tố dâu Demo');
DECLARE @AdminId2 INT = (SELECT TOP 1 NguoiDungId FROM dbo.NguoiDung WHERE TenDangNhap = N'admin');

DECLARE @NgayNhapDemo DATETIME2 = '2026-01-15T08:00:00';
DECLARE @TongNhapDemo DECIMAL(18,2) = 214000;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.HoaDonNhap
    WHERE NgayNhap = @NgayNhapDemo
      AND NhaCungCapId = @NccDemoId
      AND CreatedByUserId = @AdminId2
      AND TongTien = @TongNhapDemo
)
BEGIN
    BEGIN TRANSACTION;

    DECLARE @NewHoaDonNhapId INT;

    INSERT INTO dbo.HoaDonNhap (NgayNhap, NhaCungCapId, TongTien, GhiChu, CreatedByUserId)
    VALUES (@NgayNhapDemo, @NccDemoId, @TongNhapDemo, N'Demo Seed Import', @AdminId2);

    SET @NewHoaDonNhapId = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.ChiTietHoaDonNhap (HoaDonNhapId, MonId, DonGiaNhap, SoLuong)
    VALUES
        (@NewHoaDonNhapId, @MonEspresso, 38000, 3),
        (@NewHoaDonNhapId, @MonSinhTo, 50000, 2);

    UPDATE dbo.Mon SET TonKho = TonKho + 3 WHERE MonId = @MonEspresso;
    UPDATE dbo.Mon SET TonKho = TonKho + 2 WHERE MonId = @MonSinhTo;

    COMMIT TRANSACTION;
END
GO

-- 3) Tạo 2 hóa đơn bán mẫu cho thống kê/report (2 ngày khác nhau)
DECLARE @AdminId3 INT = (SELECT TOP 1 NguoiDungId FROM dbo.NguoiDung WHERE TenDangNhap = N'admin');
DECLARE @MonEspresso2 INT = (SELECT TOP 1 MonId FROM dbo.Mon WHERE TenMon = N'Espresso Demo');
DECLARE @MonSinhTo2 INT = (SELECT TOP 1 MonId FROM dbo.Mon WHERE TenMon = N'Sinh tố dâu Demo');

IF (SELECT TonKho FROM dbo.Mon WHERE MonId = @MonEspresso2) < 2
    UPDATE dbo.Mon SET TonKho = 2 WHERE MonId = @MonEspresso2;
IF (SELECT TonKho FROM dbo.Mon WHERE MonId = @MonSinhTo2) < 2
    UPDATE dbo.Mon SET TonKho = 2 WHERE MonId = @MonSinhTo2;

DECLARE @NgayBan1 DATETIME2 = '2026-01-15T09:30:00';
DECLARE @NgayBan2 DATETIME2 = '2026-01-16T10:15:00';

IF NOT EXISTS (
    SELECT 1 FROM dbo.HoaDonBan
    WHERE NgayBan = @NgayBan1
      AND CreatedByUserId = @AdminId3
      AND TongTien = 90000
      AND GiamGia = 5000
)
BEGIN
    BEGIN TRANSACTION;

    DECLARE @NewHoaDonBanId1 INT;

    INSERT INTO dbo.HoaDonBan (NgayBan, TongTien, GiamGia, CreatedByUserId)
    VALUES (@NgayBan1, 90000, 5000, @AdminId3);

    SET @NewHoaDonBanId1 = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.ChiTietHoaDonBan (HoaDonBanId, MonId, DonGiaBan, SoLuong)
    VALUES (@NewHoaDonBanId1, @MonEspresso2, 45000, 2);

    UPDATE dbo.Mon SET TonKho = TonKho - 2 WHERE MonId = @MonEspresso2;

    COMMIT TRANSACTION;
END

IF NOT EXISTS (
    SELECT 1 FROM dbo.HoaDonBan
    WHERE NgayBan = @NgayBan2
      AND CreatedByUserId = @AdminId3
      AND TongTien = 104000
      AND GiamGia = 4000
)
BEGIN
    BEGIN TRANSACTION;

    DECLARE @NewHoaDonBanId2 INT;

    INSERT INTO dbo.HoaDonBan (NgayBan, TongTien, GiamGia, CreatedByUserId)
    VALUES (@NgayBan2, 104000, 4000, @AdminId3);

    SET @NewHoaDonBanId2 = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.ChiTietHoaDonBan (HoaDonBanId, MonId, DonGiaBan, SoLuong)
    VALUES (@NewHoaDonBanId2, @MonSinhTo2, 52000, 2);

    UPDATE dbo.Mon SET TonKho = TonKho - 2 WHERE MonId = @MonSinhTo2;

    COMMIT TRANSACTION;
END
GO

SELECT N'05_DemoData.sql completed' AS [Status];
GO


