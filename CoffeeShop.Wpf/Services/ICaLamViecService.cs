using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface ICaLamViecService
{
    Task<ServiceResult<CaLamViec>> MoCaAsync(
        int nguoiDungId,
        decimal tienDauCa,
        string? ghiChu,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> DongCaAsync(
        int nguoiDungId,
        decimal tienMatThucDem,
        string? ghiChu,
        string? ghiChuDoiSoat,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CaLamViec?>> GetCaDangMoAsync(
        int nguoiDungId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CaTongKetModel>> GetTongKetCaAsync(
        int caLamViecId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CaLamViec>>> GetLichSuCaAsync(
        DateTime fromDate,
        DateTime toDate,
        int? nguoiDungId,
        CancellationToken cancellationToken = default);
}
