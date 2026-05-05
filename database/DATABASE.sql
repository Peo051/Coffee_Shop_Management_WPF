-- Cơ sở dữ liệu đồ án NET
-- Tạo cơ sở dữ liệu

IF DB_ID(N'CoffeeShopDb') IS NULL
BEGIN
    CREATE DATABASE CoffeeShopDb;
END
GO

USE CoffeeShopDb;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO


IF OBJECT_ID(N'dbo.VaiTro', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VaiTro
    (
        VaiTroId INT IDENTITY(1,1) PRIMARY KEY,
        MaVaiTro NVARCHAR(50) NOT NULL UNIQUE,
        TenVaiTro NVARCHAR(100) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.NguoiDung', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NguoiDung
    (
        NguoiDungId INT IDENTITY(1,1) PRIMARY KEY,
        TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
        MatKhau NVARCHAR(255) NOT NULL,
        HoTen NVARCHAR(150) NOT NULL,
        VaiTroId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
        CONSTRAINT FK_NguoiDung_VaiTro FOREIGN KEY (VaiTroId) REFERENCES dbo.VaiTro(VaiTroId)
    );
END
GO

IF OBJECT_ID(N'dbo.DanhMuc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DanhMuc
    (
        DanhMucId INT IDENTITY(1,1) PRIMARY KEY,
        TenDanhMuc NVARCHAR(150) NOT NULL,
        MoTa NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(SYSDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.NhaCungCap', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NhaCungCap
    (
        NhaCungCapId INT IDENTITY(1,1) PRIMARY KEY,
        TenNhaCungCap NVARCHAR(150) NOT NULL,
        SoDienThoai NVARCHAR(20) NULL,
        DiaChi NVARCHAR(300) NULL,
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(SYSDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.Mon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Mon
    (
        MonId INT IDENTITY(1,1) PRIMARY KEY,
        TenMon NVARCHAR(150) NOT NULL,
        DanhMucId INT NOT NULL,
        DonGia DECIMAL(18,2) NOT NULL,
        TonKho INT NOT NULL DEFAULT(0),
        HinhAnhPath NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
        CONSTRAINT FK_Mon_DanhMuc FOREIGN KEY (DanhMucId) REFERENCES dbo.DanhMuc(DanhMucId)
    );
END
GO

IF OBJECT_ID(N'dbo.HoaDonNhap', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDonNhap
    (
        HoaDonNhapId INT IDENTITY(1,1) PRIMARY KEY,
        NgayNhap DATETIME2 NOT NULL,
        NhaCungCapId INT NOT NULL,
        TongTien DECIMAL(18,2) NOT NULL,
        GhiChu NVARCHAR(500) NULL,
        CreatedByUserId INT NOT NULL,
        CONSTRAINT FK_HoaDonNhap_NhaCungCap FOREIGN KEY (NhaCungCapId) REFERENCES dbo.NhaCungCap(NhaCungCapId),
        CONSTRAINT FK_HoaDonNhap_NguoiDung FOREIGN KEY (CreatedByUserId) REFERENCES dbo.NguoiDung(NguoiDungId)
    );
END
GO

IF OBJECT_ID(N'dbo.ChiTietHoaDonNhap', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietHoaDonNhap
    (
        ChiTietHoaDonNhapId INT IDENTITY(1,1) PRIMARY KEY,
        HoaDonNhapId INT NOT NULL,
        MonId INT NOT NULL,
        DonGiaNhap DECIMAL(18,2) NOT NULL,
        SoLuong INT NOT NULL,
        ThanhTien AS (DonGiaNhap * SoLuong) PERSISTED,
        CONSTRAINT FK_ChiTietHoaDonNhap_HoaDonNhap FOREIGN KEY (HoaDonNhapId) REFERENCES dbo.HoaDonNhap(HoaDonNhapId),
        CONSTRAINT FK_ChiTietHoaDonNhap_Mon FOREIGN KEY (MonId) REFERENCES dbo.Mon(MonId)
    );
END
GO

IF OBJECT_ID(N'dbo.HoaDonBan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDonBan
    (
        HoaDonBanId INT IDENTITY(1,1) PRIMARY KEY,
        NgayBan DATETIME2 NOT NULL,
        TongTien DECIMAL(18,2) NOT NULL,
        GiamGia DECIMAL(18,2) NOT NULL DEFAULT(0),
        ThanhToan AS (TongTien - GiamGia) PERSISTED,
        CreatedByUserId INT NOT NULL,
        CONSTRAINT FK_HoaDonBan_NguoiDung FOREIGN KEY (CreatedByUserId) REFERENCES dbo.NguoiDung(NguoiDungId)
    );
END
GO

IF OBJECT_ID(N'dbo.ChiTietHoaDonBan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietHoaDonBan
    (
        ChiTietHoaDonBanId INT IDENTITY(1,1) PRIMARY KEY,
        HoaDonBanId INT NOT NULL,
        MonId INT NOT NULL,
        DonGiaBan DECIMAL(18,2) NOT NULL,
        SoLuong INT NOT NULL,
        ThanhTien AS (DonGiaBan * SoLuong) PERSISTED,
        CONSTRAINT FK_ChiTietHoaDonBan_HoaDonBan FOREIGN KEY (HoaDonBanId) REFERENCES dbo.HoaDonBan(HoaDonBanId),
        CONSTRAINT FK_ChiTietHoaDonBan_Mon FOREIGN KEY (MonId) REFERENCES dbo.Mon(MonId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Mon_DanhMucId' AND object_id = OBJECT_ID(N'dbo.Mon'))
    CREATE INDEX IX_Mon_DanhMucId ON dbo.Mon(DanhMucId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HoaDonNhap_NgayNhap' AND object_id = OBJECT_ID(N'dbo.HoaDonNhap'))
    CREATE INDEX IX_HoaDonNhap_NgayNhap ON dbo.HoaDonNhap(NgayNhap);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HoaDonBan_NgayBan' AND object_id = OBJECT_ID(N'dbo.HoaDonBan'))
    CREATE INDEX IX_HoaDonBan_NgayBan ON dbo.HoaDonBan(NgayBan);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro)
BEGIN
    INSERT INTO dbo.VaiTro (MaVaiTro, TenVaiTro)
    VALUES
        (N'Admin', N'Quản trị viên'),
        (N'Kho', N'Nhân viên kho'),
        (N'ThuNgan', N'Thu ngân');
END
GO

DECLARE @VaiTroAdmin INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'Admin');
DECLARE @VaiTroKho INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'Kho');
DECLARE @VaiTroThuNgan INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'ThuNgan');

IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap = N'admin')
BEGIN
    INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive)
    VALUES (N'admin', N'123123', N'Trần Gia Bảo', @VaiTroAdmin, 1);
END
ELSE
BEGIN
    UPDATE dbo.NguoiDung
    SET MatKhau = N'123123',
        HoTen = N'Trần Gia Bảo',
        VaiTroId = @VaiTroAdmin,
        IsActive = 1
    WHERE TenDangNhap = N'admin';
END

IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap = N'kho')
BEGIN
    INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive)
    VALUES (N'kho', N'123456', N'Nguyễn Thế Anh', @VaiTroKho, 1);
END
ELSE
BEGIN
    UPDATE dbo.NguoiDung
    SET MatKhau = N'123456',
        HoTen = N'Nguyễn Thế Anh',
        VaiTroId = @VaiTroKho,
        IsActive = 1
    WHERE TenDangNhap = N'kho';
END

IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap = N'thungan')
BEGIN
    INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive)
    VALUES (N'thungan', N'123456', N'Trần Dương Gia Bảo', @VaiTroThuNgan, 1);
END
ELSE
BEGIN
    UPDATE dbo.NguoiDung
    SET MatKhau = N'123456',
        HoTen = N'Trần Dương Gia Bảo',
        VaiTroId = @VaiTroThuNgan,
        IsActive = 1
    WHERE TenDangNhap = N'thungan';
END
GO

DECLARE @VaiTroThuNgan2 INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'ThuNgan');

IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap = N'khoa_test')
BEGIN
    INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive)
    VALUES (N'khoa_test', N'123456', N'Tài khoản khóa test', @VaiTroThuNgan2, 0);
END
ELSE
BEGIN
    UPDATE dbo.NguoiDung
    SET MatKhau = N'123456',
        HoTen = N'Tài khoản khóa test',
        VaiTroId = @VaiTroThuNgan2,
        IsActive = 0
    WHERE TenDangNhap = N'khoa_test';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.DanhMuc)
BEGIN
    INSERT INTO dbo.DanhMuc (TenDanhMuc, MoTa)
    VALUES
        (N'Cà phê', N'Đồ uống từ cà phê'),
        (N'Trà', N'Đồ uống từ trà'),
        (N'Bánh', N'Đồ ăn nhẹ');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap)
BEGIN
    INSERT INTO dbo.NhaCungCap (TenNhaCungCap, SoDienThoai, DiaChi)
    VALUES
        (N'Công ty Hạt Việt', N'0909123456', N'Quận 1, TP.HCM'),
        (N'Nông trại Sạch', N'0909654321', N'Đà Lạt');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Mon)
BEGIN
    DECLARE @DanhMucCaPhe INT = (SELECT TOP 1 DanhMucId FROM dbo.DanhMuc WHERE TenDanhMuc = N'Cà phê');
    DECLARE @DanhMucTra INT = (SELECT TOP 1 DanhMucId FROM dbo.DanhMuc WHERE TenDanhMuc = N'Trà');

    INSERT INTO dbo.Mon (TenMon, DanhMucId, DonGia, TonKho, HinhAnhPath)
    VALUES
        (N'Cà phê sữa', @DanhMucCaPhe, 32000, 30, N'images/caphe-sua.jpg'),
        (N'Bạc xỉu', @DanhMucCaPhe, 35000, 20, N'images/bac-xiu.jpg'),
        (N'Trà đào', @DanhMucTra, 38000, 25, N'images/tra-dao.jpg');
END
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

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.Mon', N'MucCanhBaoTonKho') IS NULL
BEGIN
    ALTER TABLE dbo.Mon
    ADD MucCanhBaoTonKho INT NOT NULL
        CONSTRAINT DF_Mon_MucCanhBaoTonKho DEFAULT(10);
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Mon')
      AND name = N'MucCanhBaoTonKho'
      AND is_nullable = 1)
BEGIN
    UPDATE dbo.Mon
    SET MucCanhBaoTonKho = ISNULL(MucCanhBaoTonKho, 10);

    ALTER TABLE dbo.Mon
    ALTER COLUMN MucCanhBaoTonKho INT NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.object_id = dc.parent_object_id
       AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Mon')
      AND c.name = N'MucCanhBaoTonKho')
BEGIN
    ALTER TABLE dbo.Mon
    ADD CONSTRAINT DF_Mon_MucCanhBaoTonKho DEFAULT(10) FOR MucCanhBaoTonKho;
END
GO

UPDATE dbo.Mon
SET MucCanhBaoTonKho = 0
WHERE MucCanhBaoTonKho < 0;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Mon_MucCanhBaoTonKho_NonNegative'
      AND parent_object_id = OBJECT_ID(N'dbo.Mon'))
BEGIN
    ALTER TABLE dbo.Mon
    ADD CONSTRAINT CK_Mon_MucCanhBaoTonKho_NonNegative
        CHECK (MucCanhBaoTonKho >= 0);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Mon_CanhBaoTonKho'
      AND object_id = OBJECT_ID(N'dbo.Mon'))
BEGIN
    CREATE INDEX IX_Mon_CanhBaoTonKho
        ON dbo.Mon(IsActive, TonKho, MucCanhBaoTonKho, DanhMucId);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ChiTietHoaDonBan_MonId_HoaDonBanId'
      AND object_id = OBJECT_ID(N'dbo.ChiTietHoaDonBan'))
BEGIN
    CREATE INDEX IX_ChiTietHoaDonBan_MonId_HoaDonBanId
        ON dbo.ChiTietHoaDonBan(MonId, HoaDonBanId)
        INCLUDE (SoLuong);
END
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.KhuVuc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhuVuc
    (
        KhuVucId INT IDENTITY(1,1) PRIMARY KEY,
        TenKhuVuc NVARCHAR(100) NOT NULL,
        MoTa NVARCHAR(300) NULL,
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
        CONSTRAINT UQ_KhuVuc_TenKhuVuc UNIQUE(TenKhuVuc)
    );
END
GO

IF OBJECT_ID(N'dbo.Ban', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ban
    (
        BanId INT IDENTITY(1,1) PRIMARY KEY,
        KhuVucId INT NOT NULL,
        TenBan NVARCHAR(100) NOT NULL,
        TrangThaiBan NVARCHAR(30) NOT NULL DEFAULT(N'Trong'),
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
        CONSTRAINT FK_Ban_KhuVuc FOREIGN KEY (KhuVucId) REFERENCES dbo.KhuVuc(KhuVucId),
        CONSTRAINT UQ_Ban_KhuVuc_TenBan UNIQUE(KhuVucId, TenBan),
        CONSTRAINT CK_Ban_TrangThaiBan CHECK (TrangThaiBan IN (N'Trong', N'DangPhucVu', N'ChoThanhToan', N'TamKhoa'))
    );
END
GO

IF OBJECT_ID(N'dbo.CaLamViec', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CaLamViec
    (
        CaLamViecId INT IDENTITY(1,1) PRIMARY KEY,
        NguoiDungId INT NOT NULL,
        ThoiGianMoCa DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
        ThoiGianDongCa DATETIME2 NULL,
        TrangThaiCa NVARCHAR(20) NOT NULL DEFAULT(N'DangMo'),
        GhiChu NVARCHAR(500) NULL,
        CONSTRAINT FK_CaLamViec_NguoiDung FOREIGN KEY (NguoiDungId) REFERENCES dbo.NguoiDung(NguoiDungId),
        CONSTRAINT CK_CaLamViec_TrangThaiCa CHECK (TrangThaiCa IN (N'DangMo', N'DaDong')),
        CONSTRAINT CK_CaLamViec_ThoiGian CHECK (ThoiGianDongCa IS NULL OR ThoiGianDongCa >= ThoiGianMoCa)
    );
END
GO

IF COL_LENGTH(N'dbo.HoaDonBan', N'BanId') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD BanId INT NULL;
END
GO

IF COL_LENGTH(N'dbo.HoaDonBan', N'CaLamViecId') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CaLamViecId INT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_HoaDonBan_Ban'
      AND parent_object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT FK_HoaDonBan_Ban
        FOREIGN KEY (BanId) REFERENCES dbo.Ban(BanId);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_HoaDonBan_CaLamViec'
      AND parent_object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT FK_HoaDonBan_CaLamViec
        FOREIGN KEY (CaLamViecId) REFERENCES dbo.CaLamViec(CaLamViecId);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Ban_KhuVuc_TrangThai'
      AND object_id = OBJECT_ID(N'dbo.Ban'))
BEGIN
    CREATE INDEX IX_Ban_KhuVuc_TrangThai
        ON dbo.Ban(KhuVucId, IsActive, TrangThaiBan);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_CaLamViec_NguoiDung_TrangThai'
      AND object_id = OBJECT_ID(N'dbo.CaLamViec'))
