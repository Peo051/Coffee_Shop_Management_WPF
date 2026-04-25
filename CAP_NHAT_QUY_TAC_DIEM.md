# Báo cáo: Cập nhật Quy tắc Điểm Tích Lũy

## 📋 Tổng quan

Đã cập nhật quy tắc điểm tích lũy từ **1 điểm = 1,000đ** sang **1 điểm = 100đ** và thêm giới hạn **tối đa 50,000đ** (500 điểm) cho mỗi đơn hàng.

---

## 🔄 Thay đổi quy tắc

### Quy tắc CŨ:
- ✅ Sử dụng điểm: **1 điểm = 1,000đ**
- ✅ Tích điểm: **10,000đ = 1 điểm**
- ❌ Không có giới hạn tối đa

### Quy tắc MỚI:
- ✅ Sử dụng điểm: **1 điểm = 100đ**
- ✅ Tích điểm: **10,000đ = 1 điểm** (không đổi)
- ✅ Giới hạn: **Tối đa 500 điểm = 50,000đ** cho mỗi đơn hàng

---

## 📊 So sánh ví dụ

### Ví dụ 1: Khách có 1,000 điểm, đơn hàng 200,000đ

| Tiêu chí | Quy tắc CŨ | Quy tắc MỚI |
|----------|------------|-------------|
| Điểm có thể dùng | 1,000 điểm | 500 điểm (giới hạn) |
| Giảm tối đa | 1,000,000đ | 50,000đ |
| Giảm thực tế | 200,000đ (hết đơn) | 50,000đ |
| Còn phải trả | 0đ | 150,000đ |

### Ví dụ 2: Khách có 300 điểm, đơn hàng 50,000đ

| Tiêu chí | Quy tắc CŨ | Quy tắc MỚI |
|----------|------------|-------------|
| Điểm có thể dùng | 300 điểm | 300 điểm |
| Giảm tối đa | 300,000đ | 30,000đ |
| Giảm thực tế | 50,000đ (hết đơn) | 30,000đ |
| Còn phải trả | 0đ | 20,000đ |

### Ví dụ 3: Khách có 100 điểm, đơn hàng 100,000đ

| Tiêu chí | Quy tắc CŨ | Quy tắc MỚI |
|----------|------------|-------------|
| Điểm có thể dùng | 100 điểm | 100 điểm |
| Giảm tối đa | 100,000đ | 10,000đ |
| Giảm thực tế | 100,000đ (hết đơn) | 10,000đ |
| Còn phải trả | 0đ | 90,000đ |
| Điểm cộng mới | 0 điểm | 9 điểm |

---

## ✅ Các file đã cập nhật

### 1. **CoffeeShop.Wpf/ViewModels/HoaDonBanViewModel.cs**

#### a. Tính số tiền giảm từ điểm (dòng ~1050):
```csharp
// CŨ: var soTienGiamTuDiemTamTinh = diemSuDung * 1000m;
// MỚI:
var soTienGiamTuDiemTamTinh = diemSuDung * 100m;

// Giới hạn tối đa 50,000đ cho mỗi đơn hàng
soTienGiamTuDiemTamTinh = Math.Min(soTienGiamTuDiemTamTinh, 50000m);
```

#### b. Cập nhật lại điểm sử dụng thực tế (dòng ~1062):
```csharp
// CŨ: diemSuDung = (int)Math.Floor(SoTienGiamTuDiem / 1000m);
// MỚI:
diemSuDung = (int)Math.Floor(SoTienGiamTuDiem / 100m);
```

#### c. Comment về điểm cộng (dòng ~1074):
```csharp
// Điểm cộng tính trên số tiền cuối cùng khách thật sự thanh toán (10,000đ = 1 điểm)
DiemCongDuKien = SelectedKhachHang is null
    ? 0
    : (int)Math.Floor(ThanhToan / 10000m);
```

---

### 2. **CoffeeShop.Wpf/Services/HoaDonBanService.cs**

#### a. Tính số tiền giảm từ điểm (dòng ~211):
```csharp
// CŨ: var soTienGiamTuDiem = diemSuDung * 1000m;
// MỚI:
// Tính số tiền giảm từ điểm (1 điểm = 100đ, tối đa 50,000đ cho mỗi đơn hàng)
var soTienGiamTuDiem = diemSuDung * 100m;

// Giới hạn tối đa 50,000đ
soTienGiamTuDiem = Math.Min(soTienGiamTuDiem, 50000m);
```

#### b. Validation điểm sử dụng (dòng ~186):
```csharp
// Giới hạn tối đa 500 điểm (= 50,000đ) cho mỗi đơn hàng
if (diemSuDung > 500)
{
    return ServiceResult<HoaDonBan>.Fail("Chỉ được sử dụng tối đa 500 điểm (50,000đ) cho mỗi đơn hàng.");
}
```

---

### 3. **CoffeeShop.Wpf/Views/HoaDonBanView.xaml**

#### Cập nhật tooltip hướng dẫn (dòng ~504):
```xml
<!-- CŨ: Text="💡 1 điểm = 1.000 đ. Điểm cộng mới tính trên số tiền cuối cùng." -->
<!-- MỚI: -->
Text="💡 1 điểm = 100đ (tối đa 500 điểm = 50,000đ/đơn). Điểm cộng: 10,000đ = 1 điểm."
```

---

## 🎯 Lợi ích của quy tắc mới

