# 🤝 Hướng dẫn đóng góp

Cảm ơn bạn đã quan tâm đến việc đóng góp cho Coffee Shop Management System!

---

## 📋 Mục lục

1. [Code of Conduct](#code-of-conduct)
2. [Quy trình đóng góp](#quy-trình-đóng-góp)
3. [Coding Standards](#coding-standards)
4. [Commit Messages](#commit-messages)
5. [Pull Request Process](#pull-request-process)

---

## Code of Conduct

### Nguyên tắc

- Tôn trọng mọi người
- Chấp nhận phản hồi mang tính xây dựng
- Tập trung vào điều tốt nhất cho cộng đồng
- Thể hiện sự đồng cảm với các thành viên khác

---

## Quy trình đóng góp

### 1. Fork Repository

```bash
# Fork trên GitHub UI
# Clone về máy
git clone https://github.com/YOUR_USERNAME/Coffee_Shop_Management_WPF.git
cd Coffee_Shop_Management_WPF
```

### 2. Tạo Branch Mới

```bash
# Tạo branch từ main
git checkout -b feature/ten-tinh-nang

# Hoặc cho bug fix
git checkout -b fix/ten-bug
```

### 3. Thực hiện thay đổi

- Viết code theo coding standards
- Thêm tests nếu cần
- Cập nhật documentation

### 4. Commit Changes

```bash
git add .
git commit -m "feat: thêm tính năng XYZ"
```

### 5. Push và tạo Pull Request

```bash
git push origin feature/ten-tinh-nang
```

Sau đó tạo Pull Request trên GitHub.

---

## Coding Standards

### C# Code Style

#### Naming Conventions

```csharp
// PascalCase cho classes, methods, properties
public class PaymentService { }
public void ProcessPayment() { }
public string CustomerName { get; set; }

// camelCase cho private fields
private readonly IPaymentRepository _paymentRepository;
private string _customerName;

// PascalCase cho constants
public const int MaxRetryCount = 3;

// Prefix I cho interfaces
public interface IPaymentService { }
```

#### Code Organization

```csharp
// 1. Using statements
using System;
using System.Linq;

// 2. Namespace
namespace CoffeeShop.Wpf.Services;

// 3. Class với XML comments
/// <summary>
/// Service xử lý thanh toán
/// </summary>
public class PaymentService
{
    // 4. Constants
    private const int DefaultTimeout = 30;
    
    // 5. Fields
    private readonly IPaymentRepository _repository;
    
    // 6. Constructor
    public PaymentService(IPaymentRepository repository)
    {
        _repository = repository;
    }
    
    // 7. Properties
    public bool IsProcessing { get; private set; }
    
    // 8. Public methods
    public async Task ProcessAsync() { }
    
    // 9. Private methods
    private void ValidateInput() { }
}
```

#### Best Practices

```csharp
// ✅ Good
public async Task<Result<Payment>> GetPaymentAsync(int id, CancellationToken ct)
{
    if (id <= 0)
        return Result<Payment>.Failure("Invalid ID");
        
    var payment = await _repository.GetByIdAsync(id, ct);
    return payment is not null 
        ? Result<Payment>.Success(payment)
        : Result<Payment>.Failure("Not found");
}

// ❌ Bad
public Payment GetPayment(int id)
{
    return _repository.GetById(id); // Blocking call, no validation
}
```

### XAML Style

```xml
<!-- Indent: 4 spaces -->
<!-- PascalCase cho properties -->
<Button Content="Click Me"
        Width="100"
        Height="30"
        Command="{Binding ClickCommand}"
        Style="{StaticResource PrimaryButton}" />

<!-- Group related properties -->
<TextBlock Text="{Binding CustomerName}"
           FontSize="14"
           FontWeight="Bold"
           Foreground="{DynamicResource PrimaryBrush}"
           Margin="0,8,0,0" />
```

### SQL Style

```sql
-- UPPERCASE cho keywords
-- snake_case cho tables/columns
SELECT 
    hd.HoaDonBanId,
    hd.TongTien,
    kh.HoTen
FROM dbo.HoaDonBan hd
INNER JOIN dbo.KhachHang kh ON hd.KhachHangId = kh.KhachHangId
WHERE hd.TrangThaiThanhToan = N'Đã thanh toán'
ORDER BY hd.NgayTao DESC;
```

---

## Commit Messages

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: Tính năng mới
- **fix**: Sửa bug
- **docs**: Thay đổi documentation
- **style**: Format code (không ảnh hưởng logic)
- **refactor**: Refactor code
- **test**: Thêm tests
- **chore**: Cập nhật build, dependencies

### Examples

```bash
# Feature
feat(payment): thêm tích hợp PayOS QR Code

Tích hợp PayOS API để tạo QR Code thanh toán.
Hỗ trợ webhook callback và đối soát thủ công.

Closes #123

# Bug fix
fix(wpf): sửa lỗi hiển thị QR Code

QR Code không hiển thị vì bind sai property.
Đã tạo MonHienThiViewModel để wrap Mon model.

Fixes #456

# Documentation
docs(readme): cập nhật hướng dẫn cài đặt ngrok

Thêm hướng dẫn chi tiết cài đặt và cấu hình ngrok
cho webhook development.
```

---

## Pull Request Process

### Checklist trước khi submit

- [ ] Code build thành công
- [ ] Không có warnings
- [ ] Tests pass (nếu có)
- [ ] Documentation đã cập nhật
- [ ] CHANGELOG.md đã cập nhật
- [ ] Commit messages rõ ràng
- [ ] Code đã được review bởi bản thân

### PR Template

```markdown
## Mô tả

Mô tả ngắn gọn về thay đổi.

## Loại thay đổi

- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Checklist

- [ ] Code build thành công
- [ ] Tests pass
- [ ] Documentation updated
- [ ] CHANGELOG updated

## Screenshots (nếu có)

Thêm screenshots nếu thay đổi UI.

## Related Issues

Closes #123
```

### Review Process

1. Maintainer sẽ review PR
2. Có thể yêu cầu thay đổi
3. Sau khi approve → Merge vào main
4. PR sẽ được squash merge

---

## Development Setup

### Prerequisites

- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 hoặc VS Code
- Git

### Setup

```bash
# Clone repo
git clone https://github.com/YOUR_USERNAME/Coffee_Shop_Management_WPF.git
cd Coffee_Shop_Management_WPF

# Restore packages
dotnet restore

# Setup database
sqlcmd -S YOUR_SERVER -E -i database/01_CreateTables.sql
# ... chạy các scripts khác

# Configure
cd CoffeeShop.Wpf
copy App.config.example App.config
# Sửa connection string

cd ../CoffeeShop.PaymentApi
copy appsettings.Development.json.example appsettings.Development.json
# Sửa connection string và PayOS credentials

# Run
dotnet run --project CoffeeShop.PaymentApi
dotnet run --project CoffeeShop.Wpf
```

---

## Testing

### Unit Tests

```csharp
[Fact]
public async Task ProcessPayment_ValidInput_ReturnsSuccess()
{
    // Arrange
    var service = new PaymentService(_mockRepository.Object);
    
    // Act
    var result = await service.ProcessAsync(validInput);
    
    // Assert
    Assert.True(result.IsSuccess);
}
```

### Integration Tests

```csharp
[Fact]
public async Task CreateQRPayment_ValidRequest_ReturnsQRCode()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new CreateQRPaymentRequest { ... };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/payments/qr/create", request);
    
    // Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<CreateQRPaymentResponse>();
    Assert.NotNull(result.QRCodeRaw);
}
```

---

## Reporting Bugs

### Bug Report Template

```markdown
**Mô tả bug**
Mô tả rõ ràng và ngắn gọn về bug.

**Các bước tái hiện**
1. Vào '...'
2. Click vào '...'
3. Scroll xuống '...'
4. Thấy lỗi

**Kết quả mong đợi**
Mô tả kết quả bạn mong đợi.

**Kết quả thực tế**
Mô tả kết quả thực tế xảy ra.

**Screenshots**
Nếu có, thêm screenshots.

**Môi trường**
- OS: [e.g. Windows 11]
- .NET Version: [e.g. 8.0.1]
- SQL Server: [e.g. Express 2019]

**Thông tin thêm**
Thêm bất kỳ thông tin nào khác về bug.
```

---

## Feature Requests

### Feature Request Template

```markdown
**Tính năng đề xuất**
Mô tả rõ ràng tính năng bạn muốn.

**Vấn đề hiện tại**
Mô tả vấn đề hiện tại (nếu có).

**Giải pháp đề xuất**
Mô tả giải pháp bạn muốn.

**Giải pháp thay thế**
Mô tả các giải pháp thay thế khác.

**Thông tin thêm**
Thêm bất kỳ thông tin nào khác.
```

---

## Questions?

Nếu có câu hỏi, liên hệ:

- **Email:** tranduonggiabao0501@gmail.com
- **Facebook:** https://www.facebook.com/peo.0501/
- **GitHub Issues:** https://github.com/Peo051/Coffee_Shop_Management_WPF/issues

---

**Cảm ơn bạn đã đóng góp! 🎉**
