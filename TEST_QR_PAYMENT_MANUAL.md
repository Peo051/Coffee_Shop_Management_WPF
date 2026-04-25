# CHECKLIST KIỂM THỬ THỦ CÔNG - QR PAYMENT

## CHUẨN BỊ

### Ghi nhận dữ liệu ban đầu

```sql
-- Chọn món để test (ví dụ: MonId = 1)
SELECT MonId, TenMon, TonKho, DonGia FROM dbo.Mon WHERE MonId = 1;

-- Chọn khách hàng để test (ví dụ: KhachHangId = 1)
SELECT KhachHangId, HoTen, DiemTichLuy FROM dbo.KhachHang WHERE KhachHangId = 1;

-- Kiểm tra nguyên liệu của món
SELECT nl.NguyenLieuId, nl.TenNguyenLieu, nl.TonKho, nl.DonViTinh, ct.DinhLuong
FROM dbo.CongThucMon ct
INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
WHERE ct.MonId = 1 AND ct.IsActive = 1;
```

**Ghi lại:**
- Mon.TonKho ban đầu: ______
- NguyenLieu.TonKho ban đầu: ______
- KhachHang.DiemTichLuy ban đầu: ______

---

## TEST 1: QR PAYMENT - TẠO HÓA ĐƠN PENDING

### Thao tác WPF

1. Mở WPF → Màn hình Bán hàng
2. Chọn món (MonId = 1, SoLuong = 2)
3. Chọn khách hàng (KhachHangId = 1)
4. Nhập điểm sử dụng: 10 điểm
5. **Chọn hình thức thanh toán: "QR Payment"**
6. Bấm **"Tạo hóa đơn chờ thanh toán QR"**
7. Ghi lại HoaDonBanId: ______

### SQL Kiểm tra

```sql
DECLARE @HoaDonBanId INT = 123; -- <-- Thay bằng ID vừa tạo

-- 1. Kiểm tra trạng thái hóa đơn
SELECT HoaDonBanId, TrangThaiThanhToan, PaymentStatus, HinhThucThanhToan, 
       DiemSuDung, DiemCong, ProviderOrderCode, QRExpiredAt
FROM dbo.HoaDonBan WHERE HoaDonBanId = @HoaDonBanId;

-- 2. Kiểm tra tồn kho món KHÔNG bị trừ
SELECT MonId, TenMon, TonKho FROM dbo.Mon WHERE MonId = 1;

-- 3. Kiểm tra tồn kho nguyên liệu KHÔNG bị trừ
SELECT nl.NguyenLieuId, nl.TenNguyenLieu, nl.TonKho
FROM dbo.CongThucMon ct
INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
WHERE ct.MonId = 1 AND ct.IsActive = 1;

-- 4. Kiểm tra điểm khách hàng KHÔNG bị trừ
SELECT KhachHangId, HoTen, DiemTichLuy FROM dbo.KhachHang WHERE KhachHangId = 1;

-- 5. Kiểm tra KHÔNG có lịch sử tồn kho
SELECT COUNT(*) AS SoLuongLichSuTonKho FROM dbo.LichSuTonKho WHERE HoaDonBanId = @HoaDonBanId;
SELECT COUNT(*) AS SoLuongLichSuNguyenLieu FROM dbo.LichSuNguyenLieu WHERE HoaDonBanId = @HoaDonBanId;
```

### Kết quả kỳ vọng

| Kiểm tra | Kỳ vọng |
|----------|---------|
| TrangThaiThanhToan | `"Chờ thanh toán"` |
| PaymentStatus | `"PENDING"` |
| HinhThucThanhToan | `"QR Payment"` hoặc `"QR Code"` |
| ProviderOrderCode | Có giá trị (không NULL) |
| QRExpiredAt | Có giá trị (thời gian hết hạn) |
| Mon.TonKho | **= Giá trị ban đầu** (KHÔNG thay đổi) |
| NguyenLieu.TonKho | **= Giá trị ban đầu** (KHÔNG thay đổi) |
| KhachHang.DiemTichLuy | **= Giá trị ban đầu** (KHÔNG thay đổi) |
| LichSuTonKho | **0 record** |
| LichSuNguyenLieu | **0 record** |

---

## TEST 2: QR PAYMENT - WEBHOOK PAID

### Thao tác Postman

Gửi webhook PAID đến backend:

```http
POST https://localhost:5001/api/payments/payos/webhook
Content-Type: application/json

{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 1234567890123,
    "amount": 50000,
    "description": "HD00123",
    "reference": "FT123456789",
    "code": "00"
  },
  "signature": ""
}
```

