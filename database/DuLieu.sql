-- ============================================================
-- Seed_50_Each_Table_REAL.sql
-- CoffeeShopDb - dữ liệu seed thực tế, không dùng chữ Demo/Test
-- Yêu cầu:
--   - VaiTro CHỈ có 3 vai trò: Admin, ThuNgan, Kho
--   - Các bảng nghiệp vụ còn lại được bổ sung tối thiểu 50 dòng nếu bảng tồn tại
--   - Script cố gắng idempotent: chạy lại không tạo trùng dữ liệu theo các mã/tên seed
-- Chạy sau toàn bộ script tạo bảng/migration.
-- ============================================================

USE CoffeeShopDb;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ============================================================
   1) VaiTro - chỉ giữ 3 vai trò nghiệp vụ
============================================================ */
MERGE dbo.VaiTro AS T
USING (VALUES
    (N'Admin',   N'Quản trị viên'),
    (N'ThuNgan', N'Thu ngân'),
    (N'Kho',     N'Nhân viên kho')
) AS S(MaVaiTro, TenVaiTro)
ON T.MaVaiTro = S.MaVaiTro
WHEN MATCHED THEN UPDATE SET TenVaiTro = S.TenVaiTro
WHEN NOT MATCHED THEN INSERT (MaVaiTro, TenVaiTro) VALUES (S.MaVaiTro, S.TenVaiTro);
GO

-- Chuyển mọi người dùng đang tham chiếu vai trò khác về Thu ngân rồi xóa vai trò ngoài 3 vai trò chuẩn.
DECLARE @ThuNganId INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'ThuNgan');
UPDATE dbo.NguoiDung
SET VaiTroId = @ThuNganId
WHERE VaiTroId NOT IN (SELECT VaiTroId FROM dbo.VaiTro WHERE MaVaiTro IN (N'Admin', N'ThuNgan', N'Kho'));

DELETE FROM dbo.VaiTro
WHERE MaVaiTro NOT IN (N'Admin', N'ThuNgan', N'Kho');
GO

/* ============================================================
   2) Danh mục thực tế
============================================================ */
DECLARE @DanhMuc TABLE (Ten NVARCHAR(150), MoTa NVARCHAR(500));
INSERT INTO @DanhMuc VALUES
(N'Cà phê truyền thống', N'Cà phê phin và các món pha kiểu Việt Nam'),
(N'Cà phê máy', N'Espresso, Americano, Latte, Cappuccino'),
(N'Cà phê đá xay', N'Nhóm cà phê xay cùng đá và kem'),
(N'Trà trái cây', N'Trà kết hợp trái cây tươi'),
(N'Trà sữa', N'Trà sữa và topping'),
(N'Trà nguyên lá', N'Trà xanh, trà đen, trà ô long'),
(N'Matcha', N'Đồ uống từ bột trà xanh'),
(N'Socola', N'Đồ uống cacao và socola'),
(N'Sữa chua', N'Sữa chua uống, đá xay và trái cây'),
(N'Nước ép', N'Nước ép trái cây tươi'),
(N'Sinh tố', N'Trái cây xay với sữa hoặc sữa chua'),
(N'Soda', N'Soda trái cây và soda thảo mộc'),
(N'Đồ uống nóng', N'Các món nóng phục vụ mùa mưa/lạnh'),
(N'Bánh ngọt', N'Bánh kem, mousse, tiramisu'),
(N'Bánh mặn', N'Bánh mì, croissant mặn'),
(N'Ăn nhẹ', N'Món ăn nhẹ dùng kèm đồ uống'),
(N'Topping', N'Trân châu, thạch, kem cheese'),
(N'Combo sáng', N'Đồ uống và bánh cho buổi sáng'),
(N'Combo văn phòng', N'Combo phục vụ khách văn phòng'),
(N'Đồ uống mang đi', N'Nhóm món tối ưu cho take-away'),
(N'Món theo mùa', N'Sản phẩm bán theo mùa'),
(N'Signature', N'Món đặc trưng của quán'),
(N'Healthy drink', N'Đồ uống ít đường và tốt cho sức khỏe'),
(N'Detox', N'Nước detox trái cây'),
(N'Cold brew', N'Cà phê ủ lạnh'),
(N'Macchiato', N'Trà/cà phê phủ macchiato'),
(N'Kem', N'Kem viên và kem ly'),
(N'Đồ uống trẻ em', N'Món không caffeine cho trẻ em'),
(N'Bánh quy', N'Cookie và bánh quy đóng gói'),
(N'Hạt dinh dưỡng', N'Hạt ăn nhẹ'),
(N'Nước đóng chai', N'Nước suối và nước khoáng'),
(N'Sữa tươi', N'Đồ uống nền sữa tươi'),
(N'Latte đặc biệt', N'Latte kết hợp hương vị'),
(N'Mocha', N'Cà phê kết hợp socola'),
(N'Affogato', N'Cà phê dùng cùng kem'),
(N'Yogurt trái cây', N'Sữa chua kết hợp trái cây'),
(N'Bánh cheesecake', N'Cheesecake nhiều hương vị'),
(N'Bánh chocolate', N'Bánh vị chocolate'),
(N'Bánh trái cây', N'Bánh dùng trái cây tươi'),
(N'Salad', N'Salad nhẹ'),
(N'Sandwich', N'Sandwich ăn nhanh'),
(N'Pasta nhẹ', N'Mì Ý khẩu phần nhỏ'),
(N'Món chay', N'Đồ ăn/đồ uống phù hợp khách ăn chay'),
(N'Trà thảo mộc', N'Trà hoa và thảo mộc'),
(N'Nước mát', N'Nước mát truyền thống'),
(N'Siro đá bào', N'Đá bào siro'),
(N'Bánh trung thu', N'Món theo mùa trung thu'),
(N'Quà tặng', N'Sản phẩm đóng gói làm quà'),
(N'Cà phê hạt', N'Cà phê hạt/bột bán lẻ'),
(N'Dụng cụ pha chế', N'Phin, ly, bình pha và phụ kiện');

