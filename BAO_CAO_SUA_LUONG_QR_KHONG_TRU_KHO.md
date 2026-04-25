# BÁO CÁO SỬA LUỒNG QR PAYMENT - KHÔNG TRỪ KHO KHI PENDING

## Tổng quan

Đã sửa luồng QR Payment để **KHÔNG trừ kho/nguyên liệu/điểm** khi tạo hóa đơn pending. Chỉ finalize (trừ kho/nguyên liệu/điểm) khi webhook xác nhận PAID từ backend.

---

## File đã chỉnh

### 1. WPF - Repository Layer
**File**: `CoffeeShop.Wpf/Repositories/IHoaDonBanRepository.cs`
- Thêm parameter `bool skipInventoryDeduction = false` vào method `CreateAsync`

**File**: `CoffeeShop.Wpf/Repositories/HoaDonBanRepository.cs`
- Thêm parameter `bool skipInventoryDeduction = false` vào method `CreateAsync`
- Bọc toàn bộ logic trừ kho/nguyên liệu trong điều kiện `if (!skipInventoryDeduction)`
- Bọc logic cập nhật điểm khách hàng trong điều kiện `if (!skipInventoryDeduction && ...)`
- Vẫn insert `HoaDonBan` và `ChiTietHoaDonBan` bình thường

### 2. WPF - Service Layer
**File**: `CoffeeShop.Wpf/Services/HoaDonBanService.cs`
- Truyền `isQrPendingPayment` vào `CreateAsync` của repository
- Khi `isQrPendingPayment = true`, repository sẽ skip trừ kho/nguyên liệu/điểm

### 3. Backend PaymentApi - Repository Layer
**File**: `CoffeeShop.PaymentApi/Repositories/IPaymentRepository.cs`
- Thêm method mới: `Task<(bool Success, string? ErrorMessage)> FinalizeQrPaymentAsync(...)`

**File**: `CoffeeShop.PaymentApi/Repositories/PaymentRepository.cs`
- Thêm method `FinalizeQrPaymentAsync` với logic:
  1. Kiểm tra hóa đơn tồn tại và đang ở trạng thái "Chờ thanh toán"
  2. Idempotency: Nếu đã PAID rồi thì return success
  3. Lấy chi tiết hóa đơn
  4. Kiểm tra tồn kho món
  5. Kiểm tra tồn kho nguyên liệu
  6. Trừ tồn kho món và ghi lịch sử
  7. Trừ tồn kho nguyên liệu và ghi lịch sử
  8. Cập nhật điểm khách hàng (trừ điểm dùng, cộng điểm mới)
  9. Cập nhật trạng thái hóa đơn sang "Đã thanh toán"
  10. Commit transaction

### 4. Backend PaymentApi - Service Layer
**File**: `CoffeeShop.PaymentApi/Services/PayOsPaymentGatewayService.cs`
- Sửa `HandleWebhookAsync`: Gọi `FinalizeQrPaymentAsync` thay vì `MarkPaymentPaidAsync`
- Xử lý error message từ finalize để log chi tiết

---

## Khi tạo QR pending, những bước nào đã được bỏ qua

### Trong WPF khi tạo hóa đơn QR pending (`isQrPendingPayment = true`)

**Các bước VẪN THỰC HIỆN**:
- ✅ Insert `HoaDonBan` với `TrangThaiThanhToan = "Chờ thanh toán"`
- ✅ Insert `ChiTietHoaDonBan`
- ✅ Tính `SoThuTuGoiMon` (số gọi món)
- ✅ Gọi backend tạo QR

**Các bước BỊ BỎ QUA** (nhờ `skipInventoryDeduction = true`):
- ❌ KHÔNG lấy công thức món
- ❌ KHÔNG kiểm tra tồn kho nguyên liệu
- ❌ KHÔNG lấy tồn kho thành phẩm
- ❌ KHÔNG trừ `Mon.TonKho`
- ❌ KHÔNG ghi `LichSuTonKho`
- ❌ KHÔNG trừ `NguyenLieu.TonKho`
- ❌ KHÔNG ghi `LichSuNguyenLieu`
- ❌ KHÔNG trừ `KhachHang.DiemTichLuy` (điểm sử dụng)
- ❌ KHÔNG cộng `KhachHang.DiemTichLuy` (điểm mới)

