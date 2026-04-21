USE CoffeeShopDb;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro)
BEGIN
    INSERT INTO dbo.VaiTro (MaVaiTro, TenVaiTro)
    VALUES
        (N'Admin', N'Quản trị viên'),
        (N'Kho', N'Nhân viên kho'),
        (N'ThuNgan', N'Thu ngân');
END
GO

DECLARE @VaiTroAdmin INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'Admin');
DECLARE @VaiTroKho INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'Kho');
DECLARE @VaiTroThuNgan INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'ThuNgan');

IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap = N'admin')
BEGIN
    INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive)
    VALUES (N'admin', N'123456', N'Quản trị viên', @VaiTroAdmin, 1);
END
ELSE
BEGIN
    UPDATE dbo.NguoiDung
    SET MatKhau = N'123456',
        HoTen = N'Quản trị viên',
        VaiTroId = @VaiTroAdmin,
        IsActive = 1
    WHERE TenDangNhap = N'admin';
END

IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap = N'kho')
BEGIN
    INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive)
    VALUES (N'kho', N'123456', N'Nhân viên kho', @VaiTroKho, 1);
END
ELSE
BEGIN
    UPDATE dbo.NguoiDung
    SET MatKhau = N'123456',
        HoTen = N'Nhân viên kho',
        VaiTroId = @VaiTroKho,
        IsActive = 1
    WHERE TenDangNhap = N'kho';
END

IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap = N'thungan')
BEGIN
    INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive)
    VALUES (N'thungan', N'123456', N'Thu ngân', @VaiTroThuNgan, 1);
END
ELSE
BEGIN
    UPDATE dbo.NguoiDung
    SET MatKhau = N'123456',
        HoTen = N'Thu ngân',
        VaiTroId = @VaiTroThuNgan,
        IsActive = 1
    WHERE TenDangNhap = N'thungan';
END
GO

DECLARE @VaiTroThuNgan2 INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'ThuNgan');

IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap = N'khoa_test')
BEGIN
    INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive)
    VALUES (N'khoa_test', N'123456', N'Tài khoản khóa test', @VaiTroThuNgan2, 0);
END
ELSE
BEGIN
    UPDATE dbo.NguoiDung
    SET MatKhau = N'123456',
        HoTen = N'Tài khoản khóa test',
        VaiTroId = @VaiTroThuNgan2,
        IsActive = 0
    WHERE TenDangNhap = N'khoa_test';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.DanhMuc)
BEGIN
    INSERT INTO dbo.DanhMuc (TenDanhMuc, MoTa)
    VALUES
        (N'Cà phê', N'Đồ uống từ cà phê'),
        (N'Trà', N'Đồ uống từ trà'),
        (N'Bánh', N'Đồ ăn nhẹ');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap)
BEGIN
    INSERT INTO dbo.NhaCungCap (TenNhaCungCap, SoDienThoai, DiaChi)
    VALUES
        (N'Công ty Hạt Việt', N'0909123456', N'Quận 1, TP.HCM'),
        (N'Nông trại Sạch', N'0909654321', N'Đà Lạt');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Mon)
BEGIN
    DECLARE @DanhMucCaPhe INT = (SELECT TOP 1 DanhMucId FROM dbo.DanhMuc WHERE TenDanhMuc = N'Cà phê');
    DECLARE @DanhMucTra INT = (SELECT TOP 1 DanhMucId FROM dbo.DanhMuc WHERE TenDanhMuc = N'Trà');

    INSERT INTO dbo.Mon (TenMon, DanhMucId, DonGia, TonKho, HinhAnhPath)
    VALUES
        (N'Cà phê sữa', @DanhMucCaPhe, 32000, 30, N'images/caphe-sua.jpg'),
        (N'Bạc xỉu', @DanhMucCaPhe, 35000, 20, N'images/bac-xiu.jpg'),
        (N'Trà đào', @DanhMucTra, 38000, 25, N'images/tra-dao.jpg');
END
GO