BEGIN
    CREATE INDEX IX_CaLamViec_NguoiDung_TrangThai
        ON dbo.CaLamViec(NguoiDungId, TrangThaiCa, ThoiGianMoCa DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_HoaDonBan_BanId'
      AND object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    CREATE INDEX IX_HoaDonBan_BanId
        ON dbo.HoaDonBan(BanId);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_HoaDonBan_CaLamViecId'
      AND object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    CREATE INDEX IX_HoaDonBan_CaLamViecId
        ON dbo.HoaDonBan(CaLamViecId);
END
GO

DECLARE @TenKhuVucTang1 NVARCHAR(100) = N'T' + NCHAR(7847) + N'ng 1';
DECLARE @TenKhuVucTang2 NVARCHAR(100) = N'T' + NCHAR(7847) + N'ng 2';
DECLARE @TenKhuVucSanVuon NVARCHAR(100) = N'S' + NCHAR(226) + N'n v' + NCHAR(432) + NCHAR(7901) + N'n';
DECLARE @TenKhuVucMangDi NVARCHAR(100) = N'Mang ' + NCHAR(273) + N'i';
DECLARE @TenBan01 NVARCHAR(100) = N'B' + NCHAR(224) + N'n 01';
DECLARE @TenBan02 NVARCHAR(100) = N'B' + NCHAR(224) + N'n 02';
DECLARE @TenBan03 NVARCHAR(100) = N'B' + NCHAR(224) + N'n 03';
DECLARE @TenBan04 NVARCHAR(100) = N'B' + NCHAR(224) + N'n 04';

-- Chuẩn hóa dữ liệu seed nếu từng bị lỗi mã hóa (chạy bằng sqlcmd codepage cũ).
UPDATE dbo.KhuVuc SET TenKhuVuc = @TenKhuVucTang1 WHERE TenKhuVuc LIKE N'T%ng 1' AND TenKhuVuc <> @TenKhuVucTang1;
UPDATE dbo.KhuVuc SET TenKhuVuc = @TenKhuVucTang2 WHERE TenKhuVuc LIKE N'T%ng 2' AND TenKhuVuc <> @TenKhuVucTang2;
UPDATE dbo.KhuVuc SET TenKhuVuc = @TenKhuVucSanVuon WHERE TenKhuVuc LIKE N'S%n v%n' AND TenKhuVuc <> @TenKhuVucSanVuon;
UPDATE dbo.KhuVuc SET TenKhuVuc = @TenKhuVucMangDi WHERE TenKhuVuc LIKE N'Mang %i' AND TenKhuVuc <> @TenKhuVucMangDi;

UPDATE dbo.Ban SET TenBan = @TenBan01 WHERE TenBan LIKE N'B%n 01' AND TenBan <> @TenBan01;
UPDATE dbo.Ban SET TenBan = @TenBan02 WHERE TenBan LIKE N'B%n 02' AND TenBan <> @TenBan02;
UPDATE dbo.Ban SET TenBan = @TenBan03 WHERE TenBan LIKE N'B%n 03' AND TenBan <> @TenBan03;
UPDATE dbo.Ban SET TenBan = @TenBan04 WHERE TenBan LIKE N'B%n 04' AND TenBan <> @TenBan04;
UPDATE dbo.Ban SET TenBan = @TenKhuVucMangDi WHERE TenBan LIKE N'Mang %i' AND TenBan <> @TenKhuVucMangDi;

IF NOT EXISTS (SELECT 1 FROM dbo.KhuVuc WHERE TenKhuVuc = @TenKhuVucTang1)
    INSERT INTO dbo.KhuVuc (TenKhuVuc, MoTa, IsActive) VALUES (@TenKhuVucTang1, N'Khu vuc trong nha tang tret.', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.KhuVuc WHERE TenKhuVuc = @TenKhuVucTang2)
    INSERT INTO dbo.KhuVuc (TenKhuVuc, MoTa, IsActive) VALUES (@TenKhuVucTang2, N'Khu vuc yen tinh.', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.KhuVuc WHERE TenKhuVuc = @TenKhuVucSanVuon)
    INSERT INTO dbo.KhuVuc (TenKhuVuc, MoTa, IsActive) VALUES (@TenKhuVucSanVuon, N'Khu vuc ngoai troi.', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.KhuVuc WHERE TenKhuVuc = @TenKhuVucMangDi)
    INSERT INTO dbo.KhuVuc (TenKhuVuc, MoTa, IsActive) VALUES (@TenKhuVucMangDi, N'Don hang khong phuc vu tai ban.', 1);
GO

DECLARE @TenKhuVucTang1 NVARCHAR(100) = N'T' + NCHAR(7847) + N'ng 1';
DECLARE @TenKhuVucTang2 NVARCHAR(100) = N'T' + NCHAR(7847) + N'ng 2';
DECLARE @TenKhuVucSanVuon NVARCHAR(100) = N'S' + NCHAR(226) + N'n v' + NCHAR(432) + NCHAR(7901) + N'n';
DECLARE @TenKhuVucMangDi NVARCHAR(100) = N'Mang ' + NCHAR(273) + N'i';
DECLARE @TenBan01 NVARCHAR(100) = N'B' + NCHAR(224) + N'n 01';
DECLARE @TenBan02 NVARCHAR(100) = N'B' + NCHAR(224) + N'n 02';
DECLARE @TenBan03 NVARCHAR(100) = N'B' + NCHAR(224) + N'n 03';
DECLARE @TenBan04 NVARCHAR(100) = N'B' + NCHAR(224) + N'n 04';

DECLARE @KhuVucTang1 INT = (SELECT TOP 1 KhuVucId FROM dbo.KhuVuc WHERE TenKhuVuc = @TenKhuVucTang1);
DECLARE @KhuVucTang2 INT = (SELECT TOP 1 KhuVucId FROM dbo.KhuVuc WHERE TenKhuVuc = @TenKhuVucTang2);
DECLARE @KhuVucSanVuon INT = (SELECT TOP 1 KhuVucId FROM dbo.KhuVuc WHERE TenKhuVuc = @TenKhuVucSanVuon);
DECLARE @KhuVucMangDi INT = (SELECT TOP 1 KhuVucId FROM dbo.KhuVuc WHERE TenKhuVuc = @TenKhuVucMangDi);

IF @KhuVucTang1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Ban WHERE KhuVucId = @KhuVucTang1 AND TenBan = @TenBan01)
    INSERT INTO dbo.Ban (KhuVucId, TenBan, TrangThaiBan, IsActive) VALUES (@KhuVucTang1, @TenBan01, N'Trong', 1);
IF @KhuVucTang1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Ban WHERE KhuVucId = @KhuVucTang1 AND TenBan = @TenBan02)
    INSERT INTO dbo.Ban (KhuVucId, TenBan, TrangThaiBan, IsActive) VALUES (@KhuVucTang1, @TenBan02, N'Trong', 1);
IF @KhuVucTang2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Ban WHERE KhuVucId = @KhuVucTang2 AND TenBan = @TenBan03)
    INSERT INTO dbo.Ban (KhuVucId, TenBan, TrangThaiBan, IsActive) VALUES (@KhuVucTang2, @TenBan03, N'Trong', 1);
IF @KhuVucSanVuon IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Ban WHERE KhuVucId = @KhuVucSanVuon AND TenBan = @TenBan04)
    INSERT INTO dbo.Ban (KhuVucId, TenBan, TrangThaiBan, IsActive) VALUES (@KhuVucSanVuon, @TenBan04, N'Trong', 1);
IF @KhuVucMangDi IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Ban WHERE KhuVucId = @KhuVucMangDi AND TenBan = @TenKhuVucMangDi)
    INSERT INTO dbo.Ban (KhuVucId, TenBan, TrangThaiBan, IsActive) VALUES (@KhuVucMangDi, @TenKhuVucMangDi, N'Trong', 1);
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog
    (
        AuditLogId INT IDENTITY(1,1) PRIMARY KEY,
        NguoiDungId INT NULL,
        HanhDong NVARCHAR(120) NOT NULL,
        DoiTuong NVARCHAR(120) NULL,
        DuLieuTomTat NVARCHAR(1000) NULL,
        ThoiGianTao DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
        MayTram NVARCHAR(120) NULL,
        CONSTRAINT FK_AuditLog_NguoiDung FOREIGN KEY (NguoiDungId) REFERENCES dbo.NguoiDung(NguoiDungId)
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_AuditLog_ThoiGianTao'
      AND object_id = OBJECT_ID(N'dbo.AuditLog'))
BEGIN
    CREATE INDEX IX_AuditLog_ThoiGianTao
        ON dbo.AuditLog(ThoiGianTao DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_AuditLog_NguoiDung_ThoiGian'
      AND object_id = OBJECT_ID(N'dbo.AuditLog'))
BEGIN
    CREATE INDEX IX_AuditLog_NguoiDung_ThoiGian
        ON dbo.AuditLog(NguoiDungId, ThoiGianTao DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_AuditLog_HanhDong_ThoiGian'
      AND object_id = OBJECT_ID(N'dbo.AuditLog'))
BEGIN
    CREATE INDEX IX_AuditLog_HanhDong_ThoiGian
        ON dbo.AuditLog(HanhDong, ThoiGianTao DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_HoaDonBan_NgayBan_ThanhToan'
      AND object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    CREATE INDEX IX_HoaDonBan_NgayBan_ThanhToan
        ON dbo.HoaDonBan(NgayBan, ThanhToan);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Mon_IsActive_TonKho_CanhBao'
      AND object_id = OBJECT_ID(N'dbo.Mon'))
BEGIN
    CREATE INDEX IX_Mon_IsActive_TonKho_CanhBao
        ON dbo.Mon(IsActive, TonKho, MucCanhBaoTonKho);
END
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.KhuyenMai', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhuyenMai
    (
        KhuyenMaiId INT IDENTITY(1,1) PRIMARY KEY,
        TenKhuyenMai NVARCHAR(150) NOT NULL,
        LoaiKhuyenMai NVARCHAR(30) NOT NULL,
        GiaTri DECIMAL(18,2) NOT NULL,
        TuNgay DATETIME2 NOT NULL,
        DenNgay DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT(1),
        MonId INT NULL,
        MoTa NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
        CONSTRAINT CK_KhuyenMai_Loai CHECK (LoaiKhuyenMai IN (N'PhanTramHoaDon', N'SoTienCoDinh', N'TheoSanPham')),
        CONSTRAINT CK_KhuyenMai_GiaTri CHECK (GiaTri > 0),
        CONSTRAINT CK_KhuyenMai_ThoiGian CHECK (DenNgay >= TuNgay),
        CONSTRAINT FK_KhuyenMai_Mon FOREIGN KEY (MonId) REFERENCES dbo.Mon(MonId)
    );
END
GO

IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhachHang
    (
        KhachHangId INT IDENTITY(1,1) PRIMARY KEY,
        HoTen NVARCHAR(150) NOT NULL,
        SoDienThoai NVARCHAR(20) NULL,
        Email NVARCHAR(150) NULL,
        DiemTichLuy INT NOT NULL DEFAULT(0),
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
        CONSTRAINT CK_KhachHang_Diem CHECK (DiemTichLuy >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.CauHinhHeThong', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CauHinhHeThong
    (
        CauHinhHeThongId INT IDENTITY(1,1) PRIMARY KEY,
        TenQuan NVARCHAR(200) NOT NULL,
        DiaChi NVARCHAR(300) NULL,
        SoDienThoai NVARCHAR(20) NULL,
        FooterHoaDon NVARCHAR(500) NULL,
        LogoPath NVARCHAR(500) NULL,
        UpdatedAt DATETIME2 NOT NULL DEFAULT(SYSDATETIME())
    );
END
GO

IF COL_LENGTH(N'dbo.HoaDonBan', N'KhachHangId') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD KhachHangId INT NULL;
END
GO

IF COL_LENGTH(N'dbo.HoaDonBan', N'KhuyenMaiId') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD KhuyenMaiId INT NULL;
END
GO

IF COL_LENGTH(N'dbo.HoaDonBan', N'SoTienGiam') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD SoTienGiam DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDonBan_SoTienGiam DEFAULT(0);
END
GO

IF COL_LENGTH(N'dbo.HoaDonBan', N'DiemCong') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD DiemCong INT NOT NULL CONSTRAINT DF_HoaDonBan_DiemCong DEFAULT(0);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_HoaDonBan_KhachHang'
      AND parent_object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT FK_HoaDonBan_KhachHang
        FOREIGN KEY (KhachHangId) REFERENCES dbo.KhachHang(KhachHangId);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_HoaDonBan_KhuyenMai'
      AND parent_object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT FK_HoaDonBan_KhuyenMai
        FOREIGN KEY (KhuyenMaiId) REFERENCES dbo.KhuyenMai(KhuyenMaiId);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_HoaDonBan_SoTienGiam'
      AND parent_object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT CK_HoaDonBan_SoTienGiam CHECK (SoTienGiam >= 0);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_HoaDonBan_DiemCong'
      AND parent_object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT CK_HoaDonBan_DiemCong CHECK (DiemCong >= 0);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_KhuyenMai_HieuLuc'
      AND object_id = OBJECT_ID(N'dbo.KhuyenMai'))
BEGIN
    CREATE INDEX IX_KhuyenMai_HieuLuc
        ON dbo.KhuyenMai(IsActive, TuNgay, DenNgay, LoaiKhuyenMai);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_KhachHang_SoDienThoai'
      AND object_id = OBJECT_ID(N'dbo.KhachHang'))