### Kết quả
- Hóa đơn được tạo với trạng thái "Chờ thanh toán"
- Tồn kho món/nguyên liệu KHÔNG bị thay đổi
- Điểm khách hàng KHÔNG bị thay đổi
- Nếu khách không thanh toán, dữ liệu kho/điểm vẫn chính xác

---

## Khi webhook PAID, finalize thanh toán thực hiện những bước nào

### Trong Backend PaymentApi khi nhận webhook PAID

**Method**: `PaymentRepository.FinalizeQrPaymentAsync`

**Các bước thực hiện** (trong transaction):

1. **Kiểm tra hóa đơn**:
   - Lấy hóa đơn với `UPDLOCK, HOLDLOCK` để tránh race condition
   - Kiểm tra `TrangThaiThanhToan = "Chờ thanh toán"`
   - Lấy `CreatedByUserId`, `KhachHangId`, `DiemSuDung`, `DiemCong`

2. **Idempotency**:
   - Nếu `PaymentStatus = "PAID"` hoặc `TrangThaiThanhToan = "Đã thanh toán"` → return success
   - Webhook gửi lại không làm trừ kho/điểm lặp

3. **Validate trạng thái**:
   - Nếu không phải "Chờ thanh toán" → return error
   - Tránh finalize nhầm hóa đơn đã thanh toán thường

4. **Lấy chi tiết hóa đơn**:
   - Query `ChiTietHoaDonBan` để biết cần trừ món gì, số lượng bao nhiêu

5. **Kiểm tra tồn kho món**:
   - Với mỗi món trong chi tiết, kiểm tra `Mon.TonKho >= SoLuong`
   - Nếu không đủ → rollback transaction, return error

6. **Kiểm tra tồn kho nguyên liệu**:
   - Lấy công thức món từ `CongThucMon`
   - Với mỗi nguyên liệu, tính `soLuongCanDung = DinhLuong * SoLuong`
   - Kiểm tra `NguyenLieu.TonKho >= soLuongCanDung`
   - Nếu không đủ → rollback transaction, return error

7. **Trừ tồn kho món**:
   - Lấy `TonTruoc` từ `Mon.TonKho`
   - `UPDATE Mon SET TonKho = TonKho - SoLuong WHERE MonId = @MonId AND TonKho >= @SoLuong`
   - Tính `TonSau = TonTruoc - SoLuong`
   - `INSERT INTO LichSuTonKho` với `LoaiPhatSinh = 'BanHang'`, `GhiChu = 'QR Payment finalized'`

8. **Trừ tồn kho nguyên liệu**:
   - Với mỗi nguyên liệu trong công thức:
     - Lấy `TonTruoc` từ `NguyenLieu.TonKho`
     - `UPDATE NguyenLieu SET TonKho = TonKho - SoLuong WHERE NguyenLieuId = @NguyenLieuId AND TonKho >= @SoLuong`
     - Tính `TonSau = TonTruoc - SoLuong`
     - `INSERT INTO LichSuNguyenLieu` với `LoaiPhatSinh = 'XuatKho'`, `GhiChu = 'QR Payment finalized cho món ...'`

9. **Cập nhật điểm khách hàng**:
   - Nếu có `KhachHangId` và (`DiemSuDung > 0` hoặc `DiemCong > 0`):
     - `UPDATE KhachHang SET DiemTichLuy = DiemTichLuy - @DiemSuDung + @DiemCong`
     - `WHERE KhachHangId = @KhachHangId AND IsActive = 1 AND DiemTichLuy >= @DiemSuDung`
     - Nếu không đủ điểm → rollback transaction, return error

