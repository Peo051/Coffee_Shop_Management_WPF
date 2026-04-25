# Báo cáo: Hoàn thành Backend API QR Payment

## Tổng quan
Đã hoàn thiện backend API xử lý thanh toán QR code với payOS cho hệ thống quản lý quán cafe WPF.

---

## ✅ File đã tạo

### 1. Models

#### PayOsWebhookData.cs
**Path**: `CoffeeShop.PaymentApi/Models/PayOsWebhookData.cs`

**Nội dung**:
- Model để deserialize webhook payload từ payOS
- Class `PayOsWebhookData`: code, desc, success, data, signature
- Class `PayOsWebhookDataDetail`: orderCode, amount, description, reference, transactionDateTime, paymentLinkId, code, desc, và các field khác

### 2. Repositories

#### PaymentRepository.cs
**Path**: `CoffeeShop.PaymentApi/Repositories/PaymentRepository.cs`

**Nội dung**:
- Implementation của `IPaymentRepository`
- Dùng `Microsoft.Data.SqlClient` để kết nối SQL Server
- Connection string từ config key `CoffeeShopDb`

**Methods**:
1. `GetHoaDonForPaymentAsync` - Lấy hóa đơn để tạo payment (bao gồm ThanhToan, TrangThaiThanhToan, PaymentStatus, v.v.)
2. `SaveCreatedQrPaymentAsync` - Lưu thông tin QR payment đã tạo (PaymentProvider, ProviderPaymentId, ProviderOrderCode, QRCodeRaw, CheckoutUrl, QRExpiredAt, PaymentStatus)
3. `GetPaymentStatusAsync` - Lấy trạng thái thanh toán (trả về `PaymentStatusResponse`)
4. `MarkPaymentPaidAsync` - Đánh dấu payment đã thanh toán (cập nhật PaymentStatus = "PAID", TrangThaiThanhToan = "Đã thanh toán", MaGiaoDich, PaymentConfirmedAt)
5. `MarkPaymentCancelledAsync` - Đánh dấu payment đã hủy (PaymentStatus = "CANCELLED")
6. `FindHoaDonByOrderCodeAsync` - Tìm hóa đơn theo ProviderOrderCode
7. `IsAlreadyPaidAsync` - Kiểm tra hóa đơn đã thanh toán chưa

**Xử lý an toàn**:
- Null-safe với `IsDBNull` check
- Try-catch với logging
- Không crash nếu không tìm thấy hóa đơn
- Idempotency: `MarkPaymentPaidAsync` có WHERE clause `PaymentStatus != 'PAID'`

### 3. Services

#### PayOsPaymentGatewayService.cs
**Path**: `CoffeeShop.PaymentApi/Services/PayOsPaymentGatewayService.cs`

**Nội dung**:
- Implementation của `IPaymentGatewayService`
- Dùng `HttpClient` để gọi payOS API
- Đọc config từ `IConfiguration`: ClientId, ApiKey, ChecksumKey, BaseUrl

**Methods**:

1. **CreateQrAsync(int hoaDonBanId)**
   - Lấy thông tin hóa đơn qua repository
   - Kiểm tra đã thanh toán chưa
   - Kiểm tra đã có QR active chưa → nếu có thì trả lại QR cũ
   - Generate orderCode duy nhất: `timestamp * 100000 + hoaDonBanId`
   - Gọi payOS API tạo payment (hiện tại mock)
   - Lưu vào database qua `SaveCreatedQrPaymentAsync`
   - Trả về `CreateQRPaymentResponse`

2. **GetStatusAsync(int hoaDonBanId)**
   - Ưu tiên đọc từ database
   - Trả về `PaymentStatusResponse`

3. **HandleWebhookAsync(string rawJson)**
   - Deserialize payload
   - Tìm hóa đơn theo orderCode
   - Kiểm tra đã paid chưa (idempotency)
   - Nếu code = "00" → gọi `MarkPaymentPaidAsync`
   - Lưu MaGiaoDich từ reference
   - Return true/false

4. **CancelAsync(int hoaDonBanId)**
   - Lấy hóa đơn
   - Kiểm tra không phải đã paid
   - Gọi provider cancel (nếu có)
   - Cập nhật local state qua `MarkPaymentCancelledAsync`

