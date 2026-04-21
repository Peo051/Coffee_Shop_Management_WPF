using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public interface ILichSuNguyenLieuRepository
{
    /// <summary>
    /// Thêm lịch sử nguyên liệu (trong transaction)
    /// </summary>
    Task ThemLichSuAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int nguyenLieuId,
        string loaiPhatSinh,
        decimal soLuongThayDoi,
        decimal tonTruoc,
        decimal tonSau,
        int? hoaDonBanId,
        int? hoaDonNhapId,
        string? ghiChu,
        int? nguoiDungId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy lịch sử theo nguyên liệu
    /// </summary>
    Task<IReadOnlyList<LichSuNguyenLieu>> GetLichSuTheoNguyenLieuAsync(
        int nguyenLieuId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy lịch sử gần đây
    /// </summary>
    Task<IReadOnlyList<LichSuNguyenLieu>> GetLichSuGanDayAsync(
        int top = 100,
        CancellationToken cancellationToken = default);
}
