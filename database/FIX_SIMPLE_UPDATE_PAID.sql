-- Script đơn giản cập nhật hóa đơn đã thanh toán
-- THAY ĐỔI @HoaDonBanId thành ID hóa đơn của bạn

DECLARE @HoaDonBanId INT = 29; -- ← THAY ĐỔI ID NÀY (29 hoặc 30)
DECLARE @MaGiaoDich NVARCHAR(100) = 'MANUAL-FIX-' + CAST(@HoaDonBanId AS NVARCHAR(10));

BEGIN TRANSACTION;

BEGIN TRY
    -- Kiểm tra hóa đơn tồn tại
    IF NOT EXISTS (SELECT 1 FROM HoaDonBan WHERE HoaDonBanId = @HoaDonBanId)
    BEGIN
        RAISERROR('Hóa đơn không tồn tại', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Kiểm tra đã thanh toán chưa
    DECLARE @CurrentStatus NVARCHAR(50);
    SELECT @CurrentStatus = TrangThaiThanhToan
    FROM HoaDonBan
    WHERE HoaDonBanId = @HoaDonBanId;

    IF @CurrentStatus = N'Đã thanh toán'
    BEGIN
        PRINT N'⚠️ Hóa đơn ' + CAST(@HoaDonBanId AS NVARCHAR(10)) + N' đã thanh toán rồi!';
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- 1. Cập nhật trạng thái thanh toán
    UPDATE HoaDonBan
    SET 
        TrangThaiThanhToan = N'Đã thanh toán',
        PaymentStatus = 'PAID',
        MaGiaoDich = @MaGiaoDich,
        PaymentConfirmedAt = GETDATE()
    WHERE HoaDonBanId = @HoaDonBanId;

    PRINT N'✅ Đã cập nhật trạng thái thanh toán';

    -- 2. Trừ kho món
    UPDATE m
    SET m.TonKho = m.TonKho - ct.SoLuong
    FROM Mon m
    INNER JOIN ChiTietHoaDonBan ct ON m.MonId = ct.MonId
    WHERE ct.HoaDonBanId = @HoaDonBanId;

    PRINT N'✅ Đã trừ kho món';

    -- 3. Trừ nguyên liệu (nếu có bảng CongThucNguyenLieu)
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CongThucNguyenLieu')
    BEGIN
        UPDATE nl
        SET nl.TonKho = nl.TonKho - (ct.SoLuong * cnl.SoLuong)
        FROM NguyenLieu nl
        INNER JOIN CongThucNguyenLieu cnl ON nl.NguyenLieuId = cnl.NguyenLieuId
        INNER JOIN ChiTietHoaDonBan ct ON cnl.MonId = ct.MonId
        WHERE ct.HoaDonBanId = @HoaDonBanId;

        PRINT N'✅ Đã trừ nguyên liệu';
    END

    -- 4. Xử lý điểm khách hàng (nếu có)
    DECLARE @KhachHangId INT, @DiemSuDung INT, @DiemCong INT;
    
    SELECT 
        @KhachHangId = KhachHangId, 
        @DiemSuDung = ISNULL(DiemSuDung, 0),
        @DiemCong = ISNULL(DiemCong, 0)
    FROM HoaDonBan
    WHERE HoaDonBanId = @HoaDonBanId;

    IF @KhachHangId IS NOT NULL
    BEGIN
        -- Trừ điểm sử dụng
        IF @DiemSuDung > 0
        BEGIN
            UPDATE KhachHang
            SET DiemTichLuy = DiemTichLuy - @DiemSuDung
            WHERE KhachHangId = @KhachHangId;
            
            PRINT N'✅ Đã trừ ' + CAST(@DiemSuDung AS NVARCHAR(10)) + N' điểm sử dụng';
        END

        -- Cộng điểm tích lũy
        IF @DiemCong > 0
        BEGIN
            UPDATE KhachHang
            SET DiemTichLuy = DiemTichLuy + @DiemCong
            WHERE KhachHangId = @KhachHangId;
            
            PRINT N'✅ Đã cộng ' + CAST(@DiemCong AS NVARCHAR(10)) + N' điểm tích lũy';
        END
    END

    COMMIT TRANSACTION;

    PRINT N'';
    PRINT N'🎉 HOÀN THÀNH! Hóa đơn ' + CAST(@HoaDonBanId AS NVARCHAR(10)) + N' đã được cập nhật thành công!';
    PRINT N'   Mã giao dịch: ' + @MaGiaoDich;

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT N'';
    PRINT N'❌ LỖI: ' + ERROR_MESSAGE();
    PRINT N'   Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10));
END CATCH;

-- Kiểm tra kết quả
PRINT N'';
PRINT N'--- Kết quả kiểm tra ---';
SELECT 
    HoaDonBanId,
    ThanhToan,
    TrangThaiThanhToan,
    PaymentStatus,
    MaGiaoDich,
    PaymentConfirmedAt
FROM HoaDonBan
WHERE HoaDonBanId = @HoaDonBanId;
