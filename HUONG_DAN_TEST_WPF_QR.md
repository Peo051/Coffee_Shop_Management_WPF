# Hướng dẫn Test QR Payment từ WPF

## Chuẩn bị

1. **Chạy backend API**:
```bash
cd CoffeeShop.PaymentApi
dotnet run
```
Backend chạy tại: https://localhost:5001

2. **Chạy WPF application**:
- Mở solution trong Visual Studio
- Set `CoffeeShop.Wpf` làm startup project
- Nhấn F5

---

## Test Flow

### 1. Tạo hóa đơn
1. Đăng nhập vào WPF
2. Mở màn hình "Bán hàng"
3. Chọn món → Thêm vào hóa đơn
4. Chọn hình thức thanh toán: "Tiền mặt"
5. Nhập tiền khách đưa
6. Bấm "💳 Thanh toán & tạo hóa đơn"
7. ✅ Kiểm tra: Thông báo thành công, khu vực QR payment hiển thị

### 2. Tạo QR payment
1. Bấm nút "📱 Tạo QR Thanh Toán"
2. ✅ Kiểm tra:
   - QR code hiển thị (200x200 px)
   - Trạng thái: "⏳ Chờ thanh toán"
   - Thời gian hết hạn hiển thị
   - Link checkout hiển thị
   - Nút "Kiểm tra" và "Hủy" hiển thị

### 3. Giả lập thanh toán (dùng Swagger)
1. Mở Swagger UI: https://localhost:5001
2. Tìm endpoint **POST /api/payments/payos/webhook**
3. Click **Try it out**
4. Lấy `orderCode` từ database:
```sql
SELECT TOP 1 ProviderOrderCode FROM HoaDonBan 
WHERE PaymentStatus = 'PENDING' 
ORDER BY HoaDonBanId DESC;
```
5. Nhập request body:
```json
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 1714567890000001,
    "amount": 50000,
    "reference": "FT26115123456",
    "code": "00"
  }
}
```
6. Click **Execute**

### 4. Kiểm tra trạng thái từ WPF
1. Quay lại WPF
2. Bấm nút "🔄 Kiểm Tra"
3. ✅ Kiểm tra:
   - Thông báo: "🎉 Thanh toán thành công! Mã giao dịch: FT26115123456"
   - QR code biến mất
   - Thông báo xanh: "✅ Đã thanh toán thành công!"
   - Có thể in bill / chuyển pha chế

### 5. Test hủy QR
1. Tạo hóa đơn mới
2. Tạo QR payment
3. Bấm nút "❌ Hủy QR"
4. ✅ Kiểm tra:
   - Thông báo: "✅ Đã hủy QR payment."
   - QR code biến mất
   - Có thể tạo QR mới

### 6. Test làm mới
1. Bấm nút "Làm mới"
2. ✅ Kiểm tra:
   - Khu vực QR payment biến mất
   - Form reset về trạng thái ban đầu

---

## Kiểm tra Database

### Sau khi tạo QR:
```sql
SELECT HoaDonBanId, PaymentProvider, ProviderOrderCode, 
       QRCodeRaw, CheckoutUrl, PaymentStatus, QRExpiredAt
FROM HoaDonBan
WHERE HoaDonBanId = 1;
```

**Expected**:
- `PaymentProvider` = "payOS"
- `ProviderOrderCode` = số long (timestamp * 100000 + hoaDonBanId)
- `QRCodeRaw` = URL QR code
- `CheckoutUrl` = URL checkout
- `PaymentStatus` = "PENDING"
- `QRExpiredAt` = thời gian hết hạn (now + 10 phút)

### Sau khi webhook xác nhận:
```sql
SELECT HoaDonBanId, TrangThaiThanhToan, PaymentStatus, 
       MaGiaoDich, PaymentConfirmedAt
FROM HoaDonBan
WHERE HoaDonBanId = 1;
```

**Expected**:
- `TrangThaiThanhToan` = "Đã thanh toán"
- `PaymentStatus` = "PAID"
- `MaGiaoDich` = "FT26115123456"
- `PaymentConfirmedAt` = thời gian xác nhận

---

## Troubleshooting

### Lỗi: "Không thể tạo QR thanh toán"
→ Kiểm tra backend API đang chạy tại https://localhost:5001

### QR code không hiển thị
→ Kiểm tra `QrCodeUrl` có giá trị hợp lệ không (URL image)

### Nút "Tạo QR" không hiển thị
→ Kiểm tra `HoaDonBanIdDaTao > 0` (đã lưu hóa đơn thành công chưa)

### Webhook không cập nhật
→ Kiểm tra `orderCode` trong webhook payload có khớp với database không

---

## Quick Test Commands

### Tạo hóa đơn test:
```sql
INSERT INTO HoaDonBan (NgayBan, TongTien, GiamGia, ThanhToan, CreatedByUserId, TrangThaiThanhToan)
VALUES (GETDATE(), 50000, 0, 50000, 1, N'Chưa thanh toán');

SELECT TOP 1 HoaDonBanId FROM HoaDonBan ORDER BY HoaDonBanId DESC;
```

### Kiểm tra QR payment:
```sql
SELECT * FROM HoaDonBan WHERE PaymentStatus IS NOT NULL ORDER BY HoaDonBanId DESC;
```

### Reset QR payment (để test lại):
```sql
UPDATE HoaDonBan
SET PaymentStatus = NULL,
    PaymentProvider = NULL,
    ProviderPaymentId = NULL,
    ProviderOrderCode = NULL,
    QRCodeRaw = NULL,
    CheckoutUrl = NULL,
    QRExpiredAt = NULL,
    PaymentConfirmedAt = NULL
WHERE HoaDonBanId = 1;
```

---

## Kết quả mong đợi

✅ Tạo hóa đơn thành công  
✅ Tạo QR payment thành công  
✅ QR code hiển thị trên WPF  
✅ Kiểm tra trạng thái thành công  
✅ Webhook cập nhật database  
✅ WPF hiển thị thông báo thanh toán thành công  
✅ Hủy QR thành công  
✅ Làm mới reset QR payment state  
