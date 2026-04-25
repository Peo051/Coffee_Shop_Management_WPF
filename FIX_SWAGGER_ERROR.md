# Fix Swagger GetSwagger Error

## Vấn đề
`System.TypeLoadException: 'Method 'GetSwagger' in type 'Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenerator' from assembly 'Swashbuckle.AspNetCore.SwaggerGen, Version=7.2.0.0' does not have an implementation.'`

## Nguyên nhân
Swashbuckle.AspNetCore version 7.2.0 không tương thích với .NET 10.0.

## Giải pháp

### Bước 1: Dừng ứng dụng đang chạy
- Đóng cửa sổ error dialog
- Trong Visual Studio, nhấn **Stop** (Shift+F5) để dừng debug
- Hoặc đóng terminal/console đang chạy API

### Bước 2: Đã cập nhật Swashbuckle lên version 8.0.0
File `CoffeeShop.PaymentApi.csproj` đã được cập nhật:
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="8.0.0" />
```

### Bước 3: Clean và Rebuild
Chạy các lệnh sau trong terminal:

```bash
# Clean project
dotnet clean CoffeeShop.PaymentApi/CoffeeShop.PaymentApi.csproj

# Restore packages
dotnet restore CoffeeShop.PaymentApi/CoffeeShop.PaymentApi.csproj

# Build
dotnet build CoffeeShop.PaymentApi/CoffeeShop.PaymentApi.csproj
```

### Bước 4: Chạy lại ứng dụng
```bash
dotnet run --project CoffeeShop.PaymentApi/CoffeeShop.PaymentApi.csproj
```

Hoặc nhấn F5 trong Visual Studio.

## Kiểm tra
- Mở browser: https://localhost:5001
- Swagger UI sẽ hiển thị bình thường
- Không còn lỗi TypeLoadException

## Lưu ý
- Swashbuckle 8.0.0 tương thích với .NET 9.0 và .NET 10.0
- Nếu vẫn gặp lỗi, xóa thư mục `bin` và `obj` rồi build lại
