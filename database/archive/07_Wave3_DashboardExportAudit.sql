USE CoffeeShopDb;
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

