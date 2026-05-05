using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public interface ILichSuTonKhoRepository
{
    Task ThemLichSuAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int monId,
        string loaiPhatSinh,
        int soLuongThayDoi,
        int tonTruoc,
        int tonSau,
        int? hoaDonBanId = null,
        int? hoaDonNhapId = null,
        string? ghiChu = null,
        int? nguoiDungId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LichSuTonKho>> GetLichSuTheoMonAsync(
        int monId,
        DateTime? tuNgay = null,
        DateTime? denNgay = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LichSuTonKho>> GetLichSuGanDayAsync(
        int soLuong = 100,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LichSuTonKho>> GetLichSuTheoHoaDonBanAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LichSuTonKho>> GetLichSuTheoHoaDonNhapAsync(
        int hoaDonNhapId,
        CancellationToken cancellationToken = default);
}
