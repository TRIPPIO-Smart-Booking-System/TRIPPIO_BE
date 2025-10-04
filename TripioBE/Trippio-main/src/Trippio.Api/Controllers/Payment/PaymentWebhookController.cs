using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Trippio.Api.Idempotency;
using Trippio.Api.Security;
using Trippio.Core.Models.Common;
using Trippio.Core.Models.Payment;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers
{
    [ApiController]
    [Route("api/payments/webhook")]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly IPaymentService _payments;
        private readonly IEmailService _email;
        private readonly IIdempotencyStore _idem;
        private readonly IConfiguration _config;

        public PaymentWebhookController(
            IPaymentService payments,
            IEmailService email,
            IIdempotencyStore idem,
            IConfiguration config)
        {
            _payments = payments; _email = email; _idem = idem; _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] PaymentWebhookDto body, CancellationToken ct)
        {
            var idemKey = Request.Headers["Idempotency-Key"].FirstOrDefault()
                          ?? $"{body.PaymentId}:{body.Status}:{body.OccurredAt:O}";
            if (!await _idem.TryUseAsync(idemKey, TimeSpan.FromHours(24)))
                return Ok(BaseResponse<string>.Success("Duplicate (ignored)"));

            var sigHeader = Request.Headers["X-Signature"].FirstOrDefault();
            var secret = _config["Payments:WebhookSecret"] ?? "dev-secret";
            var raw = JsonSerializer.Serialize(body);
            if (!HmacSignatureValidator.IsValid(raw, sigHeader ?? "", secret))
                return Unauthorized(BaseResponse<string>.Error("Invalid signature", 401));

            var result = await _payments.UpdatePaymentStatusAsync(body.PaymentId, body.Status);
            if (result.Code != 200)
                return StatusCode(result.Code, result);

            var subject = body.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)
                ? "Thanh toán thành công"
                : "Thanh toán thất bại";
            await _email.SendEmailAsync(
                   "user@example.com",
                    subject,
                    $"Payment {body.PaymentId} - {body.Status}"
);

            return Ok(BaseResponse<string>.Success("OK"));
        }
    }
}