10. **Cập nhật trạng thái hóa đơn**:
    - `UPDATE HoaDonBan SET PaymentStatus = 'PAID', TrangThaiThanhToan = N'Đã thanh toán', MaGiaoDich = @MaGiaoDich, PaymentConfirmedAt = @PaymentConfirmedAt`

11. **Commit transaction**:
    - Nếu tất cả bước trên thành công → commit
    - Nếu có lỗi bất kỳ → rollback, return error message

### Kết quả
- Hóa đơn chuyển sang "Đã thanh toán"
- Tồn kho món/nguyên liệu bị trừ
- Điểm khách hàng được cập nhật
- Lịch sử tồn kho/nguyên liệu được ghi nhận
- Tất cả trong 1 transaction, đảm bảo tính toàn vẹn dữ liệu

---

## Cách chống webhook trừ kho/điểm lặp

### 1. Idempotency check đầu tiên
```csharp
// Nếu đã PAID rồi thì return success
if (paymentStatus == "PAID" || trangThaiThanhToan == "Đã thanh toán")
{
    await transaction.CommitAsync(cancellationToken);
    _logger.LogInformation("Payment already finalized for HoaDonBan {HoaDonBanId} (idempotency)", hoaDonBanId);
    return (true, null);
}
```

- Webhook gửi lại nhiều lần, lần đầu finalize thành công
- Lần 2, 3, 4... check thấy đã PAID → return success ngay, không trừ kho/điểm lặp

### 2. Transaction isolation với UPDLOCK, HOLDLOCK
```sql
SELECT HoaDonBanId, TrangThaiThanhToan, PaymentStatus, ...
FROM dbo.HoaDonBan WITH (UPDLOCK, HOLDLOCK)
WHERE HoaDonBanId = @HoaDonBanId
```

- `UPDLOCK`: Lock để update, tránh 2 webhook cùng lúc đọc trạng thái cũ
- `HOLDLOCK`: Giữ lock đến khi transaction kết thúc
- Đảm bảo chỉ 1 webhook finalize được, webhook khác phải đợi

### 3. Conditional update trong SQL
```sql
UPDATE dbo.Mon
SET TonKho = TonKho - @SoLuong
WHERE MonId = @MonId AND TonKho >= @SoLuong
```

- Chỉ update nếu `TonKho >= SoLuong`
- Nếu đã trừ rồi, `TonKho` không đủ → update fail → rollback

### 4. Check trạng thái trước khi finalize
```csharp
if (trangThaiThanhToan != "Chờ thanh toán")
{
    await transaction.RollbackAsync(cancellationToken);
    return (false, $"Hóa đơn không ở trạng thái 'Chờ thanh toán'");
}
```

- Chỉ finalize nếu đang "Chờ thanh toán"
- Tránh finalize nhầm hóa đơn thanh toán thường

### 5. Toàn bộ trong transaction
- Tất cả bước trừ kho/nguyên liệu/điểm/update trạng thái trong 1 transaction
- Nếu có lỗi bất kỳ → rollback toàn bộ
- Đảm bảo tính nguyên tử (atomicity)

---

## Các test case cần chạy

### TEST 1: Tạo hóa đơn QR pending - Kiểm tra KHÔNG trừ kho
**Bước**:
1. Kiểm tra tồn kho món trước: `SELECT TonKho FROM Mon WHERE MonId = 1` → Giả sử = 100
2. Kiểm tra tồn kho nguyên liệu trước: `SELECT TonKho FROM NguyenLieu WHERE NguyenLieuId = 1` → Giả sử = 50
3. Kiểm tra điểm khách hàng trước: `SELECT DiemTichLuy FROM KhachHang WHERE KhachHangId = 1` → Giả sử = 200
4. Trong WPF, chọn món (MonId = 1, SoLuong = 5), chọn khách hàng (KhachHangId = 1, dùng 10 điểm)
5. Chọn hình thức thanh toán: `QR Payment`
6. Bấm `Tạo hóa đơn chờ thanh toán QR`

