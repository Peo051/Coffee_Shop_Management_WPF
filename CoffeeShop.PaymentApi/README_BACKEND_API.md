# CoffeeShop Payment API - Backend Documentation

## Tổng quan
Backend API xử lý thanh toán QR code cho hệ thống quản lý quán cafe.

**Kiến trúc**: WPF Client → ASP.NET Core Web API → payOS Payment Gateway

---

## Cấu hình ban đầu

### 1. Cài đặt packages
```bash
cd CoffeeShop.PaymentApi
dotnet restore
```

### 2. Cấu hình Database Connection String

Mở file `appsettings.Development.json` và sửa connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Ví dụ**:
- SQL Server local: `Server=.;Database=CoffeeShopDB;...`
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=CoffeeShopDB;...`
- SQL Server remote: `Server=192.168.1.100;Database=CoffeeShopDB;User Id=sa;Password=YourPassword;...`

### 3. Chạy SQL Migration

Chạy file migration để thêm các cột QR payment vào bảng `HoaDonBan`:

```sql
-- File: database/13_QRPayment_Integration.sql
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio
```

Migration sẽ thêm 8 cột mới:
- `PaymentProvider` - Tên provider (payOS, VietQR, ...)
- `ProviderPaymentId` - ID payment từ provider
- `ProviderOrderCode` - Mã đơn hàng duy nhất
- `QRCodeRaw` - URL hoặc data của QR code
- `CheckoutUrl` - Link checkout trên web
- `QRExpiredAt` - Thời gian hết hạn QR
- `PaymentStatus` - Trạng thái (PENDING, PAID, CANCELLED, EXPIRED)
- `PaymentConfirmedAt` - Thời gian xác nhận thanh toán

### 4. Cấu hình payOS Credentials

**QUAN TRỌNG**: Bạn cần đăng ký tài khoản payOS và lấy credentials tại: https://payos.vn

Sau khi có credentials, điền vào `appsettings.Development.json`:

```json
{
  "PayOS": {
    "ClientId": "YOUR_PAYOS_CLIENT_ID",
    "ApiKey": "YOUR_PAYOS_API_KEY",
    "ChecksumKey": "YOUR_PAYOS_CHECKSUM_KEY"
  }
}
```

**Lưu ý**: 
- Hiện tại service đang dùng **mock implementation** để test
- Khi tích hợp thật với payOS, cần cài package `Net.payOS` và sửa code trong `PayOsPaymentGatewayService.cs`

---

## Chạy API

### Khởi động server

```bash
cd CoffeeShop.PaymentApi
dotnet run
```

Hoặc trong Visual Studio: F5 (Debug) hoặc Ctrl+F5 (Run without debugging)

### Kiểm tra API đã chạy

Mở trình duyệt và truy cập:
- **Swagger UI**: https://localhost:5001 hoặc http://localhost:5000
- Bạn sẽ thấy giao diện Swagger với 4 endpoints

---

## API Endpoints

### 1. POST /api/payments/qr/create
**Mục đích**: Tạo QR payment cho hóa đơn

**Request Body**:
```json
{
  "hoaDonBanId": 1,
  "amount": 50000,
  "description": "Thanh toán hóa đơn #1",
  "buyerName": "Nguyễn Văn A",
  "buyerPhone": "0901234567",
  "expiryMinutes": 15
}
```

**Response Success (200)**:
```json
{
  "success": true,
  "message": "Tạo QR payment thành công",
  "qrCode": "https://api.vietqr.io/image/970422-0123456789-compact.jpg?amount=50000&addInfo=HD1",
  "checkoutUrl": "https://pay.payos.vn/web/1234567890",
  "paymentLinkId": "abc-123-def-456",
  "orderCode": 1714567890000001,
  "status": "PENDING",
  "expiredAt": "2026-04-25T15:30:00"
}
```

**Response Error (400)**:
```json
{
  "success": false,
  "message": "Hóa đơn đã được thanh toán"
}
```

---

### 2. GET /api/payments/{hoaDonBanId}/status
**Mục đích**: Lấy trạng thái thanh toán của hóa đơn

**Request**: GET /api/payments/1/status

**Response (200)**:
```json
{
  "hoaDonBanId": 1,
  "paymentStatus": "PENDING",
  "trangThaiThanhToan": "Chưa thanh toán",
  "maGiaoDich": null,
  "paymentConfirmedAt": null,
  "paymentProvider": "payOS",
  "qrCode": "https://api.vietqr.io/image/...",
  "checkoutUrl": "https://pay.payos.vn/web/1234567890",
  "qrExpiredAt": "2026-04-25T15:30:00"
}
```

---

### 3. POST /api/payments/payos/webhook
**Mục đích**: Nhận webhook từ payOS khi thanh toán thành công

**Headers**:
- `X-Signature`: HMAC SHA256 signature của payload

**Request Body** (từ payOS):
```json
{
  "code": "00",
  "desc": "success",
  "data": {
    "orderCode": 1714567890000001,
    "amount": 50000,
    "description": "Thanh toán hóa đơn #1",
    "accountNumber": "0123456789",
    "reference": "FT26115123456",
    "transactionDateTime": "2026-04-25T14:30:00",
    "paymentLinkId": "abc-123-def-456",
    "code": "00",
    "desc": "Thành công",
    "counterAccountBankId": "970422",
    "counterAccountBankName": "MB Bank",
    "counterAccountName": "NGUYEN VAN A",
    "counterAccountNumber": "9876543210"
  },
  "signature": "abc123def456..."
}
```

**Response (200)**:
```json
{
  "message": "Webhook processed successfully"
}
```

**Xử lý**:
1. Verify signature
2. Tìm hóa đơn theo `orderCode`
3. Nếu `code = "00"` → cập nhật:
   - `PaymentStatus = "PAID"`
   - `TrangThaiThanhToan = "Đã thanh toán"`
   - `MaGiaoDich = reference`
   - `PaymentConfirmedAt = now`
4. Idempotency: không update lặp nếu đã PAID

---

### 4. POST /api/payments/{hoaDonBanId}/cancel
**Mục đích**: Hủy QR payment

**Request**: POST /api/payments/1/cancel

**Response (200)**:
```json
{
  "message": "Đã hủy QR payment thành công"
}
```

**Response Error (400)**:
```json
{
  "message": "Không thể hủy payment đã thanh toán"
}
```

---

## Test bằng Swagger UI

### Bước 1: Tạo hóa đơn test trong database

```sql
-- Tạo hóa đơn test
INSERT INTO dbo.HoaDonBan (NgayBan, TongTien, GiamGia, ThanhToan, CreatedByUserId, TrangThaiThanhToan, HinhThucThanhToan)
VALUES (GETDATE(), 50000, 0, 50000, 1, N'Chưa thanh toán', N'Tiền mặt');

