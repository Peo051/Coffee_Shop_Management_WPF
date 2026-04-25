# Hướng dẫn sửa lỗi: Đã thanh toán nhưng hệ thống chưa nhận

## Vấn đề

Bạn đã thanh toán trên PayOS (hiển thị "Đã thanh toán" trên PayOS dashboard) nhưng trong hệ thống WPF vẫn hiển thị "Chờ thanh toán".

**Nguyên nhân:** Webhook từ PayOS không được gọi hoặc bị lỗi.

---

## Giải pháp 1: Sử dụng nút "Kiểm tra trạng thái" (Khuyến nghị)

### Bước 1: Trong WPF, bấm nút "Kiểm tra trạng thái"

Khi bấm nút này, backend sẽ:
1. Query PayOS API để lấy trạng thái thật
2. Nếu PayOS trả "PAID" → Tự động finalize payment
3. Cập nhật database và trừ kho

### Bước 2: Đợi vài giây

WPF sẽ hiển thị:
```
🎉 Thanh toán thành công! Mã giao dịch: REF-123456789. Có thể in bill và chuyển pha chế.
```

### Bước 3: Kiểm tra lại danh sách hóa đơn

Trạng thái sẽ chuyển từ "Chờ thanh toán" → "Đã thanh toán"

---

## Giải pháp 2: Cập nhật thủ công qua SQL (Nếu giải pháp 1 không hoạt động)

### Bước 1: Mở SQL Server Management Studio

Kết nối đến database: `CoffeeShopDB`

### Bước 2: Chạy script kiểm tra

```sql
-- Xem các hóa đơn đang chờ thanh toán
SELECT 
    HoaDonBanId,
    SoGoiMon,
    ThanhToan,
    HinhThucThanhToan,
    TrangThaiThanhToan,
    PaymentStatus,
    ProviderOrderCode,
    CreatedAt
FROM HoaDonBan
WHERE HinhThucThanhToan = N'QR Payment'
  AND TrangThaiThanhToan = N'Chờ thanh toán'
  AND PaymentStatus = 'PENDING'
ORDER BY CreatedAt DESC;
```

**Kết quả mẫu:**
```
HoaDonBanId | SoGoiMon | ThanhToan | TrangThaiThanhToan | PaymentStatus | ProviderOrderCode
29          | 005      | 2000      | Chờ thanh toán     | PENDING       | 1777124369000030
30          | 006      | 5000      | Chờ thanh toán     | PENDING       | 1777124242700029
```

### Bước 3: Chạy script cập nhật

Mở file: `database/FIX_MANUAL_UPDATE_PAID_INVOICES.sql`

**Sửa dòng này:**
```sql
DECLARE @HoaDonBanId INT = 29; -- ← THAY ĐỔI ID NÀY
```

Thay `29` bằng ID hóa đơn của bạn (ví dụ: 30 cho hóa đơn 5000đ)

### Bước 4: Execute script

Bấm F5 hoặc Execute

**Kết quả:**
```
✅ Đã cập nhật thành công hóa đơn 30
   - Trạng thái: Đã thanh toán
   - Mã giao dịch: MANUAL-FIX-30
   - Đã trừ kho món
   - Đã trừ nguyên liệu
   - Đã cộng 0 điểm tích lũy
```

### Bước 5: Kiểm tra lại trong WPF

Reload danh sách hóa đơn → Trạng thái đã chuyển "Đã thanh toán"

---

## Giải pháp 3: Cấu hình webhook đúng (Để tránh lỗi lần sau)

### Vấn đề: Webhook không được gọi

**Nguyên nhân:**
1. Ngrok đã tắt
2. Webhook URL chưa được cấu hình trên PayOS
3. Backend API không chạy

### Bước 1: Khởi động ngrok

```bash
ngrok http 5002
```

**Lấy URL:** `https://abc-xyz-123.ngrok-free.app`

### Bước 2: Cấu hình webhook trên PayOS

1. Đăng nhập: https://my.payos.vn
2. Vào **Settings** → **Webhook**
3. Nhập Webhook URL:
```
https://abc-xyz-123.ngrok-free.app/api/Payments/payos/webhook
```
4. Bấm **Save** hoặc **Test**

**Kỳ vọng:** PayOS báo "Webhook URL is valid" ✅

### Bước 3: Đảm bảo backend đang chạy

```bash
cd CoffeeShop.PaymentApi
dotnet run
```

Hoặc chạy từ Visual Studio (F5)

**Kiểm tra:** Mở browser `https://localhost:5002/swagger`