**Kỳ vọng**:
- ✅ Hóa đơn được tạo với `TrangThaiThanhToan = "Chờ thanh toán"`, `PaymentStatus = "PENDING"`
- ✅ QR code được hiển thị
- ✅ Tồn kho món VẪN = 100 (KHÔNG bị trừ)
- ✅ Tồn kho nguyên liệu VẪN = 50 (KHÔNG bị trừ)
- ✅ Điểm khách hàng VẪN = 200 (KHÔNG bị trừ)
- ✅ KHÔNG có record trong `LichSuTonKho`
- ✅ KHÔNG có record trong `LichSuNguyenLieu`

**Verify SQL**:
```sql
SELECT TonKho FROM Mon WHERE MonId = 1; -- Phải = 100
SELECT TonKho FROM NguyenLieu WHERE NguyenLieuId = 1; -- Phải = 50
SELECT DiemTichLuy FROM KhachHang WHERE KhachHangId = 1; -- Phải = 200
SELECT COUNT(*) FROM LichSuTonKho WHERE HoaDonBanId = @NewHoaDonBanId; -- Phải = 0
SELECT COUNT(*) FROM LichSuNguyenLieu WHERE HoaDonBanId = @NewHoaDonBanId; -- Phải = 0
```

---

### TEST 2: Webhook PAID - Kiểm tra finalize trừ kho thành công
**Bước**:
1. Sau TEST 1, gửi webhook giả lập:
```json
POST https://localhost:5001/api/payments/payos/webhook
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 1234567890001,
    "amount": 50000,
    "description": "HD00001",
    "reference": "FT123456789",
    "code": "00"
  }
}
```

**Kỳ vọng**:
- ✅ Backend log: "Successfully finalized QR payment for HoaDonBan ..."
- ✅ Hóa đơn: `TrangThaiThanhToan = "Đã thanh toán"`, `PaymentStatus = "PAID"`, `MaGiaoDich = "FT123456789"`
- ✅ Tồn kho món = 100 - 5 = 95
- ✅ Tồn kho nguyên liệu = 50 - (DinhLuong * 5) (tùy công thức)
- ✅ Điểm khách hàng = 200 - 10 + DiemCong (tùy số tiền thanh toán)
- ✅ CÓ record trong `LichSuTonKho` với `LoaiPhatSinh = 'BanHang'`, `GhiChu = 'QR Payment finalized'`
- ✅ CÓ record trong `LichSuNguyenLieu` với `LoaiPhatSinh = 'XuatKho'`

**Verify SQL**:
```sql
SELECT TrangThaiThanhToan, PaymentStatus, MaGiaoDich FROM HoaDonBan WHERE HoaDonBanId = @HoaDonBanId;
SELECT TonKho FROM Mon WHERE MonId = 1; -- Phải = 95
SELECT DiemTichLuy FROM KhachHang WHERE KhachHangId = 1; -- Phải = 200 - 10 + DiemCong
SELECT * FROM LichSuTonKho WHERE HoaDonBanId = @HoaDonBanId;
SELECT * FROM LichSuNguyenLieu WHERE HoaDonBanId = @HoaDonBanId;
```

---

### TEST 3: Webhook gửi lại - Kiểm tra idempotency
**Bước**:
1. Sau TEST 2, gửi lại webhook giống hệt (lần 2)
2. Gửi lại lần 3, lần 4

**Kỳ vọng**:
- ✅ Backend log: "Payment already finalized for HoaDonBan ... (idempotency)"
- ✅ Tồn kho món VẪN = 95 (KHÔNG bị trừ lặp)
- ✅ Tồn kho nguyên liệu KHÔNG bị trừ lặp
- ✅ Điểm khách hàng KHÔNG bị trừ/cộng lặp
- ✅ KHÔNG có record mới trong `LichSuTonKho`
- ✅ KHÔNG có record mới trong `LichSuNguyenLieu`

