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


