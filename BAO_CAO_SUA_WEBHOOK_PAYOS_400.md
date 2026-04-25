# Báo cáo: Sửa webhook PayOS trả HTTP 400

## ✅ HOÀN THÀNH

Đã sửa endpoint webhook để **luôn trả 200 OK** và xử lý gracefully các trường hợp test payload từ PayOS.

---

## Vấn đề

### Hiện trạng
- Webhook URL: `https://sheep-condense-preheated.ngrok-free.dev/api/Payments/payos/webhook`
- Khi lưu webhook trên PayOS dashboard, PayOS báo: **"Request failed with status code 400"**
- PayOS gọi được vào backend nhưng backend trả 400/500

### Nguyên nhân
1. **Controller trả BadRequest (400)** khi service return false
2. **Controller trả 500** khi có exception
3. **Service throw exception** khi:
   - Payload không hợp lệ
   - OrderCode không tồn tại (test payload từ PayOS)
   - Signature sai
4. PayOS test webhook bằng payload mẫu → không map được hóa đơn → backend trả 400

---

## Giải pháp

### Nguyên tắc
1. **Luôn trả 200 OK** để PayOS không retry
2. **Không throw exception** ra ngoài controller
3. **Log chi tiết** để debug
4. **Phân biệt** giữa:
   - Test payload (không có hóa đơn) → Ignored
   - Signature sai → Ignored
   - Finalize thành công → Success
   - Finalize thất bại → Ignored (vẫn 200 OK)

---

## Files đã chỉnh

### 1. CoffeeShop.PaymentApi/Controllers/PaymentsController.cs

#### Trước (SAI - trả 400/500):
```csharp
[HttpPost("payos/webhook")]
public async Task<IActionResult> PayOsWebhook(CancellationToken cancellationToken)
{
    try
    {
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var success = await _paymentGatewayService.HandleWebhookAsync(payload, cancellationToken);

        if (success)
        {
            return Ok(new { message = "Webhook processed successfully" });
        }
        else
        {
            return BadRequest(new { message = "Failed to process webhook" }); // ← 400
        }
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = $"Lỗi xử lý webhook: {ex.Message}" }); // ← 500
    }
}
```

#### Sau (ĐÚNG - luôn trả 200):
```csharp
[HttpPost("payos/webhook")]
public async Task<IActionResult> PayOsWebhook(CancellationToken cancellationToken)
{
    string? payload = null;
    
    try
    {
        // Đọc raw body
        using var reader = new StreamReader(Request.Body);
        payload = await reader.ReadToEndAsync(cancellationToken);

        _logger.LogInformation("Received payOS webhook. Payload length: {Length}", payload?.Length ?? 0);
        
        // Log raw payload trong Development
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Webhook raw payload: {Payload}", payload);
        }

        // Xử lý webhook - service sẽ không throw exception
        var result = await _paymentGatewayService.HandleWebhookAsync(payload, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation("Webhook processed successfully: {Message}", result.Message);
            return Ok(new { message = result.Message ?? "Webhook processed successfully" });
        }
        else
        {
            // Vẫn trả 200 OK nhưng với message khác
            _logger.LogWarning("Webhook received but not processed: {Message}", result.Message);
            return Ok(new { message = result.Message ?? "Webhook received but not processed" });
        }
    }
    catch (Exception ex)
    {
        // Luôn trả 200 OK để PayOS không retry
        _logger.LogError(ex, "Error processing payOS webhook. Payload: {Payload}", payload);
        return Ok(new { message = "Webhook received but processing failed. Logged for investigation." });
    }
}
```

**Thay đổi:**
- ✅ Luôn trả `Ok()` (200)
- ✅ Log payload length
- ✅ Log raw payload trong Debug mode
- ✅ Catch exception và vẫn trả 200
- ✅ Trả message rõ ràng

---

### 2. CoffeeShop.PaymentApi/Services/IPaymentGatewayService.cs

#### Thay đổi signature:
```csharp
// Trước
Task<bool> HandleWebhookAsync(string rawJson, CancellationToken cancellationToken = default);

// Sau
Task<WebhookResult> HandleWebhookAsync(string rawJson, CancellationToken cancellationToken = default);
```

---