---

### TEST 4: Webhook PAID nhưng không đủ tồn kho - Kiểm tra rollback
**Bước**:
1. Tạo hóa đơn QR pending với món (MonId = 2, SoLuong = 10)
2. Sửa DB thủ công: `UPDATE Mon SET TonKho = 5 WHERE MonId = 2` (giả lập hết hàng)
3. Gửi webhook PAID

**Kỳ vọng**:
- ✅ Backend log error: "Không đủ tồn kho món '...' (cần 10, còn 5)"
- ✅ Hóa đơn VẪN ở trạng thái "Chờ thanh toán" (KHÔNG chuyển sang "Đã thanh toán")
- ✅ Tồn kho món VẪN = 5 (KHÔNG bị trừ)
- ✅ Transaction bị rollback, không có thay đổi nào

---

### TEST 5: Webhook PAID nhưng không đủ nguyên liệu - Kiểm tra rollback
**Bước**:
1. Tạo hóa đơn QR pending với món cần nguyên liệu (MonId = 3, SoLuong = 5)
2. Sửa DB thủ công: `UPDATE NguyenLieu SET TonKho = 0 WHERE NguyenLieuId = 1` (giả lập hết nguyên liệu)
3. Gửi webhook PAID

**Kỳ vọng**:
- ✅ Backend log error: "Không đủ nguyên liệu '...' cho món '...' (cần X, còn 0)"
- ✅ Hóa đơn VẪN ở trạng thái "Chờ thanh toán"
- ✅ Tồn kho món KHÔNG bị trừ
- ✅ Tồn kho nguyên liệu KHÔNG bị trừ
- ✅ Transaction bị rollback

---

### TEST 6: Webhook PAID nhưng khách không đủ điểm - Kiểm tra rollback
**Bước**:
1. Tạo hóa đơn QR pending với khách hàng dùng 50 điểm
2. Sửa DB thủ công: `UPDATE KhachHang SET DiemTichLuy = 30 WHERE KhachHangId = 1` (giả lập không đủ điểm)
3. Gửi webhook PAID

**Kỳ vọng**:
- ✅ Backend log error: "Khách hàng không đủ điểm tích lũy (yêu cầu: 50 điểm)"
- ✅ Hóa đơn VẪN ở trạng thái "Chờ thanh toán"
- ✅ Tồn kho món KHÔNG bị trừ
- ✅ Tồn kho nguyên liệu KHÔNG bị trừ
- ✅ Điểm khách hàng VẪN = 30
- ✅ Transaction bị rollback

---

### TEST 7: Thanh toán thường - Kiểm tra vẫn trừ kho ngay
**Bước**:
1. Kiểm tra tồn kho món trước: `SELECT TonKho FROM Mon WHERE MonId = 4` → Giả sử = 80
2. Trong WPF, chọn món (MonId = 4, SoLuong = 3)
3. Chọn hình thức thanh toán: `Tiền mặt`
4. Nhập tiền khách đưa: 100000
5. Bấm `Thanh toán & tạo hóa đơn`

**Kỳ vọng**:
- ✅ Hóa đơn được tạo với `TrangThaiThanhToan = "Đã thanh toán"` NGAY LẬP TỨC
- ✅ Tồn kho món = 80 - 3 = 77 (bị trừ NGAY)
- ✅ Tồn kho nguyên liệu bị trừ NGAY
- ✅ Điểm khách hàng được cập nhật NGAY
- ✅ CÓ record trong `LichSuTonKho` với `LoaiPhatSinh = 'BanHang'`, `GhiChu = 'Bán hàng - Hóa đơn #...'`
- ✅ Form được reset để bán đơn mới

**Verify SQL**:
```sql
SELECT TrangThaiThanhToan FROM HoaDonBan WHERE HoaDonBanId = @NewHoaDonBanId; -- Phải = 'Đã thanh toán'
SELECT TonKho FROM Mon WHERE MonId = 4; -- Phải = 77
SELECT COUNT(*) FROM LichSuTonKho WHERE HoaDonBanId = @NewHoaDonBanId; -- Phải > 0
```