**Mock implementation**:
- `CallPayOsCreatePaymentAsync`: Trả mock QR code, checkout URL, paymentLinkId
- `CallPayOsCancelPaymentAsync`: Mock cancel
- Khi có credentials thật, uncomment code tích hợp payOS API

**Bảo mật**:
- Không log secret key
- Credentials từ config, không hard-code

### 4. Controllers

#### PaymentsController.cs (đã sửa)
**Path**: `CoffeeShop.PaymentApi/Controllers/PaymentsController.cs`

**Nội dung**:
- Đã refactor để dùng `IPaymentGatewayService` thay vì gọi trực tiếp repository
- Đơn giản hóa logic, để service xử lý business logic

**Endpoints**:

1. **POST /api/payments/qr/create**
   - Body: `{ "hoaDonBanId": 1 }`
   - Validate HoaDonBanId > 0
   - Gọi `_paymentGatewayService.CreateQrAsync`
   - Return 200 với `CreateQRPaymentResponse` hoặc 400/500

2. **GET /api/payments/{hoaDonBanId}/status**
   - Gọi `_paymentGatewayService.GetStatusAsync`
   - Return 200 với `PaymentStatusResponse` hoặc 404

3. **POST /api/payments/payos/webhook**
   - Đọc raw body
   - Gọi `_paymentGatewayService.HandleWebhookAsync`
   - Return 200 nếu success, 400 nếu fail
   - Không throw HTML error

4. **POST /api/payments/{hoaDonBanId}/cancel**
   - Gọi `_paymentGatewayService.CancelAsync`
   - Return 200 nếu success, 400 nếu fail

### 5. Configuration

#### appsettings.json (đã sửa)
**Path**: `CoffeeShop.PaymentApi/appsettings.json`

**Thay đổi**:
- Connection string key: `CoffeeShopDb` (thay vì `DefaultConnection`)
- PayOS credentials: ClientId, ApiKey, ChecksumKey để trống (user tự điền)
- Thêm `PayOS:BaseUrl`: "https://api-merchant.payos.vn"
- Thêm `AllowedOrigins`: ["http://localhost", "app://wpf"]
- Xóa `AppUrl` (không cần)

#### appsettings.Development.json (đã sửa)
**Path**: `CoffeeShop.PaymentApi/appsettings.Development.json`

**Thay đổi**:
- Connection string key: `CoffeeShopDb`
- Server: `.` (local SQL Server)
- PayOS credentials để trống
- AllowedOrigins thêm "https://localhost"

#### Program.cs (đã sửa)
**Path**: `CoffeeShop.PaymentApi/Program.cs`

**Thay đổi**:
- CORS: Đọc `AllowedOrigins` từ config
- SetIsOriginAllowed: Cho phép localhost và app://
- DI: Đăng ký `IPaymentRepository`, `IPaymentGatewayService`
- Swagger: Bật ở Development mode, route prefix = "" (root URL)

### 6. Documentation

#### HUONG_DAN_TEST.md
**Path**: `CoffeeShop.PaymentApi/HUONG_DAN_TEST.md`

**Nội dung**:
- Hướng dẫn cấu hình connection string
- Hướng dẫn chạy migration
- Hướng dẫn tạo hóa đơn test
- Hướng dẫn test từng endpoint bằng Swagger UI
- Hướng dẫn test bằng Postman
- Test cases chi tiết
- Troubleshooting

#### BAO_CAO_HOAN_THANH_BACKEND.md (file này)
**Path**: `BAO_CAO_HOAN_THANH_BACKEND.md`

---

## ✅ File đã sửa

### 1. CreateQRPaymentRequest.cs
**Thay đổi**: Chỉ giữ lại field `HoaDonBanId` (xóa Amount, Description, BuyerName, BuyerPhone, ExpiryMinutes)

### 2. CreateQRPaymentResponse.cs
**Thay đổi**: Đổi structure theo yêu cầu:
- Xóa: Success, Message, QrCode, CheckoutUrl, PaymentLinkId, OrderCode, Status, ExpiredAt
- Thêm: HoaDonBanId, PaymentProvider, ProviderPaymentId, ProviderOrderCode, QRCodeRaw, CheckoutUrl, PaymentStatus, QRExpiredAt, Amount, Description

### 3. PaymentStatusResponse.cs
**Thay đổi**: Đổi structure theo yêu cầu:
- Giữ: HoaDonBanId, PaymentStatus, MaGiaoDich, PaymentConfirmedAt, PaymentProvider
- Thêm: ProviderPaymentId, ProviderOrderCode, ThanhToan, TrangThaiThanhToan
- Xóa: QrCode, CheckoutUrl, QRExpiredAt