BEGIN
    CREATE INDEX IX_KhachHang_SoDienThoai
        ON dbo.KhachHang(SoDienThoai)
        WHERE SoDienThoai IS NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_HoaDonBan_KhachHangId'
      AND object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    CREATE INDEX IX_HoaDonBan_KhachHangId
        ON dbo.HoaDonBan(KhachHangId, NgayBan DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_HoaDonBan_KhuyenMaiId'
      AND object_id = OBJECT_ID(N'dbo.HoaDonBan'))
BEGIN
    CREATE INDEX IX_HoaDonBan_KhuyenMaiId
        ON dbo.HoaDonBan(KhuyenMaiId, NgayBan DESC);
END
GO

DECLARE @DiaChiDemo NVARCHAR(300) = N'123 '
    + NCHAR(272) + NCHAR(432) + NCHAR(7901) + N'ng Demo, Qu'
    + NCHAR(7853) + N'n 1, TP.HCM';
DECLARE @FooterHoaDon NVARCHAR(500) = N'C'
    + NCHAR(7843) + N'm '
    + NCHAR(417) + N'n qu'
    + NCHAR(253) + N' kh'
    + NCHAR(225) + N'ch v'
    + NCHAR(224) + N' h'
    + NCHAR(7865) + N'n g'
    + NCHAR(7863) + N'p l'
    + NCHAR(7841) + N'i.';
DECLARE @HoTenKhA NVARCHAR(150) = N'Nguy'
    + NCHAR(7877) + N'n V'
    + NCHAR(259) + N'n A';
DECLARE @HoTenKhB NVARCHAR(150) = N'Tr'
    + NCHAR(7847) + N'n Th'
    + NCHAR(7883) + N' B';
DECLARE @TenKmPhanTram NVARCHAR(150) = N'Gi'
    + NCHAR(7843) + N'm 10% h'
    + NCHAR(243) + N'a '
    + NCHAR(273) + NCHAR(417) + N'n';
DECLARE @MoTaKmPhanTram NVARCHAR(500) = N''
    + NCHAR(193) + N'p d'
    + NCHAR(7909) + N'ng gi'
    + NCHAR(7843) + N'm theo ph'
    + NCHAR(7847) + N'n tr'
    + NCHAR(259) + N'm to'
    + NCHAR(224) + N'n h'
    + NCHAR(243) + N'a '
    + NCHAR(273) + NCHAR(417) + N'n.';
DECLARE @TenKmSoTien NVARCHAR(150) = N'Gi'
    + NCHAR(7843) + N'm 20.000 '
    + NCHAR(273) + NCHAR(7891) + N'ng';
DECLARE @MoTaKmSoTien NVARCHAR(500) = N''
    + NCHAR(193) + N'p d'
    + NCHAR(7909) + N'ng gi'
    + NCHAR(7843) + N'm s'
    + NCHAR(7889) + N' ti'
    + NCHAR(7873) + N'n c'
    + NCHAR(7889) + N' '
    + NCHAR(273) + NCHAR(7883) + N'nh tr'
    + NCHAR(234) + N'n h'
    + NCHAR(243) + N'a '
    + NCHAR(273) + NCHAR(417) + N'n.';

-- Chuan hoa du lieu seed neu da tung bi loi ma hoa.
UPDATE dbo.CauHinhHeThong
SET DiaChi = @DiaChiDemo,
    FooterHoaDon = @FooterHoaDon
WHERE TenQuan = N'CoffeeShop CF4';

UPDATE dbo.KhachHang
SET HoTen = @HoTenKhA
WHERE SoDienThoai = N'0901000001';

UPDATE dbo.KhachHang
SET HoTen = @HoTenKhB
WHERE SoDienThoai = N'0901000002';

UPDATE dbo.KhuyenMai
SET TenKhuyenMai = @TenKmPhanTram,
    MoTa = @MoTaKmPhanTram
WHERE LoaiKhuyenMai = N'PhanTramHoaDon'
  AND GiaTri = 10
  AND MonId IS NULL;

UPDATE dbo.KhuyenMai
SET TenKhuyenMai = @TenKmSoTien,
    MoTa = @MoTaKmSoTien
WHERE LoaiKhuyenMai = N'SoTienCoDinh'
  AND GiaTri = 20000
  AND MonId IS NULL;

IF NOT EXISTS (SELECT 1 FROM dbo.CauHinhHeThong)
BEGIN
    INSERT INTO dbo.CauHinhHeThong (TenQuan, DiaChi, SoDienThoai, FooterHoaDon, LogoPath)
    VALUES
    (
        N'CoffeeShop CF4',
        @DiaChiDemo,
        N'0909000999',
        @FooterHoaDon,
        NULL
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE SoDienThoai = N'0901000001')
BEGIN
    INSERT INTO dbo.KhachHang (HoTen, SoDienThoai, Email, DiemTichLuy, IsActive)
    VALUES (@HoTenKhA, N'0901000001', N'nguyenvana@example.com', 0, 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE SoDienThoai = N'0901000002')
BEGIN
    INSERT INTO dbo.KhachHang (HoTen, SoDienThoai, Email, DiemTichLuy, IsActive)
    VALUES (@HoTenKhB, N'0901000002', N'tranthib@example.com', 25, 1);
END

IF NOT EXISTS (
    SELECT 1
    FROM dbo.KhuyenMai
    WHERE TenKhuyenMai = @TenKmPhanTram
      AND LoaiKhuyenMai = N'PhanTramHoaDon')
BEGIN
    INSERT INTO dbo.KhuyenMai (TenKhuyenMai, LoaiKhuyenMai, GiaTri, TuNgay, DenNgay, IsActive, MonId, MoTa)
    VALUES
    (
        @TenKmPhanTram,
        N'PhanTramHoaDon',
        10,
        DATEADD(DAY, -15, SYSDATETIME()),
        DATEADD(DAY, 60, SYSDATETIME()),
        1,
        NULL,
        @MoTaKmPhanTram
    );
END

IF NOT EXISTS (
    SELECT 1
    FROM dbo.KhuyenMai
    WHERE TenKhuyenMai = @TenKmSoTien
      AND LoaiKhuyenMai = N'SoTienCoDinh')
BEGIN
    INSERT INTO dbo.KhuyenMai (TenKhuyenMai, LoaiKhuyenMai, GiaTri, TuNgay, DenNgay, IsActive, MonId, MoTa)
    VALUES
    (
        @TenKmSoTien,
        N'SoTienCoDinh',
        20000,
        DATEADD(DAY, -15, SYSDATETIME()),
        DATEADD(DAY, 60, SYSDATETIME()),
        1,
        NULL,
        @MoTaKmSoTien
    );
END
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


-- ============================================================
-- GIBOR Coffee Shop - Consolidated Upgrade Scripts
-- Database: CoffeeShopDb
-- Lưu ý:
--   1) Đây là script NÂNG CẤP, dùng sau khi đã chạy database gốc 01-09.
--   2) Script được viết theo hướng idempotent: chạy lại nhiều lần sẽ hạn chế lỗi.
--   3) Nên backup database trước khi chạy.
-- ============================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================
-- FILE 01 - BÁN HÀNG / THU NGÂN / HÓA ĐƠN
-- Gộp từ: 10, 11, 12, 13, 15, 18
-- 14_MakeBanOptionalForHoaDonBan đã được tích hợp trong phần HinhThucPhucVu.
-- ============================================================


-- ============================================================
-- PHẦN 10
-- Source: 10_CashierUpgrade_Migration.sql
-- ============================================================
-- =============================================
-- Script: Nâng cấp nghiệp vụ thu ngân (PHIÊN BẢN ĐẦY ĐỦ)
-- Mô tả:
--   PHẦN A: Thêm các cột thu ngân vào bảng HoaDonBan
--   PHẦN B: Sửa dữ liệu tiếng Việt bị mã hóa sai (UTF-8 → Latin-1)
--   PHẦN C: Tạo trigger tự động chuẩn hóa HinhThucThanhToan & TrangThaiThanhToan
-- Lưu ý: Script an toàn, chạy nhiều lần không gây lỗi.
--         Không xóa, không rename bất kỳ cột/bảng nào.
-- =============================================
-- ========================================
-- PHẦN A: ĐẢM BẢO CÁC CỘT THU NGÂN TỒN TẠI
-- ========================================

-- 1. Hình thức thanh toán: Tiền mặt, Chuyển khoản, Thẻ, Ví điện tử
IF COL_LENGTH(N'dbo.HoaDonBan', N'HinhThucThanhToan') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD HinhThucThanhToan NVARCHAR(50) NULL
        CONSTRAINT DF_HoaDonBan_HinhThucThanhToan DEFAULT N'Tiền mặt';
    PRINT N'✅ Đã thêm cột HinhThucThanhToan';
END
ELSE
    PRINT N'ℹ️ Cột HinhThucThanhToan đã tồn tại';
GO

-- 2. Trạng thái thanh toán: Đã thanh toán, Chưa thanh toán, Đã hủy
IF COL_LENGTH(N'dbo.HoaDonBan', N'TrangThaiThanhToan') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD TrangThaiThanhToan NVARCHAR(50) NULL
        CONSTRAINT DF_HoaDonBan_TrangThaiThanhToan DEFAULT N'Đã thanh toán';
    PRINT N'✅ Đã thêm cột TrangThaiThanhToan';
END
ELSE
    PRINT N'ℹ️ Cột TrangThaiThanhToan đã tồn tại';
GO

-- 3. Tiền khách đưa (chỉ áp dụng khi thanh toán tiền mặt)
IF COL_LENGTH(N'dbo.HoaDonBan', N'TienKhachDua') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD TienKhachDua DECIMAL(18,2) NULL;
    PRINT N'✅ Đã thêm cột TienKhachDua';
END
ELSE
    PRINT N'ℹ️ Cột TienKhachDua đã tồn tại';
GO

-- 4. Tiền thối lại (= TienKhachDua - ThanhToan)
IF COL_LENGTH(N'dbo.HoaDonBan', N'TienThoiLai') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD TienThoiLai DECIMAL(18,2) NULL;
    PRINT N'✅ Đã thêm cột TienThoiLai';
END
ELSE
    PRINT N'ℹ️ Cột TienThoiLai đã tồn tại';
GO

-- 5. Mã giao dịch (dùng cho chuyển khoản, thẻ, ví điện tử)
IF COL_LENGTH(N'dbo.HoaDonBan', N'MaGiaoDich') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD MaGiaoDich NVARCHAR(100) NULL;
    PRINT N'✅ Đã thêm cột MaGiaoDich';
END
ELSE
    PRINT N'ℹ️ Cột MaGiaoDich đã tồn tại';
GO

-- 6. Ghi chú thanh toán
IF COL_LENGTH(N'dbo.HoaDonBan', N'GhiChuThanhToan') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD GhiChuThanhToan NVARCHAR(255) NULL;
    PRINT N'✅ Đã thêm cột GhiChuThanhToan';
END
ELSE
    PRINT N'ℹ️ Cột GhiChuThanhToan đã tồn tại';
GO

-- 7. Ghi chú hóa đơn (ghi chú chung cho hóa đơn)
IF COL_LENGTH(N'dbo.HoaDonBan', N'GhiChuHoaDon') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD GhiChuHoaDon NVARCHAR(255) NULL;
    PRINT N'✅ Đã thêm cột GhiChuHoaDon';
END
ELSE
    PRINT N'ℹ️ Cột GhiChuHoaDon đã tồn tại';
GO

-- 8. Lý do hủy hóa đơn
IF COL_LENGTH(N'dbo.HoaDonBan', N'LyDoHuy') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD LyDoHuy NVARCHAR(255) NULL;
    PRINT N'✅ Đã thêm cột LyDoHuy';
END
ELSE
    PRINT N'ℹ️ Cột LyDoHuy đã tồn tại';
GO

-- 9. Người thực hiện hủy hóa đơn
IF COL_LENGTH(N'dbo.HoaDonBan', N'NguoiHuy') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD NguoiHuy NVARCHAR(50) NULL;
    PRINT N'✅ Đã thêm cột NguoiHuy';
END
ELSE
    PRINT N'ℹ️ Cột NguoiHuy đã tồn tại';
GO

-- 10. Ngày hủy hóa đơn
IF COL_LENGTH(N'dbo.HoaDonBan', N'NgayHuy') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD NgayHuy DATETIME NULL;
    PRINT N'✅ Đã thêm cột NgayHuy';
END
ELSE
    PRINT N'ℹ️ Cột NgayHuy đã tồn tại';
GO

-- ========================================
-- PHẦN B: SỬA DỮ LIỆU TIẾNG VIỆT BỊ LỖI MÃ HÓA
-- Chỉ update dòng có giá trị bị lỗi, không ảnh hưởng dòng đã đúng.
-- ========================================

PRINT N'';
PRINT N'=== Bắt đầu sửa lỗi encoding tiếng Việt ===';

-- --- Sửa HinhThucThanhToan ---

-- Tiền mặt
UPDATE dbo.HoaDonBan
SET HinhThucThanhToan = N'Tiền mặt'
WHERE HinhThucThanhToan IN (
    N'Tiá»n máº·t',
    N'Tiá»n mạt',
    N'Tiá»n mặt'
);
PRINT N'  Sửa HinhThucThanhToan → Tiền mặt: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Thẻ
UPDATE dbo.HoaDonBan
SET HinhThucThanhToan = N'Thẻ'
WHERE HinhThucThanhToan IN (
    N'Tháº»',
    N'Tháº',
    N'Tháº½'
);
PRINT N'  Sửa HinhThucThanhToan → Thẻ: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Chuyển khoản
UPDATE dbo.HoaDonBan
SET HinhThucThanhToan = N'Chuyển khoản'
WHERE HinhThucThanhToan IN (
    N'Chuyá»ƒn khoáº£n',
    N'Chuyá»ƒn khoản',
    N'Chuyển khoáº£n'
);
PRINT N'  Sửa HinhThucThanhToan → Chuyển khoản: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Ví điện tử
UPDATE dbo.HoaDonBan
SET HinhThucThanhToan = N'Ví điện tử'
WHERE HinhThucThanhToan IN (
    N'VÃ Äiá»‡n tá»',
    N'VÃ điện tử',
    N'Ví Äiá»‡n tử'
);
PRINT N'  Sửa HinhThucThanhToan → Ví điện tử: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Dữ liệu cũ NULL → mặc định Tiền mặt
UPDATE dbo.HoaDonBan
SET HinhThucThanhToan = N'Tiền mặt'
WHERE HinhThucThanhToan IS NULL;
PRINT N'  HinhThucThanhToan NULL → Tiền mặt: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- --- Sửa TrangThaiThanhToan ---

