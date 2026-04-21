# Database Scripts - Thứ Tự Chạy

1. `01_CreateDatabase.sql`
2. `02_CreateTables.sql`
3. `03_SeedData.sql`
4. `06_Wave1_InventoryAndTop.sql` (mở rộng đợt 1: tồn kho cảnh báo + top)
5. `05_DemoData.sql` (dữ liệu mẫu cho demo/report)
6. `04_TestQueries.sql` (đối chiếu nhanh)

Ghi chú:
- Script `05_DemoData.sql` được viết theo hướng idempotent tương đối để tránh chèn trùng dữ liệu demo chính.
- Khi chuẩn bị demo cuối, luôn chạy lại `04_TestQueries.sql` để xác nhận dữ liệu đầu vào.
