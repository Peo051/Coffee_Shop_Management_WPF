# BÁO CÁO SỬA LUỒNG QR PAYMENT TRONG WPF

## Tổng quan

Đã sửa lại luồng QR Payment trong màn hình Bán hàng tại quầy để đúng nghiệp vụ thanh toán. Tách riêng 2 luồng thanh toán: **Thanh toán thường** (tiền mặt/chuyển khoản/thẻ/ví) và **QR Payment** (chờ xác nhận từ backend).

---

## File đã chỉnh

### 1. Backend Service Layer
- **`CoffeeShop.Wpf/Services/IHoaDonBanService.cs`**
  - Thêm parameter `bool isQrPendingPayment = false` vào method `CreateAsync`

- **`CoffeeShop.Wpf/Services/HoaDonBanService.cs`**
  - Thêm parameter `bool isQrPendingPayment = false` vào method `CreateAsync`
  - Sửa logic validate: Nếu `isQrPendingPayment = true` thì không cần validate tiền khách đưa / mã giao dịch
  - Sửa `TrangThaiThanhToan`: Nếu `isQrPendingPayment = true` thì set `"Chờ thanh toán"`, ngược lại set `"Đã thanh toán"`

### 2. Payment API Client
- **`CoffeeShop.Wpf/Services/PaymentApiClient.cs`**
  - Thêm property `QRExpiredAt` vào `PaymentStatusResponse` để WPF biết khi nào QR hết hạn

### 3. ViewModel
- **`CoffeeShop.Wpf/ViewModels/HoaDonBanViewModel.cs`**
  - **Thêm hình thức thanh toán**: Thêm `"QR Payment"` vào `DanhSachHinhThucThanhToan`
  - **Thêm property**: `IsQrPaymentSelected` để nhận diện khi chọn QR Payment
  - **Sửa `IsThanhToanKhongDungTienMat`**: Loại trừ QR Payment (vì QR không cần nhập mã giao dịch ngay)
  - **Sửa `LuuHoaDonAsync`**:
    - Nếu chọn QR Payment: Không validate tiền khách đưa / mã giao dịch
    - Gọi `CreateAsync` với `isQrPendingPayment = true`
    - Sau khi tạo hóa đơn pending thành công, tự động gọi `TaoQrThanhToanAsync`
    - Không reset form sau khi tạo hóa đơn QR (để user có thể kiểm tra trạng thái)
  - **Sửa `KiemTraTrangThaiThanhToanAsync`**:
    - Thêm xử lý trạng thái `EXPIRED`: Hiển thị thông báo "QR đã hết hạn. Vui lòng tạo lại QR mới."
    - Cập nhật `QrExpiredAt` từ backend response
    - Reload data khi thanh toán thành công để cập nhật trạng thái hóa đơn
  - **Sửa `HuyQrThanhToanAsync`**:
    - Cải thiện thông báo lỗi khi không thể hủy (có thể do đã hết hạn hoặc đã thanh toán)

### 4. View (XAML)
- **`CoffeeShop.Wpf/Views/HoaDonBanView.xaml`**
  - **Tách nút thanh toán**:
    - Nút `"💳 Thanh toán & tạo hóa đơn"`: Hiển thị khi KHÔNG chọn QR Payment
    - Nút `"📱 Tạo hóa đơn chờ thanh toán QR"`: Hiển thị khi chọn QR Payment
  - **Thêm cảnh báo nghiệp vụ**: Hiển thị text nhỏ khi chọn QR Payment:
    > "ℹ️ Với QR payment, hóa đơn chỉ được xác nhận thanh toán sau khi hệ thống nhận trạng thái PAID từ backend."
  - **Sửa visibility khu vực QR**: Hiển thị khi chọn QR Payment HOẶC đã có hóa đơn pending

---

## Luồng thanh toán thường (Tiền mặt / Chuyển khoản / Thẻ / Ví)

### Bước 1: Chọn món và điền thông tin
- Chọn món, số lượng, size, ghi chú
- Chọn khách hàng (nếu có)
- Chọn khuyến mãi (nếu có)
- Chọn hình thức thanh toán: `Tiền mặt` / `Chuyển khoản` / `Thẻ` / `Ví điện tử`

