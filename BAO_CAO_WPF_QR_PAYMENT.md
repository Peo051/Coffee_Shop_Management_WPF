# Báo cáo: Tích hợp QR Payment vào WPF

## Tổng quan
Đã hoàn thành tích hợp QR payment vào màn hình bán hàng WPF, cho phép thu ngân tạo QR code để khách thanh toán.

---

## ✅ File đã tạo

### 1. PaymentApiClient.cs
**Path**: `CoffeeShop.Wpf/Services/PaymentApiClient.cs`

**Nội dung**:
- HttpClient gọi backend API (mặc định https://localhost:5001)
- Methods:
  - `CreateQrPaymentAsync(int hoaDonBanId)` - Tạo QR payment
  - `GetPaymentStatusAsync(int hoaDonBanId)` - Lấy trạng thái thanh toán
  - `CancelPaymentAsync(int hoaDonBanId)` - Hủy QR payment
- Models:
  - `CreateQRPaymentResponse` - Response khi tạo QR
  - `PaymentStatusResponse` - Response trạng thái thanh toán

**Timeout**: 30 giây

### 2. IntToBoolConverter.cs
**Path**: `CoffeeShop.Wpf/Converters/IntToBoolConverter.cs`

**Nội dung**:
- Converter chuyển int > 0 thành true, ngược lại false
- Dùng để hiển thị/ẩn UI QR payment khi đã có HoaDonBanId

---

## ✅ File đã sửa

### 1. HoaDonBanViewModel.cs
**Path**: `CoffeeShop.Wpf/ViewModels/HoaDonBanViewModel.cs`

**Thay đổi**:

#### Properties mới:
- `HoaDonBanIdDaTao` (int) - ID hóa đơn đã tạo (sau khi lưu thành công)
- `QrCodeUrl` (string?) - URL QR code để hiển thị
- `CheckoutUrl` (string?) - URL checkout trên web
- `PaymentStatus` (string?) - Trạng thái payment (PENDING, PAID, CANCELLED)
- `IsWaitingQrPayment` (bool) - Đang chờ thanh toán QR
- `QrExpiredAt` (DateTime?) - Thời gian hết hạn QR
- `IsPaymentPending` (bool) - Computed property: PaymentStatus == "PENDING"
- `IsPaymentPaid` (bool) - Computed property: PaymentStatus == "PAID"
- `PaymentStatusDisplay` (string) - Hiển thị trạng thái: "⏳ Chờ thanh toán", "✅ Đã thanh toán", "❌ Đã hủy"

#### Commands mới:
- `TaoQrThanhToanCommand` - Tạo QR thanh toán
- `KiemTraTrangThaiThanhToanCommand` - Kiểm tra trạng thái thanh toán
- `HuyQrThanhToanCommand` - Hủy QR thanh toán

#### Methods mới:
- `InitializeQrPaymentCommands()` - Khởi tạo commands QR payment
- `CanExecuteTaoQrThanhToan()` - Cho phép tạo QR khi: có HoaDonBanId, chưa waiting, chưa PAID
- `CanExecuteKiemTraTrangThaiThanhToan()` - Cho phép kiểm tra khi: đang waiting
- `CanExecuteHuyQrThanhToan()` - Cho phép hủy khi: đang waiting và PENDING
- `ExecuteTaoQrThanhToan()` / `TaoQrThanhToanAsync()` - Gọi API tạo QR, lưu QrCodeUrl, CheckoutUrl, PaymentStatus
- `ExecuteKiemTraTrangThaiThanhToan()` / `KiemTraTrangThaiThanhToanAsync()` - Gọi API lấy status, nếu PAID thì hiện thông báo thành công
- `ExecuteHuyQrThanhToan()` / `HuyQrThanhToanAsync()` - Gọi API hủy, reset QR state
- `ResetQrPaymentState()` - Reset tất cả properties QR payment

#### Logic cập nhật:
- **Constructor**: Gọi `InitializeQrPaymentCommands()` để khởi tạo commands
- **LuuHoaDonAsync**: Sau khi lưu thành công, lưu `HoaDonBanIdDaTao = hoaDon.HoaDonBanId` để có thể tạo QR
- **ResetFormAfterSave**: KHÔNG reset QR payment state - để user có thể tạo QR sau khi lưu hóa đơn
- **ExecuteLamMoi**: Gọi `ResetQrPaymentState()` để reset QR payment khi làm mới

### 2. HoaDonBanView.xaml
**Path**: `CoffeeShop.Wpf/Views/HoaDonBanView.xaml`

**Thay đổi**:

Thêm khu vực QR Payment sau nút "Thanh toán & tạo hóa đơn":

```xaml
<!-- ===== KHU VỰC QR PAYMENT ===== -->
<Border Margin="0,10,0,0" ... >
    <!-- Hiển thị khi HoaDonBanIdDaTao > 0 -->
    <StackPanel>
        <TextBlock Text="📱 QR Code Thanh Toán" ... />
        
        <!-- Hiển thị QR code khi IsWaitingQrPayment = true -->
        <Border>
            <StackPanel>
                <Image Source="{Binding QrCodeUrl}" Width="200" Height="200" />
                <TextBlock Text="{Binding PaymentStatusDisplay}" />
                <TextBlock Text="Hết hạn lúc: {QrExpiredAt}" />
                <TextBlock Text="Link: {CheckoutUrl}" />
            </StackPanel>
        </Border>
        
        <!-- Nút tạo QR (hiển thị khi chưa waiting và chưa paid) -->
        <Button Content="📱 Tạo QR Thanh Toán"
                Command="{Binding TaoQrThanhToanCommand}" />
        
        <!-- Nút kiểm tra và hủy (hiển thị khi đang waiting) -->
        <Grid>
            <Button Content="🔄 Kiểm Tra"
                    Command="{Binding KiemTraTrangThaiThanhToanCommand}" />
            <Button Content="❌ Hủy QR"
                    Command="{Binding HuyQrThanhToanCommand}" />
        </Grid>
        
        <!-- Thông báo đã thanh toán (hiển thị khi paid) -->
        <Border Background="SuccessBrush">
            <TextBlock Text="✅ Đã thanh toán thành công!" />
        </Border>
    </StackPanel>
</Border>
```

**Visibility logic**:
- Khu vực QR Payment: Hiển thị khi `HoaDonBanIdDaTao > 0` (dùng IntToBoolConverter)
- QR code image: Hiển thị khi `IsWaitingQrPayment = true`
- Nút "Tạo QR": Hiển thị khi `!IsWaitingQrPayment && !IsPaymentPaid`
- Nút "Kiểm tra" và "Hủy": Hiển thị khi `IsWaitingQrPayment = true`
- Thông báo thành công: Hiển thị khi `IsPaymentPaid = true`

### 3. App.xaml
**Path**: `CoffeeShop.Wpf/App.xaml`

**Thay đổi**:
- Thêm namespace: `xmlns:converters="clr-namespace:CoffeeShop.Wpf.Converters"`
- Thêm converter vào Resources: `<converters:IntToBoolConverter x:Key="IntToBoolConverter" />`

---

## 📋 Binding đã thêm

### Properties binding:
1. `HoaDonBanIdDaTao` - Dùng để hiển thị/ẩn khu vực QR payment
2. `QrCodeUrl` - Binding vào Image.Source để hiển thị QR code
3. `CheckoutUrl` - Hiển thị link checkout
4. `PaymentStatus` - Trạng thái payment (PENDING, PAID, CANCELLED)
5. `PaymentStatusDisplay` - Hiển thị trạng thái có icon
6. `QrExpiredAt` - Hiển thị thời gian hết hạn
7. `IsWaitingQrPayment` - Dùng cho Visibility của QR code và nút
8. `IsPaymentPaid` - Dùng cho Visibility của thông báo thành công

### Commands binding:
1. `TaoQrThanhToanCommand` - Nút "Tạo QR Thanh Toán"
2. `KiemTraTrangThaiThanhToanCommand` - Nút "Kiểm Tra"
3. `HuyQrThanhToanCommand` - Nút "Hủy QR"

---

## 🧪 Cách test thủ công từ WPF

### Bước 1: Chuẩn bị
1. Đảm bảo backend API đang chạy tại https://localhost:5001
2. Đảm bảo đã chạy migration `database/13_QRPayment_Integration.sql`
3. Khởi động WPF application

### Bước 2: Tạo hóa đơn
1. Mở màn hình "Bán hàng"
2. Chọn món, thêm vào hóa đơn
3. Chọn khách hàng (optional)
4. Chọn hình thức thanh toán (Tiền mặt, Chuyển khoản, v.v.)
5. Bấm "💳 Thanh toán & tạo hóa đơn"
6. Kiểm tra thông báo thành công: "Thanh toán thành công. Số gọi món: XXX. Mã hóa đơn: HDXXXXX."

### Bước 3: Tạo QR payment
1. Sau khi lưu hóa đơn thành công, khu vực "📱 QR Code Thanh Toán" sẽ hiển thị
2. Bấm nút "📱 Tạo QR Thanh Toán"
3. Kiểm tra:
   - QR code hiển thị (image 200x200)
   - Trạng thái: "⏳ Chờ thanh toán"
   - Thời gian hết hạn hiển thị
   - Link checkout hiển thị
   - Nút "Tạo QR" biến mất
   - Nút "🔄 Kiểm Tra" và "❌ Hủy QR" hiển thị

### Bước 4: Kiểm tra trạng thái thanh toán
1. Bấm nút "🔄 Kiểm Tra"
2. Nếu chưa thanh toán: Thông báo "⏳ Đang chờ khách thanh toán. Vui lòng kiểm tra lại sau."
3. Nếu đã thanh toán (test bằng Swagger/Postman webhook): 
   - Thông báo "🎉 Thanh toán thành công! Mã giao dịch: XXX. Có thể in bill và chuyển pha chế."
   - QR code biến mất
   - Nút "Kiểm tra" và "Hủy" biến mất
   - Thông báo xanh "✅ Đã thanh toán thành công!" hiển thị

### Bước 5: Test hủy QR
1. Tạo hóa đơn mới và tạo QR payment
2. Bấm nút "❌ Hủy QR"
3. Kiểm tra:
   - Thông báo "✅ Đã hủy QR payment."
   - QR code biến mất
   - Trạng thái chuyển sang "❌ Đã hủy"
   - Có thể tạo QR mới (nút "Tạo QR" hiển thị lại)

### Bước 6: Test làm mới
1. Bấm nút "Làm mới"
2. Kiểm tra:
   - Khu vực QR payment biến mất
   - Form reset về trạng thái ban đầu

### Bước 7: Test với backend thật (webhook)
1. Tạo QR payment từ WPF
2. Ghi nhớ `orderCode` từ response (xem trong log backend hoặc database)
3. Dùng Swagger/Postman gọi webhook:
```
POST https://localhost:5001/api/payments/payos/webhook
Content-Type: application/json

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
4. Quay lại WPF, bấm "🔄 Kiểm Tra"
5. Kiểm tra thông báo thành công

---

## 🔧 Cấu hình

### Backend API URL
Mặc định: `https://localhost:5001`

Để thay đổi, sửa trong `PaymentApiClient.cs`:
```csharp
private readonly PaymentApiClient _paymentApiClient = new("https://your-api-url");
```

Hoặc tạo constructor parameter trong `HoaDonBanViewModel` để inject URL từ config.

---

## 📝 Lưu ý

### 1. Luồng nghiệp vụ
- Thu ngân tạo hóa đơn trước (bấm "Thanh toán & tạo hóa đơn")
- Sau đó mới có thể tạo QR payment
- QR payment là bước phụ, không bắt buộc
- Nếu khách thanh toán tiền mặt/chuyển khoản thủ công thì không cần QR

### 2. Trạng thái QR payment
- **PENDING**: Đang chờ khách thanh toán
- **PAID**: Đã thanh toán thành công
- **CANCELLED**: Đã hủy QR

### 3. Hết hạn QR
- Mặc định: 10 phút (backend config)
- Sau khi hết hạn, có thể tạo QR mới

### 4. Idempotency
- Nếu webhook gửi lại, backend không update lặp
- WPF có thể bấm "Kiểm tra" nhiều lần mà không ảnh hưởng

### 5. Error handling
- Nếu backend không chạy: Hiển thị lỗi "Không thể tạo QR thanh toán. Vui lòng kiểm tra kết nối backend API."
- Nếu API trả lỗi: Hiển thị message từ backend

---

## 🎯 Kết quả

✅ Thu ngân có thể tạo QR thanh toán sau khi lưu hóa đơn  
✅ QR code hiển thị trên màn hình WPF  
✅ Có nút "Kiểm tra thanh toán" để kiểm tra trạng thái  
✅ Có nút "Hủy QR" để hủy payment  
✅ Khi thanh toán thành công, hiển thị thông báo và cho phép in bill / chuyển pha chế  
✅ Không sửa backend ở bước này  
✅ Không chạy build tự động  

---

## 🚀 Next Steps

1. Test thủ công theo hướng dẫn trên
2. Tích hợp in bill sau khi thanh toán thành công (nếu cần)
3. Tích hợp chuyển pha chế sau khi thanh toán thành công (nếu cần)
4. Thêm auto-refresh trạng thái thanh toán (polling mỗi 5 giây) nếu muốn tự động kiểm tra
5. Tích hợp payOS SDK thật vào backend (hiện đang mock)
