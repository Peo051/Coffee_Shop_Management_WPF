-- ============================================================
-- GIBOR Coffee Shop - Consolidated Upgrade Scripts
-- Database: CoffeeShopDb
-- Lưu ý:
--   1) Đây là script NÂNG CẤP, dùng sau khi đã chạy database gốc 01-09.
--   2) Script được viết theo hướng idempotent: chạy lại nhiều lần sẽ hạn chế lỗi.
--   3) Nên backup database trước khi chạy.
-- ============================================================
USE CoffeeShopDb;
GO

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
