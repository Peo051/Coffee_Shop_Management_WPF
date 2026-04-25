# Changelog

Tất cả các thay đổi quan trọng của dự án sẽ được ghi lại trong file này.

---

## [1.0.0] - 2026-04-25

### ✨ Tính năng mới

#### Payment & QR Code
- ✅ Tích hợp thanh toán QR Code với PayOS
- ✅ Tự động generate QR Code trong WPF
- ✅ Webhook xử lý callback từ PayOS
- ✅ Đối soát thủ công khi webhook không hoạt động
- ✅ Hiển thị trạng thái thanh toán real-time

#### Quản lý khách hàng
- ✅ Tìm kiếm khách hàng theo số điện thoại
- ✅ Hiển thị thông tin khách hàng đã chọn
- ✅ Quản lý điểm tích lũy nâng cao
- ✅ Tự động tính điểm: 10,000đ = 1 điểm
- ✅ Sử dụng điểm: 1 điểm = 100đ giảm giá

#### Quản lý món
- ✅ Giá món thay đổi theo size (M/L/XL)
- ✅ Phụ thu size: L = +5,000đ, XL = +10,000đ
- ✅ Hiển thị giá real-time khi chọn size
- ✅ Tìm kiếm món theo tên, danh mục, mã

#### Giao diện
- ✅ Xóa các comment hướng dẫn không cần thiết
- ✅ Cải thiện UX cho màn hình bán hàng
- ✅ Hiển thị QR Code trực tiếp trong WPF
- ✅ Thông báo trạng thái thanh toán rõ ràng

### 🐛 Bug Fixes

#### Payment API
- ✅ Sửa lỗi webhook trả HTTP 400
- ✅ Sửa lỗi QR Payment không trừ kho đúng thời điểm
- ✅ Sửa lỗi signature PayOS không đúng
- ✅ Sửa lỗi compilation trong PayOsPaymentGatewayService
- ✅ Sửa lỗi FinalizeQrPaymentAsync với 5 tham số

#### WPF
- ✅ Sửa lỗi hiển thị QR Code (dùng QRCoder)
- ✅ Sửa lỗi đăng nhập bị treo (connection string)
- ✅ Sửa lỗi PaymentApiClient duplicate code
- ✅ Sửa lỗi missing methods: ExecuteChonKhachHang, ExecuteXoaKhachHang

#### Database
- ✅ Sửa lỗi connection string sai server name
- ✅ Script fix manual cho hóa đơn đã thanh toán

### 🔧 Cải tiến

#### Architecture
- ✅ Tách Payment API thành service riêng
- ✅ WPF có thể dùng Payment API public (ngrok/domain)
- ✅ Cấu hình PaymentApiBaseUrl qua App.config
- ✅ Bảo mật PayOS credentials trên server

#### Code Quality
- ✅ Tạo MonHienThiViewModel wrapper cho Mon
- ✅ Thêm converters: NullToBooleanConverter, StringNullOrEmptyToBooleanConverter
- ✅ Tối ưu performance query database
- ✅ Cải thiện error handling

#### Documentation
- ✅ README.md hoàn chỉnh với kiến trúc và hướng dẫn
- ✅ QUICK_START.md cho người dùng nhanh
- ✅ HUONG_DAN_CHAY.md chi tiết từng bước
- ✅ Tài liệu kỹ thuật trong docs/
- ✅ Hướng dẫn cho người chỉ chạy WPF
- ✅ Hướng dẫn cấu hình ngrok

#### Configuration
- ✅ App.config.example cho WPF
- ✅ appsettings.Development.json.example cho API
- ✅ .gitignore cập nhật để không commit credentials
- ✅ Hướng dẫn cấu hình cho người khác

### 📝 Tài liệu

#### Tài liệu người dùng
- README.md - Tổng quan dự án
- QUICK_START.md - Hướng dẫn nhanh 5 phút
- HUONG_DAN_CHAY.md - Hướng dẫn chi tiết đầy đủ

#### Tài liệu kỹ thuật
- docs/payment-api/ - Tài liệu Payment API
- docs/wpf-config/ - Tài liệu cấu hình WPF
- docs/STRUCTURE.md - Cấu trúc tài liệu

#### Scripts
- database/ - SQL scripts đầy đủ
- database/FIX_*.sql - Scripts fix lỗi

### 🔒 Security

- ✅ PayOS API Key không được commit lên Git
- ✅ App.config không được commit
- ✅ appsettings.Development.json không được commit
- ✅ Dùng file .example làm template
- ✅ Connection string không hard-code trong code

### 🚀 Deployment

- ✅ Hỗ trợ chạy local với localhost
- ✅ Hỗ trợ chạy với ngrok (development)
- ✅ Hỗ trợ deploy lên domain cố định (production)
- ✅ Hướng dẫn cấu hình webhook PayOS

---

## [0.9.0] - 2026-04-24

### Initial Release
- Phiên bản đầu tiên với các tính năng cơ bản
- Quản lý bán hàng, kho, khách hàng, nhân viên
- Chưa có tích hợp thanh toán QR

---

## Quy tắc versioning

Dự án sử dụng [Semantic Versioning](https://semver.org/):
- **MAJOR**: Thay đổi breaking changes
- **MINOR**: Thêm tính năng mới (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

## Loại thay đổi

- **✨ Tính năng mới** - Features
- **🐛 Bug Fixes** - Sửa lỗi
- **🔧 Cải tiến** - Improvements
- **📝 Tài liệu** - Documentation
- **🔒 Security** - Bảo mật
- **🚀 Deployment** - Deploy và infrastructure
- **⚠️ Deprecated** - Tính năng sắp bị loại bỏ
- **🗑️ Removed** - Tính năng đã bị loại bỏ

---

**Cập nhật:** 2026-04-25