-- Lấy ID vừa tạo
SELECT TOP 1 HoaDonBanId FROM dbo.HoaDonBan ORDER BY HoaDonBanId DESC;
```

### Bước 2: Test endpoint tạo QR

1. Mở Swagger UI: https://localhost:5001
2. Tìm endpoint **POST /api/payments/qr/create**
3. Click **Try it out**
4. Nhập request body:
```json
{
  "hoaDonBanId": 1,
  "amount": 50000,
  "description": "Test QR payment"
}
```
5. Click **Execute**
6. Kiểm tra response có `success: true` và có `qrCode`, `checkoutUrl`

### Bước 3: Test endpoint lấy status

1. Tìm endpoint **GET /api/payments/{hoaDonBanId}/status**
2. Click **Try it out**
3. Nhập `hoaDonBanId = 1`
4. Click **Execute**
5. Kiểm tra response có `paymentStatus: "PENDING"`

### Bước 4: Test webhook (mock)

1. Tìm endpoint **POST /api/payments/payos/webhook**
2. Click **Try it out**
3. Nhập request body (lấy `orderCode` từ bước 2):
```json
{
  "code": "00",
  "desc": "success",
  "data": {
    "orderCode": 1714567890000001,
    "amount": 50000,
    "reference": "FT26115123456",
    "paymentLinkId": "abc-123",
    "code": "00"
  }
}
```
4. Thêm header `X-Signature` với giá trị bất kỳ (mock sẽ accept)
5. Click **Execute**
6. Kiểm tra response `message: "Webhook processed successfully"`

### Bước 5: Kiểm tra lại status

1. Gọi lại **GET /api/payments/1/status**
2. Kiểm tra:
   - `paymentStatus: "PAID"`
   - `trangThaiThanhToan: "Đã thanh toán"`
   - `maGiaoDich: "FT26115123456"`
   - `paymentConfirmedAt` có giá trị

### Bước 6: Test cancel payment

1. Tạo hóa đơn mới và tạo QR payment
2. Gọi **POST /api/payments/{hoaDonBanId}/cancel**
3. Kiểm tra response thành công
4. Gọi lại status → `paymentStatus: "CANCELLED"`

---

## Test bằng Postman

### Import collection

Tạo collection mới với 4 requests:

#### 1. Create QR Payment
```
POST https://localhost:5001/api/payments/qr/create
Content-Type: application/json

{
  "hoaDonBanId": 1,
  "amount": 50000,
  "description": "Test payment"
}
```

#### 2. Get Payment Status
```
GET https://localhost:5001/api/payments/1/status
```

#### 3. PayOS Webhook
```
POST https://localhost:5001/api/payments/payos/webhook
Content-Type: application/json
X-Signature: test-signature

{
  "code": "00",
  "data": {
    "orderCode": 1714567890000001,
    "amount": 50000,
    "reference": "FT26115123456",
    "code": "00"
  }
}
```

#### 4. Cancel Payment
```
POST https://localhost:5001/api/payments/1/cancel
```

---

## Những chỗ cần tự điền/config

### 1. Connection String
File: `appsettings.Development.json`
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=CoffeeShopDB;..."
}
```

