USE CoffeeShopDb;
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
