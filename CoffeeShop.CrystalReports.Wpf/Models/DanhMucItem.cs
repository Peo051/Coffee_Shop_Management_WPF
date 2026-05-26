namespace CoffeeShop.CrystalReports.Wpf.Models
{
    /// <summary>
    /// Mục item phục vụ cho ComboBox chọn danh mục món.
    /// Map sang bảng dbo.DanhMuc (DanhMucId, TenDanhMuc).
    /// </summary>
    public sealed class DanhMucItem
    {
        public int DanhMucId { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;

        public override string ToString() => TenDanhMuc;
    }
}