-- Đã thanh toán
UPDATE dbo.HoaDonBan
SET TrangThaiThanhToan = N'Đã thanh toán'
WHERE TrangThaiThanhToan IN (
    N'ÄÃ£ thanh toÃ¡n',
    N'ĐÃ£ thanh toán',
    N'Äã thanh toán'
);
PRINT N'  Sửa TrangThaiThanhToan → Đã thanh toán: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Đã hủy
UPDATE dbo.HoaDonBan
SET TrangThaiThanhToan = N'Đã hủy'
WHERE TrangThaiThanhToan IN (
    N'ÄÃ£ há»§y',
    N'ĐÃ£ hủy',
    N'Äã hủy'
);
PRINT N'  Sửa TrangThaiThanhToan → Đã hủy: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Chưa thanh toán
UPDATE dbo.HoaDonBan
SET TrangThaiThanhToan = N'Chưa thanh toán'
WHERE TrangThaiThanhToan IN (
    N'ChÆ°a thanh toÃ¡n',
    N'Chưa thanh toÃ¡n',
    N'ChÆ°a thanh toán'
);
PRINT N'  Sửa TrangThaiThanhToan → Chưa thanh toán: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Dữ liệu cũ NULL → mặc định Đã thanh toán
UPDATE dbo.HoaDonBan
SET TrangThaiThanhToan = N'Đã thanh toán'
WHERE TrangThaiThanhToan IS NULL;
PRINT N'  TrangThaiThanhToan NULL → Đã thanh toán: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

PRINT N'=== Hoàn thành sửa lỗi encoding ===';
GO

-- ========================================
-- PHẦN C: TRIGGER TỰ ĐỘNG CHUẨN HÓA TIẾNG VIỆT
-- Tự động sửa HinhThucThanhToan & TrangThaiThanhToan
-- khi INSERT hoặc UPDATE vào bảng HoaDonBan.
-- Đảm bảo dữ liệu mới luôn đúng tiếng Việt.
-- ========================================

PRINT N'';
PRINT N'=== Tạo trigger chuẩn hóa tiếng Việt ===';
GO

-- Trigger INSERT: tự động sửa encoding khi thêm mới hóa đơn
IF OBJECT_ID(N'dbo.TR_HoaDonBan_NormalizeVietnamese_Insert', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_HoaDonBan_NormalizeVietnamese_Insert;
GO

CREATE TRIGGER dbo.TR_HoaDonBan_NormalizeVietnamese_Insert
ON dbo.HoaDonBan
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Chuẩn hóa HinhThucThanhToan
    UPDATE h
    SET h.HinhThucThanhToan = 
        CASE
            -- NULL hoặc rỗng → Tiền mặt
            WHEN h.HinhThucThanhToan IS NULL OR LTRIM(RTRIM(h.HinhThucThanhToan)) = N'' 
                THEN N'Tiền mặt'
            -- Các dạng encoding lỗi của "Tiền mặt"
            WHEN h.HinhThucThanhToan IN (N'Tiá»n máº·t', N'Tiá»n mạt', N'Tiá»n mặt')
                THEN N'Tiền mặt'
            -- Các dạng encoding lỗi của "Thẻ"
            WHEN h.HinhThucThanhToan IN (N'Tháº»', N'Tháº', N'Tháº½')
                THEN N'Thẻ'
            -- Các dạng encoding lỗi của "Chuyển khoản"
            WHEN h.HinhThucThanhToan IN (N'Chuyá»ƒn khoáº£n', N'Chuyá»ƒn khoản', N'Chuyển khoáº£n')
                THEN N'Chuyển khoản'
            -- Các dạng encoding lỗi của "Ví điện tử"
            WHEN h.HinhThucThanhToan IN (N'VÃ Ä''iá»‡n tá»', N'VÃ điện tử', N'Ví Ä''iá»‡n tử')
                THEN N'Ví điện tử'
            -- Giữ nguyên nếu đã đúng
            ELSE h.HinhThucThanhToan
        END
    FROM dbo.HoaDonBan h
    INNER JOIN inserted i ON h.HoaDonBanId = i.HoaDonBanId
    WHERE h.HinhThucThanhToan IS NULL
       OR h.HinhThucThanhToan NOT IN (N'Tiền mặt', N'Chuyển khoản', N'Thẻ', N'Ví điện tử');

    -- Chuẩn hóa TrangThaiThanhToan
    UPDATE h
    SET h.TrangThaiThanhToan = 
        CASE
            -- NULL hoặc rỗng → Đã thanh toán
            WHEN h.TrangThaiThanhToan IS NULL OR LTRIM(RTRIM(h.TrangThaiThanhToan)) = N''
                THEN N'Đã thanh toán'
            -- Các dạng encoding lỗi của "Đã thanh toán"
            WHEN h.TrangThaiThanhToan IN (N'ÄÃ£ thanh toÃ¡n', N'ĐÃ£ thanh toán', N'Äã thanh toán')
                THEN N'Đã thanh toán'
            -- Các dạng encoding lỗi của "Đã hủy"
            WHEN h.TrangThaiThanhToan IN (N'ÄÃ£ há»§y', N'ĐÃ£ hủy', N'Äã hủy')
                THEN N'Đã hủy'
            -- Các dạng encoding lỗi của "Chưa thanh toán"
            WHEN h.TrangThaiThanhToan IN (N'ChÆ°a thanh toÃ¡n', N'Chưa thanh toÃ¡n', N'ChÆ°a thanh toán')
                THEN N'Chưa thanh toán'
            -- Giữ nguyên nếu đã đúng
            ELSE h.TrangThaiThanhToan
        END
    FROM dbo.HoaDonBan h
    INNER JOIN inserted i ON h.HoaDonBanId = i.HoaDonBanId
    WHERE h.TrangThaiThanhToan IS NULL
       OR h.TrangThaiThanhToan NOT IN (N'Đã thanh toán', N'Chưa thanh toán', N'Đã hủy');
END;
GO

PRINT N'✅ Đã tạo trigger TR_HoaDonBan_NormalizeVietnamese_Insert';
GO

-- Trigger UPDATE: tự động sửa encoding khi cập nhật hóa đơn
IF OBJECT_ID(N'dbo.TR_HoaDonBan_NormalizeVietnamese_Update', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_HoaDonBan_NormalizeVietnamese_Update;
GO

CREATE TRIGGER dbo.TR_HoaDonBan_NormalizeVietnamese_Update
ON dbo.HoaDonBan
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Chỉ chạy nếu HinhThucThanhToan hoặc TrangThaiThanhToan bị thay đổi
    IF NOT (UPDATE(HinhThucThanhToan) OR UPDATE(TrangThaiThanhToan))
        RETURN;

    -- Chuẩn hóa HinhThucThanhToan nếu bị thay đổi
    IF UPDATE(HinhThucThanhToan)
    BEGIN
        UPDATE h
        SET h.HinhThucThanhToan = 
            CASE
                WHEN h.HinhThucThanhToan IS NULL OR LTRIM(RTRIM(h.HinhThucThanhToan)) = N'' 
                    THEN N'Tiền mặt'
                WHEN h.HinhThucThanhToan IN (N'Tiá»n máº·t', N'Tiá»n mạt', N'Tiá»n mặt')
                    THEN N'Tiền mặt'
                WHEN h.HinhThucThanhToan IN (N'Tháº»', N'Tháº', N'Tháº½')
                    THEN N'Thẻ'
                WHEN h.HinhThucThanhToan IN (N'Chuyá»ƒn khoáº£n', N'Chuyá»ƒn khoản', N'Chuyển khoáº£n')
                    THEN N'Chuyển khoản'
                WHEN h.HinhThucThanhToan IN (N'VÃ Ä''iá»‡n tá»', N'VÃ điện tử', N'Ví Ä''iá»‡n tử')
                    THEN N'Ví điện tử'
                ELSE h.HinhThucThanhToan
            END
        FROM dbo.HoaDonBan h
        INNER JOIN inserted i ON h.HoaDonBanId = i.HoaDonBanId
        WHERE h.HinhThucThanhToan IS NULL
           OR h.HinhThucThanhToan NOT IN (N'Tiền mặt', N'Chuyển khoản', N'Thẻ', N'Ví điện tử');
    END

    -- Chuẩn hóa TrangThaiThanhToan nếu bị thay đổi
    IF UPDATE(TrangThaiThanhToan)
    BEGIN
        UPDATE h
        SET h.TrangThaiThanhToan = 
            CASE
                WHEN h.TrangThaiThanhToan IS NULL OR LTRIM(RTRIM(h.TrangThaiThanhToan)) = N''
                    THEN N'Đã thanh toán'
                WHEN h.TrangThaiThanhToan IN (N'ÄÃ£ thanh toÃ¡n', N'ĐÃ£ thanh toán', N'Äã thanh toán')
                    THEN N'Đã thanh toán'
                WHEN h.TrangThaiThanhToan IN (N'ÄÃ£ há»§y', N'ĐÃ£ hủy', N'Äã hủy')
                    THEN N'Đã hủy'
                WHEN h.TrangThaiThanhToan IN (N'ChÆ°a thanh toÃ¡n', N'Chưa thanh toÃ¡n', N'ChÆ°a thanh toán')
                    THEN N'Chưa thanh toán'
                ELSE h.TrangThaiThanhToan
            END
        FROM dbo.HoaDonBan h
        INNER JOIN inserted i ON h.HoaDonBanId = i.HoaDonBanId
        WHERE h.TrangThaiThanhToan IS NULL
           OR h.TrangThaiThanhToan NOT IN (N'Đã thanh toán', N'Chưa thanh toán', N'Đã hủy');
    END
END;
GO

PRINT N'✅ Đã tạo trigger TR_HoaDonBan_NormalizeVietnamese_Update';
GO

-- ========================================
-- TỔNG KẾT
-- ========================================
PRINT N'';
PRINT N'============================================';
PRINT N'✅ Hoàn thành migration nâng cấp thu ngân';
PRINT N'============================================';
PRINT N'Các cột đã thêm vào bảng HoaDonBan:';
PRINT N'  - HinhThucThanhToan (Tiền mặt / Chuyển khoản / Thẻ / Ví điện tử)';
PRINT N'  - TrangThaiThanhToan (Đã thanh toán / Chưa thanh toán / Đã hủy)';
PRINT N'  - TienKhachDua, TienThoiLai';
PRINT N'  - MaGiaoDich, GhiChuThanhToan, GhiChuHoaDon';
PRINT N'  - LyDoHuy, NguoiHuy, NgayHuy';
PRINT N'Dữ liệu tiếng Việt bị lỗi encoding đã được sửa.';
PRINT N'Trigger tự động chuẩn hóa đã được tạo:';
PRINT N'  - TR_HoaDonBan_NormalizeVietnamese_Insert';
PRINT N'  - TR_HoaDonBan_NormalizeVietnamese_Update';
GO


-- ============================================================
-- PHẦN 11
-- Source: 11_AddSoGoiMon.sql
-- ============================================================
IF COL_LENGTH(N'dbo.HoaDonBan', N'SoThuTuGoiMon') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD SoThuTuGoiMon INT NULL;
END
GO

IF COL_LENGTH(N'dbo.HoaDonBan', N'NgaySoThuTu') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD NgaySoThuTu DATE NULL;
END
GO

UPDATE dbo.HoaDonBan
SET NgaySoThuTu = CAST(NgayBan AS DATE)
WHERE NgaySoThuTu IS NULL;
GO

;WITH Ranked AS
(
    SELECT 
        HoaDonBanId,
        ROW_NUMBER() OVER (
            PARTITION BY CAST(NgayBan AS DATE)
            ORDER BY NgayBan, HoaDonBanId
        ) AS SoMoi
    FROM dbo.HoaDonBan
    WHERE SoThuTuGoiMon IS NULL
)
UPDATE hb
SET hb.SoThuTuGoiMon = r.SoMoi
FROM dbo.HoaDonBan hb
JOIN Ranked r ON r.HoaDonBanId = hb.HoaDonBanId;
GO

IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = N'UX_HoaDonBan_NgaySoThuTu_SoThuTuGoiMon'
      AND object_id = OBJECT_ID(N'dbo.HoaDonBan')
)
BEGIN
    CREATE UNIQUE INDEX UX_HoaDonBan_NgaySoThuTu_SoThuTuGoiMon
    ON dbo.HoaDonBan(NgaySoThuTu, SoThuTuGoiMon)
    WHERE NgaySoThuTu IS NOT NULL AND SoThuTuGoiMon IS NOT NULL;
END
GO


-- ============================================================
-- PHẦN 12
-- Source: 12_AddTrangThaiPhaChe.sql
-- ============================================================
-- 1. Thêm cột TrangThaiPhaChe
IF COL_LENGTH(N'dbo.HoaDonBan', N'TrangThaiPhaChe') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD TrangThaiPhaChe NVARCHAR(30) NULL;
END
GO

-- 2. Thêm cột thời gian pha chế
IF COL_LENGTH(N'dbo.HoaDonBan', N'ThoiGianBatDauPhaChe') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD ThoiGianBatDauPhaChe DATETIME2 NULL;
END
GO

IF COL_LENGTH(N'dbo.HoaDonBan', N'ThoiGianHoanThanhPhaChe') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD ThoiGianHoanThanhPhaChe DATETIME2 NULL;
END
GO

IF COL_LENGTH(N'dbo.HoaDonBan', N'ThoiGianGiaoKhach') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD ThoiGianGiaoKhach DATETIME2 NULL;
END
GO

-- 3. Backfill dữ liệu cũ
UPDATE dbo.HoaDonBan
SET TrangThaiPhaChe = N'DaHuy'
WHERE TrangThaiPhaChe IS NULL
  AND TrangThaiThanhToan = N'Đã hủy';
GO

UPDATE dbo.HoaDonBan
SET TrangThaiPhaChe = N'DaGiaoKhach'
WHERE TrangThaiPhaChe IS NULL;
GO

-- 4. Default cho hóa đơn mới
IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.HoaDonBan')
      AND name = N'DF_HoaDonBan_TrangThaiPhaChe'
)
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT DF_HoaDonBan_TrangThaiPhaChe
    DEFAULT N'ChoPhaChe' FOR TrangThaiPhaChe;
END
GO

