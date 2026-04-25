# Tài liệu Nghiệp vụ Quản lý Tồn kho - Coffee Shop Management System

## 📋 Tổng quan

Hệ thống quản lý quán cà phê sử dụng mô hình **Tồn kho 2 tầng (Two-tier Inventory Management)** để kiểm soát đồng thời cả thành phẩm và nguyên liệu thô.

---

## 🎯 Hai loại tồn kho trong hệ thống

### 1️⃣ Mon.TonKho - Tồn kho Thành phẩm

**Định nghĩa**: Số lượng món ăn/đồ uống có thể bán ngay cho khách hàng

**Đặc điểm**:
- **Đơn vị**: Số nguyên (phần, ly, suất, cái...)
- **Kiểu dữ liệu**: `int`
- **Ví dụ thực tế**:
  - Bánh ngọt làm sẵn: 20 cái
  - Cà phê có thể pha: 50 ly
  - Combo đặc biệt giới hạn: 10 suất
  - Sandwich đã chuẩn bị: 15 phần

**Mục đích**:
- ✅ Kiểm soát **khả năng phục vụ** khách hàng (service capacity)
- ✅ Quản lý món có **số lượng giới hạn** (limited edition)
- ✅ Cảnh báo khi **sắp hết món** để bổ sung kịp thời
- ✅ Tránh nhận đơn quá khả năng phục vụ

**Khi nào thay đổi**:
- ➖ **Trừ**: Mỗi khi bán hàng thành công
- ➕ **Cộng**: Khi nhập hàng/bổ sung món

**Vị trí trong code**:
- Model: `CoffeeShop.Wpf/Models/Mon.cs`
- Database: Bảng `dbo.Mon`, cột `TonKho`

---

### 2️⃣ NguyenLieu.TonKho - Tồn kho Nguyên liệu thô

**Định nghĩa**: Số lượng nguyên liệu đầu vào dùng để chế biến món

**Đặc điểm**:
- **Đơn vị**: Số thập phân (kg, lít, gram, ml...)
- **Kiểu dữ liệu**: `decimal`
- **Ví dụ thực tế**:
  - Cà phê hạt: 15.5 kg
  - Sữa tươi: 20 lít
  - Đường: 10 kg
  - Bột mì: 8.75 kg

**Mục đích**:
- ✅ Theo dõi **chi phí nguyên liệu** thực tế (cost tracking)
- ✅ Tính toán **lợi nhuận** chính xác cho mỗi món
- ✅ Cảnh báo khi **cần nhập hàng** nguyên liệu
- ✅ Quản lý **định mức tiêu hao** nguyên liệu

**Khi nào thay đổi**:
- ➖ **Trừ**: Tự động theo công thức món khi bán hàng
- ➕ **Cộng**: Khi nhập nguyên liệu từ nhà cung cấp

**Vị trí trong code**:
- Model: `CoffeeShop.Wpf/Models/NguyenLieu.cs`
- Database: Bảng `dbo.NguyenLieu`, cột `TonKho`

---

## 🔄 Quy trình trừ tồn kho khi bán hàng

### Sơ đồ luồng:

