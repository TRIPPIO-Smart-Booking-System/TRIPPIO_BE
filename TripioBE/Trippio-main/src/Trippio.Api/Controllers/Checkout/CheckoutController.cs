using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Models.Common;
using Trippio.Core.Services;

namespace Trippio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly IBasketService _basket;
    private readonly IOrderService _orders;

    public CheckoutController(IBasketService basket, IOrderService orders)
    {
        _basket = basket; _orders = orders;
    }

    public record StartCheckoutDto(Guid UserId, string PaymentMethod);

    // B -> C -> D -> E
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartCheckoutDto dto, CancellationToken ct)
    {
        var basket = (await _basket.GetAsync(dto.UserId, ct)).Data!;
        var created = await _orders.CreateFromBasketAsync(dto.UserId, basket, ct); 
        if (created.Code != 200) return StatusCode(created.Code, created);

        await _basket.ClearAsync(dto.UserId, ct);
        var order = created.Data!;
        var paymentUrl = Url.ActionLink("Return", "Payment", new
        {
            orderId = order.Id,
            status = "Paid",
            amount = order.TotalAmount,
            userId = order.UserId,
            method = dto.PaymentMethod
        })!;

        return Ok(BaseResponse<object>.Success(new { orderId = order.Id, paymentUrl }));
    }
}