INSERT INTO dbo.DanhMuc (TenDanhMuc, MoTa, IsActive)
SELECT Ten, MoTa, 1
FROM @DanhMuc d
WHERE NOT EXISTS (SELECT 1 FROM dbo.DanhMuc x WHERE x.TenDanhMuc = d.Ten);
GO

/* ============================================================
   3) Nhà cung cấp thực tế
============================================================ */
DECLARE @NCC TABLE (Ten NVARCHAR(150), SDT NVARCHAR(20), DiaChi NVARCHAR(300));
INSERT INTO @NCC VALUES
(N'Công ty Cà phê Mộc Nguyên', N'0902001001', N'Quận 1, TP.HCM'),
(N'Rang Xay Ban Mê', N'0902001002', N'Buôn Ma Thuột, Đắk Lắk'),
(N'Nông trại Cầu Đất', N'0902001003', N'Cầu Đất, Đà Lạt'),
(N'Sữa tươi Long Thành', N'0902001004', N'Long Thành, Đồng Nai'),
(N'Trà Ô Long Bảo Lộc', N'0902001005', N'Bảo Lộc, Lâm Đồng'),
(N'Công ty Đường Biên Hòa', N'0902001006', N'Biên Hòa, Đồng Nai'),
(N'Nhà cung ứng Trái Cây Sạch Sài Gòn', N'0902001007', N'Thủ Đức, TP.HCM'),
(N'Bakery Ngọc Hà', N'0902001008', N'Quận 3, TP.HCM'),
(N'Topping Việt Food', N'0902001009', N'Quận 7, TP.HCM'),
(N'Bao Bì Xanh Việt', N'0902001010', N'Bình Tân, TP.HCM'),
(N'Kem Sữa An Phú', N'0902001011', N'Quận 2, TP.HCM'),
(N'Cacao Tiền Giang', N'0902001012', N'Tiền Giang'),
(N'Matcha House Việt Nam', N'0902001013', N'Quận Bình Thạnh, TP.HCM'),
(N'Siro Golden Farm', N'0902001014', N'Tân Bình, TP.HCM'),
(N'Nước Khoáng Vĩnh Hảo', N'0902001015', N'Bình Thuận'),
(N'Đá Sạch Nam Sài Gòn', N'0902001016', N'Nhà Bè, TP.HCM'),
(N'Bánh Croissant Paris Việt', N'0902001017', N'Quận Phú Nhuận, TP.HCM'),
(N'Hạt Dinh Dưỡng An Nhiên', N'0902001018', N'Quận 10, TP.HCM'),
(N'Rau Sạch Đà Lạt Farm', N'0902001019', N'Đà Lạt, Lâm Đồng'),
(N'Trà Thảo Mộc Mộc Châu', N'0902001020', N'Mộc Châu, Sơn La'),
(N'Vina Syrup', N'0902001021', N'Quận 12, TP.HCM'),
(N'Công ty Ly Giấy An Phát', N'0902001022', N'Bình Dương'),
(N'Công ty Ống Hút Giấy Việt', N'0902001023', N'Long An'),
(N'Mứt Trái Cây Đà Lạt', N'0902001024', N'Đà Lạt, Lâm Đồng'),
(N'Mật Ong Hoa Cà Phê', N'0902001025', N'Đắk Nông'),
(N'Bột Béo Thái Sơn', N'0902001026', N'Quận 6, TP.HCM'),
(N'Phô Mai New Zealand Việt', N'0902001027', N'Tân Phú, TP.HCM'),
(N'Bơ Lạt Golden Dairy', N'0902001028', N'Quận 5, TP.HCM'),
(N'Chocolate Couverture Việt', N'0902001029', N'Quận 4, TP.HCM'),
(N'Kho Nguyên Liệu Pha Chế Hương Việt', N'0902001030', N'Gò Vấp, TP.HCM'),
(N'Yến Mạch Healthy Food', N'0902001031', N'Quận 11, TP.HCM'),
(N'Sữa Hạt Nhà Xanh', N'0902001032', N'Thủ Đức, TP.HCM'),
(N'Bột Cacao Bến Tre', N'0902001033', N'Bến Tre'),
(N'Bột Quế Trà My', N'0902001034', N'Quảng Nam'),
(N'Đậu Đỏ Tây Nguyên', N'0902001035', N'Gia Lai'),
(N'Hồng Trà Lâm Hà', N'0902001036', N'Lâm Hà, Lâm Đồng'),
(N'Trái Cây Miền Tây', N'0902001037', N'Cần Thơ'),
(N'Bánh Quy Butter Home', N'0902001038', N'Quận 8, TP.HCM'),
(N'Salad Box Supplier', N'0902001039', N'Tân Bình, TP.HCM'),
(N'Sandwich Bread Việt', N'0902001040', N'Bình Thạnh, TP.HCM'),
(N'Pasta Mini Food', N'0902001041', N'Quận 1, TP.HCM'),
(N'Hộp Giấy Kraft Việt', N'0902001042', N'Bình Chánh, TP.HCM'),
(N'Tem Nhãn Minh Long', N'0902001043', N'Quận 6, TP.HCM'),
(N'Máy Pha Cà Phê An Khang', N'0902001044', N'Quận 10, TP.HCM'),
(N'Phụ Kiện Barista Việt', N'0902001045', N'Thủ Đức, TP.HCM'),
(N'Công ty Vệ Sinh An Toàn', N'0902001046', N'Quận 7, TP.HCM'),
(N'Khăn Giấy Sài Gòn', N'0902001047', N'Tân Phú, TP.HCM'),
(N'Nước Ép Đóng Chai Fresh Day', N'0902001048', N'Quận 9, TP.HCM'),
(N'Thạch Trái Cây Việt', N'0902001049', N'Long An'),
(N'Kho Hàng Tổng Hợp Coffee Mart', N'0902001050', N'Bình Dương');

