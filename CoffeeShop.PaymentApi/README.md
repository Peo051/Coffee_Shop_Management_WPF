# CoffeeShop Payment API

Backend API xử lý thanh toán QR code với payOS cho hệ thống quản lý quán cafe WPF.

## Quick Start

### 1. Cấu hình

Sửa `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "CoffeeShopDb": "Server=.;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 2. Chạy Migration

Chạy file `database/13_QRPayment_Integration.sql` trong SQL Server.

### 3. Khởi động API

```bash
dotnet run
```

Mở: https://localhost:5001

## Endpoints

1. **POST /api/payments/qr/create** - Tạo QR payment
2. **GET /api/payments/{hoaDonBanId}/status** - Lấy trạng thái
3. **POST /api/payments/payos/webhook** - Webhook từ payOS
4. **POST /api/payments/{hoaDonBanId}/cancel** - Hủy payment

## Documentation

- `HUONG_DAN_TEST.md` - Hướng dẫn test chi tiết
- `BAO_CAO_HOAN_THANH_BACKEND.md` - Báo cáo hoàn thành

## Tech Stack

- ASP.NET Core Web API
- Microsoft.Data.SqlClient
- Newtonsoft.Json
- Swagger/OpenAPI

## Features

✅ Tạo QR payment động  
✅ Xử lý webhook từ payOS  
✅ Idempotency  
✅ CORS cho WPF client  
✅ Mock mode (không cần payOS credentials để test)  
