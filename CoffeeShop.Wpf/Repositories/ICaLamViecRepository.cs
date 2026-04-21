using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Repositories;

public interface ICaLamViecRepository
{
    Task<CaLamViec?> GetCaDangMoAsync(int nguoiDungId, CancellationToken cancellationToken = default);

    Task<int> MoCaAsync(int nguoiDungId, decimal tienDauCa, string? ghiChu, CancellationToken cancellationToken = default);

    Task<bool> DongCaAsync(int caLamViecId, string? ghiChu,
        decimal tienMatThucDem, decimal chenhLechTienMat, string? ghiChuDoiSoat,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CaLamViec>> GetLichSuCaAsync(
        DateTime fromDate,
        DateTime toDate,
        int? nguoiDungId,
        CancellationToken cancellationToken = default);

    Task<CaTongKetModel?> GetTongKetCaAsync(int caLamViecId, CancellationToken cancellationToken = default);
}