### 4. IPaymentRepository.cs
**Thay đổi**: Đổi methods theo yêu cầu:
- `GetHoaDonByIdAsync` → `GetHoaDonForPaymentAsync`
- `UpdateQRPaymentInfoAsync` → `SaveCreatedQrPaymentAsync`
- `UpdatePaymentStatusAsync` → `MarkPaymentPaidAsync`
- `CancelPaymentAsync` → `MarkPaymentCancelledAsync`
- `GetHoaDonByOrderCodeAsync` → `FindHoaDonByOrderCodeAsync`
- Thêm: `GetPaymentStatusAsync`, `IsAlreadyPaidAsync`

### 5. IPaymentGatewayService.cs
**Thay đổi**: Đổi methods theo yêu cầu:
- `CreatePaymentAsync(CreateQRPaymentRequest)` → `CreateQrAsync(int hoaDonBanId)`
- `CancelPaymentAsync(string paymentLinkId)` → `CancelAsync(int hoaDonBanId)`
- `VerifyWebhookSignature` → xóa (logic verify trong HandleWebhookAsync)
- Thêm: `GetStatusAsync`, `HandleWebhookAsync`

### 6. HoaDonBan.cs
**Thay đổi**: Thêm field `ThanhToan` (decimal)

---

## ✅ Endpoint đã xong

### 1. POST /api/payments/qr/create
**Input**: `{ "hoaDonBanId": 1 }`

**Output**:
```json
{
  "hoaDonBanId": 1,
  "paymentProvider": "payOS",
  "providerPaymentId": "abc-123",
  "providerOrderCode": 1714567890000001,
  "qrCodeRaw": "https://img.vietqr.io/...",
  "checkoutUrl": "https://pay.payos.vn/web/...",
  "paymentStatus": "PENDING",
  "qrExpiredAt": "2026-04-25T15:30:00",
  "amount": 50000,
  "description": "HD00001"
}
```

**Nghiệp vụ**:
- Không cho tạo QR nếu đã paid
- Nếu đã có QR PENDING chưa hết hạn → trả lại QR cũ
- Lưu vào database: PaymentProvider, ProviderPaymentId, ProviderOrderCode, QRCodeRaw, CheckoutUrl, QRExpiredAt, PaymentStatus = "PENDING"

### 2. GET /api/payments/{hoaDonBanId}/status
**Output**:
```json
{
  "hoaDonBanId": 1,
  "paymentProvider": "payOS",
  "providerPaymentId": "abc-123",
  "providerOrderCode": 1714567890000001,
  "paymentStatus": "PENDING",
  "maGiaoDich": null,
  "paymentConfirmedAt": null,
  "thanhToan": 50000,
  "trangThaiThanhToan": "Chưa thanh toán"
}
```

**Nghiệp vụ**:
- Ưu tiên đọc từ database
- Có thể query provider nếu cần (tùy business logic)

### 3. POST /api/payments/payos/webhook
**Input**:
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

**Output**: `{ "message": "Webhook processed successfully" }`

**Nghiệp vụ**:
- Deserialize payload
- Tìm hóa đơn theo orderCode
- Kiểm tra đã paid chưa (idempotency)
- Nếu code = "00" → cập nhật:
  - PaymentStatus = "PAID"
  - TrangThaiThanhToan = "Đã thanh toán"
  - MaGiaoDich = reference
  - PaymentConfirmedAt = now
- Idempotency: không update lại nếu đã PAID

### 4. POST /api/payments/{hoaDonBanId}/cancel
**Output**: `{ "message": "Đã hủy QR payment thành công" }`

**Nghiệp vụ**:
- Không cho cancel nếu đã paid
- Gọi provider cancel (nếu có)
- Cập nhật PaymentStatus = "CANCELLED"

---

## ⚠️ appsettings cần tôi điền gì

### 1. Connection String
**File**: `appsettings.Development.json`