**Lưu ý:** Thay `orderCode` bằng giá trị `ProviderOrderCode` từ Test 1

### SQL Kiểm tra

```sql
DECLARE @HoaDonBanId INT = 123; -- <-- Cùng ID từ Test 1

-- 1. Kiểm tra trạng thái hóa đơn đã chuyển sang PAID
SELECT HoaDonBanId, TrangThaiThanhToan, PaymentStatus, MaGiaoDich, PaymentConfirmedAt
FROM dbo.HoaDonBan WHERE HoaDonBanId = @HoaDonBanId;

-- 2. Kiểm tra tồn kho món ĐÃ BỊ TRỪ
SELECT MonId, TenMon, TonKho FROM dbo.Mon WHERE MonId = 1;

-- 3. Kiểm tra tồn kho nguyên liệu ĐÃ BỊ TRỪ
SELECT nl.NguyenLieuId, nl.TenNguyenLieu, nl.TonKho, ct.DinhLuong
FROM dbo.CongThucMon ct
INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
WHERE ct.MonId = 1 AND ct.IsActive = 1;

-- 4. Kiểm tra điểm khách hàng ĐÃ CẬP NHẬT
SELECT KhachHangId, HoTen, DiemTichLuy FROM dbo.KhachHang WHERE KhachHangId = 1;

-- 5. Kiểm tra CÓ lịch sử tồn kho
SELECT LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, GhiChu, ThoiGian
FROM dbo.LichSuTonKho WHERE HoaDonBanId = @HoaDonBanId;

SELECT LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, GhiChu, ThoiGian
FROM dbo.LichSuNguyenLieu WHERE HoaDonBanId = @HoaDonBanId;
```

### Kết quả kỳ vọng

| Kiểm tra | Kỳ vọng |
|----------|---------|
| TrangThaiThanhToan | `"Đã thanh toán"` |
| PaymentStatus | `"PAID"` |
| MaGiaoDich | `"FT123456789"` (từ webhook) |
| PaymentConfirmedAt | Có giá trị (thời gian xác nhận) |
| Mon.TonKho | **= Giá trị ban đầu - 2** |
| NguyenLieu.TonKho | **= Giá trị ban đầu - (DinhLuong × 2)** |
| KhachHang.DiemTichLuy | **= Giá trị ban đầu - 10 + DiemCong** |
| LichSuTonKho | **Có record**, LoaiPhatSinh = `'BanHang'`, GhiChu chứa "QR Payment finalized" |
| LichSuNguyenLieu | **Có record**, LoaiPhatSinh = `'XuatKho'`, GhiChu chứa "QR Payment finalized" |

---

## TEST 3: WEBHOOK GỬI LẶP (IDEMPOTENCY)

### Thao tác Postman

Gửi lại webhook PAID lần 2 (giống hệt Test 2)

### SQL Kiểm tra

```sql
DECLARE @HoaDonBanId INT = 123; -- <-- Cùng ID từ Test 1

-- 1. Kiểm tra tồn kho KHÔNG bị trừ lặp
SELECT MonId, TenMon, TonKho FROM dbo.Mon WHERE MonId = 1;

SELECT nl.NguyenLieuId, nl.TenNguyenLieu, nl.TonKho
FROM dbo.CongThucMon ct
INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
WHERE ct.MonId = 1 AND ct.IsActive = 1;

SELECT KhachHangId, DiemTichLuy FROM dbo.KhachHang WHERE KhachHangId = 1;

-- 2. Kiểm tra KHÔNG có lịch sử trùng lặp
SELECT COUNT(*) AS SoLuongLichSuTonKho FROM dbo.LichSuTonKho WHERE HoaDonBanId = @HoaDonBanId;
SELECT COUNT(*) AS SoLuongLichSuNguyenLieu FROM dbo.LichSuNguyenLieu WHERE HoaDonBanId = @HoaDonBanId;
```

### Kết quả kỳ vọng

| Kiểm tra | Kỳ vọng |
|----------|---------|
| Mon.TonKho | **= Giá trị từ Test 2** (KHÔNG thay đổi thêm) |
| NguyenLieu.TonKho | **= Giá trị từ Test 2** (KHÔNG thay đổi thêm) |
| KhachHang.DiemTichLuy | **= Giá trị từ Test 2** (KHÔNG thay đổi thêm) |
| Số lượng LichSuTonKho | **= Số lượng từ Test 2** (KHÔNG tăng thêm) |
| Số lượng LichSuNguyenLieu | **= Số lượng từ Test 2** (KHÔNG tăng thêm) |

