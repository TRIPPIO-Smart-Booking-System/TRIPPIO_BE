using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Trippio.Api.Controllers.Payment
{
    [ApiController]
    [Route("api/payments/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WebhookController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("payos")]
        public async Task<IActionResult> HandlePayOSWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            // Lấy signature từ header
            var sigHeader = Request.Headers["X-Signature"].FirstOrDefault();
            if (string.IsNullOrEmpty(sigHeader))
                return BadRequest("Missing signature");

            // Lấy ChecksumKey từ appsettings
            var checksumKey = _config["PayOS:ChecksumKey"];
            if (string.IsNullOrEmpty(checksumKey))
                return StatusCode(500, "ChecksumKey not configured");

            // Xác thực signature
            if (!IsValidSignature(payload, sigHeader, checksumKey))
                return Unauthorized("Invalid signature");

            // Parse payload (giả sử JSON từ PayOS)
            // Ví dụ: { "paymentId": "...", "status": "success", ... }
            var data = JsonSerializer.Deserialize<PayOSWebhookData>(payload);
            if (data == null)
                return BadRequest("Invalid payload");

            // Xử lý logic: cập nhật DB, gửi email, v.v.
            // Ví dụ: await _paymentService.UpdateStatusAsync(data.PaymentId, data.Status);

            return Ok(new { message = "Webhook processed successfully" });
        }

        private bool IsValidSignature(string payload, string signature, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToHexString(hash).ToLower();
            return signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class PayOSWebhookData
    {
        public required string PaymentId { get; set; }
        public required string Status { get; set; }
        // Thêm các trường khác theo tài liệu PayOS
    }
}