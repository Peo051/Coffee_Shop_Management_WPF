# 🚀 Hướng dẫn chạy code Coffee Shop Management System

**Dành cho:** Người nhận folder code lần đầu  
**Thời gian:** 15-30 phút  
**Yêu cầu:** Windows 10/11, có quyền admin

---

## 📋 Mục lục

1. [Cài đặt môi trường](#1-cài-đặt-môi-trường)
2. [Cấu hình Database](#2-cấu-hình-database)
3. [Cấu hình ứng dụng](#3-cấu-hình-ứng-dụng)
4. [Chạy ứng dụng](#4-chạy-ứng-dụng)
5. [Xử lý lỗi thường gặp](#5-xử-lý-lỗi-thường-gặp)

---

## 1. Cài đặt môi trường

### 1.1. Cài đặt .NET 8.0 SDK

**Bước 1:** Download .NET 8.0 SDK
- Truy cập: https://dotnet.microsoft.com/download/dotnet/8.0
- Chọn: **.NET 8.0 SDK** (Windows x64)
- Download và cài đặt

**Bước 2:** Kiểm tra cài đặt
```bash
dotnet --version
```
Kết quả mong đợi: `8.0.x`

---

### 1.2. Cài đặt SQL Server

**Tùy chọn 1: SQL Server Express (Miễn phí, khuyến nghị)**

1. Download: https://www.microsoft.com/sql-server/sql-server-downloads
2. Chọn: **Express Edition**
3. Cài đặt với tùy chọn mặc định
4. Ghi nhớ **Server Name** (ví dụ: `LAPTOP-ABC\SQLEXPRESS` hoặc `localhost\SQLEXPRESS`)

**Tùy chọn 2: SQL Server Developer (Miễn phí, đầy đủ tính năng)**

1. Download từ link trên
2. Chọn: **Developer Edition**
3. Cài đặt

**Bước 3:** Cài đặt SQL Server Management Studio (SSMS)
- Download: https://aka.ms/ssmsfullsetup
- Cài đặt để quản lý database dễ dàng

---

### 1.3. Cài đặt Visual Studio (Tùy chọn)

**Nếu muốn sửa code:**
- Download: https://visualstudio.microsoft.com/downloads/
- Chọn: **Community Edition** (miễn phí)
- Workloads: Chọn ".NET desktop development"

**Nếu chỉ chạy code:**
- Không cần Visual Studio, dùng command line

---

## 2. Cấu hình Database

### 2.1. Tìm SQL Server Name

**Cách 1: Qua SSMS**
1. Mở SQL Server Management Studio
2. Click "Connect" → Xem "Server name"
3. Ví dụ: `LAPTOP-VHGPK0SP`, `localhost\SQLEXPRESS`

**Cách 2: Qua Command Line**
```bash
sqlcmd -L
```

**Cách 3: Kiểm tra Services**
1. Nhấn `Win + R` → gõ `services.msc`
2. Tìm "SQL Server (SQLEXPRESS)" hoặc "SQL Server (MSSQLSERVER)"
3. Nếu đang chạy → OK

---

### 2.2. Tạo Database

**Bước 1:** Mở SSMS và kết nối SQL Server

**Bước 2:** Chạy các script theo thứ tự

Trong folder `database/`, chạy lần lượt:

```sql
-- 1. Tạo database
-- File: 01_CreateTables.sql
-- Mở file và Execute (F5)

-- 2. Thêm dữ liệu mẫu
-- File: 02_SeedData.sql
-- Mở file và Execute (F5)

-- 3. Chạy các file còn lại từ 03 đến 13
-- Mỗi file Execute một lần
```

**Hoặc dùng Command Line:**
```bash
# Thay YOUR_SERVER bằng server name của bạn
sqlcmd -S YOUR_SERVER -E -i database/01_CreateTables.sql
sqlcmd -S YOUR_SERVER -E -i database/02_SeedData.sql
sqlcmd -S YOUR_SERVER -E -i database/03_*.sql
# ... tiếp tục với các file khác
```

**Bước 3:** Kiểm tra database đã tạo
```sql
USE CoffeeShopDB;
GO

-- Kiểm tra có bảng không
SELECT * FROM INFORMATION_SCHEMA.TABLES;

-- Kiểm tra có dữ liệu không
SELECT * FROM NhanVien;
```

---

## 3. Cấu hình ứng dụng

### 3.1. Cấu hình WPF App

**Bước 1:** Copy file config
```bash
cd CoffeeShop.Wpf
copy App.config.example App.config
```

**Bước 2:** Mở `CoffeeShop.Wpf/App.config` và sửa

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <appSettings>
        <!-- Dùng localhost khi chạy API local -->
        <add key="PaymentApiBaseUrl" value="https://localhost:5002" />
    </appSettings>
    
    <connectionStrings>
        <!-- THAY ĐỔI SERVER NAME Ở ĐÂY -->
        <add name="CoffeeShopDb"
            connectionString="Server=YOUR_SERVER_NAME;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" 
            providerName="System.Data.SqlClient" />
    </connectionStrings>
</configuration>
```

**Ví dụ:**
- Nếu server name là `LAPTOP-ABC\SQLEXPRESS`:
  ```xml
  connectionString="Server=LAPTOP-ABC\SQLEXPRESS;Database=CoffeeShopDB;..."
  ```
- Nếu server name là `localhost`:
  ```xml
  connectionString="Server=localhost;Database=CoffeeShopDB;..."
  ```

---

### 3.2. Cấu hình Payment API

**Bước 1:** Copy file config
```bash
cd CoffeeShop.PaymentApi
copy appsettings.Development.json.example appsettings.Development.json
```

**Bước 2:** Mở `CoffeeShop.PaymentApi/appsettings.Development.json` và sửa

```json
{
  "ConnectionStrings": {
    "CoffeeShopDb": "Server=YOUR_SERVER_NAME;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "PayOS": {
    "ClientId": "YOUR_CLIENT_ID",
    "ApiKey": "YOUR_API_KEY",
    "ChecksumKey": "YOUR_CHECKSUM_KEY",
    "BaseUrl": "https://api-merchant.payos.vn",
    "ReturnUrl": "https://localhost:5002/payment/success",
    "CancelUrl": "https://localhost:5002/payment/cancel"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Lưu ý:**
- Thay `YOUR_SERVER_NAME` bằng SQL Server name của bạn
- PayOS credentials: Nếu không có, để tạm (sẽ không dùng QR Payment)

---

## 4. Chạy ứng dụng

### 4.1. Chạy Payment API (Terminal 1)

**Bước 1:** Mở Command Prompt hoặc PowerShell

**Bước 2:** Di chuyển vào folder Payment API
```bash
cd path\to\Coffee_Shop_Management_WPF\CoffeeShop.PaymentApi
```

**Bước 3:** Restore packages
```bash
dotnet restore
```

**Bước 4:** Chạy API
```bash
dotnet run
```

**Kết quả mong đợi:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5002
      Now listening on: http://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Bước 5:** Kiểm tra API
- Mở browser: `https://localhost:5002/swagger`
- Nếu thấy Swagger UI → API đang chạy ✅

⚠️ **Quan trọng:** Giữ terminal này mở, không đóng!

---

### 4.2. Chạy WPF App (Terminal 2)

**Cách 1: Dùng Command Line**

**Bước 1:** Mở Command Prompt/PowerShell mới (terminal thứ 2)

**Bước 2:** Di chuyển vào folder WPF
```bash
cd path\to\Coffee_Shop_Management_WPF\CoffeeShop.Wpf
```

**Bước 3:** Restore packages
```bash
dotnet restore
```

**Bước 4:** Chạy WPF
```bash
dotnet run
```

---

**Cách 2: Dùng Visual Studio**

1. Mở file `CoffeeShop.Wpf.sln`
2. Set `CoffeeShop.Wpf` làm startup project (right-click → Set as Startup Project)
3. Bấm F5 hoặc Ctrl+F5

---

### 4.3. Đăng nhập

**Tài khoản mặc định:**
- **Username:** `admin`
- **Password:** `admin123`

Hoặc kiểm tra trong database:
```sql
SELECT TenDangNhap, MatKhau FROM NhanVien WHERE VaiTro = N'Quản lý';
```

---

## 5. Xử lý lỗi thường gặp

### ❌ Lỗi 1: "Cannot open database CoffeeShopDB"

**Nguyên nhân:**
- Database chưa được tạo
- Server name sai trong connection string

**Giải pháp:**
1. Kiểm tra database tồn tại:
   ```sql
   SELECT name FROM sys.databases WHERE name = 'CoffeeShopDB';
   ```
2. Nếu không có → Chạy lại script tạo database
3. Kiểm tra server name trong `App.config`

---

### ❌ Lỗi 2: "Could not open a connection to SQL Server (error 40)"

**Nguyên nhân:**
- SQL Server không chạy
- Server name sai
- Firewall chặn

**Giải pháp:**

**1. Kiểm tra SQL Server có chạy không:**
```bash
# Mở Services
Win + R → services.msc

# Tìm "SQL Server (SQLEXPRESS)" hoặc "SQL Server (MSSQLSERVER)"
# Nếu Stopped → Right-click → Start
```

**2. Kiểm tra server name:**
```bash
sqlcmd -L
```

**3. Test kết nối:**
```bash
sqlcmd -S YOUR_SERVER_NAME -E
```
Nếu kết nối được → Server name đúng

---

### ❌ Lỗi 3: "Failed to connect to Payment API"

**Nguyên nhân:**
- Payment API không chạy
- URL sai trong `App.config`

**Giải pháp:**

**1. Kiểm tra Payment API có chạy không:**
- Mở browser: `https://localhost:5002/swagger`
- Nếu không mở được → API chưa chạy

**2. Chạy Payment API:**
```bash
cd CoffeeShop.PaymentApi
dotnet run
```

**3. Kiểm tra URL trong App.config:**
```xml
<add key="PaymentApiBaseUrl" value="https://localhost:5002" />
```

---

### ❌ Lỗi 4: "dotnet command not found"

**Nguyên nhân:**
- .NET SDK chưa được cài đặt
- PATH environment variable chưa được cập nhật

**Giải pháp:**

**1. Cài đặt .NET 8.0 SDK:**
- Download: https://dotnet.microsoft.com/download/dotnet/8.0

**2. Restart terminal sau khi cài:**
- Đóng và mở lại Command Prompt/PowerShell

**3. Kiểm tra:**
```bash
dotnet --version
```

---

### ❌ Lỗi 5: "Login failed for user"

**Nguyên nhân:**
- Dùng SQL Authentication nhưng chưa tạo user
- Windows Authentication bị disable

**Giải pháp:**

**Dùng Windows Authentication (Khuyến nghị):**
```xml
<connectionString>
    Server=YOUR_SERVER;Database=CoffeeShopDB;Trusted_Connection=True;...
</connectionString>
```

**Hoặc tạo SQL User:**
```sql
CREATE LOGIN coffee_user WITH PASSWORD = 'YourPassword123!';
USE CoffeeShopDB;
CREATE USER coffee_user FOR LOGIN coffee_user;
ALTER ROLE db_owner ADD MEMBER coffee_user;
```

Sau đó dùng:
```xml
<connectionString>
    Server=YOUR_SERVER;Database=CoffeeShopDB;User Id=coffee_user;Password=YourPassword123!;...
</connectionString>
```

---

### ❌ Lỗi 6: "The certificate chain was issued by an authority that is not trusted"

**Nguyên nhân:**
- SSL certificate chưa được trust

**Giải pháp:**
```bash
dotnet dev-certs https --trust
```

Chọn "Yes" khi được hỏi.

---

### ❌ Lỗi 7: "Port 5002 is already in use"

**Nguyên nhân:**
- Có process khác đang dùng port 5002

**Giải pháp:**

**1. Tìm process đang dùng port:**
```bash
netstat -ano | findstr :5002
```

**2. Kill process:**
```bash
taskkill /PID <PID_NUMBER> /F
```

**3. Hoặc đổi port trong `launchSettings.json`:**
```json
"applicationUrl": "https://localhost:5003;http://localhost:5001"
```

---

## 6. Tính năng chính

### 6.1. Bán hàng tại quầy

1. Vào "Bán hàng tại quầy"
2. Chọn món từ danh sách
3. Chọn size và số lượng
4. Thêm món vào hóa đơn
5. Chọn khách hàng (tùy chọn)
6. Chọn khuyến mãi (tùy chọn)
7. Chọn hình thức thanh toán:
   - **Tiền mặt:** Nhập tiền khách đưa
   - **Chuyển khoản/Thẻ/Ví:** Nhập mã giao dịch
   - **QR Payment:** Tạo QR Code (cần Payment API)

### 6.2. QR Payment (Cần PayOS credentials)

**Nếu có PayOS credentials:**
1. Cấu hình trong `appsettings.Development.json`
2. Chọn "QR Payment" khi thanh toán
3. Bấm "Tạo hóa đơn chờ thanh toán QR"
4. Khách quét QR và thanh toán
5. Hệ thống tự động cập nhật

**Nếu không có PayOS credentials:**
- Dùng các hình thức thanh toán khác
- Không bấm nút "QR Payment"

### 6.3. Quản lý kho

- Xem tồn kho món
- Nhập/xuất nguyên liệu
- Cảnh báo hết hàng

### 6.4. Quản lý khách hàng

- Thêm/sửa/xóa khách hàng
- Điểm tích lũy: 10,000đ = 1 điểm
- Sử dụng điểm: 1 điểm = 100đ giảm

### 6.5. Báo cáo

- Doanh thu theo ngày/tháng
- Tồn kho
- Lịch sử ca làm việc

---

## 7. Cấu trúc folder

```
Coffee_Shop_Management_WPF/
├── CoffeeShop.Wpf/              # WPF Desktop App
│   ├── App.config               # Cấu hình (cần tạo từ .example)
│   ├── ViewModels/              # MVVM ViewModels
│   ├── Views/                   # XAML Views
│   └── Services/                # Business logic
│
├── CoffeeShop.PaymentApi/       # Payment API Backend
│   ├── appsettings.Development.json  # Cấu hình (cần tạo từ .example)
│   ├── Controllers/             # API Controllers
│   └── Services/                # Payment services
│
├── database/                    # Database scripts
│   ├── 01_CreateTables.sql     # Tạo bảng
│   ├── 02_SeedData.sql         # Dữ liệu mẫu
│   └── ...                      # Các script khác
│
├── README.md                    # Tài liệu chính
├── HUONG_DAN_CHAY_CODE_CHO_NGUOI_KHAC.md  # File này
└── HUONG_DAN_CAU_HINH_WPF_CHO_NGUOI_KHAC.md  # Hướng dẫn WPF only
```

---

## 8. Checklist trước khi chạy

- [ ] Đã cài .NET 8.0 SDK
- [ ] Đã cài SQL Server
- [ ] Đã chạy script tạo database
- [ ] Đã tạo file `App.config` từ `.example`
- [ ] Đã sửa server name trong `App.config`
- [ ] Đã tạo file `appsettings.Development.json` từ `.example`
- [ ] Đã sửa server name trong `appsettings.Development.json`
- [ ] Đã chạy Payment API (terminal 1)
- [ ] Đã kiểm tra API qua Swagger
- [ ] Đã chạy WPF (terminal 2)
- [ ] Đã đăng nhập thành công

---

## 9. Liên hệ hỗ trợ

Nếu gặp vấn đề không giải quyết được:

- **Email:** tranduonggiabao0501@gmail.com
- **Facebook:** https://www.facebook.com/peo.0501/
- **GitHub:** https://github.com/Peo051

---

## 10. Tài liệu tham khảo

- **README.md** - Tài liệu tổng quan
- **HUONG_DAN_CAU_HINH_WPF_CHO_NGUOI_KHAC.md** - Hướng dẫn chỉ chạy WPF
- **HUONG_DAN_CHAY_PAYMENT_API.md** - Hướng dẫn Payment API
- **BAO_CAO_CAU_HINH_WPF_PUBLIC_API.md** - Báo cáo kỹ thuật

---

## 11. Tips và Best Practices

### Khi phát triển:

✅ **Nên:**
- Luôn chạy Payment API trước khi chạy WPF
- Commit code thường xuyên
- Backup database định kỳ
- Test trên môi trường local trước

❌ **Không nên:**
- Commit file `App.config` hoặc `appsettings.Development.json`
- Hard-code connection string trong code
- Đóng terminal Payment API khi WPF đang chạy
- Xóa file `.example`

### Khi gặp lỗi:

1. Đọc error message kỹ
2. Kiểm tra SQL Server có chạy không
3. Kiểm tra Payment API có chạy không
4. Kiểm tra connection string đúng không
5. Xem phần "Xử lý lỗi thường gặp" ở trên
6. Google error message
7. Liên hệ hỗ trợ

---

**Chúc bạn chạy code thành công! ☕**

**Thời gian cập nhật:** 2026-04-25  
**Phiên bản:** 1.0
