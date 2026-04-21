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