---

## TEST 4: THANH TOÁN TIỀN MẶT

### Ghi nhận dữ liệu ban đầu

```sql
-- Chọn món khác để test (ví dụ: MonId = 2)
SELECT MonId, TenMon, TonKho FROM dbo.Mon WHERE MonId = 2;

SELECT nl.NguyenLieuId, nl.TenNguyenLieu, nl.TonKho, ct.DinhLuong
FROM dbo.CongThucMon ct
INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
WHERE ct.MonId = 2 AND ct.IsActive = 1;

SELECT KhachHangId, HoTen, DiemTichLuy FROM dbo.KhachHang WHERE KhachHangId = 2;
```

**Ghi lại:**
- Mon.TonKho ban đầu: ______
- NguyenLieu.TonKho ban đầu: ______
- KhachHang.DiemTichLuy ban đầu: ______

### Thao tác WPF

1. Mở WPF → Màn hình Bán hàng
2. Chọn món (MonId = 2, SoLuong = 3)
3. Chọn khách hàng (KhachHangId = 2)
4. **Chọn hình thức thanh toán: "Tiền mặt"**
5. Nhập tiền khách đưa: 100000
6. Bấm **"Thanh toán & tạo hóa đơn"**
7. Ghi lại HoaDonBanId: ______

### SQL Kiểm tra

```sql
DECLARE @HoaDonBanId INT = 456; -- <-- Thay bằng ID vừa tạo

-- 1. Kiểm tra trạng thái hóa đơn
SELECT HoaDonBanId, TrangThaiThanhToan, PaymentStatus, HinhThucThanhToan, 
       TienKhachDua, TienThoiLai
FROM dbo.HoaDonBan WHERE HoaDonBanId = @HoaDonBanId;

-- 2. Kiểm tra tồn kho món ĐÃ BỊ TRỪ NGAY
SELECT MonId, TenMon, TonKho FROM dbo.Mon WHERE MonId = 2;

-- 3. Kiểm tra tồn kho nguyên liệu ĐÃ BỊ TRỪ NGAY
SELECT nl.NguyenLieuId, nl.TenNguyenLieu, nl.TonKho, ct.DinhLuong
FROM dbo.CongThucMon ct
INNER JOIN dbo.NguyenLieu nl ON ct.NguyenLieuId = nl.NguyenLieuId
WHERE ct.MonId = 2 AND ct.IsActive = 1;

-- 4. Kiểm tra điểm khách hàng ĐÃ CẬP NHẬT NGAY
SELECT KhachHangId, DiemTichLuy FROM dbo.KhachHang WHERE KhachHangId = 2;

-- 5. Kiểm tra CÓ lịch sử tồn kho NGAY
SELECT LoaiPhatSinh, SoLuongThayDoi, GhiChu FROM dbo.LichSuTonKho WHERE HoaDonBanId = @HoaDonBanId;
SELECT LoaiPhatSinh, SoLuongThayDoi, GhiChu FROM dbo.LichSuNguyenLieu WHERE HoaDonBanId = @HoaDonBanId;
```

### Kết quả kỳ vọng

| Kiểm tra | Kỳ vọng |
|----------|---------|
| TrangThaiThanhToan | `"Đã thanh toán"` (NGAY LẬP TỨC) |
| HinhThucThanhToan | `"Tiền mặt"` |
| TienKhachDua | 100000 |
| TienThoiLai | Có giá trị (tiền thối) |
| Mon.TonKho | **= Giá trị ban đầu - 3** (đã trừ NGAY) |
| NguyenLieu.TonKho | **= Giá trị ban đầu - (DinhLuong × 3)** (đã trừ NGAY) |
| KhachHang.DiemTichLuy | **= Giá trị ban đầu - DiemSuDung + DiemCong** (đã cập nhật NGAY) |
| LichSuTonKho | **Có record NGAY**, GhiChu chứa "Bán hàng" (KHÔNG chứa "QR Payment finalized") |
| LichSuNguyenLieu | **Có record NGAY**, GhiChu chứa "Xuất kho" |

---

## BẢNG TỔNG HỢP KẾT QUẢ