INSERT INTO dbo.NhaCungCap (TenNhaCungCap, SoDienThoai, DiaChi, IsActive)
SELECT Ten, SDT, DiaChi, 1
FROM @NCC n
WHERE NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap x WHERE x.TenNhaCungCap = n.Ten);
GO

/* ============================================================
   4) Người dùng thực tế - 50 nhân sự, chỉ dùng 3 vai trò chuẩn
============================================================ */
DECLARE @AdminId INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'Admin');
DECLARE @ThuNganId INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'ThuNgan');
DECLARE @KhoId INT = (SELECT TOP 1 VaiTroId FROM dbo.VaiTro WHERE MaVaiTro = N'Kho');
DECLARE @NguoiDung TABLE (TenDangNhap NVARCHAR(50), HoTen NVARCHAR(150), VaiTroId INT);
INSERT INTO @NguoiDung VALUES
(N'admin', N'Trần Gia Bảo', @AdminId),
(N'thu_ngan_01', N'Nguyễn Minh Anh', @ThuNganId),(N'thu_ngan_02', N'Trần Thảo Vy', @ThuNganId),(N'thu_ngan_03', N'Lê Hoàng Nam', @ThuNganId),(N'thu_ngan_04', N'Phạm Ngọc Hân', @ThuNganId),(N'thu_ngan_05', N'Võ Gia Linh', @ThuNganId),(N'thu_ngan_06', N'Đặng Hải Yến', @ThuNganId),(N'thu_ngan_07', N'Bùi Quốc Huy', @ThuNganId),(N'thu_ngan_08', N'Ngô Thanh Trúc', @ThuNganId),(N'thu_ngan_09', N'Hồ Anh Khoa', @ThuNganId),(N'thu_ngan_10', N'Đỗ Mỹ Duyên', @ThuNganId),(N'thu_ngan_11', N'Nguyễn Tường Vi', @ThuNganId),(N'thu_ngan_12', N'Phan Nhật Minh', @ThuNganId),(N'thu_ngan_13', N'Trịnh Bảo Ngọc', @ThuNganId),(N'thu_ngan_14', N'Lý Hoàng Phúc', @ThuNganId),(N'thu_ngan_15', N'Mai Khánh Linh', @ThuNganId),(N'thu_ngan_16', N'Cao Minh Quân', @ThuNganId),(N'thu_ngan_17', N'Vũ Phương Anh', @ThuNganId),(N'thu_ngan_18', N'Đinh Gia Huy', @ThuNganId),(N'thu_ngan_19', N'Tạ Thu Hà', @ThuNganId),(N'thu_ngan_20', N'Nguyễn Hoài Nam', @ThuNganId),(N'thu_ngan_21', N'Trần Bích Ngân', @ThuNganId),(N'thu_ngan_22', N'Lê Đức Anh', @ThuNganId),(N'thu_ngan_23', N'Phạm Thanh Mai', @ThuNganId),(N'thu_ngan_24', N'Võ Nhật Tân', @ThuNganId),
(N'kho_01', N'Nguyễn Thế Anh', @KhoId),(N'kho_02', N'Trần Văn Toàn', @KhoId),(N'kho_03', N'Lê Quang Bình', @KhoId),(N'kho_04', N'Phạm Hữu Lộc', @KhoId),(N'kho_05', N'Võ Minh Tâm', @KhoId),(N'kho_06', N'Đặng Quốc Việt', @KhoId),(N'kho_07', N'Bùi Hoàng Sơn', @KhoId),(N'kho_08', N'Ngô Đức Mạnh', @KhoId),(N'kho_09', N'Hồ Thanh Phong', @KhoId),(N'kho_10', N'Đỗ Anh Tuấn', @KhoId),(N'kho_11', N'Phan Gia Bảo', @KhoId),(N'kho_12', N'Trịnh Minh Khôi', @KhoId),(N'kho_13', N'Lý Quốc Cường', @KhoId),(N'kho_14', N'Mai Đức Long', @KhoId),(N'kho_15', N'Cao Văn Hiếu', @KhoId),(N'quan_ly_01', N'Nguyễn Hải Đăng', @AdminId),(N'quan_ly_02', N'Trần Mai Phương', @AdminId),(N'quan_ly_03', N'Lê Bảo Châu', @AdminId),(N'quan_ly_04', N'Phạm Tuấn Kiệt', @AdminId),(N'quan_ly_05', N'Võ Kim Ngân', @AdminId),(N'quan_ly_06', N'Đặng Phúc Hưng', @AdminId),(N'quan_ly_07', N'Bùi Khánh Vy', @AdminId),(N'quan_ly_08', N'Ngô Thành Đạt', @AdminId),(N'quan_ly_09', N'Hồ Gia Nghi', @AdminId),(N'quan_ly_10', N'Đỗ Minh Triết', @AdminId);

