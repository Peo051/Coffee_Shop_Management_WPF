namespace CoffeeShop.Wpf.Models;

public sealed class CongThucMon
{
    public int CongThucMonId { get; set; }

    public int MonId { get; set; }

    public string? TenMon { get; set; }

    public int NguyenLieuId { get; set; }

    public string? TenNguyenLieu { get; set; }

    public string? DonViTinh { get; set; }

    public decimal DinhLuong { get; set; }

    public string? GhiChu { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal GiaVonDonVi { get; set; }

    public string? CacBuocThucHien { get; set; }

    public decimal TyLeHaoHut { get; set; }

    public string DinhLuongHienThi => $"{DinhLuong:N2} {DonViTinh}";

    public string DinhLuongChiTiet
    {
        get
        {
            if (DinhLuong < 1)
            {
                // Chuyển đổi sang đơn vị nhỏ hơn cho dễ đọc
                if (DonViTinh == "kg")
                    return $"{DinhLuong * 1000:N0}g";
                if (DonViTinh == "lít")
                    return $"{DinhLuong * 1000:N0}ml";
            }
            return DinhLuongHienThi;
        }
    }

    public decimal GiaVonNguyenLieu => DinhLuong * GiaVonDonVi;

    public string GiaVonNguyenLieuHienThi => $"{GiaVonNguyenLieu:N0} đ";
}
