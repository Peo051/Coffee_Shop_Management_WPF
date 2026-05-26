namespace CoffeeShop.CrystalReports.Wpf.Models
{
    /// <summary>
    /// Mục item phục vụ cho ComboBox chọn nhân viên (NguoiDung).
    /// Map sang bảng dbo.NguoiDung (NguoiDungId, HoTen).
    /// </summary>
    public sealed class NhanVienItem
    {
        public int NguoiDungId { get; set; }
        public string HoTen { get; set; } = string.Empty;

        public override string ToString() => HoTen;
    }
}