MERGE dbo.NguoiDung AS T
USING @NguoiDung AS S
ON T.TenDangNhap = S.TenDangNhap
WHEN MATCHED THEN UPDATE SET HoTen = S.HoTen, VaiTroId = S.VaiTroId, IsActive = 1
WHEN NOT MATCHED THEN INSERT (TenDangNhap, MatKhau, HoTen, VaiTroId, IsActive)
VALUES (S.TenDangNhap, N'123456', S.HoTen, S.VaiTroId, 1);
GO

/* ============================================================
   5) Món bán thực tế - 50 món
============================================================ */
DECLARE @Mon TABLE (TenMon NVARCHAR(150), TenDanhMuc NVARCHAR(150), DonGia DECIMAL(18,2), TonKho INT, HinhAnhPath NVARCHAR(500));
INSERT INTO @Mon VALUES
(N'Cà phê đen đá', N'Cà phê truyền thống', 28000, 80, N'images/ca-phe-den-da.jpg'),
(N'Cà phê sữa đá', N'Cà phê truyền thống', 32000, 85, N'images/ca-phe-sua-da.jpg'),
(N'Bạc xỉu', N'Cà phê truyền thống', 35000, 70, N'images/bac-xiu.jpg'),
(N'Cà phê muối', N'Signature', 42000, 65, N'images/ca-phe-muoi.jpg'),
(N'Espresso', N'Cà phê máy', 35000, 60, N'images/espresso.jpg'),
(N'Americano đá', N'Cà phê máy', 39000, 60, N'images/americano-da.jpg'),
(N'Latte nóng', N'Cà phê máy', 48000, 55, N'images/latte-nong.jpg'),
(N'Cappuccino', N'Cà phê máy', 50000, 50, N'images/cappuccino.jpg'),
(N'Mocha đá', N'Mocha', 54000, 45, N'images/mocha-da.jpg'),
(N'Cold brew cam vàng', N'Cold brew', 59000, 40, N'images/cold-brew-cam-vang.jpg'),
(N'Trà đào cam sả', N'Trà trái cây', 45000, 75, N'images/tra-dao-cam-sa.jpg'),
(N'Trà vải hoa hồng', N'Trà trái cây', 47000, 70, N'images/tra-vai-hoa-hong.jpg'),
(N'Trà dâu tằm', N'Trà trái cây', 46000, 65, N'images/tra-dau-tam.jpg'),
(N'Trà chanh mật ong', N'Trà trái cây', 39000, 75, N'images/tra-chanh-mat-ong.jpg'),
(N'Trà ô long sen', N'Trà nguyên lá', 42000, 60, N'images/tra-o-long-sen.jpg'),
(N'Trà sữa truyền thống', N'Trà sữa', 43000, 80, N'images/tra-sua-truyen-thong.jpg'),
(N'Trà sữa ô long nướng', N'Trà sữa', 49000, 75, N'images/tra-sua-o-long-nuong.jpg'),
(N'Trà sữa trân châu đường đen', N'Trà sữa', 52000, 70, N'images/tra-sua-tran-chau-duong-den.jpg'),
(N'Matcha latte đá', N'Matcha', 52000, 55, N'images/matcha-latte-da.jpg'),
(N'Matcha macchiato', N'Macchiato', 56000, 50, N'images/matcha-macchiato.jpg'),
(N'Cacao nóng', N'Socola', 45000, 50, N'images/cacao-nong.jpg'),
(N'Socola đá xay', N'Socola', 59000, 45, N'images/socola-da-xay.jpg'),
(N'Cookie cream đá xay', N'Cà phê đá xay', 62000, 40, N'images/cookie-cream-da-xay.jpg'),
(N'Caramel coffee đá xay', N'Cà phê đá xay', 65000, 40, N'images/caramel-coffee-da-xay.jpg'),
(N'Sinh tố xoài', N'Sinh tố', 49000, 55, N'images/sinh-to-xoai.jpg'),
(N'Sinh tố bơ', N'Sinh tố', 55000, 50, N'images/sinh-to-bo.jpg'),
(N'Sinh tố dâu', N'Sinh tố', 52000, 55, N'images/sinh-to-dau.jpg'),
(N'Nước ép cam', N'Nước ép', 45000, 60, N'images/nuoc-ep-cam.jpg'),
(N'Nước ép táo', N'Nước ép', 47000, 55, N'images/nuoc-ep-tao.jpg'),
(N'Nước ép thơm', N'Nước ép', 43000, 55, N'images/nuoc-ep-thom.jpg'),
(N'Soda việt quất', N'Soda', 45000, 50, N'images/soda-viet-quat.jpg'),
(N'Soda chanh dây', N'Soda', 45000, 50, N'images/soda-chanh-day.jpg'),
(N'Sữa chua đá', N'Sữa chua', 39000, 60, N'images/sua-chua-da.jpg'),
(N'Sữa chua việt quất', N'Yogurt trái cây', 49000, 55, N'images/sua-chua-viet-quat.jpg'),
(N'Croissant bơ', N'Bánh ngọt', 39000, 50, N'images/croissant-bo.jpg'),
(N'Tiramisu', N'Bánh ngọt', 55000, 40, N'images/tiramisu.jpg'),
(N'Cheesecake việt quất', N'Bánh cheesecake', 59000, 35, N'images/cheesecake-viet-quat.jpg'),
(N'Bánh mousse chocolate', N'Bánh chocolate', 58000, 35, N'images/mousse-chocolate.jpg'),
(N'Bánh chuối nướng', N'Bánh trái cây', 42000, 45, N'images/banh-chuoi-nuong.jpg'),
(N'Bánh mì chà bông', N'Bánh mặn', 35000, 45, N'images/banh-mi-cha-bong.jpg'),
(N'Sandwich gà phô mai', N'Sandwich', 59000, 40, N'images/sandwich-ga-pho-mai.jpg'),
(N'Salad ức gà', N'Salad', 69000, 35, N'images/salad-uc-ga.jpg'),
(N'Pasta bò bằm', N'Pasta nhẹ', 79000, 30, N'images/pasta-bo-bam.jpg'),
(N'Khoai tây chiên', N'Ăn nhẹ', 39000, 50, N'images/khoai-tay-chien.jpg'),
(N'Combo cà phê sữa và croissant', N'Combo sáng', 65000, 45, N'images/combo-ca-phe-sua-croissant.jpg'),
(N'Combo latte và tiramisu', N'Combo văn phòng', 95000, 35, N'images/combo-latte-tiramisu.jpg'),
(N'Trân châu đen', N'Topping', 10000, 200, N'images/tran-chau-den.jpg'),
(N'Thạch cà phê', N'Topping', 10000, 180, N'images/thach-ca-phe.jpg'),
(N'Kem cheese', N'Topping', 15000, 150, N'images/kem-cheese.jpg'),
(N'Cà phê hạt rang mộc 250g', N'Cà phê hạt', 125000, 45, N'images/ca-phe-hat-rang-moc-250g.jpg');