### Bước 2: Validate thanh toán
- **Nếu Tiền mặt**: Nhập tiền khách đưa, phải >= số tiền thanh toán
- **Nếu không phải Tiền mặt**: Nhập mã giao dịch (bắt buộc)

### Bước 3: Bấm "💳 Thanh toán & tạo hóa đơn"
- Tạo hóa đơn với `TrangThaiThanhToan = "Đã thanh toán"`
- Trừ tồn kho thành phẩm và nguyên liệu
- Cập nhật điểm tích lũy khách hàng
- Hiển thị thông báo thành công với số gọi món, mã hóa đơn, tiền thối lại (nếu tiền mặt)
- Reset form để bán đơn mới

### Kết quả
- Hóa đơn được tạo và xác nhận thanh toán ngay lập tức
- Có thể in bill và chuyển pha chế ngay

---

## Luồng QR Payment

### Bước 1: Chọn món và điền thông tin
- Chọn món, số lượng, size, ghi chú
- Chọn khách hàng (nếu có)
- Chọn khuyến mãi (nếu có)
- **Chọn hình thức thanh toán: `QR Payment`**

### Bước 2: Bấm "📱 Tạo hóa đơn chờ thanh toán QR"
- **KHÔNG cần nhập tiền khách đưa / mã giao dịch**
- Tạo hóa đơn với `TrangThaiThanhToan = "Chờ thanh toán"`
- Trừ tồn kho thành phẩm và nguyên liệu (để đảm bảo món không bị bán trùng)
- Lưu `HoaDonBanIdDaTao`
- **Tự động gọi backend tạo QR**

### Bước 3: Hiển thị QR Code
- Hiển thị QR code image
- Hiển thị trạng thái: `⏳ Chờ thanh toán`
- Hiển thị thời gian hết hạn: `Hết hạn lúc: HH:mm:ss dd/MM/yyyy`
- Hiển thị link checkout (nếu khách muốn thanh toán qua web)
- Hiển thị 2 nút:
  - `🔄 Kiểm Tra`: Kiểm tra trạng thái thanh toán từ backend
  - `❌ Hủy QR`: Hủy QR payment

### Bước 4: Khách quét QR và thanh toán
- Khách quét QR bằng app ngân hàng
- Khách xác nhận thanh toán
- Backend nhận webhook từ payOS
- Backend cập nhật `PaymentStatus = "PAID"`, `TrangThaiThanhToan = "Đã thanh toán"`, `MaGiaoDich = reference`

### Bước 5: Thu ngân kiểm tra trạng thái
- Bấm nút `🔄 Kiểm Tra`
- Gọi `GET /api/payments/{hoaDonBanId}/status`
- **Nếu PAID**:
  - Hiển thị: `🎉 Thanh toán thành công! Mã giao dịch: {MaGiaoDich}. Có thể in bill và chuyển pha chế.`
  - `IsWaitingQrPayment = false`
  - Reload data để cập nhật trạng thái hóa đơn
  - Có thể in bill và chuyển pha chế
- **Nếu PENDING**:
  - Hiển thị: `⏳ Đang chờ khách thanh toán. Vui lòng kiểm tra lại sau.`
- **Nếu EXPIRED**:
  - Hiển thị: `⏰ QR đã hết hạn. Vui lòng tạo lại QR mới.`
  - `IsWaitingQrPayment = false`
  - Có thể bấm `📱 Tạo QR Thanh Toán` để tạo QR mới
- **Nếu CANCELLED**:
  - Hiển thị: `❌ QR payment đã bị hủy.`
  - `IsWaitingQrPayment = false`

### Bước 6 (Optional): Hủy QR
- Bấm nút `❌ Hủy QR`
- Gọi `POST /api/payments/{hoaDonBanId}/cancel`
- Backend cập nhật `PaymentStatus = "CANCELLED"`
- Hiển thị: `✅ Đã hủy QR payment.`
- **Lưu ý**: Không thể hủy nếu QR đã EXPIRED hoặc đã PAID

