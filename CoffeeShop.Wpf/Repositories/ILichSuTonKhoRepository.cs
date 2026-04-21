using CoffeeShop.Wpf.Models;
using Microsoft.Data.SqlClient;

namespace CoffeeShop.Wpf.Repositories;

public interface ILichSuTonKhoRepository
{
    /// <summary>
    /// Thêm lịch sử tồn kho (trong transaction)
    /// </summary>
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

    /// <summary>
    /// Lấy lịch sử tồn kho theo món
    /// </summary>
    Task<IReadOnlyList<LichSuTonKho>> GetLichSuTheoMonAsync(
        int monId,
        DateTime? tuNgay = null,
        DateTime? denNgay = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy lịch sử tồn kho gần đây (tất cả món)
    /// </summary>
    Task<IReadOnlyList<LichSuTonKho>> GetLichSuGanDayAsync(
        int soLuong = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy lịch sử theo hóa đơn bán
    /// </summary>
    Task<IReadOnlyList<LichSuTonKho>> GetLichSuTheoHoaDonBanAsync(
        int hoaDonBanId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy lịch sử theo hóa đơn nhập
    /// </summary>
    Task<IReadOnlyList<LichSuTonKho>> GetLichSuTheoHoaDonNhapAsync(
        int hoaDonNhapId,
        CancellationToken cancellationToken = default);
}
