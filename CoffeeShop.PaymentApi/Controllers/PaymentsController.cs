using CoffeeShop.PaymentApi.Models;
using CoffeeShop.PaymentApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.PaymentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentGatewayService paymentGatewayService,
        ILogger<PaymentsController> logger)
    {
        _paymentGatewayService = paymentGatewayService;
        _logger = logger;
    }

    /// <summary>
    /// Tạo QR payment cho hóa đơn
    /// </summary>
    [HttpPost("qr/create")]
    public async Task<ActionResult<CreateQRPaymentResponse>> CreateQRPayment(
        [FromBody] CreateQRPaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate request
            if (request.HoaDonBanId <= 0)
            {
                return BadRequest(new { message = "HoaDonBanId không hợp lệ" });
            }

            var response = await _paymentGatewayService.CreateQrAsync(request.HoaDonBanId, cancellationToken);

            _logger.LogInformation("Created QR payment for HoaDonBan {HoaDonBanId}", request.HoaDonBanId);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation creating QR payment for HoaDonBan {HoaDonBanId}", request.HoaDonBanId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating QR payment for HoaDonBan {HoaDonBanId}", request.HoaDonBanId);
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    /// <summary>
    /// Lấy trạng thái thanh toán của hóa đơn
    /// </summary>
    [HttpGet("{hoaDonBanId}/status")]
    public async Task<ActionResult<PaymentStatusResponse>> GetPaymentStatus(
        int hoaDonBanId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (hoaDonBanId <= 0)
            {
                return BadRequest(new { message = "HoaDonBanId không hợp lệ" });
            }

            var status = await _paymentGatewayService.GetStatusAsync(hoaDonBanId, cancellationToken);
            if (status == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn" });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for HoaDonBan {HoaDonBanId}", hoaDonBanId);
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    /// <summary>
    /// Webhook từ payOS khi thanh toán thành công
    /// </summary>
    [HttpPost("payos/webhook")]
    public async Task<IActionResult> PayOsWebhook(CancellationToken cancellationToken)
    {
        string? payload = null;
        
        try
        {
            // Đọc raw body
            using var reader = new StreamReader(Request.Body);
            payload = await reader.ReadToEndAsync(cancellationToken);

            _logger.LogInformation("Received payOS webhook. Payload length: {Length}", payload?.Length ?? 0);
            
            // Log raw payload trong Development
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Webhook raw payload: {Payload}", payload);
            }

            // Xử lý webhook - service sẽ không throw exception
            var result = await _paymentGatewayService.HandleWebhookAsync(payload ?? string.Empty, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Webhook processed successfully: {Message}", result.Message);
                return Ok(new { message = result.Message ?? "Webhook processed successfully" });
            }
            else
            {
                // Vẫn trả 200 OK nhưng với message khác
                _logger.LogWarning("Webhook received but not processed: {Message}", result.Message);
                return Ok(new { message = result.Message ?? "Webhook received but not processed" });
            }
        }
        catch (Exception ex)
        {
            // Luôn trả 200 OK để PayOS không retry
            _logger.LogError(ex, "Error processing payOS webhook. Payload: {Payload}", payload);
            return Ok(new { message = "Webhook received but processing failed. Logged for investigation." });
        }
    }

    /// <summary>
    /// Hủy QR payment
    /// </summary>
    [HttpPost("{hoaDonBanId}/cancel")]
    public async Task<IActionResult> CancelPayment(
        int hoaDonBanId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (hoaDonBanId <= 0)
            {
                return BadRequest(new { message = "HoaDonBanId không hợp lệ" });
            }

            var cancelled = await _paymentGatewayService.CancelAsync(hoaDonBanId, cancellationToken);

            if (cancelled)
            {
                _logger.LogInformation("Cancelled payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
                return Ok(new { message = "Đã hủy QR payment thành công" });
            }
            else
            {
                return BadRequest(new { message = "Không thể hủy payment" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    /// <summary>
    /// Test endpoint: Xác nhận thanh toán thành công (simulate webhook)
    /// </summary>
    [HttpPost("test/confirm/{hoaDonBanId}")]
    public async Task<IActionResult> TestConfirmPayment(
        int hoaDonBanId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (hoaDonBanId <= 0)
            {
                return BadRequest(new { message = "HoaDonBanId không hợp lệ" });
            }

            _logger.LogInformation("Test confirm payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);

            // Lấy thông tin payment hiện tại để lấy orderCode thật
            var status = await _paymentGatewayService.GetStatusAsync(hoaDonBanId, cancellationToken);
            
            if (status == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn" });
            }

            if (!status.ProviderOrderCode.HasValue)
            {
                return BadRequest(new { message = "Hóa đơn chưa có QR payment. Vui lòng tạo QR trước." });
            }

            // Tạo webhook giả với orderCode thật
            var fakeWebhook = new
            {
                code = "00",
                desc = "success",
                signature = "test",
                data = new
                {
                    orderCode = status.ProviderOrderCode.Value,
                    amount = (int)status.ThanhToan,
                    code = "00",
                    desc = "Thanh toán thành công (Xác nhận thủ công)",
                    reference = $"MANUAL-{DateTime.Now:yyyyMMddHHmmss}",
                    paymentLinkId = status.ProviderPaymentId ?? "manual-confirm"
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(fakeWebhook);
            var result = await _paymentGatewayService.HandleWebhookAsync(json, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Test payment confirmed successfully for HoaDonBan {HoaDonBanId}", hoaDonBanId);
                return Ok(new { message = "✅ Xác nhận thanh toán thành công!", result });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test confirm payment for HoaDonBan {HoaDonBanId}", hoaDonBanId);
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }
}
