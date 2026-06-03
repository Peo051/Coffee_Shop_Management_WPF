using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Threading;
using CoffeeShop.CrystalReports.Wpf.Infrastructure;
using CoffeeShop.CrystalReports.Wpf.Models;
using CoffeeShop.CrystalReports.Wpf.Services;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalReportsWinFormsViewer = CrystalDecisions.Windows.Forms.CrystalReportViewer;

namespace CoffeeShop.CrystalReports.Wpf.Views
{
    /// <summary>
    /// View hiển thị Crystal Report "Báo cáo doanh thu bán hàng theo khoảng ngày".
    /// Crystal Report viewer khó binding thuần MVVM nên giữ logic gán
    /// ReportSource trong code-behind. Phần truy vấn dữ liệu và load
    /// ComboBox được tách sang <see cref="CrystalReportService"/>.
    /// </summary>
    public partial class CrystalReportDoanhThuView : UserControl
    {
        // Tên file .rpt nằm trong thư mục Reports/, copy ra output theo csproj.
        private const string ReportFileName = "CrystalDoanhThuTheoNgay.rpt";

        // Sentinel cho ComboBox "Tất cả".
        private static readonly NhanVienItem AllNhanVien = new NhanVienItem { NguoiDungId = 0, HoTen = "-- Tất cả --" };
        private static readonly DanhMucItem AllDanhMuc = new DanhMucItem { DanhMucId = 0, TenDanhMuc = "-- Tất cả --" };

        private readonly ICrystalReportService _service;    //Lấy dữ liệu từ database
        private WindowsFormsHost _viewerHostControl;   //Nhúng WinForms control vào WPF 
        private CrystalReportsWinFormsViewer _crystalReportsViewer;
        private ReportDocument _currentReport;

        public CrystalReportDoanhThuView()
        {
            InitializeComponent();

            _service = new CrystalReportService();

            // Set ngày mặc định: 7 ngày gần đây.
            dpTuNgay.SelectedDate = DateTime.Today.AddDays(-7);
            dpDenNgay.SelectedDate = DateTime.Today;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadComboBoxes();
                EnsureCrystalReportsViewer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không tải được danh sách nhân viên / danh mục.\n\nChi tiết: " + ex.Message,
                    "Lỗi tải dữ liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_crystalReportsViewer != null)
            {
                _crystalReportsViewer.ReportSource = null;
                _crystalReportsViewer.Dispose();
                _crystalReportsViewer = null;
            }

            viewerHost.Children.Clear();
            _viewerHostControl?.Dispose();
            _viewerHostControl = null;

            _currentReport?.Close();
            _currentReport?.Dispose();
            _currentReport = null;
        }

        private void LoadComboBoxes()
        {
            // Nạp ComboBox nhân viên: thêm dòng "-- Tất cả --" ở đầu để không truyền tham số.
            var nhanViens = new List<NhanVienItem> { AllNhanVien };
            nhanViens.AddRange(_service.LoadEmployees());
            cboNhanVien.ItemsSource = nhanViens;
            cboNhanVien.SelectedIndex = 0;

            // Nạp ComboBox danh mục.
            var danhMucs = new List<DanhMucItem> { AllDanhMuc };
            danhMucs.AddRange(_service.LoadCategories());
            cboDanhMuc.ItemsSource = danhMucs;
            cboDanhMuc.SelectedIndex = 0;
        }

        private void btnThoat_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window?.Close();
        }

