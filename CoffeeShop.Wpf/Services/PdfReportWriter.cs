using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

/// <summary>
/// PDF Report Writer sử dụng QuestPDF để tạo báo cáo chuyên nghiệp với header, footer, và layout đẹp
/// </summary>
internal static class PdfReportWriter
{
    static PdfReportWriter()
    {
        // Cấu hình QuestPDF cho mục đích học tập/phát triển
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static async Task TaoBaoCaoDonGianAsync(
        string outputPath,
        DateTime fromDate,
        DateTime toDate,
        IReadOnlyList<BaoCaoDonGianDong> rows,
        CauHinhHeThong? cauHinh,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Header
                    page.Header().Element(c => TaoHeader(c, cauHinh, "BÁO CÁO DOANH THU THEO NGÀY"));

                    // Content
                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        // Thông tin khoảng thời gian
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Từ ngày: {fromDate:dd/MM/yyyy}").FontSize(11).SemiBold();
                            row.RelativeItem().Text($"Đến ngày: {toDate:dd/MM/yyyy}").FontSize(11).SemiBold();
                            row.RelativeItem().AlignRight().Text($"Tổng: {rows.Count} dòng").FontSize(11).SemiBold();
                        });

                        column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Bảng dữ liệu
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);  // STT
                                columns.RelativeColumn(2);   // Ngày
                                columns.RelativeColumn(1.5f); // Số HĐ
                                columns.RelativeColumn(2);   // Tổng tiền
                                columns.RelativeColumn(2);   // Giảm giá
                                columns.RelativeColumn(2);   // Doanh thu thuần
                            });

                            // Header bảng
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).Text("STT").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).Text("Ngày").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).Text("Số HĐ").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).AlignRight().Text("Tổng tiền").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).AlignRight().Text("Giảm giá").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).AlignRight().Text("Doanh thu thuần").FontColor(Colors.White).SemiBold();
                            });

                            // Dữ liệu
                            var stt = 1;
                            var tongTien = 0m;
                            var tongGiamGia = 0m;
                            var tongDoanhThuThuan = 0m;

                            foreach (var item in rows)
                            {
                                var bgColor = stt % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;

                                table.Cell().Background(bgColor).Padding(5).Text(stt.ToString());
                                table.Cell().Background(bgColor).Padding(5).Text(item.Ngay.ToString("dd/MM/yyyy"));
                                table.Cell().Background(bgColor).Padding(5).Text(item.SoHoaDon.ToString());
                                table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"{item.TongTien:N0}");
                                table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"{item.TongGiamGia:N0}");
                                table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"{item.DoanhThuThuan:N0}");

                                tongTien += item.TongTien;
                                tongGiamGia += item.TongGiamGia;
                                tongDoanhThuThuan += item.DoanhThuThuan;
                                stt++;
                            }

                            // Tổng cộng
                            table.Cell().ColumnSpan(3).Background(Colors.Brown.Lighten3).Padding(5).AlignRight().Text("TỔNG CỘNG:").SemiBold().FontSize(11);
                            table.Cell().Background(Colors.Brown.Lighten3).Padding(5).AlignRight().Text($"{tongTien:N0}").SemiBold().FontSize(11);
                            table.Cell().Background(Colors.Brown.Lighten3).Padding(5).AlignRight().Text($"{tongGiamGia:N0}").SemiBold().FontSize(11);
                            table.Cell().Background(Colors.Brown.Lighten3).Padding(5).AlignRight().Text($"{tongDoanhThuThuan:N0}").SemiBold().FontSize(11);
                        });
                    });

                    // Footer
                    page.Footer().Element(c => TaoFooter(c, cauHinh));
                });
            }).GeneratePdf(outputPath);

            cancellationToken.ThrowIfCancellationRequested();
        }, cancellationToken);
    }

    /// <summary>
    /// Tạo PDF báo cáo nâng cao với header, footer chuyên nghiệp
    /// </summary>
    public static async Task TaoBaoCaoNangCaoAsync(
        string outputPath,
        DateTime fromDate,
        DateTime toDate,
        IReadOnlyList<BaoCaoNangCaoDong> rows,
        CauHinhHeThong? cauHinh,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Header
                    page.Header().Element(c => TaoHeader(c, cauHinh, "BÁO CÁO DOANH THU THEO SẢN PHẨM"));

                    // Content
                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        // Thông tin khoảng thời gian
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Từ ngày: {fromDate:dd/MM/yyyy}").FontSize(11).SemiBold();
                            row.RelativeItem().Text($"Đến ngày: {toDate:dd/MM/yyyy}").FontSize(11).SemiBold();
                            row.RelativeItem().AlignRight().Text($"Tổng: {rows.Count} sản phẩm").FontSize(11).SemiBold();
                        });

                        column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Bảng dữ liệu
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);  // STT
                                columns.ConstantColumn(50);  // Mã món
                                columns.RelativeColumn(3);   // Tên món
                                columns.RelativeColumn(1.5f); // Số lượng
                                columns.RelativeColumn(2);   // Doanh thu
                                columns.RelativeColumn(2);   // Giá TB
                            });

                            // Header bảng
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).Text("STT").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).Text("Mã món").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).Text("Tên món").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).AlignRight().Text("SL bán").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).AlignRight().Text("Doanh thu").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Brown.Medium).Padding(5).AlignRight().Text("Giá TB").FontColor(Colors.White).SemiBold();
                            });

                            // Dữ liệu
                            var stt = 1;
                            var tongSoLuong = 0;
                            var tongDoanhThu = 0m;

                            foreach (var item in rows)
                            {
                                var bgColor = stt % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;

                                table.Cell().Background(bgColor).Padding(5).Text(stt.ToString());
                                table.Cell().Background(bgColor).Padding(5).Text(item.MonId.ToString());
                                table.Cell().Background(bgColor).Padding(5).Text(item.TenMon);
                                table.Cell().Background(bgColor).Padding(5).AlignRight().Text(item.SoLuongBan.ToString());
                                table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"{item.DoanhThuGop:N0}");
                                table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"{item.GiaBanTrungBinh:N0}");

                                tongSoLuong += item.SoLuongBan;
                                tongDoanhThu += item.DoanhThuGop;
                                stt++;
                            }

                            // Tổng cộng
                            table.Cell().ColumnSpan(3).Background(Colors.Brown.Lighten3).Padding(5).AlignRight().Text("TỔNG CỘNG:").SemiBold().FontSize(11);
                            table.Cell().Background(Colors.Brown.Lighten3).Padding(5).AlignRight().Text(tongSoLuong.ToString()).SemiBold().FontSize(11);
                            table.Cell().Background(Colors.Brown.Lighten3).Padding(5).AlignRight().Text($"{tongDoanhThu:N0}").SemiBold().FontSize(11);
                            table.Cell().Background(Colors.Brown.Lighten3).Padding(5).AlignRight().Text("-").SemiBold().FontSize(11);
                        });

                        // Thống kê tóm tắt
                        column.Item().PaddingTop(15).Column(summary =>
                        {
                            summary.Item().Text("THỐNG KÊ TÓM TẮT").FontSize(12).SemiBold().FontColor(Colors.Brown.Darken2);
                            summary.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text($"• Tổng số sản phẩm: {rows.Count}");
                                row.RelativeItem().Text($"• Tổng số lượng bán: {rows.Sum(x => x.SoLuongBan):N0}");
                                row.RelativeItem().Text($"• Tổng doanh thu: {rows.Sum(x => x.DoanhThuGop):N0} đ");
                            });
                        });
                    });

                    // Footer
                    page.Footer().Element(c => TaoFooter(c, cauHinh));
                });
            }).GeneratePdf(outputPath);

            cancellationToken.ThrowIfCancellationRequested();
        }, cancellationToken);
    }

    /// <summary>
    /// Tạo header chuyên nghiệp cho báo cáo
    /// </summary>
    private static void TaoHeader(IContainer container, CauHinhHeThong? cauHinh, string tieuDe)
    {
        container.Column(column =>
        {
            // Logo và thông tin quán
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(cauHinh?.TenQuan ?? "COFFEE SHOP")
                        .FontSize(16)
                        .SemiBold()
                        .FontColor(Colors.Brown.Darken2);

                    if (!string.IsNullOrWhiteSpace(cauHinh?.DiaChi))
                    {
                        col.Item().Text($"Địa chỉ: {cauHinh.DiaChi}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    }

                    if (!string.IsNullOrWhiteSpace(cauHinh?.SoDienThoai))
                    {
                        col.Item().Text($"Điện thoại: {cauHinh.SoDienThoai}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    }
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text($"Ngày in: {DateTime.Now:dd/MM/yyyy}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                    col.Item().AlignRight().Text($"Giờ in: {DateTime.Now:HH:mm:ss}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                });
            });

            // Tiêu đề báo cáo
            column.Item().PaddingTop(10).AlignCenter().Text(tieuDe)
                .FontSize(14)
                .SemiBold()
                .FontColor(Colors.Brown.Darken2);

            // Đường kẻ phân cách
            column.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Brown.Medium);
        });
    }

    /// <summary>
    /// Tạo footer chuyên nghiệp cho báo cáo
    /// </summary>
    private static void TaoFooter(IContainer container, CauHinhHeThong? cauHinh)
    {
        container.Column(column =>
        {
            // Đường kẻ phân cách
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

            // Thông tin footer
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Colors.Grey.Darken1));
                    text.Span("Trang ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });
    }
}
