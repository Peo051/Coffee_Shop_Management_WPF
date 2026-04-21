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