-- 5. Check constraint
IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.HoaDonBan')
      AND name = N'CK_HoaDonBan_TrangThaiPhaChe'
)
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT CK_HoaDonBan_TrangThaiPhaChe
    CHECK (TrangThaiPhaChe IN (
        N'ChoPhaChe',
        N'DangPhaChe',
        N'DaHoanThanh',
        N'DaGiaoKhach',
        N'DaHuy'
    ));
END
GO

-- 6. Index theo TrangThaiPhaChe, NgayBan
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_HoaDonBan_TrangThaiPhaChe_NgayBan'
      AND object_id = OBJECT_ID(N'dbo.HoaDonBan')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_HoaDonBan_TrangThaiPhaChe_NgayBan
    ON dbo.HoaDonBan(TrangThaiPhaChe, NgayBan);
END
GO


-- ============================================================
-- PHẦN 13
-- Source: 13_AddSizeAndNoteToChiTietHoaDonBan.sql
-- ============================================================
-- 1. Thêm cột KichCo (Size đồ uống)
IF COL_LENGTH(N'dbo.ChiTietHoaDonBan', N'KichCo') IS NULL
BEGIN
    ALTER TABLE dbo.ChiTietHoaDonBan ADD KichCo NVARCHAR(20) NULL;
END
GO

-- 2. Thêm cột PhuThuKichCo (phụ thu theo size)
IF COL_LENGTH(N'dbo.ChiTietHoaDonBan', N'PhuThuKichCo') IS NULL
BEGIN
    ALTER TABLE dbo.ChiTietHoaDonBan ADD PhuThuKichCo DECIMAL(18,2) NOT NULL DEFAULT(0);
END
GO

-- 3. Thêm cột GhiChuMon (ghi chú riêng từng món)
IF COL_LENGTH(N'dbo.ChiTietHoaDonBan', N'GhiChuMon') IS NULL
BEGIN
    ALTER TABLE dbo.ChiTietHoaDonBan ADD GhiChuMon NVARCHAR(255) NULL;
END
GO


-- ============================================================
-- PHẦN 15
-- Source: 15_AddHinhThucPhucVuToHoaDonBan.sql
-- ============================================================
-- =============================================
-- 15_AddHinhThucPhucVuToHoaDonBan.sql
-- Thêm cột HinhThucPhucVu: UongTaiQuan / MangDi
-- Đồng thời đảm bảo BanId nullable
-- =============================================
-- 1. Thêm cột HinhThucPhucVu nếu chưa có
IF COL_LENGTH('dbo.HoaDonBan', 'HinhThucPhucVu') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD HinhThucPhucVu NVARCHAR(30) NOT NULL
        CONSTRAINT DF_HoaDonBan_HinhThucPhucVu DEFAULT(N'UongTaiQuan');

    PRINT N'Đã thêm cột HinhThucPhucVu.';
END
ELSE
BEGIN
    PRINT N'Cột HinhThucPhucVu đã tồn tại, bỏ qua.';
END
GO

-- 2. Thêm check constraint nếu chưa có
IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_HoaDonBan_HinhThucPhucVu'
)
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT CK_HoaDonBan_HinhThucPhucVu
        CHECK (HinhThucPhucVu IN (N'UongTaiQuan', N'MangDi'));

    PRINT N'Đã thêm check constraint CK_HoaDonBan_HinhThucPhucVu.';
END
GO

-- 3. Đảm bảo BanId nullable
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'HoaDonBan'
      AND COLUMN_NAME = 'BanId'
      AND IS_NULLABLE = 'NO'
)
BEGIN
    ALTER TABLE dbo.HoaDonBan ALTER COLUMN BanId INT NULL;
    PRINT N'Đã đổi HoaDonBan.BanId thành NULL.';
END
GO


-- ============================================================
-- PHẦN 18
-- Source: 18_HoaDonBan_DiemSuDung.sql
-- ============================================================
-- =============================================
-- Migration: Thêm chức năng dùng điểm tích lũy để giảm tiền
-- File: 18_HoaDonBan_DiemSuDung.sql
-- Mô tả: Thêm cột DiemSuDung và SoTienGiamTuDiem vào bảng HoaDonBan
-- =============================================
-- Kiểm tra và thêm cột DiemSuDung nếu chưa có
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.HoaDonBan') 
    AND name = 'DiemSuDung'
)
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD DiemSuDung INT NOT NULL DEFAULT(0);
    
    PRINT 'Đã thêm cột DiemSuDung vào bảng HoaDonBan';
END
ELSE
BEGIN
    PRINT 'Cột DiemSuDung đã tồn tại trong bảng HoaDonBan';
END
GO

-- Kiểm tra và thêm cột SoTienGiamTuDiem nếu chưa có
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.HoaDonBan') 
    AND name = 'SoTienGiamTuDiem'
)
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD SoTienGiamTuDiem DECIMAL(18,2) NOT NULL DEFAULT(0);
    
    PRINT 'Đã thêm cột SoTienGiamTuDiem vào bảng HoaDonBan';
END
ELSE
BEGIN
    PRINT 'Cột SoTienGiamTuDiem đã tồn tại trong bảng HoaDonBan';
END
GO

-- Thêm check constraint cho DiemSuDung
IF NOT EXISTS (
    SELECT 1 
    FROM sys.check_constraints 
    WHERE name = 'CK_HoaDonBan_DiemSuDung_NonNegative'
)
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT CK_HoaDonBan_DiemSuDung_NonNegative 
    CHECK (DiemSuDung >= 0);
    
    PRINT 'Đã thêm constraint CK_HoaDonBan_DiemSuDung_NonNegative';
END
ELSE
BEGIN
    PRINT 'Constraint CK_HoaDonBan_DiemSuDung_NonNegative đã tồn tại';
END
GO

-- Thêm check constraint cho SoTienGiamTuDiem
IF NOT EXISTS (
    SELECT 1 
    FROM sys.check_constraints 
    WHERE name = 'CK_HoaDonBan_SoTienGiamTuDiem_NonNegative'
)
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT CK_HoaDonBan_SoTienGiamTuDiem_NonNegative 
    CHECK (SoTienGiamTuDiem >= 0);
    
    PRINT 'Đã thêm constraint CK_HoaDonBan_SoTienGiamTuDiem_NonNegative';
END
ELSE
BEGIN
    PRINT 'Constraint CK_HoaDonBan_SoTienGiamTuDiem đã tồn tại';
END
GO

PRINT 'Migration 18_HoaDonBan_DiemSuDung.sql hoàn tất!';
GO

-- =============================================
-- Script: Nâng cấp nghiệp vụ thu ngân (PHIÊN BẢN ĐẦY ĐỦ)
-- Mô tả:
--   PHẦN A: Thêm các cột thu ngân vào bảng HoaDonBan
--   PHẦN B: Sửa dữ liệu tiếng Việt bị mã hóa sai (UTF-8 → Latin-1)
--   PHẦN C: Tạo trigger tự động chuẩn hóa HinhThucThanhToan & TrangThaiThanhToan
-- Lưu ý: Script an toàn, chạy nhiều lần không gây lỗi.
--         Không xóa, không rename bất kỳ cột/bảng nào.
-- =============================================
-- ========================================
-- PHẦN A: ĐẢM BẢO CÁC CỘT THU NGÂN TỒN TẠI
-- ========================================

-- 1. Hình thức thanh toán: Tiền mặt, Chuyển khoản, Thẻ, Ví điện tử
IF COL_LENGTH(N'dbo.HoaDonBan', N'HinhThucThanhToan') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD HinhThucThanhToan NVARCHAR(50) NULL
        CONSTRAINT DF_HoaDonBan_HinhThucThanhToan DEFAULT N'Tiền mặt';
    PRINT N'✅ Đã thêm cột HinhThucThanhToan';
END
ELSE
    PRINT N'ℹ️ Cột HinhThucThanhToan đã tồn tại';
GO

-- 2. Trạng thái thanh toán: Đã thanh toán, Chưa thanh toán, Đã hủy
IF COL_LENGTH(N'dbo.HoaDonBan', N'TrangThaiThanhToan') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD TrangThaiThanhToan NVARCHAR(50) NULL
        CONSTRAINT DF_HoaDonBan_TrangThaiThanhToan DEFAULT N'Đã thanh toán';
    PRINT N'✅ Đã thêm cột TrangThaiThanhToan';
END
ELSE
    PRINT N'ℹ️ Cột TrangThaiThanhToan đã tồn tại';
GO

-- 3. Tiền khách đưa (chỉ áp dụng khi thanh toán tiền mặt)
IF COL_LENGTH(N'dbo.HoaDonBan', N'TienKhachDua') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD TienKhachDua DECIMAL(18,2) NULL;
    PRINT N'✅ Đã thêm cột TienKhachDua';
END
ELSE
    PRINT N'ℹ️ Cột TienKhachDua đã tồn tại';
GO

-- 4. Tiền thối lại (= TienKhachDua - ThanhToan)
IF COL_LENGTH(N'dbo.HoaDonBan', N'TienThoiLai') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD TienThoiLai DECIMAL(18,2) NULL;
    PRINT N'✅ Đã thêm cột TienThoiLai';
END
ELSE
    PRINT N'ℹ️ Cột TienThoiLai đã tồn tại';
GO

-- 5. Mã giao dịch (dùng cho chuyển khoản, thẻ, ví điện tử)
IF COL_LENGTH(N'dbo.HoaDonBan', N'MaGiaoDich') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD MaGiaoDich NVARCHAR(100) NULL;
    PRINT N'✅ Đã thêm cột MaGiaoDich';
END
ELSE
    PRINT N'ℹ️ Cột MaGiaoDich đã tồn tại';
GO

-- 6. Ghi chú thanh toán
IF COL_LENGTH(N'dbo.HoaDonBan', N'GhiChuThanhToan') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD GhiChuThanhToan NVARCHAR(255) NULL;
    PRINT N'✅ Đã thêm cột GhiChuThanhToan';
END
ELSE
    PRINT N'ℹ️ Cột GhiChuThanhToan đã tồn tại';
GO

-- 7. Ghi chú hóa đơn (ghi chú chung cho hóa đơn)
IF COL_LENGTH(N'dbo.HoaDonBan', N'GhiChuHoaDon') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD GhiChuHoaDon NVARCHAR(255) NULL;
    PRINT N'✅ Đã thêm cột GhiChuHoaDon';
END
ELSE
    PRINT N'ℹ️ Cột GhiChuHoaDon đã tồn tại';
GO

-- 8. Lý do hủy hóa đơn
IF COL_LENGTH(N'dbo.HoaDonBan', N'LyDoHuy') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD LyDoHuy NVARCHAR(255) NULL;
    PRINT N'✅ Đã thêm cột LyDoHuy';
END
ELSE
    PRINT N'ℹ️ Cột LyDoHuy đã tồn tại';
GO

-- 9. Người thực hiện hủy hóa đơn
IF COL_LENGTH(N'dbo.HoaDonBan', N'NguoiHuy') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD NguoiHuy NVARCHAR(50) NULL;
    PRINT N'✅ Đã thêm cột NguoiHuy';
END
ELSE
    PRINT N'ℹ️ Cột NguoiHuy đã tồn tại';
GO

-- 10. Ngày hủy hóa đơn
IF COL_LENGTH(N'dbo.HoaDonBan', N'NgayHuy') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan ADD NgayHuy DATETIME NULL;
    PRINT N'✅ Đã thêm cột NgayHuy';
END
ELSE
    PRINT N'ℹ️ Cột NgayHuy đã tồn tại';
GO

-- ========================================
-- PHẦN B: SỬA DỮ LIỆU TIẾNG VIỆT BỊ LỖI MÃ HÓA
-- Chỉ update dòng có giá trị bị lỗi, không ảnh hưởng dòng đã đúng.
-- ========================================

PRINT N'';
PRINT N'=== Bắt đầu sửa lỗi encoding tiếng Việt ===';

-- --- Sửa HinhThucThanhToan ---

-- Tiền mặt
UPDATE dbo.HoaDonBan
SET HinhThucThanhToan = N'Tiền mặt'
WHERE HinhThucThanhToan IN (
    N'Tiá»n máº·t',
    N'Tiá»n mạt',
    N'Tiá»n mặt'
);
PRINT N'  Sửa HinhThucThanhToan → Tiền mặt: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Thẻ
UPDATE dbo.HoaDonBan
SET HinhThucThanhToan = N'Thẻ'
WHERE HinhThucThanhToan IN (
    N'Tháº»',
    N'Tháº',
    N'Tháº½'
);
PRINT N'  Sửa HinhThucThanhToan → Thẻ: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Chuyển khoản
UPDATE dbo.HoaDonBan
SET HinhThucThanhToan = N'Chuyển khoản'
WHERE HinhThucThanhToan IN (
    N'Chuyá»ƒn khoáº£n',
    N'Chuyá»ƒn khoản',
    N'Chuyển khoáº£n'
);
PRINT N'  Sửa HinhThucThanhToan → Chuyển khoản: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Ví điện tử
UPDATE dbo.HoaDonBan
SET HinhThucThanhToan = N'Ví điện tử'
WHERE HinhThucThanhToan IN (
    N'VÃ Äiá»‡n tá»',
    N'VÃ điện tử',
    N'Ví Äiá»‡n tử'
);
PRINT N'  Sửa HinhThucThanhToan → Ví điện tử: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Dữ liệu cũ NULL → mặc định Tiền mặt
UPDATE dbo.HoaDonBan
SET HinhThucThanhToan = N'Tiền mặt'
WHERE HinhThucThanhToan IS NULL;
PRINT N'  HinhThucThanhToan NULL → Tiền mặt: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- --- Sửa TrangThaiThanhToan ---

-- Đã thanh toán
UPDATE dbo.HoaDonBan
SET TrangThaiThanhToan = N'Đã thanh toán'
WHERE TrangThaiThanhToan IN (
    N'ÄÃ£ thanh toÃ¡n',
    N'ĐÃ£ thanh toán',
    N'Äã thanh toán'
);
PRINT N'  Sửa TrangThaiThanhToan → Đã thanh toán: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Đã hủy
UPDATE dbo.HoaDonBan
SET TrangThaiThanhToan = N'Đã hủy'
WHERE TrangThaiThanhToan IN (
    N'ÄÃ£ há»§y',
    N'ĐÃ£ hủy',
    N'Äã hủy'
);
PRINT N'  Sửa TrangThaiThanhToan → Đã hủy: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Chưa thanh toán
UPDATE dbo.HoaDonBan
SET TrangThaiThanhToan = N'Chưa thanh toán'
WHERE TrangThaiThanhToan IN (
    N'ChÆ°a thanh toÃ¡n',
    N'Chưa thanh toÃ¡n',
    N'ChÆ°a thanh toán'
);
PRINT N'  Sửa TrangThaiThanhToan → Chưa thanh toán: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

