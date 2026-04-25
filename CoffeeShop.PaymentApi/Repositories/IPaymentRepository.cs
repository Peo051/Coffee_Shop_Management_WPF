using CoffeeShop.PaymentApi.Models;

namespace CoffeeShop.PaymentApi.Repositories;

/// <summary>
/// Interface cho payment repository
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Lấy hóa đơn để tạo payment (bao gồm thông tin cần thiết)
    /// </summary>
    Task<HoaDonBan?> GetHoaDonForPaymentAsync(int hoaDonBanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu thông tin QR payment đã tạo
    /// </summary>
    Task<bool> SaveCreatedQrPaymentAsync(
        int hoaDonBanId,
        string paymentProvider,
        string providerPaymentId,
        long providerOrderCode,
        string qrCodeRaw,
        string checkoutUrl,
        DateTime qrExpiredAt,
        string paymentStatus,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy trạng thái thanh toán của hóa đơn
    /// </summary>
    Task<PaymentStatusResponse?> GetPaymentStatusAsync(int hoaDonBanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu payment đã thanh toán (từ webhook)
    /// </summary>
    Task<bool> MarkPaymentPaidAsync(
        int hoaDonBanId,
        string maGiaoDich,
        DateTime paymentConfirmedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu payment đã hủy
    /// </summary>
    Task<bool> MarkPaymentCancelledAsync(int hoaDonBanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm hóa đơn theo ProviderOrderCode
    /// </summary>
    Task<HoaDonBan?> FindHoaDonByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra hóa đơn đã thanh toán chưa
    /// </summary>
    Task<bool> IsAlreadyPaidAsync(int hoaDonBanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finalize QR payment: trừ kho, nguyên liệu, điểm khi webhook xác nhận PAID
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> FinalizeQrPaymentAsync(
        int hoaDonBanId,
        string maGiaoDich,
        DateTime paymentConfirmedAt,
        CancellationToken cancellationToken = default);
}
