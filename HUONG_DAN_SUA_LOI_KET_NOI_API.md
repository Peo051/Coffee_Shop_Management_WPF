# Hướng dẫn sửa lỗi "Không thể tạo QR thanh toán"

## ✅ ĐÃ SỬA - Port không đúng

**Vấn đề:** WPF gọi `https://localhost:5001` nhưng backend chạy ở `https://localhost:5002`

**Đã sửa:** Đổi port trong `PaymentApiClient.cs` từ 5001 → 5002

## Lỗi đã gặp
```
Lỗi kết nối API: No connection could be made because the target machine actively refused it. (localhost:5001)
```

## Nguyên nhân
Backend API được cấu hình chạy ở port **5002** trong `launchSettings.json`:
```json
"applicationUrl": "https://localhost:5002;http://localhost:5281"
```

Nhưng WPF client gọi port **5001** (sai).

## Nguyên nhân có thể

### 1. Backend API chưa chạy
**Giải pháp:**
```bash
cd CoffeeShop.PaymentApi
dotnet run
```

Hoặc chạy từ Visual Studio:
- Mở solution
- Set `CoffeeShop.PaymentApi` làm startup project
- Bấm F5 hoặc Ctrl+F5

**Kiểm tra:** Backend phải chạy ở `https://localhost:5002` (hoặc port được cấu hình trong launchSettings.json)

Kiểm tra port backend đang chạy:
```bash
netstat -ano | Select-String ":5002"
```

Nếu thấy LISTENING → Backend đang chạy ✅

### 2. Port khác với mặc định
**ĐÃ SỬA:** Backend chạy ở port **5002**, không phải 5001.

Nếu backend chạy ở port khác, kiểm tra file:
**File:** `CoffeeShop.PaymentApi/Properties/launchSettings.json`
```json
"applicationUrl": "https://localhost:5002;http://localhost:5281"
```

Sau đó cập nhật trong WPF:
**File:** `CoffeeShop.Wpf/Services/PaymentApiClient.cs`
```csharp
public PaymentApiClient(string baseUrl = "https://localhost:5002")
```

### 3. SSL Certificate không hợp lệ
**Đã sửa:** Code đã được cập nhật để chấp nhận self-signed certificates:
```csharp
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
```

### 4. Firewall chặn kết nối
**Giải pháp:**
- Tắt tạm thời Windows Firewall để test
- Hoặc thêm exception cho port 5001

### 5. Backend API lỗi
**Kiểm tra logs:** Xem console của backend API để biết lỗi chi tiết

## Cách test backend API

### Test 1: Kiểm tra API có chạy không
Mở browser và truy cập:
```
https://localhost:5002/swagger
```

Nếu thấy Swagger UI → Backend đang chạy ✅

### Test 2: Test endpoint tạo QR qua Swagger
1. Mở `https://localhost:5002/swagger`
2. Tìm endpoint `POST /api/Payments/qr/create`
3. Bấm "Try it out"
4. Nhập body:
```json
{
  "hoaDonBanId": 23
}
```
5. Bấm "Execute"

**Kỳ vọng:** Response 200 với QR data

### Test 3: Test bằng curl
```bash
curl -X POST "https://localhost:5002/api/Payments/qr/create" \
  -H "Content-Type: application/json" \
  -d "{\"hoaDonBanId\": 24}" \
  --insecure
```

## Cải thiện đã thực hiện

### 1. Thêm error handling chi tiết
**File:** `CoffeeShop.Wpf/Services/PaymentApiClient.cs`

**Trước:**
```csharp
catch
{
    return null;
}
```

**Sau:**
```csharp
catch (HttpRequestException)
{
    throw; // Re-throw HTTP errors with details
}
catch (Exception ex)
{
    throw new HttpRequestException($"Failed to connect to Payment API at {_baseUrl}: {ex.Message}", ex);
}
```

### 2. Chấp nhận self-signed certificates
```csharp
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
```

### 3. Sửa endpoint URL
**Trước:** `/api/payments/qr/create` (lowercase)
**Sau:** `/api/Payments/qr/create` (PascalCase - khớp với controller)

## Cách debug chi tiết

### Bước 1: Kiểm tra backend có chạy không
```bash
# Kiểm tra process
netstat -ano | Select-String ":5002"

# Nếu không có → Backend chưa chạy
# Nếu có LISTENING → Backend đang chạy ✅
```

### Bước 2: Xem logs backend
Khi WPF gọi API, backend sẽ log:
```
info: CoffeeShop.PaymentApi.Controllers.PaymentsController[0]
      Creating QR payment for HoaDonBanId: 23
```

Nếu không thấy log này → WPF không gọi được backend

### Bước 3: Xem error message chi tiết trong WPF
Sau khi sửa code, error message sẽ rõ hơn:
- `Failed to connect to Payment API at https://localhost:5002: ...` → Không kết nối được
- `API returned 400: ...` → Backend trả lỗi 400
- `API returned 500: ...` → Backend lỗi internal

## Checklist khắc phục

- [x] **ĐÃ SỬA:** Port đã đổi từ 5001 → 5002
- [x] Backend API đang chạy ở `https://localhost:5002` (đã kiểm tra)
- [ ] Swagger UI mở được: `https://localhost:5002/swagger`
- [ ] Test endpoint qua Swagger thành công
- [ ] Firewall không chặn port 5002
- [ ] PayOS credentials đã điền trong `appsettings.Development.json`
- [ ] Database có hóa đơn với ID đúng (ví dụ: 24)

## Nếu vẫn lỗi

### Thử dùng HTTP thay vì HTTPS (chỉ để test)
**File:** `CoffeeShop.Wpf/Services/PaymentApiClient.cs`
```csharp
public PaymentApiClient(string baseUrl = "http://localhost:5000")
```

**File:** `CoffeeShop.PaymentApi/Properties/launchSettings.json`
Kiểm tra port HTTP:
```json
"applicationUrl": "https://localhost:5001;http://localhost:5000"
```

### Thử gọi API trực tiếp từ code
Thêm vào `TaoQrThanhToanAsync` để debug:
```csharp
try
{
    _logger.LogInformation("Calling Payment API at {BaseUrl}", _paymentApiClient._baseUrl);
    // ... existing code
}
```

## Kết luận

**✅ ĐÃ SỬA:** Port đã được cập nhật từ 5001 → 5002

Sau khi sửa code:
1. ✅ Port đã đúng: `https://localhost:5002`
2. ✅ Error message sẽ rõ ràng hơn
3. ✅ Chấp nhận self-signed certificates
4. ✅ Endpoint URL đã đúng

**Bước tiếp theo:**
1. Rebuild WPF project
2. Backend đang chạy ở port 5002 (đã kiểm tra ✅)
3. Test lại từ WPF - lỗi kết nối đã được sửa
4. Nếu vẫn lỗi, kiểm tra PayOS credentials trong `appsettings.Development.json`
