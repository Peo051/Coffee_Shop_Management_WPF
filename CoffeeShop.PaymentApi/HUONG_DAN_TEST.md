# HƯỚNG DẪN TEST BACKEND API - QR PAYMENT

## Tổng quan các cải tiến đã hoàn thành

### 1. Chuẩn hóa trạng thái EXPIRED
- **Rule**: Nếu `PaymentStatus = PENDING` và `QRExpiredAt < thời gian hiện tại` → trả về `EXPIRED`
- **Áp dụng**: Endpoint `GET /api/payments/{hoaDonBanId}/status`
- **File**: `PaymentRepository.cs` - method `GetPaymentStatusAsync`

### 2. Siết lại rule QR active
- **Rule QR active**:
  - `PaymentStatus = PENDING`
  - `QRExpiredAt > now` (chưa hết hạn)
  - Chưa `PAID`
  - Chưa `CANCELLED`
- **Hành vi**:
  - Nếu có QR active → trả lại QR cũ
  - Nếu QR đã EXPIRED hoặc CANCELLED → tạo QR mới
- **File**: `PayOsPaymentGatewayService.cs` - method `CreateQrAsync`

### 3. Webhook security
- **Verify signature**: Dùng HMAC SHA256 với `ChecksumKey`
- **Idempotency**: Không update lại nếu đã PAID
- **Hành vi**:
  - Nếu signature sai → return false, không update DB
  - Nếu signature đúng → tiếp tục xử lý
  - Mock mode (không có ChecksumKey) → skip verification
- **File**: `PayOsPaymentGatewayService.cs` - method `HandleWebhookAsync` và `VerifyWebhookSignature`

### 4. Không cho cancel nếu EXPIRED
- **Rule**: Không cho cancel nếu QR đã hết hạn
- **Áp dụng**:
  - Service: `PayOsPaymentGatewayService.cs` - method `CancelAsync`
  - Repository: `PaymentRepository.cs` - method `MarkPaymentCancelledAsync` (thêm check `QRExpiredAt > GETDATE()`)

---

## Chuẩn bị test

### 1. Cấu hình appsettings.json
```json
{
  "ConnectionStrings": {
    "CoffeeShopDb": "Server=.;Database=CoffeeShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "PayOS": {
    "ClientId": "",
    "ApiKey": "",
    "ChecksumKey": "",
    "BaseUrl": "https://api-merchant.payos.vn"
  },
  "AllowedOrigins": [
    "http://localhost",
    "app://wpf"
  ]
}
```

**Lưu ý**: Để trống `ClientId`, `ApiKey`, `ChecksumKey` để chạy mock mode.

### 2. Chạy backend API
```bash
cd CoffeeShop.PaymentApi
dotnet run
```

Backend sẽ chạy tại: `https://localhost:5001`

### 3. Mở Swagger UI
Truy cập: `https://localhost:5001/swagger`

---

## Test cases

### TEST 1: Tạo QR payment mới
**Endpoint**: `POST /api/payments/qr/create`

**Request body**:
```json
{
  "hoaDonBanId": 1
}
```

**Expected response** (200 OK):
```json
{
  "hoaDonBanId": 1,
  "paymentProvider": "payOS",
  "providerPaymentId": "guid-string",
  "providerOrderCode": 1234567890001,
  "qrCodeRaw": "https://img.vietqr.io/image/...",
  "checkoutUrl": "https://pay.payos.vn/web/1234567890001",
  "paymentStatus": "PENDING",
  "qrExpiredAt": "2026-04-25T10:20:00",
  "amount": 50000,
  "description": "HD00001"
}
```

**Kiểm tra**:
- ✅ QR được tạo thành công
- ✅ `PaymentStatus = PENDING`
- ✅ `QRExpiredAt` = now + 10 phút

---

### TEST 2: Tạo QR lại khi QR active còn hiệu lực
**Endpoint**: `POST /api/payments/qr/create`

**Request body**:
```json
{
  "hoaDonBanId": 1
}
```

**Expected response** (200 OK):
- Trả lại QR cũ (cùng `providerOrderCode`, `qrCodeRaw`, `checkoutUrl`)

**Kiểm tra**:
- ✅ Không tạo QR mới
- ✅ Trả lại thông tin QR cũ

---

### TEST 3: Kiểm tra trạng thái thanh toán (PENDING)
**Endpoint**: `GET /api/payments/1/status`

**Expected response** (200 OK):
```json
{
  "hoaDonBanId": 1,
  "paymentProvider": "payOS",
  "providerPaymentId": "guid-string",
  "providerOrderCode": 1234567890001,
  "paymentStatus": "PENDING",
  "maGiaoDich": null,
  "paymentConfirmedAt": null,
  "thanhToan": 50000,
  "trangThaiThanhToan": "Chưa thanh toán",
  "qrExpiredAt": "2026-04-25T10:20:00"
}
```

**Kiểm tra**:
- ✅ `PaymentStatus = PENDING`
- ✅ `QRExpiredAt` hiển thị đúng

---

### TEST 4: Kiểm tra trạng thái EXPIRED
**Cách test**: Đợi QR hết hạn (10 phút) hoặc sửa DB thủ công:

```sql
UPDATE dbo.HoaDonBan
SET QRExpiredAt = DATEADD(MINUTE, -1, GETDATE())
WHERE HoaDonBanId = 1;
```

**Endpoint**: `GET /api/payments/1/status`

**Expected response** (200 OK):
```json
{
  "hoaDonBanId": 1,
  "paymentStatus": "EXPIRED",
  ...
}
```

**Kiểm tra**:
- ✅ `PaymentStatus = EXPIRED` (mặc dù DB vẫn là PENDING)
- ✅ Logic chuẩn hóa hoạt động đúng