### 2. payOS Credentials
File: `appsettings.Development.json`
```json
"PayOS": {
  "ClientId": "YOUR_PAYOS_CLIENT_ID",
  "ApiKey": "YOUR_PAYOS_API_KEY",
  "ChecksumKey": "YOUR_PAYOS_CHECKSUM_KEY"
}
```

Đăng ký tại: https://payos.vn

### 3. Chạy SQL Migration
File: `database/13_QRPayment_Integration.sql`

---

## Tích hợp thật với payOS SDK

Hiện tại `PayOsPaymentGatewayService.cs` đang dùng **mock implementation**.

Để tích hợp thật:

### 1. Cài package payOS
```bash
dotnet add package Net.payOS
```

### 2. Sửa code trong PayOsPaymentGatewayService.cs

Uncomment phần code mẫu trong method `CreatePaymentAsync`:

```csharp
var paymentData = new PaymentData(
    orderCode: orderCode,
    amount: (int)request.Amount,
    description: request.Description ?? $"Thanh toán hóa đơn #{request.HoaDonBanId}",
    items: new List<ItemData>
    {
        new ItemData("Hóa đơn", 1, (int)request.Amount)
    },
    cancelUrl: $"{_configuration["AppUrl"]}/payment/cancel",
    returnUrl: $"{_configuration["AppUrl"]}/payment/success",
    buyerName: request.BuyerName,
    buyerPhone: request.BuyerPhone,
    expiredAt: (int)DateTimeOffset.Now.AddMinutes(request.ExpiryMinutes).ToUnixTimeSeconds()
);

var payOS = new PayOS(_clientId, _apiKey, _checksumKey);
var createPaymentResult = await payOS.createPaymentLink(paymentData);

return new CreateQRPaymentResponse
{
    Success = true,
    Message = "Tạo QR payment thành công",
    QrCode = createPaymentResult.qrCode,
    CheckoutUrl = createPaymentResult.checkoutUrl,
    PaymentLinkId = createPaymentResult.paymentLinkId,
    OrderCode = orderCode,
    Status = createPaymentResult.status,
    ExpiredAt = DateTimeOffset.FromUnixTimeSeconds(createPaymentResult.expiredAt).DateTime
};
```

### 3. Test với payOS sandbox

Sau khi tích hợp SDK, test với môi trường sandbox của payOS.

---

## Troubleshooting

### Lỗi: Connection string not found
→ Kiểm tra `appsettings.Development.json` có đúng connection string chưa

### Lỗi: PayOS credentials not configured
→ Điền ClientId, ApiKey, ChecksumKey vào appsettings.json

### Lỗi: Cannot connect to SQL Server
→ Kiểm tra:
- SQL Server đã chạy chưa
- Connection string đúng chưa
- Database CoffeeShopDB đã tạo chưa
- Đã chạy migration chưa

### Lỗi: Column not found
→ Chạy lại migration `database/13_QRPayment_Integration.sql`

### Swagger UI không hiện
→ Kiểm tra đang chạy ở Development mode: `ASPNETCORE_ENVIRONMENT=Development`

---

## Báo cáo hoàn thành Backend API

### ✅ File đã tạo
1. `CoffeeShop.PaymentApi/Program.cs` - Entry point, DI configuration, CORS, Swagger
2. `CoffeeShop.PaymentApi/appsettings.json` - Production config
3. `CoffeeShop.PaymentApi/appsettings.Development.json` - Development config
4. `CoffeeShop.PaymentApi/README_BACKEND_API.md` - Documentation này

### ✅ File đã sửa
1. `CoffeeShop.PaymentApi/CoffeeShop.PaymentApi.csproj` - Thêm Swashbuckle.AspNetCore, Microsoft.AspNetCore.Mvc.NewtonsoftJson

### ✅ Endpoint đã xong
1. ✅ POST /api/payments/qr/create - Tạo QR payment
2. ✅ GET /api/payments/{hoaDonBanId}/status - Lấy trạng thái
3. ✅ POST /api/payments/payos/webhook - Nhận webhook từ payOS
4. ✅ POST /api/payments/{hoaDonBanId}/cancel - Hủy payment

### ✅ Cách test
- **Swagger UI**: https://localhost:5001 (xem hướng dẫn chi tiết ở trên)
- **Postman**: Import 4 requests (xem mẫu ở trên)

### ⚠️ Những chỗ cần tự điền
1. **Connection String** trong `appsettings.Development.json`
2. **payOS Credentials** (ClientId, ApiKey, ChecksumKey) trong `appsettings.Development.json`
3. **Chạy SQL Migration**: `database/13_QRPayment_Integration.sql`

### 🔜 Chưa làm
- ❌ WPF integration (theo yêu cầu, làm sau khi backend chạy OK)
- ❌ Tích hợp thật với payOS SDK (hiện đang mock)

---

## Next Steps

Sau khi backend chạy OK bằng Swagger/Postman:
1. Tích hợp payOS SDK thật (nếu cần)
2. Làm WPF integration:
   - Sửa `HoaDonBanViewModel.cs`
   - Sửa `HoaDonBanView.xaml`
   - Tạo service gọi API từ WPF