INSERT INTO dbo.Mon (TenMon, DanhMucId, DonGia, TonKho, HinhAnhPath, IsActive)
SELECT m.TenMon, dm.DanhMucId, m.DonGia, m.TonKho, m.HinhAnhPath, 1
FROM @Mon m
JOIN dbo.DanhMuc dm ON dm.TenDanhMuc = m.TenDanhMuc
WHERE NOT EXISTS (SELECT 1 FROM dbo.Mon x WHERE x.TenMon = m.TenMon);
GO

/* ============================================================
   6) Khách hàng thân thiết thực tế - 50 khách
============================================================ */
DECLARE @i INT = 1;
WHILE (SELECT COUNT(*) FROM dbo.KhachHang) < 50
BEGIN
    DECLARE @Phone NVARCHAR(20) = N'091' + RIGHT('0000000' + CAST(@i AS VARCHAR(7)), 7);
    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE SoDienThoai = @Phone)
    BEGIN
        INSERT INTO dbo.KhachHang (HoTen, SoDienThoai, Email, DiemTichLuy, IsActive)
        VALUES (
            CHOOSE((@i % 20) + 1, N'Nguyễn Hoàng An', N'Trần Minh Châu', N'Lê Gia Hân', N'Phạm Quốc Bảo', N'Võ Thanh Trúc', N'Đặng Hải Nam', N'Bùi Phương Linh', N'Ngô Tuấn Kiệt', N'Hồ Khánh Vy', N'Đỗ Nhật Minh', N'Phan Thảo Nguyên', N'Trịnh Đức Long', N'Lý Bảo Anh', N'Mai Hoàng Phúc', N'Cao Mỹ Duyên', N'Vũ Anh Khoa', N'Đinh Thu Trang', N'Tạ Quang Huy', N'Nguyễn Bích Ngọc', N'Trần Gia Khánh') + N' ' + CAST(@i AS NVARCHAR(10)),
            @Phone,
            N'khachhang' + RIGHT('000' + CAST(@i AS VARCHAR(3)), 3) + N'@gmail.com',
            (@i * 7) % 350,
            1
        );
    END
    SET @i += 1;
END
GO

/* ============================================================
   7) Khu vực, bàn, ca làm việc
============================================================ */
IF OBJECT_ID(N'dbo.KhuVuc', N'U') IS NOT NULL
BEGIN
    DECLARE @i INT = 1;
    WHILE (SELECT COUNT(*) FROM dbo.KhuVuc) < 50
    BEGIN
        DECLARE @TenKV NVARCHAR(100) = CHOOSE((@i % 10) + 1, N'Tầng trệt', N'Tầng 1', N'Tầng 2', N'Sân vườn', N'Ban công', N'Phòng lạnh', N'Khu yên tĩnh', N'Khu làm việc', N'Quầy mang đi', N'Sảnh trước') + N' - khu ' + CAST(@i AS NVARCHAR(10));
        IF NOT EXISTS (SELECT 1 FROM dbo.KhuVuc WHERE TenKhuVuc = @TenKV)
            INSERT INTO dbo.KhuVuc (TenKhuVuc, MoTa, IsActive) VALUES (@TenKV, N'Khu phục vụ khách tại quán', 1);
        SET @i += 1;
    END
END
GO

IF OBJECT_ID(N'dbo.Ban', N'U') IS NOT NULL
BEGIN
    DECLARE @i INT = 1;
    WHILE (SELECT COUNT(*) FROM dbo.Ban) < 50
    BEGIN
        DECLARE @KV INT = (SELECT TOP 1 KhuVucId FROM dbo.KhuVuc ORDER BY NEWID());
        DECLARE @TenBan NVARCHAR(100) = N'Bàn ' + RIGHT('00' + CAST(@i AS VARCHAR(2)), 2);
        IF NOT EXISTS (SELECT 1 FROM dbo.Ban WHERE KhuVucId = @KV AND TenBan = @TenBan)
            INSERT INTO dbo.Ban (KhuVucId, TenBan, TrangThaiBan, IsActive) VALUES (@KV, @TenBan, N'Trong', 1);
        SET @i += 1;
    END
END
GO

