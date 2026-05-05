using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public interface ILichSuNguyenLieuRepository
{
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

    Task<IReadOnlyList<LichSuNguyenLieu>> GetLichSuTheoNguyenLieuAsync(
        int nguyenLieuId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LichSuNguyenLieu>> GetLichSuGanDayAsync(
        int top = 100,
        CancellationToken cancellationToken = default);
}