-- Dữ liệu cũ NULL → mặc định Đã thanh toán
UPDATE dbo.HoaDonBan
SET TrangThaiThanhToan = N'Đã thanh toán'
WHERE TrangThaiThanhToan IS NULL;
PRINT N'  TrangThaiThanhToan NULL → Đã thanh toán: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng';

PRINT N'=== Hoàn thành sửa lỗi encoding ===';
GO

-- ========================================
-- PHẦN C: TRIGGER TỰ ĐỘNG CHUẨN HÓA TIẾNG VIỆT
-- Tự động sửa HinhThucThanhToan & TrangThaiThanhToan
-- khi INSERT hoặc UPDATE vào bảng HoaDonBan.
-- Đảm bảo dữ liệu mới luôn đúng tiếng Việt.
-- ========================================

PRINT N'';
PRINT N'=== Tạo trigger chuẩn hóa tiếng Việt ===';
GO

-- Trigger INSERT: tự động sửa encoding khi thêm mới hóa đơn
IF OBJECT_ID(N'dbo.TR_HoaDonBan_NormalizeVietnamese_Insert', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_HoaDonBan_NormalizeVietnamese_Insert;
GO

CREATE TRIGGER dbo.TR_HoaDonBan_NormalizeVietnamese_Insert
ON dbo.HoaDonBan
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Chuẩn hóa HinhThucThanhToan
    UPDATE h
    SET h.HinhThucThanhToan = 
        CASE
            -- NULL hoặc rỗng → Tiền mặt
            WHEN h.HinhThucThanhToan IS NULL OR LTRIM(RTRIM(h.HinhThucThanhToan)) = N'' 
                THEN N'Tiền mặt'
            -- Các dạng encoding lỗi của "Tiền mặt"
            WHEN h.HinhThucThanhToan IN (N'Tiá»n máº·t', N'Tiá»n mạt', N'Tiá»n mặt')
                THEN N'Tiền mặt'
            -- Các dạng encoding lỗi của "Thẻ"
            WHEN h.HinhThucThanhToan IN (N'Tháº»', N'Tháº', N'Tháº½')
                THEN N'Thẻ'
            -- Các dạng encoding lỗi của "Chuyển khoản"
            WHEN h.HinhThucThanhToan IN (N'Chuyá»ƒn khoáº£n', N'Chuyá»ƒn khoản', N'Chuyển khoáº£n')
                THEN N'Chuyển khoản'
            -- Các dạng encoding lỗi của "Ví điện tử"
            WHEN h.HinhThucThanhToan IN (N'VÃ Ä''iá»‡n tá»', N'VÃ điện tử', N'Ví Ä''iá»‡n tử')
                THEN N'Ví điện tử'
            -- Giữ nguyên nếu đã đúng
            ELSE h.HinhThucThanhToan
        END
    FROM dbo.HoaDonBan h
    INNER JOIN inserted i ON h.HoaDonBanId = i.HoaDonBanId
    WHERE h.HinhThucThanhToan IS NULL
       OR h.HinhThucThanhToan NOT IN (N'Tiền mặt', N'Chuyển khoản', N'Thẻ', N'Ví điện tử');

    -- Chuẩn hóa TrangThaiThanhToan
    UPDATE h
    SET h.TrangThaiThanhToan = 
        CASE
            -- NULL hoặc rỗng → Đã thanh toán
            WHEN h.TrangThaiThanhToan IS NULL OR LTRIM(RTRIM(h.TrangThaiThanhToan)) = N''
                THEN N'Đã thanh toán'
            -- Các dạng encoding lỗi của "Đã thanh toán"
            WHEN h.TrangThaiThanhToan IN (N'ÄÃ£ thanh toÃ¡n', N'ĐÃ£ thanh toán', N'Äã thanh toán')
                THEN N'Đã thanh toán'
            -- Các dạng encoding lỗi của "Đã hủy"
            WHEN h.TrangThaiThanhToan IN (N'ÄÃ£ há»§y', N'ĐÃ£ hủy', N'Äã hủy')
                THEN N'Đã hủy'
            -- Các dạng encoding lỗi của "Chưa thanh toán"
            WHEN h.TrangThaiThanhToan IN (N'ChÆ°a thanh toÃ¡n', N'Chưa thanh toÃ¡n', N'ChÆ°a thanh toán')
                THEN N'Chưa thanh toán'
            -- Giữ nguyên nếu đã đúng
            ELSE h.TrangThaiThanhToan
        END
    FROM dbo.HoaDonBan h
    INNER JOIN inserted i ON h.HoaDonBanId = i.HoaDonBanId
    WHERE h.TrangThaiThanhToan IS NULL
       OR h.TrangThaiThanhToan NOT IN (N'Đã thanh toán', N'Chưa thanh toán', N'Đã hủy');
END;
GO

PRINT N'✅ Đã tạo trigger TR_HoaDonBan_NormalizeVietnamese_Insert';
GO

-- Trigger UPDATE: tự động sửa encoding khi cập nhật hóa đơn
IF OBJECT_ID(N'dbo.TR_HoaDonBan_NormalizeVietnamese_Update', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_HoaDonBan_NormalizeVietnamese_Update;
GO

CREATE TRIGGER dbo.TR_HoaDonBan_NormalizeVietnamese_Update
ON dbo.HoaDonBan
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Chỉ chạy nếu HinhThucThanhToan hoặc TrangThaiThanhToan bị thay đổi
    IF NOT (UPDATE(HinhThucThanhToan) OR UPDATE(TrangThaiThanhToan))
        RETURN;

    -- Chuẩn hóa HinhThucThanhToan nếu bị thay đổi
    IF UPDATE(HinhThucThanhToan)
    BEGIN
        UPDATE h
        SET h.HinhThucThanhToan = 
            CASE
                WHEN h.HinhThucThanhToan IS NULL OR LTRIM(RTRIM(h.HinhThucThanhToan)) = N'' 
                    THEN N'Tiền mặt'
                WHEN h.HinhThucThanhToan IN (N'Tiá»n máº·t', N'Tiá»n mạt', N'Tiá»n mặt')
                    THEN N'Tiền mặt'
                WHEN h.HinhThucThanhToan IN (N'Tháº»', N'Tháº', N'Tháº½')
                    THEN N'Thẻ'
                WHEN h.HinhThucThanhToan IN (N'Chuyá»ƒn khoáº£n', N'Chuyá»ƒn khoản', N'Chuyển khoáº£n')
                    THEN N'Chuyển khoản'
                WHEN h.HinhThucThanhToan IN (N'VÃ Ä''iá»‡n tá»', N'VÃ điện tử', N'Ví Ä''iá»‡n tử')
                    THEN N'Ví điện tử'
                ELSE h.HinhThucThanhToan
            END
        FROM dbo.HoaDonBan h
        INNER JOIN inserted i ON h.HoaDonBanId = i.HoaDonBanId
        WHERE h.HinhThucThanhToan IS NULL
           OR h.HinhThucThanhToan NOT IN (N'Tiền mặt', N'Chuyển khoản', N'Thẻ', N'Ví điện tử');
    END

    -- Chuẩn hóa TrangThaiThanhToan nếu bị thay đổi
    IF UPDATE(TrangThaiThanhToan)
    BEGIN
        UPDATE h
        SET h.TrangThaiThanhToan = 
            CASE
                WHEN h.TrangThaiThanhToan IS NULL OR LTRIM(RTRIM(h.TrangThaiThanhToan)) = N''
                    THEN N'Đã thanh toán'
                WHEN h.TrangThaiThanhToan IN (N'ÄÃ£ thanh toÃ¡n', N'ĐÃ£ thanh toán', N'Äã thanh toán')
                    THEN N'Đã thanh toán'
                WHEN h.TrangThaiThanhToan IN (N'ÄÃ£ há»§y', N'ĐÃ£ hủy', N'Äã hủy')
                    THEN N'Đã hủy'
                WHEN h.TrangThaiThanhToan IN (N'ChÆ°a thanh toÃ¡n', N'Chưa thanh toÃ¡n', N'ChÆ°a thanh toán')
                    THEN N'Chưa thanh toán'
                ELSE h.TrangThaiThanhToan
            END
        FROM dbo.HoaDonBan h
        INNER JOIN inserted i ON h.HoaDonBanId = i.HoaDonBanId
        WHERE h.TrangThaiThanhToan IS NULL
           OR h.TrangThaiThanhToan NOT IN (N'Đã thanh toán', N'Chưa thanh toán', N'Đã hủy');
    END
END;
GO

PRINT N'✅ Đã tạo trigger TR_HoaDonBan_NormalizeVietnamese_Update';
GO

-- ========================================
-- TỔNG KẾT
-- ========================================
PRINT N'';
PRINT N'============================================';
PRINT N'✅ Hoàn thành migration nâng cấp thu ngân';
PRINT N'============================================';
PRINT N'Các cột đã thêm vào bảng HoaDonBan:';
PRINT N'  - HinhThucThanhToan (Tiền mặt / Chuyển khoản / Thẻ / Ví điện tử)';
PRINT N'  - TrangThaiThanhToan (Đã thanh toán / Chưa thanh toán / Đã hủy)';
PRINT N'  - TienKhachDua, TienThoiLai';
PRINT N'  - MaGiaoDich, GhiChuThanhToan, GhiChuHoaDon';
PRINT N'  - LyDoHuy, NguoiHuy, NgayHuy';
PRINT N'Dữ liệu tiếng Việt bị lỗi encoding đã được sửa.';
PRINT N'Trigger tự động chuẩn hóa đã được tạo:';
PRINT N'  - TR_HoaDonBan_NormalizeVietnamese_Insert';
PRINT N'  - TR_HoaDonBan_NormalizeVietnamese_Update';
GO

-- ============================================================
-- GIBOR Coffee Shop - Consolidated Upgrade Scripts
-- Database: CoffeeShopDb
-- Lưu ý:
--   1) Đây là script NÂNG CẤP, dùng sau khi đã chạy database gốc 01-09.
--   2) Script được viết theo hướng idempotent: chạy lại nhiều lần sẽ hạn chế lỗi.
--   3) Nên backup database trước khi chạy.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================
-- FILE 02 - CA LÀM VIỆC / ĐỐI SOÁT / KHUYẾN MÃI
-- Gộp từ: 16, 17
-- ============================================================


-- ============================================================
-- PHẦN 16
-- Source: 16_CaLamViec_DoiSoatTien.sql
-- ============================================================
-- =============================================
-- 16_CaLamViec_DoiSoatTien.sql
-- Thêm cột đối soát tiền mặt cho CaLamViec
-- =============================================
IF COL_LENGTH('dbo.CaLamViec', 'TienDauCa') IS NULL
BEGIN
    ALTER TABLE dbo.CaLamViec ADD TienDauCa DECIMAL(18,2) NOT NULL DEFAULT(0);
    PRINT N'Đã thêm cột TienDauCa.';
END
GO

IF COL_LENGTH('dbo.CaLamViec', 'TienMatThucDem') IS NULL
BEGIN
    ALTER TABLE dbo.CaLamViec ADD TienMatThucDem DECIMAL(18,2) NULL;
    PRINT N'Đã thêm cột TienMatThucDem.';
END
GO

IF COL_LENGTH('dbo.CaLamViec', 'ChenhLechTienMat') IS NULL
BEGIN
    ALTER TABLE dbo.CaLamViec ADD ChenhLechTienMat DECIMAL(18,2) NULL;
    PRINT N'Đã thêm cột ChenhLechTienMat.';
END
GO

IF COL_LENGTH('dbo.CaLamViec', 'GhiChuDoiSoat') IS NULL
BEGIN
    ALTER TABLE dbo.CaLamViec ADD GhiChuDoiSoat NVARCHAR(500) NULL;
    PRINT N'Đã thêm cột GhiChuDoiSoat.';
END
GO


-- ============================================================
-- PHẦN 17
-- Source: 17_KhuyenMai_Conditions.sql
-- ============================================================
-- =============================================
-- 17_KhuyenMai_Conditions.sql
-- Thêm điều kiện đơn tối thiểu và giới hạn giảm tối đa
-- =============================================
IF COL_LENGTH('dbo.KhuyenMai', 'GiaTriDonHangToiThieu') IS NULL
BEGIN
    ALTER TABLE dbo.KhuyenMai ADD GiaTriDonHangToiThieu DECIMAL(18,2) NULL;
    PRINT N'Đã thêm cột GiaTriDonHangToiThieu.';
END
GO

IF COL_LENGTH('dbo.KhuyenMai', 'SoTienGiamToiDa') IS NULL
BEGIN
    ALTER TABLE dbo.KhuyenMai ADD SoTienGiamToiDa DECIMAL(18,2) NULL;
    PRINT N'Đã thêm cột SoTienGiamToiDa.';
END
GO

-- ============================================================
-- GIBOR Coffee Shop - Consolidated Upgrade Scripts
-- Database: CoffeeShopDb
-- Lưu ý:
--   1) Đây là script NÂNG CẤP, dùng sau khi đã chạy database gốc 01-09.
--   2) Script được viết theo hướng idempotent: chạy lại nhiều lần sẽ hạn chế lỗi.
--   3) Nên backup database trước khi chạy.
-- ============================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================
-- FILE 03 - KHO / NGUYÊN LIỆU / CÔNG THỨC
-- Gộp từ: 19, 20, 21, 22, 23
-- Ghi chú: FK LichSuTonKho_NguoiDung đã được chuẩn hóa về dbo.NguoiDung(NguoiDungId)
-- vì database gốc dùng bảng NguoiDung, không phải TaiKhoanNguoiDung.
-- ============================================================