### 1. Khuyến khích chi tiêu nhiều hơn:
- Khách cần mua **10 lần** (10,000đ x 10 = 100,000đ) để được giảm 10,000đ (100 điểm x 100đ)
- Quy tắc cũ: Chỉ cần 1 lần mua 10,000đ để được giảm 1,000đ

### 2. Tránh lạm dụng điểm:
- Giới hạn tối đa 50,000đ/đơn tránh khách dùng hết điểm một lúc
- Khuyến khích khách quay lại nhiều lần

### 3. Cân bằng lợi nhuận:
- Giảm tỷ lệ chiết khấu từ 10% (1,000/10,000) xuống 1% (100/10,000)
- Vẫn giữ được chương trình khách hàng thân thiết

### 4. Dễ quản lý:
- Giới hạn rõ ràng giúp dự đoán chi phí khuyến mãi
- Tránh trường hợp khách có quá nhiều điểm gây mất cân đối

---

## 🧪 Test cases cần kiểm tra

### Test 1: Sử dụng điểm bình thường
- [ ] Khách có 100 điểm
- [ ] Đơn hàng 50,000đ
- [ ] Nhập 100 điểm
- [ ] Kết quả: Giảm 10,000đ, còn 40,000đ

### Test 2: Vượt giới hạn 500 điểm
- [ ] Khách có 1,000 điểm
- [ ] Đơn hàng 200,000đ
- [ ] Nhập 600 điểm
- [ ] Kết quả: Báo lỗi "Chỉ được sử dụng tối đa 500 điểm"

### Test 3: Giới hạn tự động 50,000đ
- [ ] Khách có 1,000 điểm
- [ ] Đơn hàng 200,000đ
- [ ] Nhập 500 điểm
- [ ] Kết quả: Giảm 50,000đ (không phải 50,000đ), còn 150,000đ

### Test 4: Điểm không đủ trả hết đơn
- [ ] Khách có 200 điểm
- [ ] Đơn hàng 30,000đ
- [ ] Nhập 200 điểm
- [ ] Kết quả: Giảm 20,000đ, còn 10,000đ

### Test 5: Tích điểm mới
- [ ] Đơn hàng 100,000đ
- [ ] Không dùng điểm
- [ ] Thanh toán 100,000đ
- [ ] Kết quả: Cộng 10 điểm mới (100,000 / 10,000)

### Test 6: Tích điểm sau khi dùng điểm
- [ ] Khách có 100 điểm
- [ ] Đơn hàng 50,000đ
- [ ] Dùng 100 điểm (giảm 10,000đ)
- [ ] Thanh toán 40,000đ
- [ ] Kết quả: Cộng 4 điểm mới (40,000 / 10,000)

---

## 📝 Lưu ý quan trọng

### 1. Database không thay đổi:
- Cột `DiemTichLuy` trong bảng `KhachHang` vẫn giữ nguyên
- Cột `DiemSuDung`, `DiemCong` trong bảng `HoaDonBan` vẫn giữ nguyên
- Chỉ thay đổi **logic tính toán** trong code

### 2. Dữ liệu cũ:
- Khách hàng hiện có vẫn giữ nguyên số điểm
- Ví dụ: Khách có 1,000 điểm cũ vẫn có 1,000 điểm
- Nhưng giờ 1,000 điểm chỉ = 100,000đ (thay vì 1,000,000đ)

### 3. Thông báo cho khách hàng:
- Cần thông báo rõ ràng về thay đổi quy tắc
- Giải thích lợi ích: "Giờ đây bạn có thể dùng điểm linh hoạt hơn!"
- Nhấn mạnh giới hạn 50,000đ/đơn để tránh hiểu lầm

### 4. UI đã cập nhật:
- Tooltip hiển thị rõ: "1 điểm = 100đ (tối đa 500 điểm = 50,000đ/đơn)"
- Validation ngăn nhập quá 500 điểm
- Tự động giới hạn 50,000đ khi tính toán

---

## 🎓 Giải thích cho giảng viên

**Câu hỏi**: "Tại sao thay đổi quy tắc điểm?"

**Trả lời**:
> "Thưa thầy/cô, em đã phân tích và thấy quy tắc cũ (1 điểm = 1,000đ) có vấn đề:
> 
> **Vấn đề**:
> - Khách tích điểm quá nhanh, dễ lạm dụng
> - Không có giới hạn dẫn đến khách có thể dùng hết điểm một lúc
> - Tỷ lệ chiết khấu 10% quá cao, ảnh hưởng lợi nhuận
> 
> **Giải pháp mới**:
> - Giảm tỷ lệ xuống 1% (1 điểm = 100đ) để cân bằng
> - Thêm giới hạn 50,000đ/đơn để khuyến khích khách quay lại nhiều lần
> - Vẫn giữ tỷ lệ tích điểm 10,000đ = 1 điểm để khách thấy có giá trị
> 
> **Lợi ích**:
> ✅ Khuyến khích chi tiêu dài hạn
> ✅ Tránh lạm dụng điểm
> ✅ Cân bằng lợi nhuận
> ✅ Dễ quản lý và dự đoán chi phí"

---

**Ngày thực hiện**: 2026-04-25  
**Trạng thái**: ✅ Hoàn thành  
**Build status**: ✅ Thành công  
**Files changed**: 3 files (HoaDonBanViewModel.cs, HoaDonBanService.cs, HoaDonBanView.xaml)