---

### TEST 8: Khách không thanh toán QR - Kiểm tra dữ liệu không bị lệch
**Bước**:
1. Tạo hóa đơn QR pending với món (MonId = 5, SoLuong = 2)
2. Kiểm tra tồn kho món: Giả sử = 60
3. KHÔNG gửi webhook (giả lập khách không thanh toán)
4. Đợi QR hết hạn (10 phút) hoặc hủy QR

**Kỳ vọng**:
- ✅ Hóa đơn VẪN ở trạng thái "Chờ thanh toán" hoặc "CANCELLED"
- ✅ Tồn kho món VẪN = 60 (KHÔNG bị lệch)
- ✅ Tồn kho nguyên liệu KHÔNG bị lệch
- ✅ Điểm khách hàng KHÔNG bị lệch
- ✅ Dữ liệu chính xác, không cần hoàn kho thủ công

---

## So sánh trước và sau

### Trước khi sửa (CÓ VẤN ĐỀ)
1. Tạo hóa đơn QR pending → Trừ kho/nguyên liệu/điểm NGAY
2. Nếu khách không thanh toán → Dữ liệu kho/điểm BỊ LỆCH
3. Cần hoàn kho/điểm thủ công → Phức tạp, dễ sai

### Sau khi sửa (ĐÚNG NGHIỆP VỤ)
1. Tạo hóa đơn QR pending → KHÔNG trừ kho/nguyên liệu/điểm
2. Webhook PAID → Finalize: Kiểm tra + Trừ kho/nguyên liệu/điểm + Cập nhật trạng thái
3. Nếu khách không thanh toán → Dữ liệu kho/điểm VẪN CHÍNH XÁC
4. Không cần hoàn kho thủ công → Đơn giản, an toàn

---

## Lưu ý quan trọng

### 1. Thanh toán thường KHÔNG bị ảnh hưởng
- Tiền mặt/Chuyển khoản/Thẻ/Ví thủ công vẫn trừ kho/nguyên liệu/điểm NGAY khi tạo hóa đơn
- Chỉ QR Payment mới delay việc trừ kho đến khi webhook xác nhận PAID

### 2. Finalize có thể fail
- Nếu không đủ kho/nguyên liệu/điểm khi webhook PAID → Finalize fail, transaction rollback
- Hóa đơn VẪN ở trạng thái "Chờ thanh toán"
- Cần xử lý thủ công: Hoàn tiền cho khách hoặc bổ sung kho rồi finalize lại

### 3. Race condition được xử lý
- `UPDLOCK, HOLDLOCK` đảm bảo chỉ 1 webhook finalize được
- Idempotency check đảm bảo webhook gửi lại không trừ kho/điểm lặp

### 4. Transaction đảm bảo tính toàn vẹn
- Tất cả bước finalize trong 1 transaction
- Nếu có lỗi bất kỳ → Rollback toàn bộ, không để dữ liệu lệch

### 5. Log chi tiết để debug
- Backend log mọi bước finalize
- Nếu fail, log error message chi tiết để xử lý

---

## Kết luận

Đã hoàn thành sửa luồng QR Payment để:
- ✅ KHÔNG trừ kho/nguyên liệu/điểm khi tạo hóa đơn pending
- ✅ CHỈ finalize (trừ kho/nguyên liệu/điểm) khi webhook xác nhận PAID
- ✅ Idempotency: Webhook gửi lại không trừ kho/điểm lặp
- ✅ Transaction: Đảm bảo tính toàn vẹn dữ liệu
- ✅ Rollback nếu không đủ kho/nguyên liệu/điểm
- ✅ Thanh toán thường KHÔNG bị ảnh hưởng

Hệ thống đã sẵn sàng để test và triển khai!