IF OBJECT_ID(N'dbo.CaLamViec', N'U') IS NOT NULL
BEGIN
    DECLARE @i INT = 1;
    WHILE (SELECT COUNT(*) FROM dbo.CaLamViec) < 50
    BEGIN
        DECLARE @UserId INT = (SELECT TOP 1 nd.NguoiDungId FROM dbo.NguoiDung nd JOIN dbo.VaiTro vt ON vt.VaiTroId = nd.VaiTroId WHERE vt.MaVaiTro IN (N'ThuNgan', N'Admin') ORDER BY NEWID());
        DECLARE @MoCa DATETIME2 = DATEADD(HOUR, CASE WHEN @i % 2 = 0 THEN 7 ELSE 14 END, DATEADD(DAY, -@i, CAST(CAST(GETDATE() AS DATE) AS DATETIME2)));
        INSERT INTO dbo.CaLamViec (NguoiDungId, ThoiGianMoCa, ThoiGianDongCa, TrangThaiCa, GhiChu)
        VALUES (@UserId, @MoCa, DATEADD(HOUR, 8, @MoCa), N'DaDong', N'Ca làm việc đã chốt sổ');
        SET @i += 1;
    END
END
GO

/* ============================================================
   8) Hóa đơn nhập và chi tiết nhập - dữ liệu hợp lý
============================================================ */
DECLARE @i INT = 1;
WHILE (SELECT COUNT(*) FROM dbo.HoaDonNhap) < 50
BEGIN
    DECLARE @NccId INT = (SELECT TOP 1 NhaCungCapId FROM dbo.NhaCungCap ORDER BY NEWID());
    DECLARE @NguoiTao INT = (SELECT TOP 1 nd.NguoiDungId FROM dbo.NguoiDung nd JOIN dbo.VaiTro vt ON vt.VaiTroId = nd.VaiTroId WHERE vt.MaVaiTro IN (N'Kho', N'Admin') ORDER BY NEWID());
    DECLARE @NgayNhap DATETIME2 = DATEADD(HOUR, 8 + (@i % 8), DATEADD(DAY, -@i, CAST(CAST(GETDATE() AS DATE) AS DATETIME2)));
    DECLARE @Tong DECIMAL(18,2) = 500000 + (@i * 37000);
    INSERT INTO dbo.HoaDonNhap (NgayNhap, NhaCungCapId, TongTien, GhiChu, CreatedByUserId)
    VALUES (@NgayNhap, @NccId, @Tong, N'Nhập hàng định kỳ cho quán', @NguoiTao);
    SET @i += 1;
END
GO

DECLARE @i INT = 1;
WHILE (SELECT COUNT(*) FROM dbo.ChiTietHoaDonNhap) < 50
BEGIN
    DECLARE @HDN INT = (SELECT TOP 1 HoaDonNhapId FROM dbo.HoaDonNhap ORDER BY NEWID());
    DECLARE @MonId INT = (SELECT TOP 1 MonId FROM dbo.Mon ORDER BY NEWID());
    DECLARE @SL INT = 5 + (@i % 20);
    DECLARE @Gia DECIMAL(18,2) = (SELECT TOP 1 DonGia * 0.65 FROM dbo.Mon WHERE MonId = @MonId);
    INSERT INTO dbo.ChiTietHoaDonNhap (HoaDonNhapId, MonId, DonGiaNhap, SoLuong)
    VALUES (@HDN, @MonId, @Gia, @SL);
    SET @i += 1;
END
GO

/* ============================================================
   9) Hóa đơn bán và chi tiết bán
============================================================ */
DECLARE @i INT = 1;
WHILE (SELECT COUNT(*) FROM dbo.HoaDonBan) < 50
BEGIN
    DECLARE @NguoiTao INT = (SELECT TOP 1 nd.NguoiDungId FROM dbo.NguoiDung nd JOIN dbo.VaiTro vt ON vt.VaiTroId = nd.VaiTroId WHERE vt.MaVaiTro IN (N'ThuNgan', N'Admin') ORDER BY NEWID());
    DECLARE @BanId INT = CASE WHEN OBJECT_ID(N'dbo.Ban', N'U') IS NOT NULL THEN (SELECT TOP 1 BanId FROM dbo.Ban ORDER BY NEWID()) ELSE NULL END;
    DECLARE @CaId INT = CASE WHEN OBJECT_ID(N'dbo.CaLamViec', N'U') IS NOT NULL THEN (SELECT TOP 1 CaLamViecId FROM dbo.CaLamViec ORDER BY NEWID()) ELSE NULL END;
    DECLARE @KhId INT = CASE WHEN OBJECT_ID(N'dbo.KhachHang', N'U') IS NOT NULL THEN (SELECT TOP 1 KhachHangId FROM dbo.KhachHang ORDER BY NEWID()) ELSE NULL END;
    DECLARE @NgayBan DATETIME2 = DATEADD(MINUTE, @i * 13, DATEADD(DAY, -(@i % 30), CAST(CAST(GETDATE() AS DATE) AS DATETIME2)));
    DECLARE @Tong DECIMAL(18,2) = 45000 + ((@i % 6) * 25000);
    DECLARE @Giam DECIMAL(18,2) = CASE WHEN @i % 5 = 0 THEN 10000 ELSE 0 END;

    INSERT INTO dbo.HoaDonBan (NgayBan, TongTien, GiamGia, CreatedByUserId, BanId, CaLamViecId, KhachHangId)
    VALUES (@NgayBan, @Tong, @Giam, @NguoiTao, @BanId, @CaId, @KhId);
    SET @i += 1;
END
GO

