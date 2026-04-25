-- Script cập nhật thủ công các hóa đơn đã thanh toán trên PayOS
-- Sử dụng khi webhook không hoạt động

-- Bước 1: Kiểm tra các hóa đơn QR Payment đang chờ thanh toán
SELECT 
    HoaDonBanId,
    SoGoiMon,
    ThanhToan,
    HinhThucThanhToan,
    TrangThaiThanhToan,
    PaymentStatus,
    ProviderOrderCode,
    QRExpiredAt,
    CreatedAt
FROM HoaDonBan
WHERE HinhThucThanhToan = N'QR Payment'
  AND TrangThaiThanhToan = N'Chờ thanh toán'
  AND PaymentStatus = 'PENDING'
ORDER BY CreatedAt DESC;

-- Bước 2: Nếu bạn đã thanh toán trên PayOS, cập nhật thủ công
-- THAY ĐỔI @HoaDonBanId thành ID hóa đơn của bạn

DECLARE @HoaDonBanId INT = 29; -- ← THAY ĐỔI ID NÀY
DECLARE @MaGiaoDich NVARCHAR(100) = 'MANUAL-FIX-' + CAST(@HoaDonBanId AS NVARCHAR(10));
DECLARE @PaymentConfirmedAt DATETIME = GETDATE();

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Cập nhật trạng thái thanh toán
    UPDATE HoaDonBan
    SET 
        TrangThaiThanhToan = N'Đã thanh toán',
        PaymentStatus = 'PAID',
        MaGiaoDich = @MaGiaoDich,
        PaymentConfirmedAt = @PaymentConfirmedAt,
        UpdatedAt = GETDATE()
    WHERE HoaDonBanId = @HoaDonBanId
      AND TrangThaiThanhToan = N'Chờ thanh toán';

    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Hóa đơn không tồn tại hoặc đã thanh toán rồi', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- 2. Trừ kho món
    UPDATE m
    SET m.TonKho = m.TonKho - ct.SoLuong
    FROM Mon m
    INNER JOIN ChiTietHoaDonBan ct ON m.MonId = ct.MonId
    WHERE ct.HoaDonBanId = @HoaDonBanId;

    -- 3. Trừ nguyên liệu
    UPDATE nl
    SET nl.TonKho = nl.TonKho - (ct.SoLuong * cnl.SoLuong)
    FROM NguyenLieu nl
    INNER JOIN CongThucNguyenLieu cnl ON nl.NguyenLieuId = cnl.NguyenLieuId
    INNER JOIN ChiTietHoaDonBan ct ON cnl.MonId = ct.MonId
    WHERE ct.HoaDonBanId = @HoaDonBanId;

    -- 4. Trừ điểm sử dụng (nếu có)
    DECLARE @KhachHangId INT, @DiemSuDung INT;
    
    SELECT @KhachHangId = KhachHangId, @DiemSuDung = DiemSuDung
    FROM HoaDonBan
    WHERE HoaDonBanId = @HoaDonBanId;

    IF @KhachHangId IS NOT NULL AND @DiemSuDung > 0
    BEGIN
        UPDATE KhachHang
        SET DiemTichLuy = DiemTichLuy - @DiemSuDung
        WHERE KhachHangId = @KhachHangId;
    END

    -- 5. Cộng điểm tích lũy (nếu có)
    DECLARE @DiemCong INT;
    
    SELECT @DiemCong = DiemCong
    FROM HoaDonBan
    WHERE HoaDonBanId = @HoaDonBanId;

    IF @KhachHangId IS NOT NULL AND @DiemCong > 0
    BEGIN
        UPDATE KhachHang
        SET DiemTichLuy = DiemTichLuy + @DiemCong
        WHERE KhachHangId = @KhachHangId;
    END

    COMMIT TRANSACTION;

    PRINT N'✅ Đã cập nhật thành công hóa đơn ' + CAST(@HoaDonBanId AS NVARCHAR(10));
    PRINT N'   - Trạng thái: Đã thanh toán';
    PRINT N'   - Mã giao dịch: ' + @MaGiaoDich;
    PRINT N'   - Đã trừ kho món';
    PRINT N'   - Đã trừ nguyên liệu';
    IF @DiemSuDung > 0
        PRINT N'   - Đã trừ ' + CAST(@DiemSuDung AS NVARCHAR(10)) + N' điểm sử dụng';
    IF @DiemCong > 0
        PRINT N'   - Đã cộng ' + CAST(@DiemCong AS NVARCHAR(10)) + N' điểm tích lũy';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT N'❌ Lỗi: ' + ERROR_MESSAGE();
    PRINT N'   Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10));
END CATCH;

-- Bước 3: Kiểm tra lại
SELECT 
    HoaDonBanId,
    SoGoiMon,
    ThanhToan,
    TrangThaiThanhToan,
    PaymentStatus,
    MaGiaoDich,
    PaymentConfirmedAt
FROM HoaDonBan
WHERE HoaDonBanId = @HoaDonBanId;