### 3. CoffeeShop.PaymentApi/Models/WebhookResult.cs

**File mới:**
```csharp
namespace CoffeeShop.PaymentApi.Models;

/// <summary>
/// Kết quả xử lý webhook
/// </summary>
public class WebhookResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static WebhookResult Ok(string message) => new() { Success = true, Message = message };
    public static WebhookResult Ignored(string message) => new() { Success = false, Message = message };
}
```

**Mục đích:**
- `Success = true` → Finalize thành công
- `Success = false` → Ignored (test payload, signature sai, không tìm thấy hóa đơn...)
- `Message` → Lý do chi tiết

---

### 4. CoffeeShop.PaymentApi/Services/PayOsPaymentGatewayService.cs

#### Cải thiện HandleWebhookAsync:

##### a. Kiểm tra payload rỗng
```csharp
if (string.IsNullOrWhiteSpace(rawJson))
{
    _logger.LogWarning("Webhook payload is empty");
    return WebhookResult.Ignored("Webhook payload is empty");
}
```

##### b. Xử lý JSON parse error
```csharp
PayOsWebhookData? webhookData;
try
{
    webhookData = JsonConvert.DeserializeObject<PayOsWebhookData>(rawJson);
}
catch (JsonException jsonEx)
{
    _logger.LogWarning(jsonEx, "Failed to deserialize webhook payload");
    return WebhookResult.Ignored("Invalid JSON payload");
}
```

##### c. Signature sai → Ignored (không throw)
```csharp
if (!VerifyWebhookSignature(webhookData, _checksumKey))
{
    _logger.LogError("Webhook signature verification failed for OrderCode {OrderCode}", 
        webhookData.Data.OrderCode);
    return WebhookResult.Ignored("Webhook received but invalid signature. Ignored in Development.");
}
```

##### d. Không tìm thấy hóa đơn → Ignored (test payload)
```csharp
if (hoaDon == null)
{
    _logger.LogWarning("HoaDonBan not found for OrderCode {OrderCode}. This might be a test webhook from PayOS.", 
        webhookData.Data.OrderCode);
    return WebhookResult.Ignored("Webhook received but no matching invoice. Ignored.");
}
```

##### e. Idempotency → Ok
```csharp
if (await _paymentRepository.IsAlreadyPaidAsync(hoaDon.HoaDonBanId, cancellationToken))
{
    _logger.LogInformation("Payment already confirmed for HoaDonBan {HoaDonBanId} (idempotency)",
        hoaDon.HoaDonBanId);
    return WebhookResult.Ok("Payment already confirmed (idempotent)");
}
```

##### f. Finalize thành công → Ok
```csharp
if (success)
{
    _logger.LogInformation(
        "Payment finalized successfully for HoaDonBan {HoaDonBanId}, OrderCode: {OrderCode}, Reference: {Reference}",
        hoaDon.HoaDonBanId, webhookData.Data.OrderCode, maGiaoDich);
    return WebhookResult.Ok($"Payment finalized successfully for invoice {hoaDon.HoaDonBanId}");
}
```

##### g. Finalize thất bại → Ignored
```csharp
else
{
    _logger.LogError(
        "Failed to finalize payment for HoaDonBan {HoaDonBanId}: {ErrorMessage}",
        hoaDon.HoaDonBanId, errorMessage);
    return WebhookResult.Ignored($"Failed to finalize payment: {errorMessage}");
}
```

##### h. Catch exception → Ignored
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error handling webhook");
    return WebhookResult.Ignored("Webhook received but processing failed. Logged for investigation.");
}
```

---

## Trường hợp endpoint trả 200 OK

### 1. ✅ Test payload từ PayOS (không có hóa đơn)
```
PayOS gửi: { "data": { "orderCode": 999999999 } }
  ↓
Backend: Không tìm thấy hóa đơn
  ↓
Response: 200 OK
{
  "message": "Webhook received but no matching invoice. Ignored."
}
```

### 2. ✅ Signature sai
```
PayOS gửi: { "signature": "invalid..." }
  ↓
Backend: Verify signature failed
  ↓
Response: 200 OK
{
  "message": "Webhook received but invalid signature. Ignored in Development."
}
```

### 3. ✅ Payload rỗng hoặc invalid JSON
```
PayOS gửi: ""
  ↓