DECLARE @i INT = 1;
WHILE (SELECT COUNT(*) FROM dbo.ChiTietHoaDonBan) < 50
BEGIN
    DECLARE @HDB INT = (SELECT TOP 1 HoaDonBanId FROM dbo.HoaDonBan ORDER BY NEWID());
    DECLARE @MonId INT = (SELECT TOP 1 MonId FROM dbo.Mon ORDER BY NEWID());
    DECLARE @Gia DECIMAL(18,2) = (SELECT TOP 1 DonGia FROM dbo.Mon WHERE MonId = @MonId);
    DECLARE @SL INT = 1 + (@i % 3);
    INSERT INTO dbo.ChiTietHoaDonBan (HoaDonBanId, MonId, DonGiaBan, SoLuong)
    VALUES (@HDB, @MonId, @Gia, @SL);
    SET @i += 1;
END
GO

/* ============================================================
   10) Khuyến mãi thực tế
============================================================ */
IF OBJECT_ID(N'dbo.KhuyenMai', N'U') IS NOT NULL
BEGIN
    DECLARE @i INT = 1;
    WHILE (SELECT COUNT(*) FROM dbo.KhuyenMai) < 50
    BEGIN
        DECLARE @TenKM NVARCHAR(150) = CHOOSE((@i % 10) + 1, N'Giảm giá giờ vàng', N'Ưu đãi khách thành viên', N'Mua combo tiết kiệm', N'Khuyến mãi cuối tuần', N'Ưu đãi sinh nhật', N'Giảm giá đơn mang đi', N'Tặng topping', N'Ưu đãi trà trái cây', N'Ưu đãi cà phê sáng', N'Giảm giá nhóm văn phòng') + N' ' + CAST(@i AS NVARCHAR(10));
        IF NOT EXISTS (SELECT 1 FROM dbo.KhuyenMai WHERE TenKhuyenMai = @TenKM)
            INSERT INTO dbo.KhuyenMai (TenKhuyenMai, LoaiKhuyenMai, GiaTri, TuNgay, DenNgay, IsActive, MonId, MoTa)
            VALUES (@TenKM, CASE WHEN @i % 3 = 0 THEN N'SoTienCoDinh' ELSE N'PhanTramHoaDon' END, CASE WHEN @i % 3 = 0 THEN 15000 ELSE 10 END, DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, 60, GETDATE()), 1, NULL, N'Chương trình ưu đãi áp dụng tại cửa hàng');
        SET @i += 1;
    END
END
GO

/* ============================================================
   11) Nguyên liệu và công thức món
============================================================ */
IF OBJECT_ID(N'dbo.NguyenLieu', N'U') IS NOT NULL
BEGIN
    DECLARE @i INT = 1;
    WHILE (SELECT COUNT(*) FROM dbo.NguyenLieu) < 50
    BEGIN
        DECLARE @TenNL NVARCHAR(150) = CHOOSE((@i % 25) + 1, N'Cà phê Arabica', N'Cà phê Robusta', N'Sữa tươi không đường', N'Sữa đặc', N'Đường cát', N'Đường nâu', N'Trân châu đen', N'Thạch cà phê', N'Bột matcha', N'Bột cacao', N'Siro caramel', N'Siro vani', N'Siro dâu', N'Siro đào', N'Trà đen', N'Trà ô long', N'Trà xanh', N'Cam tươi', N'Chanh dây', N'Dâu tây', N'Xoài', N'Bơ', N'Kem whipping', N'Phô mai kem', N'Đá viên') + N' lô ' + CAST(@i AS NVARCHAR(10));
        IF NOT EXISTS (SELECT 1 FROM dbo.NguyenLieu WHERE TenNguyenLieu = @TenNL)
            INSERT INTO dbo.NguyenLieu (TenNguyenLieu, DonViTinh, TonKho, TonKhoToiThieu, DonGiaNhap, IsActive)
            VALUES (@TenNL, CASE WHEN @i % 4 = 0 THEN N'lít' WHEN @i % 4 = 1 THEN N'kg' WHEN @i % 4 = 2 THEN N'chai' ELSE N'gói' END, 20 + @i, 5, 10000 + (@i * 2500), 1);
        SET @i += 1;
    END
END
GO

IF OBJECT_ID(N'dbo.CongThucMon', N'U') IS NOT NULL
BEGIN
    DECLARE @i INT = 1;
    WHILE (SELECT COUNT(*) FROM dbo.CongThucMon) < 50
    BEGIN
        DECLARE @MonId INT = (SELECT TOP 1 MonId FROM dbo.Mon ORDER BY NEWID());
        DECLARE @NLId INT = (SELECT TOP 1 NguyenLieuId FROM dbo.NguyenLieu ORDER BY NEWID());
        IF NOT EXISTS (SELECT 1 FROM dbo.CongThucMon WHERE MonId = @MonId AND NguyenLieuId = @NLId AND IsActive = 1)
            INSERT INTO dbo.CongThucMon (MonId, NguyenLieuId, DinhLuong, GhiChu, IsActive)
            VALUES (@MonId, @NLId, CAST((5 + (@i % 30)) AS DECIMAL(18,2)) / 100, N'Định lượng chuẩn theo công thức pha chế', 1);
        SET @i += 1;
    END
END
GO

/* ============================================================
   12) Log/lịch sử nghiệp vụ nếu bảng tồn tại
============================================================ */
IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NOT NULL
BEGIN
    DECLARE @i INT = 1;
    WHILE (SELECT COUNT(*) FROM dbo.AuditLog) < 50
    BEGIN
        INSERT INTO dbo.AuditLog (NguoiDungId, HanhDong, DoiTuong, DuLieuTomTat, ThoiGianTao, MayTram)
        VALUES ((SELECT TOP 1 NguoiDungId FROM dbo.NguoiDung ORDER BY NEWID()), CHOOSE((@i % 5)+1, N'Tạo hóa đơn', N'Cập nhật món', N'Nhập kho', N'Chốt ca', N'Áp dụng khuyến mãi'), CHOOSE((@i % 4)+1, N'HoaDonBan', N'Mon', N'HoaDonNhap', N'CaLamViec'), N'Ghi nhận thao tác nghiệp vụ tại cửa hàng', DATEADD(MINUTE, -@i*17, GETDATE()), N'POS-' + RIGHT('00' + CAST((@i % 8)+1 AS VARCHAR(2)), 2));
        SET @i += 1;
    END
