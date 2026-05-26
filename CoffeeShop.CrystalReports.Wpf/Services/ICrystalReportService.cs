using System;
using System.Collections.Generic;
using System.Data;
using CoffeeShop.CrystalReports.Wpf.Models;

namespace CoffeeShop.CrystalReports.Wpf.Services
{
    /// <summary>
    /// Service tách phần truy vấn CSDL ra khỏi code-behind.
    /// CrystalReportViewer là control khó binding thuần MVVM nên phần
    /// gán ReportSource vẫn nằm trong code-behind, còn data và metadata
    /// thì được lấy qua service này.
    /// </summary>
    public interface ICrystalReportService
    {
        /// <summary>Danh sách nhân viên (NguoiDung) đang active.</summary>
        IReadOnlyList<NhanVienItem> LoadEmployees();

        /// <summary>Danh sách danh mục món đang active.</summary>
        IReadOnlyList<DanhMucItem> LoadCategories();

        /// <summary>
        /// Lấy DataTable doanh thu theo khoảng ngày, nhân viên, danh mục.
        /// Dùng để gọi <c>SetDataSource</c> cho Crystal Report (ngoài
        /// trường hợp Crystal Report đã cấu hình nguồn dữ liệu trực tiếp
        /// bằng Add Command).
        /// </summary>
        DataTable LayDoanhThuTheoNgay(DateTime tuNgay, DateTime denNgay, int? nguoiDungId, int? danhMucId);
    }
}
