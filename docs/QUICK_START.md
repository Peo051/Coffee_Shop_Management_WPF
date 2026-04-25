# ⚡ Quick Start - Coffee Shop Management System

**Hướng dẫn nhanh 5 phút để chạy code**

---

## 📦 Yêu cầu

- Windows 10/11
- .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads

---

## 🚀 Các bước chạy

### 1️⃣ Cài đặt môi trường (Lần đầu tiên)

```bash
# Kiểm tra .NET đã cài chưa
dotnet --version
# Kết quả mong đợi: 8.0.x
```

---

### 2️⃣ Tạo Database

**Mở SQL Server Management Studio (SSMS):**

1. Kết nối SQL Server
2. Chạy lần lượt các file trong folder `database/`:
   - `01_CreateTables.sql`
   - `02_SeedData.sql`
   - `03_*.sql` đến `13_*.sql`

**Hoặc dùng command line:**
```bash
sqlcmd -S YOUR_SERVER -E -i database/01_CreateTables.sql
sqlcmd -S YOUR_SERVER -E -i database/02_SeedData.sql
# ... tiếp tục với các file khác
```

---

### 3️⃣ Cấu hình

**WPF App:**
```bash
cd CoffeeShop.Wpf
copy App.config.example App.config
notepad App.config
```

Sửa server name:
```xml
<connectionString>Server=YOUR_SERVER;Database=CoffeeShopDB;Trusted_Connection=True;...</connectionString>
```

**Payment API:**
```bash
cd CoffeeShop.PaymentApi
copy appsettings.Development.json.example appsettings.Development.json
notepad appsettings.Development.json
```

Sửa server name:
```json
"ConnectionStrings": {
  "CoffeeShopDb": "Server=YOUR_SERVER;Database=CoffeeShopDB;Trusted_Connection=True;..."
}
```

---

### 4️⃣ Chạy Payment API (Terminal 1)

```bash
cd CoffeeShop.PaymentApi
dotnet restore
dotnet run
```

Đợi thấy: `Now listening on: https://localhost:5002`

Kiểm tra: Mở browser → `https://localhost:5002/swagger`

---

### 5️⃣ Chạy WPF (Terminal 2)

```bash
cd CoffeeShop.Wpf
dotnet restore
dotnet run
```

---

### 6️⃣ Đăng nhập

- **Username:** `admin`
- **Password:** `123456`

---

## ❓ Lỗi thường gặp

### "Cannot open database"
→ Kiểm tra server name trong `App.config`

### "Could not connect to SQL Server"
→ Kiểm tra SQL Server có chạy không: `services.msc` → "SQL Server"

### "Failed to connect to Payment API"
→ Kiểm tra Payment API có chạy không: `https://localhost:5002/swagger`

### "dotnet command not found"
→ Cài .NET 8.0 SDK và restart terminal

---

## 📚 Tài liệu đầy đủ

- **HUONG_DAN_CHAY_CODE_CHO_NGUOI_KHAC.md** - Hướng dẫn chi tiết
- **README.md** - Tài liệu tổng quan
- **HUONG_DAN_CAU_HINH_WPF_CHO_NGUOI_KHAC.md** - Chỉ chạy WPF

---

## 📞 Liên hệ

- **Email:** tranduonggiabao0501@gmail.com
- **Facebook:** https://www.facebook.com/peo.0501/
- **GitHub:** https://github.com/Peo051

---

**Chúc bạn thành công! ☕**
