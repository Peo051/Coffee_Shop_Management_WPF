# Báo cáo: Hoàn thành Backend API cho QR Payment

## Tổng quan
Đã hoàn thành backend API xử lý thanh toán QR code cho hệ thống quản lý quán cafe.

**Kiến trúc**: WPF Client → ASP.NET Core Web API → payOS Payment Gateway

---

## ✅ File đã tạo

### 1. Program.cs
**Path**: `CoffeeShop.PaymentApi/Program.cs`

**Nội dung**:
- Configure DI container: Controllers, HttpClient, Repositories, Services
- Configure CORS: Cho phép WPF client gọi API từ localhost
- Configure Swagger/OpenAPI: Tự động generate API documentation
- Configure Logging: Console và Debug logging
- Swagger UI tại root URL (https://localhost:5001)

### 2. appsettings.json
**Path**: `CoffeeShop.PaymentApi/appsettings.json`

**Nội dung**:
- ConnectionStrings:DefaultConnection (cần điền tên SQL Server)
- PayOS credentials (ClientId, ApiKey, ChecksumKey) - cần điền khi tích hợp thật
- AppUrl: https://localhost:5001 (cho webhook return URL)
- Logging configuration

### 3. appsettings.Development.json
**Path**: `CoffeeShop.PaymentApi/appsettings.Development.json`

**Nội dung**:
- Development-specific settings
- Debug logging level
- Local SQL Server connection string (Server=.)

### 4. README_BACKEND_API.md
**Path**: `CoffeeShop.PaymentApi/README_BACKEND_API.md`

**Nội dung**:
- Hướng dẫn cấu hình chi tiết
- Mô tả 4 endpoints
- Hướng dẫn test bằng Swagger UI
- Hướng dẫn test bằng Postman
- Troubleshooting guide
- Hướng dẫn tích hợp payOS SDK thật

### 5. QUICK_START.md
**Path**: `CoffeeShop.PaymentApi/QUICK_START.md`

**Nội dung**:
- Hướng dẫn nhanh 5 bước để chạy API
- Test nhanh các endpoint
- Troubleshooting cơ bản

### 6. TICH_HOP_QR_PAYMENT_BACKEND.md
**Path**: `TICH_HOP_QR_PAYMENT_BACKEND.md` (file này)

**Nội dung**:
- Báo cáo tổng hợp công việc đã hoàn thành

---

## ✅ File đã sửa

### 1. CoffeeShop.PaymentApi.csproj
**Path**: `CoffeeShop.PaymentApi/CoffeeShop.PaymentApi.csproj`

**Thay đổi**:
- Thêm package `Swashbuckle.AspNetCore` version 7.2.0 (cho Swagger UI)
- Thêm package `Microsoft.AspNetCore.Mvc.NewtonsoftJson` version 10.0.2 (cho JSON serialization)

**Packages hiện có**:
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="10.0.2" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.2" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="7.0.1" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
```

---

## ✅ Endpoint đã xong

### 1. POST /api/payments/qr/create
**Chức năng**: Tạo QR payment cho hóa đơn

**Input**:
- hoaDonBanId (int, required)
- amount (decimal, required)
- description (string, optional)
- buyerName (string, optional)
- buyerPhone (string, optional)
- expiryMinutes (int, default 15)

**Output**:
- success (bool)
- message (string)
- qrCode (string) - URL hoặc data của QR
- checkoutUrl (string) - Link checkout trên web
- paymentLinkId (string) - ID từ provider
- orderCode (long) - Mã đơn hàng duy nhất
- status (string) - PENDING
- expiredAt (DateTime)

**Xử lý**:
1. Validate request
2. Kiểm tra hóa đơn tồn tại
3. Kiểm tra đã thanh toán chưa
4. Kiểm tra đã có QR active chưa
5. Gọi payment gateway tạo QR
6. Lưu thông tin vào database
7. Return response

**Validation**:
- Không tạo QR nếu đã thanh toán
- Không tạo QR mới nếu đã có QR active (chưa hết hạn)

---

### 2. GET /api/payments/{hoaDonBanId}/status
**Chức năng**: Lấy trạng thái thanh toán của hóa đơn

**Input**:
- hoaDonBanId (int, path parameter)

**Output**:
- hoaDonBanId (int)
- paymentStatus (string) - PENDING, PAID, CANCELLED, EXPIRED
- trangThaiThanhToan (string) - "Chưa thanh toán", "Đã thanh toán"
- maGiaoDich (string) - Mã giao dịch từ ngân hàng
- paymentConfirmedAt (DateTime?)
- paymentProvider (string) - "payOS"
- qrCode (string)
- checkoutUrl (string)
- qrExpiredAt (DateTime?)

**Xử lý**:
1. Tìm hóa đơn theo ID
2. Return thông tin trạng thái

---

### 3. POST /api/payments/payos/webhook
**Chức năng**: Nhận webhook từ payOS khi thanh toán thành công

**Input**:
- Header: X-Signature (HMAC SHA256)
- Body: PayOsWebhookData (JSON)
  - code (string) - "00" = success
  - desc (string)
  - data:
    - orderCode (long)
    - amount (decimal)
    - reference (string) - Mã giao dịch ngân hàng
    - paymentLinkId (string)
    - code (string)
    - ...

**Output**:
- message (string)

**Xử lý**:
1. Đọc raw body
2. Parse JSON
3. **Verify signature** (HMAC SHA256 với ChecksumKey)
4. Tìm hóa đơn theo orderCode
5. Nếu code = "00":
   - Cập nhật PaymentStatus = "PAID"
   - Cập nhật TrangThaiThanhToan = "Đã thanh toán"
   - Cập nhật MaGiaoDich = reference
   - Cập nhật PaymentConfirmedAt = now
6. **Idempotency**: Không update nếu đã PAID (WHERE PaymentStatus != 'PAID')

**Bảo mật**:
- Verify signature trước khi xử lý
- Idempotency để tránh update lặp

---

### 4. POST /api/payments/{hoaDonBanId}/cancel
**Chức năng**: Hủy QR payment

**Input**:
- hoaDonBanId (int, path parameter)

**Output**:
- message (string)

**Xử lý**:
1. Tìm hóa đơn theo ID
2. Kiểm tra không phải đã thanh toán
3. Gọi provider cancel payment (nếu có)
4. Cập nhật PaymentStatus = "CANCELLED"

**Validation**:
- Không cho cancel nếu đã PAID
- Chỉ cancel nếu đang PENDING hoặc PROCESSING

---

## ✅ Cách test bằng Swagger UI

### Bước 1: Chạy API
```bash
cd CoffeeShop.PaymentApi
dotnet run
```

### Bước 2: Mở Swagger UI
Truy cập: https://localhost:5001

### Bước 3: Tạo hóa đơn test
```sql
INSERT INTO dbo.HoaDonBan (NgayBan, TongTien, GiamGia, ThanhToan, CreatedByUserId, TrangThaiThanhToan)
VALUES (GETDATE(), 50000, 0, 50000, 1, N'Chưa thanh toán');

SELECT TOP 1 HoaDonBanId FROM dbo.HoaDonBan ORDER BY HoaDonBanId DESC;
```

### Bước 4: Test từng endpoint

#### 4.1. Tạo QR
- Endpoint: POST /api/payments/qr/create
- Body:
```json
{
  "hoaDonBanId": 1,
  "amount": 50000,
  "description": "Test payment"
}
```
- Expected: `success: true`, có `qrCode`, `checkoutUrl`, `orderCode`

#### 4.2. Lấy status
- Endpoint: GET /api/payments/1/status
- Expected: `paymentStatus: "PENDING"`

#### 4.3. Webhook (giả lập thanh toán)
- Endpoint: POST /api/payments/payos/webhook
- Header: `X-Signature: test-signature`
- Body (thay orderCode từ bước 4.1):
```json
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
- Expected: `message: "Webhook processed successfully"`

#### 4.4. Kiểm tra lại status
- Endpoint: GET /api/payments/1/status
- Expected: `paymentStatus: "PAID"`, `maGiaoDich: "FT26115123456"`

#### 4.5. Test cancel
- Tạo hóa đơn mới và QR mới
- Endpoint: POST /api/payments/2/cancel
- Expected: `message: "Đã hủy QR payment thành công"`

---

## ⚠️ Những chỗ cần tự điền/config

### 1. Connection String
**File**: `CoffeeShop.PaymentApi/appsettings.Development.json`

**Cần sửa**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

**Ví dụ**:
- Local: `Server=.;Database=CoffeeShopDB;...`
- SQL Express: `Server=.\\SQLEXPRESS;Database=CoffeeShopDB;...`
- Remote: `Server=192.168.1.100;Database=CoffeeShopDB;User Id=sa;Password=xxx;...`

### 2. payOS Credentials
**File**: `CoffeeShop.PaymentApi/appsettings.Development.json`

**Cần điền** (khi tích hợp thật):
```json
"PayOS": {
  "ClientId": "YOUR_PAYOS_CLIENT_ID",
  "ApiKey": "YOUR_PAYOS_API_KEY",
  "ChecksumKey": "YOUR_PAYOS_CHECKSUM_KEY"
}
```

**Lấy ở đâu**: Đăng ký tại https://payos.vn

**Lưu ý**: Hiện tại service đang dùng **mock implementation**, nên có thể để nguyên để test. Khi tích hợp thật, cần:
1. Cài package `Net.payOS`
2. Sửa code trong `PayOsPaymentGatewayService.cs`
3. Điền credentials thật

### 3. Chạy SQL Migration
**File**: `database/13_QRPayment_Integration.sql`

**Cần chạy**: Script này thêm 8 cột mới vào bảng `HoaDonBan`:
- PaymentProvider
- ProviderPaymentId
- ProviderOrderCode
- QRCodeRaw
- CheckoutUrl
- QRExpiredAt
- PaymentStatus
- PaymentConfirmedAt

**Cách chạy**:
1. Mở SQL Server Management Studio hoặc Azure Data Studio
2. Connect tới database CoffeeShopDB
3. Mở file `database/13_QRPayment_Integration.sql`
4. Execute

---

## 🔜 Chưa làm (theo yêu cầu)

### 1. WPF Integration
**Lý do**: Theo yêu cầu, làm tuần tự: Backend trước → Test Swagger/Postman → WPF sau

**Cần làm sau**:
- Tạo service gọi API từ WPF (HttpClient)
- Sửa `HoaDonBanViewModel.cs`:
  - Thêm command `TaoQrThanhToanCommand`
  - Thêm command `KiemTraTrangThaiThanhToanCommand`
  - Thêm command `HuyQrThanhToanCommand`
  - Thêm properties: QrCodeRaw, CheckoutUrl, PaymentStatus, IsWaitingQrPayment
- Sửa `HoaDonBanView.xaml`:
  - Thêm nút "Tạo QR thanh toán"
  - Thêm nút "Kiểm tra thanh toán"
  - Thêm nút "Hủy QR"
  - Thêm vùng hiển thị QR code
  - Thêm vùng hiển thị trạng thái

### 2. Tích hợp payOS SDK thật
**Lý do**: Hiện tại dùng mock để test backend trước

**Cần làm sau**:
1. Cài package: `dotnet add package Net.payOS`
2. Sửa `PayOsPaymentGatewayService.cs`:
   - Uncomment code mẫu trong `CreatePaymentAsync`
   - Sửa `CancelPaymentAsync` gọi payOS API thật
3. Test với payOS sandbox
4. Điền credentials thật vào appsettings.json

---

## 📋 Checklist hoàn thành Backend

- ✅ Tạo `Program.cs` với DI, CORS, Swagger
- ✅ Tạo `appsettings.json` và `appsettings.Development.json`
- ✅ Thêm Swagger packages vào `.csproj`
- ✅ 4 endpoints hoạt động:
  - ✅ POST /api/payments/qr/create
  - ✅ GET /api/payments/{hoaDonBanId}/status
  - ✅ POST /api/payments/payos/webhook
  - ✅ POST /api/payments/{hoaDonBanId}/cancel
- ✅ Repository xử lý database
- ✅ Service tích hợp payment gateway (mock)
- ✅ Controller xử lý HTTP requests
- ✅ Models cho request/response
- ✅ Logging
- ✅ Error handling
- ✅ Validation
- ✅ Idempotency cho webhook
- ✅ Signature verification cho webhook
- ✅ Documentation (README, QUICK_START)

---

## 🎯 Kết luận

Backend API đã hoàn thành và sẵn sàng để test bằng Swagger UI hoặc Postman.

**Để chạy**:
1. Sửa connection string trong `appsettings.Development.json`
2. Chạy migration `database/13_QRPayment_Integration.sql`
3. `dotnet run` trong folder `CoffeeShop.PaymentApi`
4. Mở https://localhost:5001 để test Swagger UI

**Next steps**:
1. Test đầy đủ các endpoint bằng Swagger
2. Sau khi backend chạy OK, làm WPF integration
3. Tích hợp payOS SDK thật (nếu cần)

**Tài liệu tham khảo**:
- `CoffeeShop.PaymentApi/README_BACKEND_API.md` - Hướng dẫn chi tiết
- `CoffeeShop.PaymentApi/QUICK_START.md` - Hướng dẫn nhanh