        private void btnXemBaoCao_Click(object sender, RoutedEventArgs e)
        {
            // 1) Validate tham số ngày.
            if (!dpTuNgay.SelectedDate.HasValue || !dpDenNgay.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng chọn đầy đủ 'Từ ngày' và 'Đến ngày'.",
                    "Thiếu tham số", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tuNgay = dpTuNgay.SelectedDate.Value.Date;
            var denNgay = dpDenNgay.SelectedDate.Value.Date;
            if (tuNgay > denNgay)
            {
                MessageBox.Show("'Từ ngày' phải nhỏ hơn hoặc bằng 'Đến ngày'.",
                    "Sai khoảng ngày", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Lấy id ComboBox: 0 nghĩa là "Tất cả" -> truyền null.
            int? maNhanVien = null;
            if (cboNhanVien.SelectedValue is int nv && nv > 0) maNhanVien = nv;

            int? maDanhMuc = null;
            if (cboDanhMuc.SelectedValue is int dm && dm > 0) maDanhMuc = dm;

            try
            {
                ShowReport(tuNgay, denNgay, maNhanVien, maDanhMuc);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(
                    "Không tìm thấy file báo cáo .rpt.\n\n" + ex.Message +
                    "\n\nTạo file Reports/CrystalDoanhThuTheoNgay.rpt",
                    "Thiếu file báo cáo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (TypeInitializationException ex)
            {
                // Thường gặp khi máy chưa cài Crystal Reports runtime.
                MessageBox.Show(
                    "Không khởi tạo được Crystal Reports.\n\n" +
                    "Hãy chắc chắn đã cài 'SAP Crystal Reports for Visual Studio' và runtime " +
                    "(x86 hoặc x64) tương ứng với platform build.\n\n" +
                    "Chi tiết: " + ex.Message,
                    "Thiếu Crystal Reports runtime",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Có lỗi khi hiển thị báo cáo.\n\n" + BuildExceptionMessage(ex),
                    "Lỗi báo cáo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Tải file .rpt, set tham số, set datasource và bind vào viewer.
        /// </summary>
        private void ShowReport(DateTime tuNgay, DateTime denNgay, int? maNhanVien, int? maDanhMuc)
        {
            // Tìm file .rpt copy ra cùng thư mục output.
            string rptPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory ?? string.Empty,
                "Reports",
                ReportFileName);
            if (!File.Exists(rptPath))
            {
                throw new FileNotFoundException("Không tìm thấy file .rpt. Đường dẫn đang tìm: " + rptPath, rptPath);
            }

            
            ReportDocument rpt = new ReportDocument();
            rpt.Load(rptPath);  //Load thiết kế báo cáo từ file .rpt

            // Tự động căn chỉnh hướng trang Landscape
            AdjustReportLayout(rpt);

            // lấy DataTable từ service rồi gán vào report.
            var dataTable = _service.LayDoanhThuTheoNgay(tuNgay, denNgay, maNhanVien, maDanhMuc);
            rpt.SetDataSource(dataTable);

            // Tính toán tổng số lượng và tổng doanh thu trực tiếp từ DataTable trong C# 
            // để ghi đè vào các nhãn hiển thị, tránh lỗi Summary Field của Crystal không tự tính lại.
            decimal tongDoanhThu = 0;
            int tongSoLuong = 0;
            if (dataTable != null)
            {
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    if (row["ThanhTien"] != DBNull.Value)
                        tongDoanhThu += Convert.ToDecimal(row["ThanhTien"]);
                    if (row["SoLuong"] != DBNull.Value)
                        tongSoLuong += Convert.ToInt32(row["SoLuong"]);
                }
            }

            // Ghi đè nội dung nhãn kèm giá trị đã tính toán và định dạng tiền tệ đẹp mắt
            // Tăng khoảng trắng để đẩy số lượng và doanh thu ra xa nhãn một chút cho thoáng đẹp
            SetTextObjectValue(rpt, "Text49", $"Tổng số lượng bán:  {tongSoLuong:N0}");
            SetTextObjectValue(rpt, "Text48", $"Tổng doanh thu:  {tongDoanhThu:N0}");

            // Rút ngắn tiêu đề cột Khách hàng/Bàn để không bị quấn dòng đè nét kẻ đứng
            SetTextObjectValue(rpt, "Text28", "Khách / Bàn");

            // Ẩn các Summary Field gốc bằng cách đặt Width = 0 để tránh đè lấp hoặc hiển thị trống trơn
            // Không ẩn Summary Field nữa vì report mới đang dùng Summary để hiển thị tổng.
            SetObjectLayout(rpt, "SumofSoLuong1", 9800, 1800);
            SetObjectLayout(rpt, "SumofThanhTien1", 9800, 1800);

            // (b) Set tham số TuNgay / DenNgay (Date).
            //     Nếu file .rpt có thêm Parameter MaNhanVien / MaDanhMuc,
            //     code dưới sẽ set; nếu không có thì bỏ qua an toàn.
            SetParameterIfExists(rpt, "TuNgay", tuNgay);
            SetParameterIfExists(rpt, "DenNgay", denNgay);
            SetParameterIfExists(rpt, "MaNhanVien", maNhanVien.HasValue ? maNhanVien.Value : 0);
            SetParameterIfExists(rpt, "MaDanhMuc", maDanhMuc.HasValue ? maDanhMuc.Value : 0);

            // hiển thị khoảng ngày để tránh lỗi 
            SetFormulaIfExists(rpt, "ReportPeriodText", $"\"Từ ngày: {tuNgay:dd/MM/yyyy} - Đến ngày: {denNgay:dd/MM/yyyy}\"");

            // (c) Cấu hình đăng nhập database cho mọi table trong report.
            ApplyLogonInfo(rpt);

            // (d) Gán report vào WinForms viewer hosted trong WPF.
            if (TrySetReportSource(rpt))
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!TrySetReportSource(rpt))
                {
                    rpt.Close();
                    rpt.Dispose();

                    MessageBox.Show(
                        "Không khởi tạo được Crystal Reports Viewer vì cửa sổ WPF chưa sẵn sàng.",
                        "Lỗi hiển thị báo cáo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }), DispatcherPriority.ContextIdle);
        }

        private bool TrySetReportSource(ReportDocument report)
        {
            if (!EnsureCrystalReportsViewer() || _crystalReportsViewer == null)
                return false;

            var previousReport = _currentReport;
            _currentReport = report;
            _crystalReportsViewer.ReportSource = report;
            _crystalReportsViewer.RefreshReport();

            if (!ReferenceEquals(previousReport, report))
            {
                previousReport?.Close();
                previousReport?.Dispose();
            }

            return true;
        }

        private bool EnsureCrystalReportsViewer()
        {
            if (_crystalReportsViewer != null)
                return true;

            _crystalReportsViewer = new CrystalReportsWinFormsViewer
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ToolPanelView = CrystalDecisions.Windows.Forms.ToolPanelViewType.None,
                ShowGroupTreeButton = false,
                ShowParameterPanelButton = false
            };

            _viewerHostControl = new WindowsFormsHost
            {
                Child = _crystalReportsViewer
            };

            viewerHost.Children.Add(_viewerHostControl);
            return true;
        }

        /// <summary>
        /// Tự động điều chỉnh hướng trang thành Landscape và cấu hình tọa độ các cột 
        /// để hiển thị đẹp mắt, không bị tràn lề hay đè lên nhau.
        /// </summary>
        private static void AdjustReportLayout(ReportDocument rpt)
        {
            if (rpt == null) return;

            try
            {
                // 1) Ép hướng trang là Landscape và khổ giấy là A4
                rpt.PrintOptions.PaperSize = PaperSize.PaperA4;
                rpt.PrintOptions.PaperOrientation = PaperOrientation.Landscape;

                // 2) Định nghĩa tọa độ Left và Width mới cho các cột (đơn vị: twips)
                // Sắp xếp tối ưu hóa không gian hiển thị, tránh quấn dòng tiêu đề Khách hàng/Bàn.
                // Đặt Width của cột Ngày lập là 900 twips (vừa khít 10 ký tự ngày dd/MM/yyyy) để tự động cắt bỏ phần giờ thừa phía sau.
                var columns = new[]
                {
                    // STT (Cột 1)
                    new { Header = "Text24", Detail = "RecordNumber1", Left = 0, Width = 400 },
                    // Mã HĐ (Cột 2)
                    new { Header = "Text25", Detail = "MaHoaDon1", Left = 450, Width = 650 },
                    // Ngày lập (Cột 3) - Đặt Width 900 twips để che khuất phần giờ thừa
                    new { Header = "Text26", Detail = "NgayLap1", Left = 1150, Width = 900 },
                    // Nhân viên (Cột 4) - Tăng Width lên 1700 và dịch Left sang 2150 để hiển thị tên đẹp hơn
                    new { Header = "Text27", Detail = "TenNhanVien1", Left = 2150, Width = 1700 },
                    // Khách hàng/Bàn (Cột 5) - Tăng Width lên 1300 và dịch Left sang 3950 để nhãn "Khách / Bàn" hiển thị đẹp không bị quấn dòng
                    new { Header = "Text28", Detail = "TenKhachHangHoacBan1", Left = 3950, Width = 1300 },
                    // Tên món (Cột 6)
                    new { Header = "Text29", Detail = "TenMon1", Left = 5300, Width = 2000 },
                    // SL (Cột 7)
                    new { Header = "Text30", Detail = "SoLuong1", Left = 7300, Width = 450 },
                    // Đơn giá (Cột 8)
                    new { Header = "Text31", Detail = "DonGia1", Left = 7800, Width = 1300 },
                    // Thành tiền (Cột 9) - Đảm bảo rộng 2100 để hiển thị số tiền lớn thoải mái
                    new { Header = "Text32", Detail = "ThanhTien1", Left = 9150, Width = 2100 },
                    // Phục vụ (Cột 10)
                    new { Header = "Text33", Detail = "HinhThucPhucVu1", Left = 11350, Width = 1050 },
                    // Thanh toán (Cột 11)
                    new { Header = "Text34", Detail = "HinhThucThanhToan1", Left = 12500, Width = 1150 }
                };

                foreach (var col in columns)
                {
                    SetObjectLayout(rpt, col.Header, col.Left, col.Width);
                    SetObjectLayout(rpt, col.Detail, col.Left, col.Width);
                }

                // 3) Căn chỉnh phần tổng kết ở Report Footer (Dịch sang trái, tăng rộng nhãn lên 4500 twips để tránh lỗi mất chữ T)
                SetObjectLayout(rpt, "Text49", 7500, 2500);
                SetObjectLayout(rpt, "SumofSoLuong1", 9800, 1800);

                SetObjectLayout(rpt, "Text48", 7500, 2500);
                SetObjectLayout(rpt, "SumofThanhTien1", 9800, 1800);

                // 4) Page Footer
                SetObjectLayout(rpt, "PrintedAtField", 0, 5000);     // In lúc...
                SetObjectLayout(rpt, "PageNumberField", 11500, 3000); // Trang...
            }
            catch
            {
                // Bỏ qua lỗi để báo cáo vẫn hiển thị
            }
        }

        private static void SetObjectLayout(ReportDocument rpt, string objectName, int left, int width)
        {
            if (rpt == null || string.IsNullOrWhiteSpace(objectName))
                return;

            try
            {
                var obj = rpt.ReportDefinition.ReportObjects[objectName];
                if (obj != null)
                {
                    obj.Left = left;
                    obj.Width = width;
                }
            }
            catch
            {
                // Bỏ qua nếu đối tượng không tồn tại trên bản vẽ
            }
        }

        /// <summary>
        /// Ghi đè nội dung văn bản của TextObject hoặc FieldHeadingObject trong Crystal Report.
        /// </summary>
        private static void SetTextObjectValue(ReportDocument rpt, string objectName, string text)
        {
            if (rpt == null || string.IsNullOrWhiteSpace(objectName))
                return;

            try
            {
                var obj = rpt.ReportDefinition.ReportObjects[objectName] as TextObject;
                if (obj != null)
                {
                    obj.Text = text;
                }
            }
            catch
            {
                // Bỏ qua nếu có lỗi
            }
        }

        /// <summary>
        /// Set parameter cho report nhưng không ném lỗi nếu parameter chưa
        /// được khai báo trong file .rpt (cho phép report tối giản).
        /// </summary>
        private static void SetParameterIfExists(ReportDocument rpt, string parameterName, object value)
        {
            if (rpt == null || string.IsNullOrWhiteSpace(parameterName))
                return;

            foreach (CrystalDecisions.CrystalReports.Engine.ParameterFieldDefinition parameter
                     in rpt.DataDefinition.ParameterFields)
            {
                if (string.Equals(parameter.Name, parameterName, StringComparison.OrdinalIgnoreCase))
                {
                    rpt.SetParameterValue(parameter.Name, value);
                    return;
                }
            }
        }

        /// <summary>
        /// Ghi đè công thức Crystal Report nếu tồn tại (để tránh lỗi compile công thức khi thiếu parameter).
        /// </summary>
        private static void SetFormulaIfExists(ReportDocument rpt, string formulaName, string formulaText)
        {
            if (rpt == null || string.IsNullOrWhiteSpace(formulaName))
                return;

            try
            {
                foreach (FormulaFieldDefinition formula in rpt.DataDefinition.FormulaFields)
                {
                    if (string.Equals(formula.Name, formulaName, StringComparison.OrdinalIgnoreCase))
                    {
                        formula.Text = formulaText;
                        break;
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi nếu không tìm thấy hoặc lỗi truy cập
            }
        }

        private static string BuildExceptionMessage(Exception ex)
        {
            var messages = new List<string>();
            for (Exception current = ex; current != null; current = current.InnerException)
            {
                if (!string.IsNullOrWhiteSpace(current.Message))
                {
                    messages.Add(current.Message);
                }
            }

            return string.Join("\n", messages);
        }

        /// <summary>
        /// Áp ConnectionInfo từ App.config vào toàn bộ Tables của report.
        /// Bắt buộc khi máy chạy không có DSN/ODBC mặc định cho Crystal.
        /// </summary>
        private static void ApplyLogonInfo(ReportDocument rpt)
        {
            var info = new ConnectionInfo
            {
                ServerName = AppConfig.Server,
                DatabaseName = AppConfig.Database,
                IntegratedSecurity = AppConfig.IntegratedSecurity,
                UserID = AppConfig.IntegratedSecurity ? string.Empty : AppConfig.UserId,
                Password = AppConfig.IntegratedSecurity ? string.Empty : AppConfig.Password
            };

            foreach (Table tbl in rpt.Database.Tables)
            {
                var li = tbl.LogOnInfo;
                li.ConnectionInfo = info;
                tbl.ApplyLogOnInfo(li);
                tbl.Location = tbl.Location;
            }
        }
    }
}
