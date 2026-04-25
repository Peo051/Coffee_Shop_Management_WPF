# Quick Start - CoffeeShop Payment API

## Bước 1: Cấu hình Database

### 1.1. Sửa Connection String

Mở file `appsettings.Development.json` và thay đổi:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

**Thay `Server=.` bằng tên SQL Server của bạn**:
- Local: `Server=.` hoặc `Server=localhost`
- SQL Express: `Server=.\\SQLEXPRESS`
- Remote: `Server=192.168.1.100` hoặc tên server

### 1.2. Chạy SQL Migration

Mở SQL Server Management Studio hoặc Azure Data Studio, chạy file:
```
database/13_QRPayment_Integration.sql
```

Script này sẽ thêm 8 cột mới vào bảng `HoaDonBan`.

---

## Bước 2: Cấu hình payOS (Tạm thời để mock)

File `appsettings.Development.json` đã có sẵn mock credentials:

```json
"PayOS": {
  "ClientId": "YOUR_PAYOS_CLIENT_ID",
  "ApiKey": "YOUR_PAYOS_API_KEY",
  "ChecksumKey": "YOUR_PAYOS_CHECKSUM_KEY"
}
```

**Lưu ý**: Hiện tại service đang dùng mock implementation, nên có thể để nguyên các giá trị này để test. Khi tích hợp thật với payOS, cần đăng ký tại https://payos.vn và điền credentials thật.

---

## Bước 3: Restore packages và chạy API

```bash
cd CoffeeShop.PaymentApi
dotnet restore
dotnet run
```

Hoặc trong Visual Studio:
- Mở solution
- Set `CoffeeShop.PaymentApi` làm startup project
- Nhấn F5 (Debug) hoặc Ctrl+F5 (Run)

---

## Bước 4: Mở Swagger UI

Sau khi API chạy, mở trình duyệt:

**URL**: https://localhost:5001 hoặc http://localhost:5000

Bạn sẽ thấy Swagger UI với 4 endpoints:
1. POST /api/payments/qr/create
2. GET /api/payments/{hoaDonBanId}/status
3. POST /api/payments/payos/webhook
4. POST /api/payments/{hoaDonBanId}/cancel

---

## Bước 5: Test nhanh

### 5.1. Tạo hóa đơn test trong database

```sql
INSERT INTO dbo.HoaDonBan (NgayBan, TongTien, GiamGia, ThanhToan, CreatedByUserId, TrangThaiThanhToan)
VALUES (GETDATE(), 50000, 0, 50000, 1, N'Chưa thanh toán');

SELECT TOP 1 HoaDonBanId FROM dbo.HoaDonBan ORDER BY HoaDonBanId DESC;
-- Ghi nhớ ID này (ví dụ: 1)
```

### 5.2. Test tạo QR payment

Trong Swagger UI:
1. Mở endpoint **POST /api/payments/qr/create**
2. Click **Try it out**
3. Nhập:
```json
{
  "hoaDonBanId": 1,
  "amount": 50000,
  "description": "Test payment"
}
```
4. Click **Execute**
5. Kiểm tra response có `success: true`

### 5.3. Test lấy status

1. Mở endpoint **GET /api/payments/{hoaDonBanId}/status**
2. Click **Try it out**
3. Nhập `hoaDonBanId = 1`
4. Click **Execute**
5. Kiểm tra response có `paymentStatus: "PENDING"`

### 5.4. Test webhook (giả lập thanh toán thành công)

1. Mở endpoint **POST /api/payments/payos/webhook**
2. Click **Try it out**
3. Nhập (thay `orderCode` bằng giá trị từ bước 5.2):
```json
{
  "code": "00",
  "desc": "success",
  "data": {
    "orderCode": 1714567890000001,
    "amount": 50000,
    "reference": "FT26115123456",
    "paymentLinkId": "test-123",
    "code": "00"
  }
}
```
4. Thêm header `X-Signature` với giá trị `test-signature`
5. Click **Execute**
6. Kiểm tra response `message: "Webhook processed successfully"`

### 5.5. Kiểm tra lại status

1. Gọi lại **GET /api/payments/1/status**
2. Kiểm tra:
   - `paymentStatus: "PAID"`
   - `trangThaiThanhToan: "Đã thanh toán"`
   - `maGiaoDich: "FT26115123456"`

---

## Kết quả mong đợi

✅ API chạy thành công trên https://localhost:5001  
✅ Swagger UI hiển thị 4 endpoints  
✅ Tạo QR payment thành công  
✅ Lấy status thành công  
✅ Webhook cập nhật trạng thái thành công  
✅ Database có dữ liệu QR payment  

---

## Troubleshooting

### API không chạy được
- Kiểm tra .NET SDK đã cài chưa: `dotnet --version`
- Kiểm tra port 5001 có bị chiếm chưa

### Lỗi connection string
- Kiểm tra SQL Server đã chạy chưa
- Kiểm tra tên server đúng chưa
- Kiểm tra database CoffeeShopDB đã tạo chưa

### Lỗi column not found
- Chạy lại migration `database/13_QRPayment_Integration.sql`

### Swagger UI không hiện
- Kiểm tra đang chạy Development mode
- Truy cập đúng URL: https://localhost:5001

---

## Next Steps

Sau khi backend chạy OK:
1. Đọc file `README_BACKEND_API.md` để hiểu chi tiết
2. Test đầy đủ các endpoint
3. Tích hợp payOS SDK thật (nếu cần)
4. Làm WPF integration