```
┌─────────────────────────────────────────────────────────────────┐
│                    KHÁCH HÀNG ĐẶT MÓN                           │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  BƯỚC 1: Kiểm tra Mon.TonKho (Tồn thành phẩm)                  │
│  ❓ Món này còn đủ để bán không?                                │
└────────────────────────────┬────────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │                 │
                   YES               NO
                    │                 │
                    │                 └──> ❌ Báo lỗi: "Món đã hết"
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────┐
│  BƯỚC 2: Lấy công thức món (CongThucMon)                       │
│  📋 Món này cần nguyên liệu gì, bao nhiêu?                      │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  BƯỚC 3: Kiểm tra NguyenLieu.TonKho (Tồn nguyên liệu)          │
│  ❓ Nguyên liệu còn đủ theo công thức không?                    │
└────────────────────────────┬────────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │                 │
                   YES               NO
                    │                 │
                    │                 └──> ❌ Báo lỗi: "Thiếu nguyên liệu X"
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────┐
│  BƯỚC 4: Trừ Mon.TonKho                                         │
│  ➖ Giảm số lượng món có thể bán                                │
│  📝 Ghi lịch sử: LichSuTonKho                                   │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  BƯỚC 5: Trừ NguyenLieu.TonKho (theo công thức)                │
│  ➖ Giảm nguyên liệu theo định lượng                            │
│  📝 Ghi lịch sử: LichSuNguyenLieu                               │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  BƯỚC 6: Tạo hóa đơn bán hàng                                   │
│  ✅ Hoàn tất giao dịch                                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 Ví dụ cụ thể

### Tình huống: Bán 2 ly Cà phê sữa đá

#### Dữ liệu ban đầu:

**Món: Cà phê sữa đá**
- Mon.TonKho = 50 ly
- Mon.TonKhoToiThieu = 10 ly
- DonGia = 25,000 đ

**Công thức món (CongThucMon):**
| Nguyên liệu | Định lượng (1 ly) | Đơn vị |
|-------------|-------------------|--------|
| Cà phê hạt  | 0.02              | kg     |
| Sữa tươi    | 0.05              | lít    |
| Đường       | 0.01              | kg     |

**Tồn kho nguyên liệu:**
| Nguyên liệu | TonKho | TonKhoToiThieu | Đơn vị |
|-------------|--------|----------------|--------|
| Cà phê hạt  | 15.00  | 2.00           | kg     |
| Sữa tươi    | 20.00  | 5.00           | lít    |
| Đường       | 10.00  | 1.00           | kg     |

---

#### Quy trình xử lý:

**BƯỚC 1: Kiểm tra Mon.TonKho**
```
Cần bán: 2 ly
Tồn hiện tại: 50 ly
Kết quả: ✅ Đủ món để bán
```

**BƯỚC 2: Lấy công thức món**
```
Món "Cà phê sữa đá" có 3 nguyên liệu:
- Cà phê hạt: 0.02 kg/ly
- Sữa tươi: 0.05 lít/ly
- Đường: 0.01 kg/ly
```

**BƯỚC 3: Kiểm tra NguyenLieu.TonKho**
```
Cà phê hạt:
  Cần: 2 ly × 0.02 kg = 0.04 kg
  Còn: 15.00 kg
  Kết quả: ✅ Đủ

Sữa tươi:
  Cần: 2 ly × 0.05 lít = 0.10 lít
  Còn: 20.00 lít
  Kết quả: ✅ Đủ

Đường:
  Cần: 2 ly × 0.01 kg = 0.02 kg
  Còn: 10.00 kg
  Kết quả: ✅ Đủ
```

**BƯỚC 4: Trừ Mon.TonKho**
```sql
UPDATE dbo.Mon
SET TonKho = TonKho - 2  -- 50 → 48
WHERE MonId = 1;

-- Ghi lịch sử
INSERT INTO dbo.LichSuTonKho (MonId, LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, ...)
VALUES (1, 'BanHang', -2, 50, 48, ...);
```

**BƯỚC 5: Trừ NguyenLieu.TonKho**
```sql
-- Cà phê hạt
UPDATE dbo.NguyenLieu
SET TonKho = TonKho - 0.04  -- 15.00 → 14.96
WHERE NguyenLieuId = 1;

-- Sữa tươi
UPDATE dbo.NguyenLieu
SET TonKho = TonKho - 0.10  -- 20.00 → 19.90
WHERE NguyenLieuId = 2;

-- Đường
UPDATE dbo.NguyenLieu
SET TonKho = TonKho - 0.02  -- 10.00 → 9.98
WHERE NguyenLieuId = 3;