-- ============================================================
-- PHẦN 19
-- Source: 19_Mon_TonKhoToiThieu.sql
-- ============================================================
-- =============================================
-- Migration: Thêm cảnh báo tồn kho thấp cho món
-- File: 19_Mon_TonKhoToiThieu.sql
-- Mô tả: Thêm cột TonKhoToiThieu vào bảng Mon để cảnh báo món sắp hết hàng
-- =============================================
-- Kiểm tra và thêm cột TonKhoToiThieu nếu chưa có
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Mon') 
    AND name = 'TonKhoToiThieu'
)
BEGIN
    ALTER TABLE dbo.Mon
    ADD TonKhoToiThieu INT NOT NULL DEFAULT(0);
    
    PRINT 'Đã thêm cột TonKhoToiThieu vào bảng Mon';
END
ELSE
BEGIN
    PRINT 'Cột TonKhoToiThieu đã tồn tại trong bảng Mon';
END
GO

-- Thêm check constraint cho TonKhoToiThieu
IF NOT EXISTS (
    SELECT 1 
    FROM sys.check_constraints 
    WHERE name = 'CK_Mon_TonKhoToiThieu_NonNegative'
)
BEGIN
    ALTER TABLE dbo.Mon
    ADD CONSTRAINT CK_Mon_TonKhoToiThieu_NonNegative 
    CHECK (TonKhoToiThieu >= 0);
    
    PRINT 'Đã thêm constraint CK_Mon_TonKhoToiThieu_NonNegative';
END
ELSE
BEGIN
    PRINT 'Constraint CK_Mon_TonKhoToiThieu_NonNegative đã tồn tại';
END
GO

-- Cập nhật giá trị mặc định cho các món hiện có (ví dụ: 10)
UPDATE dbo.Mon
SET TonKhoToiThieu = 10
WHERE TonKhoToiThieu = 0;
GO

PRINT 'Migration 19_Mon_TonKhoToiThieu.sql hoàn tất!';
GO


-- ============================================================
-- PHẦN 20
-- Source: 20_LichSuTonKho.sql
-- ============================================================
-- =============================================
-- Migration: Thêm bảng LichSuTonKho
-- Mục đích: Lưu lịch sử thay đổi tồn kho của món
-- =============================================
-- Kiểm tra và tạo bảng LichSuTonKho nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LichSuTonKho')
BEGIN
    CREATE TABLE dbo.LichSuTonKho
    (
        LichSuTonKhoId INT IDENTITY(1,1) PRIMARY KEY,
        MonId INT NOT NULL,
        LoaiPhatSinh NVARCHAR(30) NOT NULL,
        SoLuongThayDoi INT NOT NULL,
        TonTruoc INT NOT NULL,
        TonSau INT NOT NULL,
        HoaDonBanId INT NULL,
        HoaDonNhapId INT NULL,
        GhiChu NVARCHAR(500) NULL,
        ThoiGian DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        NguoiDungId INT NULL,

        CONSTRAINT FK_LichSuTonKho_Mon FOREIGN KEY (MonId) REFERENCES dbo.Mon(MonId),
        CONSTRAINT FK_LichSuTonKho_HoaDonBan FOREIGN KEY (HoaDonBanId) REFERENCES dbo.HoaDonBan(HoaDonBanId),
        CONSTRAINT FK_LichSuTonKho_HoaDonNhap FOREIGN KEY (HoaDonNhapId) REFERENCES dbo.HoaDonNhap(HoaDonNhapId),
        CONSTRAINT FK_LichSuTonKho_NguoiDung FOREIGN KEY (NguoiDungId) REFERENCES dbo.NguoiDung(NguoiDungId),
        
        CONSTRAINT CK_LichSuTonKho_LoaiPhatSinh CHECK (LoaiPhatSinh IN (N'NhapHang', N'BanHang', N'HuyHang', N'DieuChinh')),
        CONSTRAINT CK_LichSuTonKho_TonTruoc CHECK (TonTruoc >= 0),
        CONSTRAINT CK_LichSuTonKho_TonSau CHECK (TonSau >= 0)
    );

    PRINT 'Đã tạo bảng LichSuTonKho';
END
ELSE
BEGIN
    PRINT 'Bảng LichSuTonKho đã tồn tại';
END
GO

-- Tạo index để tối ưu truy vấn
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LichSuTonKho_MonId_ThoiGian')
BEGIN
    CREATE NONCLUSTERED INDEX IX_LichSuTonKho_MonId_ThoiGian
    ON dbo.LichSuTonKho(MonId, ThoiGian DESC);
    
    PRINT 'Đã tạo index IX_LichSuTonKho_MonId_ThoiGian';
END
GO

-- Tạo index cho HoaDonBanId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LichSuTonKho_HoaDonBanId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_LichSuTonKho_HoaDonBanId
    ON dbo.LichSuTonKho(HoaDonBanId)
    WHERE HoaDonBanId IS NOT NULL;
    
    PRINT 'Đã tạo index IX_LichSuTonKho_HoaDonBanId';
END
GO

-- Tạo index cho HoaDonNhapId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LichSuTonKho_HoaDonNhapId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_LichSuTonKho_HoaDonNhapId
    ON dbo.LichSuTonKho(HoaDonNhapId)
    WHERE HoaDonNhapId IS NOT NULL;
    
    PRINT 'Đã tạo index IX_LichSuTonKho_HoaDonNhapId';
END
GO

-- Tạo index cho ThoiGian để truy vấn lịch sử gần đây
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LichSuTonKho_ThoiGian')
BEGIN
    CREATE NONCLUSTERED INDEX IX_LichSuTonKho_ThoiGian
    ON dbo.LichSuTonKho(ThoiGian DESC);
    
    PRINT 'Đã tạo index IX_LichSuTonKho_ThoiGian';
END
GO

PRINT 'Migration hoàn tất: Bảng LichSuTonKho và các index đã sẵn sàng';
GO


-- ============================================================
-- PHẦN 21
-- Source: 21_NguyenLieu.sql
-- ============================================================
-- =============================================
-- Migration: Thêm bảng NguyenLieu
-- Mục đích: Quản lý kho nguyên liệu cho quán cafe
-- =============================================
-- Kiểm tra và tạo bảng NguyenLieu nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NguyenLieu')
BEGIN
    CREATE TABLE dbo.NguyenLieu
    (
        NguyenLieuId INT IDENTITY(1,1) PRIMARY KEY,
        TenNguyenLieu NVARCHAR(150) NOT NULL,
        DonViTinh NVARCHAR(30) NOT NULL,
        TonKho DECIMAL(18,2) NOT NULL DEFAULT(0),
        TonKhoToiThieu DECIMAL(18,2) NOT NULL DEFAULT(0),
        DonGiaNhap DECIMAL(18,2) NOT NULL DEFAULT(0),
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        UpdatedAt DATETIME2 NULL,

        CONSTRAINT CK_NguyenLieu_TonKho CHECK (TonKho >= 0),
        CONSTRAINT CK_NguyenLieu_TonKhoToiThieu CHECK (TonKhoToiThieu >= 0),
        CONSTRAINT CK_NguyenLieu_DonGiaNhap CHECK (DonGiaNhap >= 0)
    );

    PRINT 'Đã tạo bảng NguyenLieu';
END
ELSE
BEGIN
    PRINT 'Bảng NguyenLieu đã tồn tại';
END
GO

-- Tạo index để tối ưu truy vấn
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NguyenLieu_TenNguyenLieu')
BEGIN
    CREATE NONCLUSTERED INDEX IX_NguyenLieu_TenNguyenLieu
    ON dbo.NguyenLieu(TenNguyenLieu);
    
    PRINT 'Đã tạo index IX_NguyenLieu_TenNguyenLieu';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NguyenLieu_IsActive')
BEGIN
    CREATE NONCLUSTERED INDEX IX_NguyenLieu_IsActive
    ON dbo.NguyenLieu(IsActive)
    WHERE IsActive = 1;
    
    PRINT 'Đã tạo index IX_NguyenLieu_IsActive';
END
GO

-- Thêm dữ liệu mẫu (nếu bảng trống)
IF NOT EXISTS (SELECT * FROM dbo.NguyenLieu)
BEGIN
    INSERT INTO dbo.NguyenLieu (TenNguyenLieu, DonViTinh, TonKho, TonKhoToiThieu, DonGiaNhap)
    VALUES 
        (N'Cà phê hạt Arabica', N'kg', 50.00, 10.00, 250000),
        (N'Cà phê hạt Robusta', N'kg', 30.00, 10.00, 180000),
        (N'Sữa tươi', N'lít', 100.00, 20.00, 25000),
        (N'Đường trắng', N'kg', 80.00, 15.00, 18000),
        (N'Trân châu đen', N'kg', 20.00, 5.00, 45000),
        (N'Trà xanh', N'kg', 15.00, 3.00, 120000),
        (N'Bơ', N'kg', 25.00, 5.00, 60000),
        (N'Siro vani', N'chai', 30.00, 5.00, 35000),
        (N'Siro caramel', N'chai', 25.00, 5.00, 35000),
        (N'Kem whipping', N'hộp', 40.00, 10.00, 55000),
        (N'Đá viên', N'kg', 200.00, 50.00, 5000),
        (N'Ly nhựa size M', N'cái', 500.00, 100.00, 800),
        (N'Ly nhựa size L', N'cái', 400.00, 100.00, 1000),
        (N'Ống hút', N'cái', 1000.00, 200.00, 200);

    PRINT 'Đã thêm dữ liệu mẫu cho NguyenLieu';
END
GO

PRINT 'Migration hoàn tất: Bảng NguyenLieu và dữ liệu mẫu đã sẵn sàng';
GO


-- ============================================================
-- PHẦN 22
-- Source: 22_CongThucMon.sql
-- ============================================================
-- =============================================
-- Migration: Thêm bảng CongThucMon
-- Mục đích: Quản lý công thức món (món dùng nguyên liệu gì, bao nhiêu)
-- =============================================
-- Kiểm tra và tạo bảng CongThucMon nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CongThucMon')
BEGIN
    CREATE TABLE dbo.CongThucMon
    (
        CongThucMonId INT IDENTITY(1,1) PRIMARY KEY,
        MonId INT NOT NULL,
        NguyenLieuId INT NOT NULL,
        DinhLuong DECIMAL(18,2) NOT NULL,
        GhiChu NVARCHAR(255) NULL,
        IsActive BIT NOT NULL DEFAULT(1),

        CONSTRAINT FK_CongThucMon_Mon FOREIGN KEY (MonId) REFERENCES dbo.Mon(MonId),
        CONSTRAINT FK_CongThucMon_NguyenLieu FOREIGN KEY (NguyenLieuId) REFERENCES dbo.NguyenLieu(NguyenLieuId),
        CONSTRAINT CK_CongThucMon_DinhLuong CHECK (DinhLuong > 0)
    );

    PRINT 'Đã tạo bảng CongThucMon';
END
ELSE
BEGIN
    PRINT 'Bảng CongThucMon đã tồn tại';
END
GO

-- Tạo unique index để không cho trùng nguyên liệu trong cùng một món (khi IsActive = 1)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_CongThucMon_MonId_NguyenLieuId_Active')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_CongThucMon_MonId_NguyenLieuId_Active
    ON dbo.CongThucMon(MonId, NguyenLieuId)
    WHERE IsActive = 1;
    
    PRINT 'Đã tạo unique index UX_CongThucMon_MonId_NguyenLieuId_Active';
END
GO

-- Tạo index để tối ưu truy vấn theo MonId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CongThucMon_MonId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_CongThucMon_MonId
    ON dbo.CongThucMon(MonId)
    WHERE IsActive = 1;
    
    PRINT 'Đã tạo index IX_CongThucMon_MonId';
END
GO

-- Tạo index để tối ưu truy vấn theo NguyenLieuId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CongThucMon_NguyenLieuId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_CongThucMon_NguyenLieuId
    ON dbo.CongThucMon(NguyenLieuId)
    WHERE IsActive = 1;
    
    PRINT 'Đã tạo index IX_CongThucMon_NguyenLieuId';
END
GO

-- Thêm dữ liệu mẫu (nếu bảng trống)
IF NOT EXISTS (SELECT * FROM dbo.CongThucMon)
BEGIN
    -- Giả sử MonId = 1 là "Cà phê đen", MonId = 2 là "Cà phê sữa"
    -- Giả sử NguyenLieuId = 1 là "Cà phê hạt Arabica", NguyenLieuId = 3 là "Sữa tươi"
    
    -- Kiểm tra xem có món và nguyên liệu không
    IF EXISTS (SELECT * FROM dbo.Mon WHERE MonId IN (1, 2))
       AND EXISTS (SELECT * FROM dbo.NguyenLieu WHERE NguyenLieuId IN (1, 3, 4, 11))
    BEGIN
        -- Công thức Cà phê đen (MonId = 1)
        INSERT INTO dbo.CongThucMon (MonId, NguyenLieuId, DinhLuong, GhiChu)
        VALUES 
            (1, 1, 0.02, N'Cà phê hạt Arabica - 20g'),  -- 0.02 kg = 20g
            (1, 11, 0.20, N'Đá viên - 200g');           -- 0.20 kg = 200g

        -- Công thức Cà phê sữa (MonId = 2)
        INSERT INTO dbo.CongThucMon (MonId, NguyenLieuId, DinhLuong, GhiChu)
        VALUES 
            (2, 1, 0.02, N'Cà phê hạt Arabica - 20g'),  -- 0.02 kg = 20g
            (2, 3, 0.05, N'Sữa tươi - 50ml'),           -- 0.05 lít = 50ml
            (2, 4, 0.01, N'Đường trắng - 10g'),         -- 0.01 kg = 10g
            (2, 11, 0.15, N'Đá viên - 150g');           -- 0.15 kg = 150g

        PRINT 'Đã thêm dữ liệu mẫu cho CongThucMon';
    END
    ELSE
    BEGIN
        PRINT 'Không thêm dữ liệu mẫu vì chưa có món hoặc nguyên liệu';
    END
END
GO

PRINT 'Migration hoàn tất: Bảng CongThucMon đã sẵn sàng';
GO


