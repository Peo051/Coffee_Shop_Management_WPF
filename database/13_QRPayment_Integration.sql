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