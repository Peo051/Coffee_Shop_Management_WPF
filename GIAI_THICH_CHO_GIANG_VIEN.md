# Giải thích Nghiệp vụ Tồn kho cho Giảng viên

## 🎯 Câu hỏi chính: "Tại sao có 2 loại TonKho?"

### Trả lời ngắn gọn:
Hệ thống quản lý **2 tầng tồn kho** (Two-tier Inventory):
1. **Mon.TonKho** = Tồn **thành phẩm** (món có thể bán ngay)
2. **NguyenLieu.TonKho** = Tồn **nguyên liệu thô** (cà phê, sữa, đường...)

---

## 📊 So sánh trực quan

| Tiêu chí | Mon.TonKho | NguyenLieu.TonKho |
|----------|------------|-------------------|
| **Là gì?** | Món ăn/đồ uống | Nguyên liệu đầu vào |
| **Đơn vị** | phần, ly, suất | kg, lít, gram |
| **Ví dụ** | 50 ly cà phê | 15 kg cà phê hạt |
| **Mục đích** | Kiểm soát phục vụ | Theo dõi chi phí |
| **Khi bán** | Trừ trực tiếp | Trừ theo công thức |

---

## 💡 Ví dụ dễ hiểu

### Bán 2 ly Cà phê sữa đá:

**Trước khi bán:**
- Mon.TonKho: 50 ly
- NguyenLieu.TonKho:
  - Cà phê: 15 kg
  - Sữa: 20 lít
  - Đường: 10 kg

**Sau khi bán:**
- Mon.TonKho: 48 ly (-2)
- NguyenLieu.TonKho:
  - Cà phê: 14.96 kg (-0.04)
  - Sữa: 19.9 lít (-0.1)
  - Đường: 9.98 kg (-0.02)

---

## ✅ Lợi ích của mô hình này

1. **Kiểm soát kép**: Đảm bảo cả món và nguyên liệu đều đủ
2. **Tính chi phí chính xác**: Biết chính xác nguyên liệu tiêu hao
3. **Cảnh báo sớm**: Phát hiện thiếu hụt ở cả 2 cấp độ
4. **Truy vết đầy đủ**: Lịch sử rõ ràng cho cả thành phẩm và nguyên liệu

---

## 🔍 Trả lời các câu hỏi thường gặp

### Q1: "Có bị dư thừa dữ liệu không?"
**A**: Không, đây là 2 cấp độ khác nhau:
- Mon.TonKho = Quản lý **sản phẩm** (product level)
- NguyenLieu.TonKho = Quản lý **chi phí** (cost level)

Giống như kế toán có cả "Kho thành phẩm" và "Kho nguyên vật liệu".

### Q2: "Tại sao không chỉ trừ nguyên liệu?"
**A**: Nếu chỉ trừ nguyên liệu thì:
- ❌ Không kiểm soát được món giới hạn (limited edition)
- ❌ Không biết món nào sắp hết
- ❌ Khó quản lý món đóng gói sẵn (không có công thức)

### Q3: "Có đảm bảo tính nhất quán không?"
**A**: Có, bằng cách:
- ✅ Transaction: Trừ cả 2 trong 1 transaction
- ✅ Kiểm tra trước: Đảm bảo đủ cả món và nguyên liệu
- ✅ Ghi lịch sử: Truy vết đầy đủ
- ✅ Validation: Không cho phép tồn kho âm

---

## 📂 Vị trí code quan trọng

### Models (Định nghĩa dữ liệu):
- `CoffeeShop.Wpf/Models/Mon.cs` - Có comment rõ ràng về Mon.TonKho
- `CoffeeShop.Wpf/Models/NguyenLieu.cs` - Có comment rõ ràng về NguyenLieu.TonKho
- `CoffeeShop.Wpf/Models/CongThucMon.cs` - Công thức món (định lượng nguyên liệu)

### Repository (Logic nghiệp vụ):
- `CoffeeShop.Wpf/Repositories/HoaDonBanRepository.cs` - Logic trừ tồn kho kép
  - Dòng ~150-300: Có comment chi tiết về quy trình trừ tồn kho

### Views (Giao diện):
- `CoffeeShop.Wpf/Views/MonView.xaml` - Form quản lý món (thành phẩm)
- `CoffeeShop.Wpf/Views/NguyenLieuView.xaml` - Form quản lý nguyên liệu

---

## 📖 Tài liệu tham khảo

- **README.md**: Phần "Quy ước nghiệp vụ Tồn kho" (đầu file)
- **NGHIEP_VU_TON_KHO.md**: Tài liệu chi tiết đầy đủ với ví dụ cụ thể

---

## 🎤 Script demo cho giảng viên

### Bước 1: Mở file Model
```
"Thưa thầy/cô, em mở file Mon.cs và NguyenLieu.cs.
Ở đây em có comment rõ ràng:
- Mon.TonKho: Tồn THÀNH PHẨM (món có thể bán)
- NguyenLieu.TonKho: Tồn NGUYÊN LIỆU THÔ (cà phê, sữa...)"
```

### Bước 2: Mở file HoaDonBanRepository
```
"Khi bán hàng, em trừ CẢ HAI loại tồn kho:
1. Trừ Mon.TonKho - kiểm soát khả năng phục vụ
2. Trừ NguyenLieu.TonKho - theo dõi chi phí thực tế

Em có comment chi tiết ở dòng 150-300 giải thích từng bước."
```

### Bước 3: Demo trên UI
```
"Trên giao diện:
- Form Món: Hiển thị 'Tồn kho thành phẩm' (phần/ly)
- Form Nguyên liệu: Hiển thị 'Tồn kho nguyên liệu' (kg/lít)

Khi bán hàng, cả 2 đều tự động trừ và ghi lịch sử."
```

---

## ✨ Điểm mạnh của thiết kế

1. **Rõ ràng**: Comment đầy đủ trong code
2. **Chuẩn nghiệp vụ**: Theo mô hình Two-tier Inventory thực tế
3. **Dễ bảo trì**: Tách biệt rõ ràng giữa thành phẩm và nguyên liệu
4. **Truy vết tốt**: Lịch sử đầy đủ cho cả 2 loại tồn kho

---

**Kết luận**: Đây không phải dư thừa mà là thiết kế chuẩn cho hệ thống quản lý quán cà phê, giúp kiểm soát đồng thời cả khả năng phục vụ và chi phí nguyên liệu.
