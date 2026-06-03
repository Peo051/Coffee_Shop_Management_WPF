using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CoffeeShop.CrystalReports.Wpf.Infrastructure;
using CoffeeShop.CrystalReports.Wpf.Models;

namespace CoffeeShop.CrystalReports.Wpf.Services
{
    /// <summary>
    /// Triển khai <see cref="ICrystalReportService"/> bằng ADO.NET thuần
    /// (System.Data.SqlClient) với câu lệnh tham số hoá để tránh SQL injection.
    /// Tên bảng/cột bám theo schema thực tế trong database/02_CreateTables.sql
    /// và database/DATABASE.sql của solution.
    /// </summary>
    public sealed class CrystalReportService : ICrystalReportService
    {
        public IReadOnlyList<NhanVienItem> LoadEmployees()
        {
            const string sql = @"
            SELECT NguoiDungId, HoTen
            FROM dbo.NguoiDung
            WHERE IsActive = 1
            ORDER BY HoTen;";

            var list = new List<NhanVienItem>();
            using (var conn = new SqlConnection(AppConfig.ConnectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new NhanVienItem
                        {
                            NguoiDungId = reader.GetInt32(0),
                            HoTen = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }

        public IReadOnlyList<DanhMucItem> LoadCategories()
        {
            const string sql = @"
            SELECT DanhMucId, TenDanhMuc
            FROM dbo.DanhMuc
            WHERE IsActive = 1
            ORDER BY TenDanhMuc;";

            var list = new List<DanhMucItem>();
            using (var conn = new SqlConnection(AppConfig.ConnectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DanhMucItem
                        {
                            DanhMucId = reader.GetInt32(0),
                            TenDanhMuc = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Truy vấn doanh thu chi tiết theo khoảng ngày.
        /// Hai tham số NguoiDungId và DanhMucId là optional: nếu null
        /// thì WHERE sẽ bỏ qua điều kiện đó (an toàn cho mọi tổ hợp).
        /// </summary>
        public DataTable LayDoanhThuTheoNgay(DateTime tuNgay, DateTime denNgay, int? nguoiDungId, int? danhMucId)
        {
            // SQL tham số hoá. Cột & quan hệ bảng đúng theo schema thực tế:
            //   HoaDonBan(HoaDonBanId, NgayBan, CreatedByUserId, KhachHangId, BanId, GiamGia, ThanhToan, ...)
            //   ChiTietHoaDonBan(HoaDonBanId, MonId, DonGiaBan, SoLuong, ThanhTien, ...)
            //   Mon(MonId, TenMon, DanhMucId, ...)
            //   DanhMuc(DanhMucId, TenDanhMuc, ...)
            //   NguoiDung(NguoiDungId, HoTen, ...)
            //   KhachHang(KhachHangId, HoTen, ...)
            //   Ban(BanId, TenBan, ...)
            const string sql = @"
            SELECT
                hdb.HoaDonBanId      AS MaHoaDon,
                hdb.NgayBan          AS NgayLap,
                nd.HoTen             AS TenNhanVien,
                ISNULL(kh.HoTen, b.TenBan) AS TenKhachHangHoacBan,
                m.TenMon             AS TenMon,
                dm.TenDanhMuc        AS TenDanhMuc,
                ct.SoLuong           AS SoLuong,
                ct.DonGiaBan         AS DonGia,
                ct.ThanhTien         AS ThanhTien,
                hdb.HinhThucPhucVu   AS HinhThucPhucVu,
                hdb.HinhThucThanhToan AS HinhThucThanhToan
            FROM dbo.HoaDonBan hdb
            INNER JOIN dbo.ChiTietHoaDonBan ct ON hdb.HoaDonBanId = ct.HoaDonBanId
            INNER JOIN dbo.Mon m               ON ct.MonId = m.MonId
            LEFT  JOIN dbo.DanhMuc dm          ON m.DanhMucId = dm.DanhMucId
            LEFT  JOIN dbo.NguoiDung nd        ON hdb.CreatedByUserId = nd.NguoiDungId
            LEFT  JOIN dbo.KhachHang kh        ON hdb.KhachHangId = kh.KhachHangId
            LEFT  JOIN dbo.Ban b               ON hdb.BanId = b.BanId
            WHERE hdb.NgayBan >= @TuNgay
              AND hdb.NgayBan <  DATEADD(DAY, 1, @DenNgay)
              AND ( @MaNhanVien IS NULL OR hdb.CreatedByUserId = @MaNhanVien )
              AND ( @MaDanhMuc  IS NULL OR m.DanhMucId         = @MaDanhMuc  )
              AND ISNULL(hdb.TrangThaiThanhToan, N'Đã thanh toán') <> N'Đã hủy'
            ORDER BY hdb.NgayBan, hdb.HoaDonBanId;";

            var table = new DataTable("Command");
            using (var conn = new SqlConnection(AppConfig.ConnectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TuNgay", SqlDbType.DateTime2).Value = tuNgay.Date;
                cmd.Parameters.Add("@DenNgay", SqlDbType.DateTime2).Value = denNgay.Date;
                cmd.Parameters.Add("@MaNhanVien", SqlDbType.Int).Value =
                    nguoiDungId.HasValue ? (object)nguoiDungId.Value : DBNull.Value;
                cmd.Parameters.Add("@MaDanhMuc", SqlDbType.Int).Value =
                    danhMucId.HasValue ? (object)danhMucId.Value : DBNull.Value;

                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(table);
                }
            }

            NormalizeReportDisplayValues(table);
            return table;
        }

        private static void NormalizeReportDisplayValues(DataTable table)
        {
            if (table == null) return;

            foreach (DataRow row in table.Rows)
            {
                if (table.Columns.Contains("TenKhachHangHoacBan"))
                {
                    row["TenKhachHangHoacBan"] = ToDisplayText(row["TenKhachHangHoacBan"]);
                }

                if (table.Columns.Contains("HinhThucPhucVu"))
                {
                    row["HinhThucPhucVu"] = FormatServingMethod(row["HinhThucPhucVu"]);
                }

                if (table.Columns.Contains("HinhThucThanhToan"))
                {
                    row["HinhThucThanhToan"] = FormatPaymentMethod(row["HinhThucThanhToan"]);
                }
            }
        }

        private static string ToDisplayText(object value)
        {
            if (value == null || value == DBNull.Value)
                return "-";

            var text = Convert.ToString(value)?.Trim();
            return string.IsNullOrWhiteSpace(text) ? "-" : text;
        }

        private static string FormatServingMethod(object value)
        {
            var text = ToDisplayText(value);
            if (text == "-") return text;

            switch (NormalizeKey(text))
            {
                case "uongtaiquan":
                case "uốngtạiquán":
                case "taiquan":
                case "tạiquán":
                case "dinein":
                    return "Tại quán";
                case "mangdi":
                case "mangđi":
                case "takeaway":
                case "takeout":
                    return "Mang đi";
                default:
                    return text;
            }
        }

        private static string FormatPaymentMethod(object value)
        {
            var text = ToDisplayText(value);
            if (text == "-") return text;

            switch (NormalizeKey(text))
            {
                case "qrcode":
                case "qr":
                    return "QR";
                case "cash":
                case "tienmat":
                case "tiềnmặt":
                    return "Tiền mặt";
                case "chuyenkhoan":
                case "chuyểnkhoản":
                case "banktransfer":
                    return "CK";
                case "card":
                case "the":
                case "thẻ":
                    return "Thẻ";
                default:
                    return text;
            }
        }

        private static string NormalizeKey(string value)
        {
            return value.Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }
    }
}
