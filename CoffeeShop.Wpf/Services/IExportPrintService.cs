using CoffeeShop.Wpf.Models;

namespace CoffeeShop.Wpf.Services;

public interface IExportPrintService
{
    Task<ServiceResult<string>> XuatPdfBaoCaoDonGianAsync(
        DateTime fromDate,
        DateTime toDate,
        string? outputDirectory = null,
        int? nguoiDungId = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<string>> XuatPdfBaoCaoNangCaoAsync(
        DateTime fromDate,
        DateTime toDate,
        string? outputDirectory = null,
        int? nguoiDungId = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<string>> XuatCsvThongKeAsync(
        DateTime fromDate,
        DateTime toDate,
        string? outputDirectory = null,
        int? nguoiDungId = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<string>> InHoaDonBanAsync(
        int hoaDonBanId,
        string? outputDirectory = null,
        int? nguoiDungId = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<string>> PreviewBaoCaoAsync(
        DateTime fromDate,
        DateTime toDate,
        string loaiBaoCao,
        string? outputDirectory = null,
        int? nguoiDungId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xem trước hóa đơn khách hàng (không in, chỉ mở file)
    /// </summary>
    Task<ServiceResult<string>> PreviewHoaDonBanAsync(
        int hoaDonBanId,
        string? outputDirectory = null,
        int? nguoiDungId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// In phiếu pha chế cho nhân viên (không có giá tiền)
    /// </summary>
    Task<ServiceResult<string>> InPhieuPhaCheAsync(
        int hoaDonBanId,
        string? outputDirectory = null,
        int? nguoiDungId = null,
        CancellationToken cancellationToken = default);
}