**Cần sửa**:
```json
"ConnectionStrings": {
  "CoffeeShopDb": "Server=YOUR_SERVER;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

**Ví dụ**:
- Local: `Server=.;Database=CoffeeShopDB;...`
- SQL Express: `Server=.\\SQLEXPRESS;Database=CoffeeShopDB;...`
- Remote: `Server=192.168.1.100;Database=CoffeeShopDB;User Id=sa;Password=xxx;...`

### 2. PayOS Credentials (Optional - hiện tại dùng mock)
**File**: `appsettings.Development.json`

**Cần điền** (khi tích hợp thật):
```json
"PayOS": {
  "ClientId": "YOUR_CLIENT_ID",
  "ApiKey": "YOUR_API_KEY",
  "ChecksumKey": "YOUR_CHECKSUM_KEY"
}
```

**Lấy ở đâu**: Đăng ký tại https://payos.vn

**Lưu ý**: Hiện tại service đang dùng mock implementation, nên có thể để trống để test. Khi tích hợp thật:
1. Điền credentials
2. Uncomment code trong `CallPayOsCreatePaymentAsync` và `CallPayOsCancelPaymentAsync`
3. Test với payOS sandbox

---

## 📋 Cách test bằng Swagger/Postman

Xem chi tiết trong file `CoffeeShop.PaymentApi/HUONG_DAN_TEST.md`

### Quick Test Flow

1. **Tạo hóa đơn test**:
```sql
INSERT INTO dbo.HoaDonBan (NgayBan, TongTien, GiamGia, ThanhToan, CreatedByUserId, TrangThaiThanhToan)
VALUES (GETDATE(), 50000, 0, 50000, 1, N'Chưa thanh toán');
```

2. **Chạy API**: `dotnet run` → https://localhost:5001

3. **Tạo QR**: POST /api/payments/qr/create với body `{ "hoaDonBanId": 1 }`

4. **Kiểm tra status**: GET /api/payments/1/status

5. **Webhook giả lập**: POST /api/payments/payos/webhook với payload có `orderCode` từ bước 3

6. **Kiểm tra lại status**: GET /api/payments/1/status → PaymentStatus = "PAID"

7. **Cancel**: Tạo hóa đơn mới, tạo QR, rồi POST /api/payments/2/cancel

---

## ✅ Checklist hoàn thành

- ✅ Models: CreateQRPaymentRequest, CreateQRPaymentResponse, PaymentStatusResponse, PayOsWebhookData
- ✅ Repository interface: IPaymentRepository với 7 methods
- ✅ Repository implementation: PaymentRepository với SqlConnection
- ✅ Service interface: IPaymentGatewayService với 4 methods
- ✅ Service implementation: PayOsPaymentGatewayService với mock payOS
- ✅ Controller: PaymentsController với 4 endpoints
- ✅ Program.cs: DI, CORS, Swagger
- ✅ appsettings.json: ConnectionStrings, PayOS, AllowedOrigins
- ✅ Documentation: HUONG_DAN_TEST.md, BAO_CAO_HOAN_THANH_BACKEND.md

### Nghiệp vụ
- ✅ Không cho tạo QR nếu đã paid
- ✅ Trả lại QR cũ nếu còn active
- ✅ TrangThaiThanhToan chỉ chuyển sang "Đã thanh toán" khi webhook xác nhận
- ✅ Idempotency: webhook gửi lại không làm lỗi
- ✅ Không cho cancel nếu đã paid

### Bảo mật
- ✅ Không log secret key
- ✅ Không hard-code credential
- ✅ Webhook không update bừa nếu không map được hóa đơn
- ✅ Xử lý idempotency

### Code quality
- ✅ Null-safe
- ✅ Try-catch với logging
- ✅ Không crash nếu không tìm thấy hóa đơn
- ✅ Async/await
- ✅ CancellationToken

---

## 🎯 Kết luận

Backend API đã hoàn thành và sẵn sàng để test!

**Để chạy**:
1. Sửa connection string trong `appsettings.Development.json`
2. Chạy migration `database/13_QRPayment_Integration.sql`
3. `dotnet run` trong folder `CoffeeShop.PaymentApi`
4. Mở https://localhost:5001 để test Swagger UI

**Next steps**:
1. Test đầy đủ các endpoint theo `HUONG_DAN_TEST.md`
2. Sau khi backend chạy OK, làm WPF integration
3. Tích hợp payOS SDK thật (nếu cần)

**Chưa làm** (theo yêu cầu):
- ❌ WPF integration
- ❌ Tích hợp payOS SDK thật (hiện đang mock)
- ❌ Build tự động (theo yêu cầu không chạy build)
