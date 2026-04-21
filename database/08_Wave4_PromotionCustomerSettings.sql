USE CoffeeShopDb;
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