END
GO

IF OBJECT_ID(N'dbo.LichSuTonKho', N'U') IS NOT NULL
BEGIN
    DECLARE @i INT = 1;
    WHILE (SELECT COUNT(*) FROM dbo.LichSuTonKho) < 50
    BEGIN
        DECLARE @MonId INT = (SELECT TOP 1 MonId FROM dbo.Mon ORDER BY NEWID());
        DECLARE @TonTruoc INT = 30 + @i;
        INSERT INTO dbo.LichSuTonKho (MonId, LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, GhiChu, ThoiGian, NguoiDungId)
        VALUES (@MonId, CASE WHEN @i % 2 = 0 THEN N'NhapHang' ELSE N'BanHang' END, CASE WHEN @i % 2 = 0 THEN 5 ELSE -2 END, @TonTruoc, CASE WHEN @i % 2 = 0 THEN @TonTruoc + 5 ELSE @TonTruoc - 2 END, N'Lịch sử thay đổi tồn kho thực tế', DATEADD(HOUR, -@i, GETDATE()), (SELECT TOP 1 NguoiDungId FROM dbo.NguoiDung ORDER BY NEWID()));
        SET @i += 1;
    END
END
GO

IF OBJECT_ID(N'dbo.LichSuNguyenLieu', N'U') IS NOT NULL
BEGIN
    DECLARE @i INT = 1;
    WHILE (SELECT COUNT(*) FROM dbo.LichSuNguyenLieu) < 50
    BEGIN
        DECLARE @NLId INT = (SELECT TOP 1 NguyenLieuId FROM dbo.NguyenLieu ORDER BY NEWID());
        DECLARE @TonTruoc DECIMAL(18,2) = 20 + @i;
        INSERT INTO dbo.LichSuNguyenLieu (NguyenLieuId, LoaiPhatSinh, SoLuongThayDoi, TonTruoc, TonSau, GhiChu, ThoiGian, NguoiDungId)
        VALUES (@NLId, CASE WHEN @i % 2 = 0 THEN N'NhapKho' ELSE N'XuatKho' END, CASE WHEN @i % 2 = 0 THEN 3 ELSE -1 END, @TonTruoc, CASE WHEN @i % 2 = 0 THEN @TonTruoc + 3 ELSE @TonTruoc - 1 END, N'Lịch sử nguyên liệu phục vụ pha chế', DATEADD(HOUR, -@i, GETDATE()), (SELECT TOP 1 NguoiDungId FROM dbo.NguoiDung ORDER BY NEWID()));
        SET @i += 1;
    END
END
GO

IF OBJECT_ID(N'dbo.LichSuCapNhatCongThuc', N'U') IS NOT NULL
BEGIN
    DECLARE @i INT = 1;
    WHILE (SELECT COUNT(*) FROM dbo.LichSuCapNhatCongThuc) < 50
    BEGIN
        INSERT INTO dbo.LichSuCapNhatCongThuc (MonId, NgayCapNhat, NguoiCapNhat, NoiDungCapNhat)
        VALUES ((SELECT TOP 1 MonId FROM dbo.Mon ORDER BY NEWID()), DATEADD(DAY, -@i, GETDATE()), N'Quản lý cửa hàng', N'Cập nhật định lượng và quy trình pha chế');
        SET @i += 1;
    END
END
GO

/* ============================================================
   13) Cấu hình hệ thống - bảng cấu hình chỉ nên có 1 dòng thật
============================================================ */
IF OBJECT_ID(N'dbo.CauHinhHeThong', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.CauHinhHeThong WHERE TenQuan = N'GIBOR Coffee')
    BEGIN
        INSERT INTO dbo.CauHinhHeThong (TenQuan, DiaChi, SoDienThoai, FooterHoaDon, LogoPath)
        VALUES (N'GIBOR Coffee', N'123 Nguyễn Huệ, Quận 1, TP.HCM', N'0909000999', N'Cảm ơn quý khách và hẹn gặp lại.', N'images/logo-gibor-coffee.png');
    END
END
GO

/* ============================================================
   14) Báo cáo kiểm tra số dòng
============================================================ */
SELECT N'VaiTro' AS TenBang, COUNT(*) AS SoDong FROM dbo.VaiTro
UNION ALL SELECT N'NguoiDung', COUNT(*) FROM dbo.NguoiDung
UNION ALL SELECT N'DanhMuc', COUNT(*) FROM dbo.DanhMuc
UNION ALL SELECT N'NhaCungCap', COUNT(*) FROM dbo.NhaCungCap
UNION ALL SELECT N'Mon', COUNT(*) FROM dbo.Mon
UNION ALL SELECT N'HoaDonNhap', COUNT(*) FROM dbo.HoaDonNhap
UNION ALL SELECT N'ChiTietHoaDonNhap', COUNT(*) FROM dbo.ChiTietHoaDonNhap
UNION ALL SELECT N'HoaDonBan', COUNT(*) FROM dbo.HoaDonBan
UNION ALL SELECT N'ChiTietHoaDonBan', COUNT(*) FROM dbo.ChiTietHoaDonBan;
GO
