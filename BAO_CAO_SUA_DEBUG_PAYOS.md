# Báo cáo sửa debug PayOS API

## File đã chỉnh
- `CoffeeShop.PaymentApi/Services/PayOsPaymentGatewayService.cs`

## Các thay đổi

### 1. Đọc raw response trước khi parse
**Trước:**
```csharp
var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
_logger.LogDebug("PayOS API response: {Response}", responseContent);
response.EnsureSuccessStatusCode();
var result = await response.Content.ReadFromJsonAsync<PayOsCreatePaymentResponse>(cancellationToken);
```

**Sau:**
```csharp
// Đọc raw response trước
var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

_logger.LogDebug("PayOS API HTTP {StatusCode}. Response: {Response}", 
    (int)response.StatusCode, responseContent);

// Kiểm tra HTTP status
if (!response.IsSuccessStatusCode)
{
    throw new InvalidOperationException(
        $"PayOS create payment failed. HTTP {(int)response.StatusCode}. Response: {responseContent}");
}

// Parse response
var result = JsonConvert.DeserializeObject<PayOsCreatePaymentResponse>(responseContent);
```

### 2. Trả lỗi chi tiết với HTTP status code và raw response
**Trước:**
```csharp
if (result?.Data == null)
{
    throw new InvalidOperationException("PayOS API returned null data");
}
```

**Sau:**
```csharp
if (result?.Data == null)
{
    throw new InvalidOperationException(
        $"PayOS create payment failed. HTTP {(int)response.StatusCode}. Response: {responseContent}");
}
```

### 3. Sửa signature để KHÔNG bao gồm expiredAt
**Trước:**
```csharp
var payloadData = new Dictionary<string, object?>
{
    { "amount", amount },
    { "cancelUrl", _cancelUrl },
    { "description", description },
    { "orderCode", orderCode },
    { "returnUrl", _returnUrl }
};

// Thêm expiredAt nếu có
if (expiredAtUnix > 0)
{
    payloadData["expiredAt"] = expiredAtUnix;
}

// Tạo signature
var signature = CreatePayOsSignature(payloadData);
```

**Sau:**
```csharp
// Tạo payload cho signature (KHÔNG bao gồm expiredAt)
var payloadData = new Dictionary<string, object?>
{
    { "amount", amount },
    { "cancelUrl", _cancelUrl },
    { "description", description },
    { "orderCode", orderCode },
    { "returnUrl", _returnUrl }
};

// Tạo signature
var signature = CreatePayOsSignature(payloadData);
```

### 4. Cải thiện logging
**Thêm:**
- Log HTTP status code kèm response
- Log signature đã tạo (không log ChecksumKey)
- Comment rõ ràng "không log ChecksumKey"

```csharp
_logger.LogDebug("PayOS signature data string: {DataString}", dataString);

// Tính HMAC SHA256 (không log ChecksumKey)
using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_checksumKey));
var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

_logger.LogDebug("PayOS signature created: {Signature}", signature);
```

### 5. Sửa duplicate code
Đã xóa đoạn code bị duplicate khai báo `payloadData` 2 lần.

## Chuỗi signature đúng
```
amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}
```

**Lưu ý:** KHÔNG bao gồm `expiredAt` trong chuỗi signature.

## Test lại trên Swagger

### Bước 1: Mở Swagger UI
```
https://localhost:5001
```

### Bước 2: Test endpoint POST /api/Payments/qr/create
**Request body:**
```json
{
  "hoaDonBanId": 22
}
```

### Bước 3: Kiểm tra response
**Nếu thành công:**
```json
{
  "hoaDonBanId": 22,
  "paymentProvider": "payOS",
  "providerPaymentId": "...",
  "providerOrderCode": 123456789,
  "qrCodeRaw": "https://...",
  "checkoutUrl": "https://...",
  "paymentStatus": "PENDING",
  "qrExpiredAt": "2026-04-25T...",
  "amount": 50000,
  "description": "HD00022"
}
```

**Nếu lỗi:**
Response sẽ chứa:
- HTTP status code
- Raw response từ PayOS
- Message chi tiết: "PayOS create payment failed. HTTP {statusCode}. Response: {raw}"

### Bước 4: Kiểm tra logs
Trong console/logs, sẽ thấy:
```
PayOS signature data string: amount=50000&cancelUrl=...&description=HD00022&orderCode=123456789&returnUrl=...
PayOS signature created: abc123...
PayOS API HTTP 200. Response: {"code":"00","data":{...}}
PayOS payment created successfully. PaymentLinkId: ..., OrderCode: 123456789
```

## Lưu ý
- Không log ApiKey hoặc ChecksumKey
- Signature KHÔNG bao gồm expiredAt
- Raw response luôn được log để debug
- HTTP status code luôn được log kèm response