---

## Trạng thái hóa đơn QR trước khi khách thanh toán

### Trong Database
- `TrangThaiThanhToan = "Chờ thanh toán"`
- `PaymentStatus = "PENDING"`
- `HinhThucThanhToan = "QR Payment"`
- `QRCodeRaw = "https://img.vietqr.io/..."`
- `CheckoutUrl = "https://pay.payos.vn/web/..."`
- `QRExpiredAt = now + 10 phút`
- `MaGiaoDich = NULL` (chưa có)
- `PaymentConfirmedAt = NULL` (chưa xác nhận)

### Trong WPF
- `HoaDonBanIdDaTao > 0`
- `IsWaitingQrPayment = true`
- `PaymentStatus = "PENDING"`
- `QrCodeUrl = "https://img.vietqr.io/..."`
- `CheckoutUrl = "https://pay.payos.vn/web/..."`
- `QrExpiredAt = DateTime`

### Đặc điểm
- Hóa đơn đã được tạo trong DB
- Tồn kho đã bị trừ (để tránh bán trùng)
- Điểm tích lũy khách hàng đã bị trừ (nếu có dùng điểm)
- **NHƯNG chưa được xem là doanh thu đã thanh toán** (vì `TrangThaiThanhToan = "Chờ thanh toán"`)
- Báo cáo doanh thu sẽ lọc theo `TrangThaiThanhToan = "Đã thanh toán"` nên hóa đơn này chưa được tính

---

## Khi nào hóa đơn chuyển sang "Đã thanh toán"

### Điều kiện
1. Khách quét QR và thanh toán thành công trên app ngân hàng
2. payOS gửi webhook về backend với `code = "00"`
3. Backend verify signature (nếu có ChecksumKey)
4. Backend cập nhật DB:
   - `PaymentStatus = "PAID"`
   - `TrangThaiThanhToan = "Đã thanh toán"`
   - `MaGiaoDich = reference từ webhook`
   - `PaymentConfirmedAt = now`

### Sau khi chuyển sang "Đã thanh toán"
- Hóa đơn được tính vào doanh thu
- Có thể in bill
- Có thể chuyển pha chế
- Điểm tích lũy khách hàng được cộng (nếu có)

---

## Các bước test thủ công

### Test 1: Thanh toán thường (Tiền mặt)
1. Chọn món, số lượng
2. Chọn hình thức thanh toán: `Tiền mặt`
3. Nhập tiền khách đưa: `100000`
4. Bấm `💳 Thanh toán & tạo hóa đơn`
5. **Kỳ vọng**: Hiển thị thông báo thành công với số gọi món, mã hóa đơn, tiền thối lại
6. **Kỳ vọng**: Form được reset để bán đơn mới
7. **Kỳ vọng**: Hóa đơn trong DB có `TrangThaiThanhToan = "Đã thanh toán"`

### Test 2: Thanh toán thường (Chuyển khoản)
1. Chọn món, số lượng
2. Chọn hình thức thanh toán: `Chuyển khoản`
3. Nhập mã giao dịch: `CK123456`
4. Bấm `💳 Thanh toán & tạo hóa đơn`
5. **Kỳ vọng**: Hiển thị thông báo thành công với mã giao dịch
6. **Kỳ vọng**: Form được reset
7. **Kỳ vọng**: Hóa đơn trong DB có `TrangThaiThanhToan = "Đã thanh toán"`, `MaGiaoDich = "CK123456"`

