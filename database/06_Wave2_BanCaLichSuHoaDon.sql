USE CoffeeShopDb;
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