-- Ghi lịch sử cho từng nguyên liệu
INSERT INTO dbo.LichSuNguyenLieu (NguyenLieuId, LoaiPhatSinh, SoLuongThayDoi, ...)
VALUES (1, 'XuatKho', -0.04, ...), (2, 'XuatKho', -0.10, ...), (3, 'XuatKho', -0.02, ...);
```

**BƯỚC 6: Kết quả cuối cùng**

| Loại tồn kho | Trước | Thay đổi | Sau |
|--------------|-------|----------|-----|
| **Mon.TonKho** | | | |
| Cà phê sữa đá | 50 ly | -2 ly | 48 ly |
| **NguyenLieu.TonKho** | | | |
| Cà phê hạt | 15.00 kg | -0.04 kg | 14.96 kg |
| Sữa tươi | 20.00 lít | -0.10 lít | 19.90 lít |
| Đường | 10.00 kg | -0.02 kg | 9.98 kg |

---

## 💡 Lợi ích của mô hình 2 tầng

### 1. Kiểm soát kép (Double-check)
- ✅ Đảm bảo cả món và nguyên liệu đều đủ trước khi bán
- ✅ Tránh tình trạng "có món nhưng không có nguyên liệu"
- ✅ Tránh tình trạng "có nguyên liệu nhưng không kịp chế biến"

### 2. Tính chi phí chính xác (Accurate costing)
- ✅ Biết chính xác nguyên liệu tiêu hao cho mỗi món
- ✅ Tính được giá vốn thực tế (COGS - Cost of Goods Sold)
- ✅ Phân tích lợi nhuận từng món chi tiết

### 3. Cảnh báo sớm (Early warning)
- ✅ Phát hiện thiếu hụt ở cả 2 cấp độ (món & nguyên liệu)
- ✅ Cảnh báo khi Mon.TonKho ≤ Mon.TonKhoToiThieu
- ✅ Cảnh báo khi NguyenLieu.TonKho ≤ NguyenLieu.TonKhoToiThieu

### 4. Truy vết đầy đủ (Full traceability)
- ✅ Lịch sử rõ ràng cho cả thành phẩm (LichSuTonKho)
- ✅ Lịch sử rõ ràng cho nguyên liệu (LichSuNguyenLieu)
- ✅ Dễ dàng audit và kiểm tra sai sót

### 5. Linh hoạt (Flexibility)
- ✅ Có thể bán món không cần công thức (đồ đóng gói sẵn)
- ✅ Có thể bán món có công thức phức tạp (nhiều nguyên liệu)
- ✅ Dễ dàng điều chỉnh công thức mà không ảnh hưởng tồn kho

---

## 🔍 Các trường hợp đặc biệt

### Trường hợp 1: Món không có công thức
**Ví dụ**: Nước ngọt đóng chai, snack đóng gói

**Xử lý**:
- ✅ Vẫn trừ Mon.TonKho bình thường
- ⚠️ Không trừ NguyenLieu.TonKho (vì không có công thức)
- 📝 Ghi chú: Món này quản lý như hàng hóa thành phẩm

### Trường hợp 2: Món có nhiều nguyên liệu
**Ví dụ**: Bánh kem sinh nhật (bột, trứng, sữa, kem, đường, hoa quả...)

**Xử lý**:
- ✅ Trừ Mon.TonKho: 1 cái
- ✅ Trừ NguyenLieu.TonKho: Tất cả nguyên liệu theo công thức
- 📝 Hệ thống tự động lặp qua tất cả nguyên liệu trong CongThucMon

### Trường hợp 3: Thiếu nguyên liệu giữa chừng
**Ví dụ**: Đủ cà phê và sữa, nhưng hết đường

**Xử lý**:
- ❌ Rollback toàn bộ giao dịch (transaction)
- ❌ Không trừ Mon.TonKho
- ❌ Không trừ bất kỳ NguyenLieu.TonKho nào
- 📝 Báo lỗi rõ ràng: "Không đủ nguyên liệu 'Đường' (cần 0.02 kg, còn 0 kg)"

### Trường hợp 4: Hủy hóa đơn
**Ví dụ**: Khách hàng hủy đơn sau khi đã thanh toán

**Xử lý**:
- ✅ Cộng lại Mon.TonKho
- ✅ Cộng lại NguyenLieu.TonKho (tất cả nguyên liệu)
- 📝 Ghi lịch sử: LoaiPhatSinh = "HuyHoaDon"

---

## 📂 Vị trí code liên quan

### Models
- `CoffeeShop.Wpf/Models/Mon.cs` - Model món (thành phẩm)
- `CoffeeShop.Wpf/Models/NguyenLieu.cs` - Model nguyên liệu
- `CoffeeShop.Wpf/Models/CongThucMon.cs` - Model công thức món

### Repositories
- `CoffeeShop.Wpf/Repositories/HoaDonBanRepository.cs` - Logic trừ tồn kho khi bán
- `CoffeeShop.Wpf/Repositories/MonRepository.cs` - CRUD món
- `CoffeeShop.Wpf/Repositories/NguyenLieuRepository.cs` - CRUD nguyên liệu
- `CoffeeShop.Wpf/Repositories/LichSuTonKhoRepository.cs` - Lịch sử tồn món
- `CoffeeShop.Wpf/Repositories/LichSuNguyenLieuRepository.cs` - Lịch sử tồn nguyên liệu

### Services
- `CoffeeShop.Wpf/Services/HoaDonBanService.cs` - Service bán hàng
- `CoffeeShop.Wpf/Services/MonService.cs` - Service quản lý món
- `CoffeeShop.Wpf/Services/NguyenLieuService.cs` - Service quản lý nguyên liệu

---

## 🎓 Hướng dẫn giải thích cho giảng viên

### Câu hỏi 1: "Tại sao phải quản lý 2 loại tồn kho?"

**Trả lời**:
> "Thưa thầy/cô, em thiết kế hệ thống với 2 loại tồn kho vì:
> 
> 1. **Mon.TonKho** giúp kiểm soát **khả năng phục vụ** - đảm bảo không nhận quá nhiều đơn vượt khả năng chế biến
> 2. **NguyenLieu.TonKho** giúp theo dõi **chi phí thực tế** - biết chính xác nguyên liệu tiêu hao để tính lợi nhuận
> 
> Ví dụ thực tế: Quán có thể pha được 50 ly cà phê (Mon.TonKho), nhưng nếu chỉ còn 0.5kg cà phê hạt (NguyenLieu.TonKho) thì thực tế chỉ pha được 25 ly. Hệ thống sẽ cảnh báo cả 2 để quản lý chủ động bổ sung."

### Câu hỏi 2: "Có bị dư thừa dữ liệu không?"

**Trả lời**:
> "Thưa thầy/cô, đây không phải dư thừa mà là **quản lý 2 cấp độ khác nhau**:
> 
> - Mon.TonKho = Tồn **thành phẩm** (đơn vị: ly, phần) - Quản lý **sản phẩm**
> - NguyenLieu.TonKho = Tồn **nguyên liệu** (đơn vị: kg, lít) - Quản lý **chi phí**
> 
> Giống như trong kế toán có cả **Kho thành phẩm** và **Kho nguyên vật liệu** - đây là chuẩn quản lý kho 2 tầng (Two-tier Inventory) trong thực tế."

### Câu hỏi 3: "Tại sao không chỉ trừ nguyên liệu thôi?"

**Trả lời**:
> "Thưa thầy/cô, nếu chỉ trừ nguyên liệu thì:
> 
> ❌ Không kiểm soát được **số lượng món giới hạn** (limited edition)
> ❌ Không biết **món nào sắp hết** để chuẩn bị
> ❌ Khó quản lý **món đóng gói sẵn** (không có công thức)
> 
> Ví dụ: Bánh ngọt làm sẵn 20 cái - nếu không có Mon.TonKho thì không biết còn bao nhiêu cái để bán."

### Câu hỏi 4: "Có đảm bảo tính nhất quán dữ liệu không?"

**Trả lời**:
> "Thưa thầy/cô, em đảm bảo tính nhất quán bằng:
> 
> ✅ **Transaction**: Trừ cả 2 loại tồn kho trong 1 transaction - nếu lỗi thì rollback toàn bộ
> ✅ **Kiểm tra trước**: Kiểm tra đủ cả món và nguyên liệu trước khi trừ
> ✅ **Ghi lịch sử**: Mỗi lần thay đổi đều ghi vào LichSuTonKho và LichSuNguyenLieu để truy vết
> ✅ **Validation**: Không cho phép tồn kho âm ở cả 2 cấp độ"

---

## 📝 Tóm tắt

| Tiêu chí | Mon.TonKho | NguyenLieu.TonKho |
|----------|------------|-------------------|
| **Đối tượng** | Thành phẩm (món) | Nguyên liệu thô |
| **Đơn vị** | Phần, ly, suất | kg, lít, gram |
| **Kiểu dữ liệu** | `int` | `decimal` |
| **Mục đích** | Kiểm soát phục vụ | Theo dõi chi phí |
| **Khi bán hàng** | Trừ trực tiếp | Trừ theo công thức |
| **Lịch sử** | LichSuTonKho | LichSuNguyenLieu |
| **Cảnh báo** | Sắp hết món | Cần nhập nguyên liệu |

---

**Ngày tạo**: 2026-04-25  
**Phiên bản**: 1.0  
**Tác giả**: Coffee Shop Development Team