### Test 3: QR Payment - Luồng thành công
1. Chọn món, số lượng
2. Chọn hình thức thanh toán: `QR Payment`
3. Bấm `📱 Tạo hóa đơn chờ thanh toán QR`
4. **Kỳ vọng**: Hiển thị thông báo "Đã tạo hóa đơn chờ thanh toán"
5. **Kỳ vọng**: Tự động hiển thị QR code
6. **Kỳ vọng**: Hiển thị trạng thái `⏳ Chờ thanh toán`
7. **Kỳ vọng**: Hiển thị thời gian hết hạn
8. **Kỳ vọng**: Hiển thị 2 nút: `🔄 Kiểm Tra` và `❌ Hủy QR`
9. **Kỳ vọng**: Form KHÔNG được reset (để user có thể kiểm tra trạng thái)
10. **Kỳ vọng**: Hóa đơn trong DB có `TrangThaiThanhToan = "Chờ thanh toán"`, `PaymentStatus = "PENDING"`
11. Mở Postman, gửi webhook giả lập:
    ```json
    POST https://localhost:5001/api/payments/payos/webhook
    {
      "code": "00",
      "desc": "success",
      "success": true,
      "data": {
        "orderCode": 1234567890001,
        "amount": 50000,
        "description": "HD00001",
        "reference": "FT123456789",
        "code": "00"
      }
    }
    ```
12. Bấm nút `🔄 Kiểm Tra` trong WPF
13. **Kỳ vọng**: Hiển thị `🎉 Thanh toán thành công! Mã giao dịch: FT123456789.`
14. **Kỳ vọng**: `IsWaitingQrPayment = false`
15. **Kỳ vọng**: Hóa đơn trong DB có `TrangThaiThanhToan = "Đã thanh toán"`, `PaymentStatus = "PAID"`, `MaGiaoDich = "FT123456789"`

### Test 4: QR Payment - QR hết hạn
1. Chọn món, chọn `QR Payment`, tạo hóa đơn
2. Đợi 10 phút (hoặc sửa DB thủ công: `UPDATE HoaDonBan SET QRExpiredAt = DATEADD(MINUTE, -1, GETDATE()) WHERE HoaDonBanId = 1`)
3. Bấm nút `🔄 Kiểm Tra`
4. **Kỳ vọng**: Hiển thị `⏰ QR đã hết hạn. Vui lòng tạo lại QR mới.`
5. **Kỳ vọng**: `IsWaitingQrPayment = false`
6. **Kỳ vọng**: Hiển thị nút `📱 Tạo QR Thanh Toán` để tạo lại
7. Bấm `📱 Tạo QR Thanh Toán`
8. **Kỳ vọng**: Tạo QR mới với `ProviderOrderCode` mới, `QRExpiredAt` mới

### Test 5: QR Payment - Hủy QR
1. Chọn món, chọn `QR Payment`, tạo hóa đơn
2. Bấm nút `❌ Hủy QR`
3. **Kỳ vọng**: Hiển thị `✅ Đã hủy QR payment.`
4. **Kỳ vọng**: `IsWaitingQrPayment = false`, `PaymentStatus = "CANCELLED"`
5. **Kỳ vọng**: Hóa đơn trong DB có `PaymentStatus = "CANCELLED"`
6. Thử bấm `🔄 Kiểm Tra`
7. **Kỳ vọng**: Hiển thị `❌ QR payment đã bị hủy.`

### Test 6: QR Payment - Không thể hủy nếu đã EXPIRED
1. Chọn món, chọn `QR Payment`, tạo hóa đơn
2. Sửa DB: `UPDATE HoaDonBan SET QRExpiredAt = DATEADD(MINUTE, -1, GETDATE()) WHERE HoaDonBanId = 1`
3. Bấm nút `❌ Hủy QR`
4. **Kỳ vọng**: Hiển thị lỗi "Không thể hủy QR payment. QR có thể đã hết hạn hoặc đã thanh toán."
5. **Kỳ vọng**: Backend trả `false` vì `MarkPaymentCancelledAsync` có check `QRExpiredAt > GETDATE()`

### Test 7: QR Payment - Không thể hủy nếu đã PAID
1. Chọn món, chọn `QR Payment`, tạo hóa đơn
2. Gửi webhook giả lập để set PAID
3. Bấm nút `❌ Hủy QR`
4. **Kỳ vọng**: Nút `❌ Hủy QR` bị disable (vì `CanExecuteHuyQrThanhToan` check `PaymentStatus == "PENDING"`)

---

## So sánh trước và sau

