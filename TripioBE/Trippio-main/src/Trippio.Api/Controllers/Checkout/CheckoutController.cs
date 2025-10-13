using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Models.Common;
using Trippio.Core.Services;
using Trippio.Core.Models.Payment;
using Trippio.Core.ConfigOptions;
using Microsoft.Extensions.Options;

namespace Trippio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly IBasketService _basket;
    private readonly IOrderService _orders;
    private readonly IPaymentService _payments;
    private readonly RedirectUrlsOptions _redirectUrls;

    public CheckoutController(IBasketService basket, IOrderService orders, IPaymentService payments, IOptions<RedirectUrlsOptions> redirectUrls)
    {
        _basket = basket; _orders = orders; _payments = payments; _redirectUrls = redirectUrls.Value;
    }

    public record StartCheckoutDto(Guid UserId, string PaymentMethod, string ClientType);

    // B -> C -> D -> E
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartCheckoutDto dto, CancellationToken ct)
    {
        var basket = (await _basket.GetAsync(dto.UserId, ct)).Data!;
        var created = await _orders.CreateFromBasketAsync(dto.UserId, basket, ct); 
        if (created.Code != 200) return StatusCode(created.Code, created);

        await _basket.ClearAsync(dto.UserId, ct);
        var order = created.Data!;

        var paymentRequest = new CreatePaymentRequest
        {
            UserId = dto.UserId,
            OrderId = order.Id,
            Amount = order.TotalAmount,
            PaymentMethod = dto.PaymentMethod
        };

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        // Tạo return URL dựa trên client type
        var returnUrl = dto.ClientType.ToLower() == "mobile" ? _redirectUrls.Mobile : _redirectUrls.Web;

        var paymentUrl = await _payments.CreatePaymentUrlAsync(paymentRequest, returnUrl, ipAddress);

        return Ok(BaseResponse<object>.Success(new { orderId = order.Id, paymentUrl }));
    }
}
