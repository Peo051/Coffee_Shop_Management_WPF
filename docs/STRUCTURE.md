# 📚 Cấu trúc tài liệu dự án

## Tài liệu chính (Root)

### 1. **README.md** 
- Tài liệu tổng quan dự án
- Giới thiệu tính năng
- Kiến trúc hệ thống
- Links đến các tài liệu khác

### 2. **QUICK_START.md**
- Hướng dẫn nhanh 5 phút
- Các bước cơ bản nhất
- Dành cho người muốn chạy nhanh

### 3. **HUONG_DAN_CHAY.md**
- Hướng dẫn chi tiết đầy đủ
- Cài đặt môi trường
- Cấu hình database
- Chạy ứng dụng
- Xử lý lỗi

---

## Tài liệu kỹ thuật (docs/)

### Payment API
- `BAO_CAO_TICH_HOP_PAYOS_THAT.md` - Báo cáo tích hợp PayOS
- `BAO_CAO_HIEN_THI_QR_VA_DOI_SOAT_PAYOS.md` - QR Code và đối soát
- `BAO_CAO_SUA_WEBHOOK_PAYOS_400.md` - Fix webhook 400
- `HUONG_DAN_CHAY_PAYMENT_API.md` - Hướng dẫn chạy API
- `HUONG_DAN_SUA_LOI_WEBHOOK_KHONG_HOAT_DONG.md` - Fix webhook

### WPF Configuration
- `HUONG_DAN_CAU_HINH_WPF_CHO_NGUOI_KHAC.md` - Cấu hình WPF cho người khác
- `BAO_CAO_CAU_HINH_WPF_PUBLIC_API.md` - Báo cáo kỹ thuật WPF + Public API

---

## Quy tắc đặt tên

### Hướng dẫn (HUONG_DAN_*.md)
- Dành cho người dùng cuối
- Hướng dẫn từng bước
- Có ảnh minh họa nếu cần

### Báo cáo (BAO_CAO_*.md)
- Tài liệu kỹ thuật
- Giải thích chi tiết implementation
- Dành cho developers

### Quick guides (QUICK_*.md)
- Hướng dẫn nhanh
- Tóm tắt các bước chính
- Không có giải thích chi tiết

---

## Cấu trúc thư mục đề xuất

```
Coffee_Shop_Management_WPF/
├── README.md                           # Tổng quan
├── QUICK_START.md                      # Hướng dẫn nhanh
├── HUONG_DAN_CHAY.md                   # Hướng dẫn chi tiết
│
├── docs/                               # Tài liệu kỹ thuật
│   ├── payment-api/
│   │   ├── BAO_CAO_TICH_HOP_PAYOS_THAT.md
│   │   ├── BAO_CAO_HIEN_THI_QR_VA_DOI_SOAT_PAYOS.md
│   │   ├── BAO_CAO_SUA_WEBHOOK_PAYOS_400.md
│   │   ├── HUONG_DAN_CHAY_PAYMENT_API.md
│   │   └── HUONG_DAN_SUA_LOI_WEBHOOK_KHONG_HOAT_DONG.md
│   │
│   ├── wpf-config/
│   │   ├── HUONG_DAN_CAU_HINH_WPF_CHO_NGUOI_KHAC.md
│   │   └── BAO_CAO_CAU_HINH_WPF_PUBLIC_API.md
│   │
│   └── STRUCTURE.md                    # File này
│
├── database/                           # SQL scripts
├── CoffeeShop.Wpf/                    # WPF project
└── CoffeeShop.PaymentApi/             # API project
```

---

## Checklist tài liệu

### Trước khi release
- [ ] README.md đầy đủ và cập nhật
- [ ] QUICK_START.md có thể chạy được
- [ ] HUONG_DAN_CHAY.md chi tiết đầy đủ
- [ ] Tất cả links trong README hoạt động
- [ ] Không có thông tin nhạy cảm (API keys, passwords)
- [ ] Screenshots/diagrams rõ ràng
- [ ] Changelog được cập nhật

### Khi thêm tính năng mới
- [ ] Cập nhật README.md
- [ ] Thêm vào HUONG_DAN_CHAY.md nếu cần
- [ ] Tạo báo cáo kỹ thuật trong docs/
- [ ] Cập nhật CHANGELOG

---

**Cập nhật:** 2026-04-25