---

### TEST 5: Tạo QR mới sau khi QR cũ EXPIRED
**Endpoint**: `POST /api/payments/qr/create`

**Request body**:
```json
{
  "hoaDonBanId": 1
}
```

**Expected response** (200 OK):
- Tạo QR mới với `providerOrderCode` mới

**Kiểm tra**:
- ✅ Tạo QR mới thành công
- ✅ `providerOrderCode` khác với QR cũ
- ✅ `QRExpiredAt` mới = now + 10 phút

---

### TEST 6: Webhook thanh toán thành công (mock mode)
**Endpoint**: `POST /api/payments/payos/webhook`

**Request body**:
```json
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 1234567890001,
    "amount": 50000,
    "description": "HD00001",
    "reference": "FT123456789",
    "transactionDateTime": "2026-04-25T10:15:00",
    "paymentLinkId": "guid-string",
    "code": "00",
    "desc": "Thanh toán thành công"
  }
}
```

**Expected response** (200 OK)

**Kiểm tra**:
- ✅ Webhook xử lý thành công
- ✅ DB cập nhật: `PaymentStatus = PAID`, `TrangThaiThanhToan = 'Đã thanh toán'`
- ✅ `MaGiaoDich = FT123456789`

**Verify trong DB**:
```sql
SELECT PaymentStatus, TrangThaiThanhToan, MaGiaoDich, PaymentConfirmedAt
FROM dbo.HoaDonBan
WHERE HoaDonBanId = 1;
```

---

### TEST 7: Webhook idempotency (gửi lại webhook)
**Endpoint**: `POST /api/payments/payos/webhook`

**Request body**: (giống TEST 6)

**Expected response** (200 OK)

**Kiểm tra**:
- ✅ Không update lại DB
- ✅ Log ghi "Payment already confirmed (idempotency)"

---

### TEST 8: Webhook với signature sai (nếu có ChecksumKey)
**Lưu ý**: Test này chỉ chạy khi đã cấu hình `ChecksumKey` trong appsettings.json

**Endpoint**: `POST /api/payments/payos/webhook`

**Request body**:
```json
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 1234567890001,
    "amount": 50000,
    "description": "HD00001"
  },
  "signature": "invalid-signature-here"
}
```

**Expected response** (200 OK, nhưng không update DB)

**Kiểm tra**:
- ✅ Log ghi "Webhook signature verification failed"
- ✅ DB không bị update

---

### TEST 9: Cancel QR payment (PENDING)
**Endpoint**: `POST /api/payments/1/cancel`

**Expected response** (200 OK):
```json
true
```

**Kiểm tra**:
- ✅ DB cập nhật: `PaymentStatus = CANCELLED`

**Verify trong DB**:
```sql
SELECT PaymentStatus
FROM dbo.HoaDonBan
WHERE HoaDonBanId = 1;
```

---

### TEST 10: Không cho cancel nếu đã PAID
**Chuẩn bị**: Tạo QR mới và webhook để PAID

**Endpoint**: `POST /api/payments/1/cancel`

**Expected response** (200 OK):
```json
false
```

**Kiểm tra**:
- ✅ Không cancel được
- ✅ Log ghi "Cannot cancel paid payment"

---

### TEST 11: Không cho cancel nếu đã EXPIRED
**Chuẩn bị**: Tạo QR mới và sửa DB để EXPIRED

```sql
UPDATE dbo.HoaDonBan
SET QRExpiredAt = DATEADD(MINUTE, -1, GETDATE())
WHERE HoaDonBanId = 2;
```

**Endpoint**: `POST /api/payments/2/cancel`

**Expected response** (200 OK):
```json
false
```

**Kiểm tra**:
- ✅ Không cancel được
- ✅ Log ghi "Cannot cancel expired payment"

---

### TEST 12: Tạo QR mới sau khi CANCELLED
**Chuẩn bị**: Cancel QR (TEST 9)

**Endpoint**: `POST /api/payments/qr/create`

**Request body**:
```json
{
  "hoaDonBanId": 1
}
```

**Expected response** (200 OK):
- Tạo QR mới với `providerOrderCode` mới

**Kiểm tra**:
- ✅ Tạo QR mới thành công
- ✅ `PaymentStatus = PENDING` (ghi đè CANCELLED)

---

## Test với Postman

### Import collection
Tạo Postman collection với các request trên.

### Environment variables
```
base_url = https://localhost:5001
hoaDonBanId = 1
orderCode = 1234567890001
```

### Test flow
1. Create QR → lưu `orderCode` từ response
2. Get Status → verify PENDING
3. Webhook → verify PAID
4. Get Status → verify PAID
5. Cancel → verify không cancel được

---

## Troubleshooting

### Lỗi: "Connection string not found"
- Kiểm tra `appsettings.json` có key `CoffeeShopDb` chưa

### Lỗi: "HoaDonBan not found"
- Kiểm tra DB có hóa đơn với `HoaDonBanId` đó chưa
- Chạy query: `SELECT * FROM dbo.HoaDonBan WHERE HoaDonBanId = 1`

### Lỗi: "Cannot cancel paid payment"
- Đúng rồi! Đây là rule nghiệp vụ

### Webhook không update DB
- Kiểm tra `orderCode` có khớp với DB không
- Kiểm tra `code = "00"` trong payload
- Kiểm tra log để xem lỗi gì

---

## Kết luận

Tất cả các cải tiến đã hoàn thành:
- ✅ Chuẩn hóa EXPIRED
- ✅ Siết lại rule QR active
- ✅ Webhook signature verification
- ✅ Không cho cancel EXPIRED

Backend API đã sẵn sàng để tích hợp với WPF!