### Bước 4: Test lại

1. Tạo hóa đơn QR mới
2. Thanh toán trên PayOS
3. Đợi 5-10 giây
4. Kiểm tra WPF → Trạng thái tự động chuyển "Đã thanh toán"

---

## Tại sao webhook không hoạt động?

### 1. Ngrok đã tắt
```
Error: Failed to complete tunnel connection
```

**Giải pháp:** Khởi động lại ngrok

### 2. Backend không chạy
```
Error: Connection refused (localhost:5002)
```

**Giải pháp:** Chạy backend API

### 3. Webhook URL sai
```
PayOS: Webhook URL is invalid
```

**Giải pháp:** Kiểm tra lại URL, đảm bảo có `/api/Payments/payos/webhook`

### 4. Firewall chặn
```
Error: Timeout
```

**Giải pháp:** Tắt firewall hoặc thêm exception cho ngrok

---

## Kiểm tra logs backend

### Nếu webhook hoạt động, bạn sẽ thấy logs:

```
info: Received payOS webhook. Payload length: 1234
info: Webhook signature verified successfully for OrderCode 1777124369000030
info: Payment finalized successfully for HoaDonBan 30, OrderCode: 1777124369000030
info: Webhook processed successfully: Payment finalized successfully for invoice 30
```

### Nếu webhook KHÔNG hoạt động, không có logs nào

→ Nghĩa là PayOS không gọi được vào backend

---

## Checklist khắc phục

### Giải pháp nhanh (không cần webhook):
- [ ] Bấm nút "Kiểm tra trạng thái" trong WPF
- [ ] Đợi backend query PayOS
- [ ] Kiểm tra trạng thái đã chuyển "Đã thanh toán"

### Nếu vẫn không hoạt động:
- [ ] Chạy script SQL thủ công
- [ ] Thay đổi `@HoaDonBanId` thành ID đúng
- [ ] Execute script
- [ ] Kiểm tra lại WPF

### Để tránh lỗi lần sau:
- [ ] Khởi động ngrok
- [ ] Cấu hình webhook URL trên PayOS
- [ ] Đảm bảo backend đang chạy
- [ ] Test webhook bằng cách tạo hóa đơn mới

---

## Tóm tắt

| Giải pháp | Ưu điểm | Nhược điểm |
|-----------|---------|------------|
| **Nút "Kiểm tra trạng thái"** | Tự động, an toàn, không cần SQL | Cần backend chạy và có PayOS credentials |
| **Script SQL thủ công** | Nhanh, không cần backend | Phải chạy thủ công, dễ nhầm ID |
| **Cấu hình webhook đúng** | Tự động hoàn toàn | Cần ngrok, phức tạp hơn |

**Khuyến nghị:** Dùng nút "Kiểm tra trạng thái" trước, nếu không được thì dùng SQL.

---

## Lưu ý quan trọng

### ⚠️ Không chạy script SQL nhiều lần cho cùng 1 hóa đơn

Script có kiểm tra idempotency:
```sql
WHERE HoaDonBanId = @HoaDonBanId
  AND TrangThaiThanhToan = N'Chờ thanh toán';
```

Nếu hóa đơn đã "Đã thanh toán" rồi, script sẽ báo lỗi:
```
❌ Lỗi: Hóa đơn không tồn tại hoặc đã thanh toán rồi
```

### ✅ An toàn

- Không trừ kho lặp
- Không trừ điểm lặp
- Transaction đảm bảo tính toàn vẹn dữ liệu

---

## Câu hỏi thường gặp

### Q: Tại sao không tự động finalize khi tôi thanh toán?
**A:** Webhook từ PayOS không được gọi. Nguyên nhân thường là ngrok đã tắt hoặc webhook URL chưa cấu hình.

### Q: Tôi có thể finalize thủ công không?
**A:** Có, dùng nút "Kiểm tra trạng thái" hoặc chạy script SQL.

### Q: Nếu tôi chạy script SQL nhiều lần thì sao?
**A:** Script có idempotency check, sẽ báo lỗi nếu hóa đơn đã thanh toán rồi.

### Q: Làm sao biết webhook có hoạt động không?
**A:** Kiểm tra logs backend. Nếu có dòng "Received payOS webhook" → Webhook hoạt động.

### Q: Tôi nên dùng giải pháp nào?
**A:** 
1. Thử nút "Kiểm tra trạng thái" trước (dễ nhất)
2. Nếu không được, dùng script SQL
3. Cấu hình webhook đúng để tránh lỗi lần sau