### Trước khi sửa (SAI)
1. Thu ngân bấm `Thanh toán & tạo hóa đơn` → Hóa đơn được tạo với `TrangThaiThanhToan = "Đã thanh toán"` ngay lập tức
2. Sau đó mới bấm `Tạo QR Thanh Toán`
3. **Vấn đề**: Hóa đơn đã được xem là đã thanh toán trước khi khách thật sự quét QR
4. **Vấn đề**: Nếu khách không quét QR, hóa đơn vẫn bị tính là doanh thu

### Sau khi sửa (ĐÚNG)
1. Thu ngân chọn `QR Payment` → Bấm `Tạo hóa đơn chờ thanh toán QR`
2. Hóa đơn được tạo với `TrangThaiThanhToan = "Chờ thanh toán"`
3. Tự động tạo QR và hiển thị
4. Khách quét QR và thanh toán
5. Backend nhận webhook, cập nhật `TrangThaiThanhToan = "Đã thanh toán"`
6. Thu ngân bấm `Kiểm Tra` → Xác nhận thanh toán thành công
7. **Lợi ích**: Hóa đơn chỉ được tính là doanh thu khi khách thật sự thanh toán
8. **Lợi ích**: Tách rõ 2 luồng thanh toán, không nhầm lẫn

---

## Lưu ý quan trọng

### 1. Tồn kho đã bị trừ khi tạo hóa đơn pending
- Khi tạo hóa đơn QR pending, tồn kho thành phẩm và nguyên liệu đã bị trừ ngay
- Mục đích: Tránh bán trùng món khi khách đang chờ thanh toán
- Nếu khách không thanh toán (QR hết hạn hoặc hủy), tồn kho KHÔNG được hoàn lại tự động
- **Khuyến nghị**: Cần có chức năng "Hủy hóa đơn chờ thanh toán" để hoàn lại tồn kho nếu cần

### 2. Điểm tích lũy đã bị trừ khi tạo hóa đơn pending
- Nếu khách hàng dùng điểm tích lũy, điểm đã bị trừ ngay khi tạo hóa đơn pending
- Điểm cộng mới chưa được cộng (vì chưa thanh toán)
- Nếu khách không thanh toán, điểm đã trừ KHÔNG được hoàn lại tự động
- **Khuyến nghị**: Cần có chức năng "Hủy hóa đơn chờ thanh toán" để hoàn lại điểm nếu cần

### 3. Báo cáo doanh thu
- Hóa đơn QR pending KHÔNG được tính vào doanh thu (vì `TrangThaiThanhToan = "Chờ thanh toán"`)
- Chỉ khi `TrangThaiThanhToan = "Đã thanh toán"` thì mới được tính vào doanh thu
- Đảm bảo các báo cáo lọc theo `TrangThaiThanhToan = "Đã thanh toán"` hoặc `ISNULL(TrangThaiThanhToan, N'Đã thanh toán') = N'Đã thanh toán'`

### 4. Webhook idempotency
- Backend đã xử lý idempotency: Nếu webhook gửi lại, không update lại DB
- WPF không cần xử lý idempotency, chỉ cần gọi `Kiểm Tra` để lấy trạng thái mới nhất

### 5. Không tự động polling
- WPF KHÔNG tự động polling backend để kiểm tra trạng thái
- Thu ngân phải bấm nút `🔄 Kiểm Tra` thủ công
- **Lý do**: Tránh tốn tài nguyên, tránh spam backend
- **Khuyến nghị**: Có thể thêm auto-refresh mỗi 5-10 giây nếu cần

---

## Kết luận

Đã hoàn thành sửa luồng QR Payment trong WPF để đúng nghiệp vụ:
- ✅ Tách riêng 2 luồng thanh toán: Thanh toán thường và QR Payment
- ✅ Hóa đơn QR được tạo với trạng thái `"Chờ thanh toán"` thay vì `"Đã thanh toán"`
- ✅ Chỉ khi backend xác nhận PAID thì mới chuyển sang `"Đã thanh toán"`
- ✅ Xử lý đúng trạng thái EXPIRED, CANCELLED
- ✅ UI rõ ràng, tách biệt 2 luồng thanh toán
- ✅ Cảnh báo nghiệp vụ cho user

Hệ thống đã sẵn sàng để test và triển khai!