| Test Case | Thời điểm | Mon.TonKho | NguyenLieu.TonKho | DiemTichLuy | LichSuTonKho | TrangThaiThanhToan |
|-----------|-----------|------------|-------------------|-------------|--------------|-------------------|
| **QR Pending** | Sau tạo HĐ | ✅ KHÔNG đổi | ✅ KHÔNG đổi | ✅ KHÔNG đổi | ✅ KHÔNG có | "Chờ thanh toán" |
| **QR PAID** | Sau webhook | ❌ Đã trừ | ❌ Đã trừ | ❌ Đã cập nhật | ❌ Đã có | "Đã thanh toán" |
| **Webhook lặp** | Sau webhook lần 2 | ✅ KHÔNG đổi thêm | ✅ KHÔNG đổi thêm | ✅ KHÔNG đổi thêm | ✅ KHÔNG tăng | "Đã thanh toán" |
| **Tiền mặt** | Sau tạo HĐ | ❌ Đã trừ NGAY | ❌ Đã trừ NGAY | ❌ Đã cập nhật NGAY | ❌ Đã có NGAY | "Đã thanh toán" |

---

## RỦI RO CẦN TEST SAU

### Tình huống: QR Pending nhưng hết hàng trước khi webhook PAID

**Mô tả:**
1. Tạo hóa đơn QR pending (chưa trừ kho)
2. Trong lúc chờ khách thanh toán, món/nguyên liệu bị bán hết bởi đơn hàng khác
3. Khách quét QR và webhook PAID được gửi đến
4. Backend finalize không đủ kho để trừ

**Hành vi kỳ vọng:**
- Backend `FinalizeQrPaymentAsync()` phải kiểm tra tồn kho trước khi trừ
- Nếu không đủ kho → rollback transaction, return error
- Hóa đơn VẪN ở trạng thái "Chờ thanh toán" (KHÔNG chuyển sang "Đã thanh toán")
- Cần xử lý thủ công: Hoàn tiền cho khách hoặc bổ sung kho rồi finalize lại

**Cách test:**
1. Tạo hóa đơn QR pending với món (MonId = 3, SoLuong = 5)
2. Sửa DB thủ công: `UPDATE Mon SET TonKho = 2 WHERE MonId = 3` (giả lập hết hàng)
3. Gửi webhook PAID
4. Kiểm tra:
   - Backend log error: "Không đủ tồn kho món '...' (cần 5, còn 2)"
   - Hóa đơn VẪN ở trạng thái "Chờ thanh toán"
   - Tồn kho món VẪN = 2 (KHÔNG bị trừ)
   - Transaction bị rollback, không có thay đổi nào

**SQL Kiểm tra:**
```sql
-- Giả lập hết hàng
UPDATE dbo.Mon SET TonKho = 2 WHERE MonId = 3;

-- Sau khi gửi webhook, kiểm tra
SELECT TrangThaiThanhToan, PaymentStatus FROM dbo.HoaDonBan WHERE HoaDonBanId = @HoaDonBanId;
SELECT TonKho FROM dbo.Mon WHERE MonId = 3;
```

**Kỳ vọng:**
- TrangThaiThanhToan = "Chờ thanh toán" (KHÔNG chuyển sang "Đã thanh toán")
- TonKho = 2 (KHÔNG bị trừ)

---

## SQL QUERY TỔNG HỢP (KIỂM TRA NHANH)

```sql
-- Thay @HoaDonBanId bằng ID cần kiểm tra
DECLARE @HoaDonBanId INT = 123;

-- 1. Thông tin hóa đơn
SELECT HoaDonBanId, TrangThaiThanhToan, PaymentStatus, HinhThucThanhToan, 
       TongTien, DiemSuDung, DiemCong, MaGiaoDich, PaymentConfirmedAt
FROM dbo.HoaDonBan WHERE HoaDonBanId = @HoaDonBanId;

-- 2. Chi tiết hóa đơn + tồn kho hiện tại
SELECT ct.MonId, m.TenMon, ct.SoLuong, ct.DonGiaBan, m.TonKho AS TonKhoHienTai
FROM dbo.ChiTietHoaDonBan ct
INNER JOIN dbo.Mon m ON ct.MonId = m.MonId
WHERE ct.HoaDonBanId = @HoaDonBanId;

-- 3. Lịch sử tồn kho
SELECT LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, GhiChu, ThoiGian
FROM dbo.LichSuTonKho WHERE HoaDonBanId = @HoaDonBanId;

SELECT LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, GhiChu, ThoiGian
FROM dbo.LichSuNguyenLieu WHERE HoaDonBanId = @HoaDonBanId;

-- 4. Điểm khách hàng (nếu có)
SELECT kh.KhachHangId, kh.HoTen, kh.DiemTichLuy
FROM dbo.HoaDonBan hd
INNER JOIN dbo.KhachHang kh ON hd.KhachHangId = kh.KhachHangId
WHERE hd.HoaDonBanId = @HoaDonBanId;
```