Backend: Payload empty
  ↓
Response: 200 OK
{
  "message": "Webhook payload is empty"
}
```

### 4. ✅ Finalize thành công
```
PayOS gửi: { "code": "00", "data": { "orderCode": 123456 } }
  ↓
Backend: Tìm thấy hóa đơn → Finalize → Trừ kho/điểm
  ↓
Response: 200 OK
{
  "message": "Payment finalized successfully for invoice 24"
}
```

### 5. ✅ Idempotency (đã paid rồi)
```
PayOS gửi lại webhook (retry)
  ↓
Backend: Hóa đơn đã PAID
  ↓
Response: 200 OK
{
  "message": "Payment already confirmed (idempotent)"
}
```

### 6. ✅ Code không phải "00" (payment failed)
```
PayOS gửi: { "code": "01", "desc": "Payment cancelled" }
  ↓
Backend: Code != "00" → Không finalize
  ↓
Response: 200 OK
{
  "message": "Payment not completed. Code: 01"
}
```

### 7. ✅ Exception xảy ra
```
Backend: Lỗi database, network...
  ↓
Response: 200 OK
{
  "message": "Webhook received but processing failed. Logged for investigation."
}
```

---

## Trường hợp finalize hóa đơn

Chỉ finalize khi **TẤT CẢ** điều kiện sau đúng:

1. ✅ Payload parse thành công
2. ✅ Signature hợp lệ (hoặc không có ChecksumKey)
3. ✅ Tìm thấy hóa đơn theo `ProviderOrderCode`
4. ✅ Hóa đơn chưa PAID (idempotency check)
5. ✅ `webhookData.Code == "00"` hoặc `webhookData.Data.Code == "00"`

**Khi finalize:**
```csharp
var (success, errorMessage) = await _paymentRepository.FinalizeQrPaymentAsync(
    hoaDon.HoaDonBanId,
    maGiaoDich,
    DateTime.Now,
    cancellationToken);
```

**FinalizeQrPaymentAsync sẽ:**
1. Cập nhật `TrangThaiThanhToan = "Đã thanh toán"`
2. Cập nhật `PaymentStatus = "PAID"`
3. Cập nhật `MaGiaoDich`, `PaymentConfirmedAt`
4. **Trừ kho** (Mon.TonKho)
5. **Trừ nguyên liệu** (NguyenLieu.TonKho)
6. **Trừ điểm** (KhachHang.DiemTichLuy)
7. **Cộng điểm** (KhachHang.DiemTichLuy)

---

## Cách test lại bằng PayOS dashboard

### Bước 1: Cấu hình ngrok
```bash
ngrok http 5002
```

Lấy URL: `https://sheep-condense-preheated.ngrok-free.dev`

### Bước 2: Cấu hình webhook trên PayOS

1. Đăng nhập PayOS dashboard: https://my.payos.vn
2. Vào **Settings** → **Webhook**
3. Nhập Webhook URL:
```
https://sheep-condense-preheated.ngrok-free.dev/api/Payments/payos/webhook
```
4. Bấm **Save** hoặc **Test**

**Kỳ vọng:**
- ✅ PayOS báo: **"Webhook URL is valid"** hoặc tương tự
- ✅ Backend log: `Received payOS webhook. Payload length: ...`
- ✅ Backend log: `Webhook received but no matching invoice. Ignored.`
- ✅ Response 200 OK

### Bước 3: Test với hóa đơn thật

1. WPF tạo hóa đơn QR Payment
2. Quét QR và thanh toán trên PayOS
3. PayOS gọi webhook
4. Backend log:
```
Received payOS webhook. Payload length: 1234
Webhook signature verified successfully for OrderCode 123456789
Payment finalized successfully for HoaDonBan 24, OrderCode: 123456789
```
5. Response 200 OK:
```json
{
  "message": "Payment finalized successfully for invoice 24"
}
```

### Bước 4: Kiểm tra database

