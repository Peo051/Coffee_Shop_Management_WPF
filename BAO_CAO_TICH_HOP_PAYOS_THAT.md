# Báo cáo tích hợp PayOS API thật

## Tổng quan
Đã thay thế mock implementation bằng tích hợp PayOS API thật trong backend `CoffeeShop.PaymentApi`.

## Files đã chỉnh

### 1. Models mới (tạo mới)
- **`CoffeeShop.PaymentApi/Models/PayOsCreatePaymentRequest.cs`**
  - Model cho request tạo payment gửi đến PayOS API
  - Chứa: orderCode, amount, description, cancelUrl, returnUrl, expiredAt, signature

- **`CoffeeShop.PaymentApi/Models/PayOsCreatePaymentResponse.cs`**
  - Model cho response từ PayOS API
  - Map data.paymentLinkId → ProviderPaymentId
  - Map data.orderCode → ProviderOrderCode
  - Map data.qrCode → QRCodeRaw
  - Map data.checkoutUrl → CheckoutUrl

### 2. Configuration (cập nhật)
- **`CoffeeShop.PaymentApi/appsettings.json`**
  - Thêm `ReturnUrl`: "https://localhost:5001/payment/success"
  - Thêm `CancelUrl`: "https://localhost:5001/payment/cancel"

- **`CoffeeShop.PaymentApi/appsettings.Development.json`**
  - Thêm `ReturnUrl` và `CancelUrl` tương tự

### 3. Service (cập nhật)
- **`CoffeeShop.PaymentApi/Services/PayOsPaymentGatewayService.cs`**

#### Thay mock create payment bằng API thật
**Method**: `CallPayOsCreatePaymentAsync`
- Kiểm tra credentials: nếu rỗng → dùng mock mode
- Nếu có credentials:
  - Tạo payload với orderCode, amount, description, cancelUrl, returnUrl, expiredAt
  - Tạo signature bằng `CreatePayOsSignature`
  - Gọi `POST {BaseUrl}/v2/payment-requests`
  - Headers: `x-client-id`, `x-api-key`
  - Parse response và map vào (qrCode, checkoutUrl, paymentLinkId)
  - Log chi tiết và xử lý lỗi HTTP

#### Signature payment request
**Method**: `CreatePayOsSignature` (mới tạo)
- Input: `IDictionary<string, object?> data`
- Sắp xếp keys theo alphabet
- Tạo chuỗi: `key1=value1&key2=value2&...`
- Dùng HMAC-SHA256 với ChecksumKey
- Trả về hex lowercase
- Các field ký: amount, cancelUrl, description, orderCode, returnUrl, expiredAt (nếu có)

#### Verify webhook signature
**Method**: `VerifyWebhookSignature` (cập nhật)
- Đọc signature từ `webhookData.Signature`
- Lấy data object từ webhook
- Tạo dictionary với các field: amount, code, desc, orderCode, và các field optional
- Sắp xếp keys theo alphabet
- Tạo chuỗi signature tương tự create payment
- Dùng HMAC-SHA256 với ChecksumKey
- So sánh với signature nhận được
- **Nếu sai signature**: Log error, return false → webhook bị reject, KHÔNG update DB
- **Nếu đúng signature**: Log success, return true → xử lý PAID

#### Cancel payment
**Method**: `CallPayOsCancelPaymentAsync`
- Mock mode: chỉ log
- Production mode: 
  - Có comment `// TODO: call payOS cancel API here when production credentials are available`
  - Log warning rõ ràng: "Local DB updated but provider not notified"
  - Không giả vờ đã cancel provider

## Logic không thay đổi
✅ Không tạo QR nếu hóa đơn đã paid
✅ Trả lại QR cũ nếu còn active (PENDING + chưa hết hạn + chưa CANCELLED)
✅ Webhook idempotency: không update lặp nếu đã PAID
✅ Finalize khi PAID: trừ kho, nguyên liệu, điểm
✅ Status: PENDING / PAID / CANCELLED / EXPIRED

## Config cần điền trong appsettings.Development.json

```json
"PayOS": {
  "ClientId": "YOUR_CLIENT_ID_HERE",
  "ApiKey": "YOUR_API_KEY_HERE",
  "ChecksumKey": "YOUR_CHECKSUM_KEY_HERE",
  "BaseUrl": "https://api-merchant.payos.vn",
  "ReturnUrl": "https://localhost:5001/payment/success",
  "CancelUrl": "https://localhost:5001/payment/cancel"
}
```

### Lấy credentials từ đâu?
1. Đăng ký tài khoản PayOS tại: https://payos.vn
2. Vào Dashboard → Settings → API Keys
3. Copy 3 giá trị:
   - **Client ID**: Mã định danh merchant
   - **API Key**: Key để authenticate API calls
   - **Checksum Key**: Key để tạo và verify signature

### Chế độ hoạt động
- **Nếu để trống credentials**: Hệ thống tự động chạy mock mode (dùng QR giả)
- **Nếu điền đầy đủ credentials**: Hệ thống gọi PayOS API thật

## Test
1. **Mock mode** (credentials rỗng):
   - Tạo QR → nhận QR giả từ VietQR
   - Webhook → verify signature bị skip
   
2. **Production mode** (có credentials):
   - Tạo QR → gọi PayOS API thật, nhận QR thật
   - Webhook → verify signature bắt buộc, reject nếu sai

## Lưu ý
- Signature verification là bắt buộc khi có ChecksumKey
- Webhook với signature sai sẽ bị reject hoàn toàn, không update DB
- Cancel payment chưa gọi API thật, chỉ update local DB
- Tất cả config đọc từ appsettings, không hard-code