-- ============================================================
-- PHẦN 23
-- Source: 23_LichSuNguyenLieu.sql
-- ============================================================
-- =============================================
-- Migration: Tạo bảng LichSuNguyenLieu
-- Mục đích: Lưu lịch sử xuất/nhập nguyên liệu
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LichSuNguyenLieu' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.LichSuNguyenLieu
    (
        LichSuNguyenLieuId INT IDENTITY(1,1) PRIMARY KEY,
        NguyenLieuId INT NOT NULL,
        LoaiPhatSinh NVARCHAR(30) NOT NULL, -- NhapKho, XuatKho, DieuChinh
        SoLuongThayDoi DECIMAL(18,2) NOT NULL,
        TonTruoc DECIMAL(18,2) NOT NULL,
        TonSau DECIMAL(18,2) NOT NULL,
        HoaDonBanId INT NULL,
        HoaDonNhapId INT NULL,
        GhiChu NVARCHAR(500) NULL,
        ThoiGian DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        NguoiDungId INT NULL,
        CONSTRAINT FK_LichSuNguyenLieu_NguyenLieu FOREIGN KEY (NguyenLieuId) REFERENCES dbo.NguyenLieu(NguyenLieuId),
        CONSTRAINT FK_LichSuNguyenLieu_HoaDonBan FOREIGN KEY (HoaDonBanId) REFERENCES dbo.HoaDonBan(HoaDonBanId),
        CONSTRAINT FK_LichSuNguyenLieu_HoaDonNhap FOREIGN KEY (HoaDonNhapId) REFERENCES dbo.HoaDonNhap(HoaDonNhapId),
        CONSTRAINT FK_LichSuNguyenLieu_NguoiDung FOREIGN KEY (NguoiDungId) REFERENCES dbo.NguoiDung(NguoiDungId)
    );

    PRINT 'Đã tạo bảng LichSuNguyenLieu';
END
ELSE
BEGIN
    PRINT 'Bảng LichSuNguyenLieu đã tồn tại';
END
GO

-- Tạo index cho truy vấn nhanh
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LichSuNguyenLieu_NguyenLieuId_ThoiGian' AND object_id = OBJECT_ID('dbo.LichSuNguyenLieu'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_LichSuNguyenLieu_NguyenLieuId_ThoiGian
    ON dbo.LichSuNguyenLieu(NguyenLieuId, ThoiGian DESC);
    PRINT 'Đã tạo index IX_LichSuNguyenLieu_NguyenLieuId_ThoiGian';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LichSuNguyenLieu_HoaDonBanId' AND object_id = OBJECT_ID('dbo.LichSuNguyenLieu'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_LichSuNguyenLieu_HoaDonBanId
    ON dbo.LichSuNguyenLieu(HoaDonBanId)
    WHERE HoaDonBanId IS NOT NULL;
    PRINT 'Đã tạo index IX_LichSuNguyenLieu_HoaDonBanId';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LichSuNguyenLieu_ThoiGian' AND object_id = OBJECT_ID('dbo.LichSuNguyenLieu'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_LichSuNguyenLieu_ThoiGian
    ON dbo.LichSuNguyenLieu(ThoiGian DESC);
    PRINT 'Đã tạo index IX_LichSuNguyenLieu_ThoiGian';
END
GO

PRINT 'Migration LichSuNguyenLieu hoàn tất';
GO

-- =============================================
-- Migration: QR Payment Integration
-- Mô tả: Thêm các cột hỗ trợ thanh toán QR code qua payment gateway (payOS, VietQR...)
-- Ngày tạo: 2026-04-25
-- =============================================

USE CoffeeShopDB;
GO

PRINT N'========================================';
PRINT N'Bắt đầu migration: QR Payment Integration';
PRINT N'========================================';
GO

-- =============================================
-- 1. Thêm cột PaymentProvider
-- =============================================
IF COL_LENGTH(N'dbo.HoaDonBan', N'PaymentProvider') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan 
    ADD PaymentProvider NVARCHAR(30) NULL;
    
    PRINT N'✅ Đã thêm cột PaymentProvider (payOS, VietQR, Manual...)';
END
ELSE
BEGIN
    PRINT N'⚠️ Cột PaymentProvider đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 2. Thêm cột ProviderPaymentId
-- =============================================
IF COL_LENGTH(N'dbo.HoaDonBan', N'ProviderPaymentId') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan 
    ADD ProviderPaymentId NVARCHAR(100) NULL;
    
    PRINT N'✅ Đã thêm cột ProviderPaymentId (payment link ID từ provider)';
END
ELSE
BEGIN
    PRINT N'⚠️ Cột ProviderPaymentId đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 3. Thêm cột ProviderOrderCode
-- =============================================
IF COL_LENGTH(N'dbo.HoaDonBan', N'ProviderOrderCode') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan 
    ADD ProviderOrderCode BIGINT NULL;
    
    PRINT N'✅ Đã thêm cột ProviderOrderCode (order code duy nhất cho provider)';
END
ELSE
BEGIN
    PRINT N'⚠️ Cột ProviderOrderCode đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 4. Thêm cột QRCodeRaw
-- =============================================
IF COL_LENGTH(N'dbo.HoaDonBan', N'QRCodeRaw') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan 
    ADD QRCodeRaw NVARCHAR(MAX) NULL;
    
    PRINT N'✅ Đã thêm cột QRCodeRaw (base64 hoặc URL của QR code)';
END
ELSE
BEGIN
    PRINT N'⚠️ Cột QRCodeRaw đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 5. Thêm cột CheckoutUrl
-- =============================================
IF COL_LENGTH(N'dbo.HoaDonBan', N'CheckoutUrl') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan 
    ADD CheckoutUrl NVARCHAR(500) NULL;
    
    PRINT N'✅ Đã thêm cột CheckoutUrl (link thanh toán cho khách)';
END
ELSE
BEGIN
    PRINT N'⚠️ Cột CheckoutUrl đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 6. Thêm cột QRExpiredAt
-- =============================================
IF COL_LENGTH(N'dbo.HoaDonBan', N'QRExpiredAt') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan 
    ADD QRExpiredAt DATETIME2 NULL;
    
    PRINT N'✅ Đã thêm cột QRExpiredAt (thời gian hết hạn QR)';
END
ELSE
BEGIN
    PRINT N'⚠️ Cột QRExpiredAt đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 7. Thêm cột PaymentStatus
-- =============================================
IF COL_LENGTH(N'dbo.HoaDonBan', N'PaymentStatus') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan 
    ADD PaymentStatus NVARCHAR(30) NULL;
    
    PRINT N'✅ Đã thêm cột PaymentStatus (PENDING, PAID, CANCELLED, EXPIRED)';
END
ELSE
BEGIN
    PRINT N'⚠️ Cột PaymentStatus đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 8. Thêm cột PaymentConfirmedAt
-- =============================================
IF COL_LENGTH(N'dbo.HoaDonBan', N'PaymentConfirmedAt') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDonBan 
    ADD PaymentConfirmedAt DATETIME2 NULL;
    
    PRINT N'✅ Đã thêm cột PaymentConfirmedAt (thời điểm xác nhận thanh toán)';
END
ELSE
BEGIN
    PRINT N'⚠️ Cột PaymentConfirmedAt đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 9. Thêm constraint cho PaymentStatus
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.check_constraints 
    WHERE name = 'CK_HoaDonBan_PaymentStatus'
      AND parent_object_id = OBJECT_ID(N'dbo.HoaDonBan')
)
BEGIN
    ALTER TABLE dbo.HoaDonBan
    ADD CONSTRAINT CK_HoaDonBan_PaymentStatus
    CHECK (PaymentStatus IS NULL OR PaymentStatus IN (
        N'PENDING',
        N'PROCESSING', 
        N'PAID',
        N'CANCELLED',
        N'EXPIRED'
    ));
    
    PRINT N'✅ Đã thêm constraint CK_HoaDonBan_PaymentStatus';
END
ELSE
BEGIN
    PRINT N'⚠️ Constraint CK_HoaDonBan_PaymentStatus đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 10. Thêm index cho ProviderOrderCode (để webhook tìm nhanh)
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_HoaDonBan_ProviderOrderCode'
      AND object_id = OBJECT_ID(N'dbo.HoaDonBan')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_HoaDonBan_ProviderOrderCode
    ON dbo.HoaDonBan(ProviderOrderCode)
    WHERE ProviderOrderCode IS NOT NULL;
    
    PRINT N'✅ Đã thêm index IX_HoaDonBan_ProviderOrderCode';
END
ELSE
BEGIN
    PRINT N'⚠️ Index IX_HoaDonBan_ProviderOrderCode đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 11. Thêm index cho ProviderPaymentId
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_HoaDonBan_ProviderPaymentId'
      AND object_id = OBJECT_ID(N'dbo.HoaDonBan')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_HoaDonBan_ProviderPaymentId
    ON dbo.HoaDonBan(ProviderPaymentId)
    WHERE ProviderPaymentId IS NOT NULL;
    
    PRINT N'✅ Đã thêm index IX_HoaDonBan_ProviderPaymentId';
END
ELSE
BEGIN
    PRINT N'⚠️ Index IX_HoaDonBan_ProviderPaymentId đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 12. Thêm index cho PaymentStatus
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_HoaDonBan_PaymentStatus'
      AND object_id = OBJECT_ID(N'dbo.HoaDonBan')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_HoaDonBan_PaymentStatus
    ON dbo.HoaDonBan(PaymentStatus)
    WHERE PaymentStatus IS NOT NULL;
    
    PRINT N'✅ Đã thêm index IX_HoaDonBan_PaymentStatus';
END
ELSE
BEGIN
    PRINT N'⚠️ Index IX_HoaDonBan_PaymentStatus đã tồn tại, bỏ qua.';
END
GO

-- =============================================
-- 13. Cập nhật comment cho bảng
-- =============================================
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description',
    @value = N'Bảng hóa đơn bán hàng - đã tích hợp QR payment gateway',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'HoaDonBan';
GO

PRINT N'';
PRINT N'========================================';
PRINT N'✅ Hoàn tất migration: QR Payment Integration';
PRINT N'========================================';
PRINT N'';
PRINT N'📝 Ghi chú:';
PRINT N'- PaymentProvider: Tên provider (payOS, VietQR, Manual...)';
PRINT N'- ProviderPaymentId: Payment link ID từ provider';
PRINT N'- ProviderOrderCode: Order code duy nhất cho provider';
PRINT N'- QRCodeRaw: Base64 hoặc URL của QR code';
PRINT N'- CheckoutUrl: Link thanh toán cho khách';
PRINT N'- QRExpiredAt: Thời gian hết hạn QR';
PRINT N'- PaymentStatus: PENDING, PROCESSING, PAID, CANCELLED, EXPIRED';
PRINT N'- PaymentConfirmedAt: Thời điểm xác nhận thanh toán';
PRINT N'- MaGiaoDich (cột cũ): Vẫn dùng để lưu reference cuối cùng từ ngân hàng';
GO

SELECT
    HoaDonBanId,
    TrangThaiThanhToan,
    HinhThucThanhToan,
    PaymentStatus,
    PaymentProvider,
    ProviderPaymentId,
    ProviderOrderCode,
    MaGiaoDich,
    PaymentConfirmedAt
FROM dbo.HoaDonBan
WHERE HoaDonBanId = 28;

-- =============================================
-- Script cập nhật database cho tính năng Công thức món
-- Thêm các cột mới và bảng lịch sử
-- =============================================

USE CoffeeShopDB;
GO

-- 1. Thêm cột CacBuocThucHien vào bảng CongThucMon
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.CongThucMon') AND name = 'CacBuocThucHien')
BEGIN
    ALTER TABLE dbo.CongThucMon
    ADD CacBuocThucHien NVARCHAR(MAX) NULL;
    PRINT 'Đã thêm cột CacBuocThucHien vào bảng CongThucMon';
END
ELSE
BEGIN
    PRINT 'Cột CacBuocThucHien đã tồn tại trong bảng CongThucMon';
END
GO

-- 2. Thêm cột TyLeHaoHut vào bảng CongThucMon
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.CongThucMon') AND name = 'TyLeHaoHut')
BEGIN
    ALTER TABLE dbo.CongThucMon
    ADD TyLeHaoHut DECIMAL(5, 2) NOT NULL DEFAULT 0;
    PRINT 'Đã thêm cột TyLeHaoHut vào bảng CongThucMon';
END
ELSE
BEGIN
    PRINT 'Cột TyLeHaoHut đã tồn tại trong bảng CongThucMon';
END
GO

-- 3. Tạo bảng LichSuCapNhatCongThuc
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.LichSuCapNhatCongThuc') AND type = 'U')
BEGIN
    CREATE TABLE dbo.LichSuCapNhatCongThuc
    (
        LichSuId INT IDENTITY(1,1) PRIMARY KEY,
        MonId INT NOT NULL,
        NgayCapNhat DATETIME NOT NULL DEFAULT GETDATE(),
        NguoiCapNhat NVARCHAR(100) NULL,
        NoiDungCapNhat NVARCHAR(500) NULL,
        
        CONSTRAINT FK_LichSuCapNhatCongThuc_Mon 
            FOREIGN KEY (MonId) REFERENCES dbo.Mon(MonId)
    );
    
    -- Tạo index cho tìm kiếm nhanh
    CREATE INDEX IX_LichSuCapNhatCongThuc_MonId 
        ON dbo.LichSuCapNhatCongThuc(MonId);
    
    CREATE INDEX IX_LichSuCapNhatCongThuc_NgayCapNhat 
        ON dbo.LichSuCapNhatCongThuc(NgayCapNhat DESC);
    
    PRINT 'Đã tạo bảng LichSuCapNhatCongThuc';
END
ELSE
BEGIN
    PRINT 'Bảng LichSuCapNhatCongThuc đã tồn tại';
END
GO

-- 4. Thêm dữ liệu mẫu cho lịch sử (nếu cần)
-- Uncomment phần này nếu muốn thêm dữ liệu mẫu
/*
IF EXISTS (SELECT 1 FROM dbo.Mon WHERE MonId = 1)
BEGIN
    INSERT INTO dbo.LichSuCapNhatCongThuc (MonId, NgayCapNhat, NguoiCapNhat, NoiDungCapNhat)
    VALUES 
        (1, GETDATE(), 'Admin', 'Khởi tạo công thức món'),
        (1, DATEADD(HOUR, -1, GETDATE()), 'Admin', 'Cập nhật định lượng nguyên liệu');
    
    PRINT 'Đã thêm dữ liệu mẫu vào bảng LichSuCapNhatCongThuc';
END
GO
*/

-- 5. Kiểm tra kết quả
SELECT 
    'CongThucMon' AS TableName,
    COUNT(*) AS RecordCount
FROM dbo.CongThucMon
UNION ALL
SELECT 
    'LichSuCapNhatCongThuc' AS TableName,
    COUNT(*) AS RecordCount
FROM dbo.LichSuCapNhatCongThuc;
GO

PRINT 'Hoàn tất cập nhật database!';
GO