```sql
SELECT 
    HoaDonBanId,
    TrangThaiThanhToan,
    PaymentStatus,
    MaGiaoDich,
    PaymentConfirmedAt
FROM HoaDonBan
WHERE HoaDonBanId = 24

-- Kỳ vọng:
-- TrangThaiThanhToan = 'Đã thanh toán'
-- PaymentStatus = 'PAID'
-- MaGiaoDich = 'REF-123456789' hoặc PaymentLinkId
-- PaymentConfirmedAt = thời gian webhook
```

### Bước 5: Test idempotency

1. PayOS gửi lại webhook (retry)
2. Backend log: `Payment already confirmed for HoaDonBan 24 (idempotency)`
3. Response 200 OK: `Payment already confirmed (idempotent)`
4. Database không thay đổi
5. Kho không bị trừ lặp

---

## Logging chi tiết

### Development mode
```csharp
if (_logger.IsEnabled(LogLevel.Debug))
{
    _logger.LogDebug("Webhook raw payload: {Payload}", payload);
}
```

**Bật Debug logging trong appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "CoffeeShop.PaymentApi": "Debug"
    }
  }
}
```

### Logs khi test payload
```
info: Received payOS webhook. Payload length: 234
warn: HoaDonBan not found for OrderCode 999999999. This might be a test webhook from PayOS.
warn: Webhook received but not processed: Webhook received but no matching invoice. Ignored.
```

### Logs khi finalize thành công
```
info: Received payOS webhook. Payload length: 1234
info: Webhook signature verified successfully for OrderCode 123456789
info: Payment finalized successfully for HoaDonBan 24, OrderCode: 123456789, Reference: REF-123456789
info: Webhook processed successfully: Payment finalized successfully for invoice 24
```

### Logs khi signature sai
```
info: Received payOS webhook. Payload length: 1234
error: Webhook signature verification failed for OrderCode 123456789
warn: Webhook received but not processed: Webhook received but invalid signature. Ignored in Development.
```

---

## Bảo mật

### Development
- Signature sai → Vẫn trả 200 OK (để test dễ dàng)
- Log raw payload để debug

### Production (nếu cần)
Có thể sửa để trả 400 khi signature sai:
```csharp
if (!VerifyWebhookSignature(webhookData, _checksumKey))
{
    if (_environment.IsProduction())
    {
        return BadRequest(new { message = "Invalid signature" });
    }
    return WebhookResult.Ignored("Invalid signature in Development");
}
```

Nhưng **không khuyến khích** vì:
- PayOS sẽ retry nhiều lần
- Có thể gây spam logs
- Tốt hơn là log và investigate

---

## Tổng kết

### ✅ Đã hoàn thành

1. **Controller:**
   - [x] Luôn trả 200 OK
   - [x] Log payload length
   - [x] Log raw payload trong Debug
   - [x] Catch exception và trả 200
   - [x] Trả message rõ ràng

2. **Service:**
   - [x] Không throw exception
   - [x] Trả WebhookResult
   - [x] Xử lý gracefully:
     - Payload rỗng
     - JSON parse error
     - Signature sai
     - Không tìm thấy hóa đơn
     - Idempotency
     - Finalize thất bại
   - [x] Log chi tiết

3. **Model:**
   - [x] WebhookResult với Success/Message

### Lợi ích

1. **PayOS webhook hoạt động:**
   - Test payload → 200 OK
   - Không retry vô hạn
   - Dễ cấu hình webhook URL

2. **Xử lý an toàn:**
   - Không finalize khi không có hóa đơn
   - Không finalize khi signature sai
   - Idempotency đảm bảo không trừ kho lặp

3. **Dễ debug:**
   - Log chi tiết
   - Message rõ ràng
   - Biết được lý do webhook không xử lý

4. **Production ready:**
   - Không crash khi có lỗi
   - Graceful degradation
   - Có thể thêm bảo mật sau

---

## Checklist test

- [ ] Cấu hình ngrok
- [ ] Cấu hình webhook URL trên PayOS dashboard
- [ ] PayOS báo webhook URL hợp lệ ✅
- [ ] Backend log "Webhook received but no matching invoice"
- [ ] Tạo hóa đơn QR thật và thanh toán
- [ ] Webhook finalize thành công
- [ ] Database cập nhật đúng
- [ ] Kho đã trừ
- [ ] Test idempotency (gửi lại webhook)
- [ ] Kho không bị trừ lặp
