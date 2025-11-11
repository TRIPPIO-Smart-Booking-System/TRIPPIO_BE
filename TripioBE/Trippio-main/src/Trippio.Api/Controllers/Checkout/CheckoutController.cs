using Microsoft.AspNetCore.Mvc;
using Trippio.Core.Models.Common;
using Trippio.Core.Services;
using Trippio.Core.Models.Payment;
using Trippio.Core.ConfigOptions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Net.payOS;
using Net.payOS.Types;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Trippio.Core.Domain.Identity;

namespace Trippio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly IBasketService _basket;
    private readonly IOrderService _orders;
    private readonly IPaymentService _payments;
    private readonly PayOSSettings _payOSSettings;
    private readonly Net.payOS.PayOS _payOS;
    private readonly ILogger<CheckoutController> _logger;
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;

    public CheckoutController(
        IBasketService basket, 
        IOrderService orders, 
        IPaymentService payments, 
        IOptions<PayOSSettings> payOSSettings,
        ILogger<CheckoutController> logger,
        IEmailService emailService,
        UserManager<AppUser> userManager)
    {
        _basket = basket; 
        _orders = orders; 
        _payments = payments; 
        _payOSSettings = payOSSettings.Value;
        _logger = logger;
        _emailService = emailService;
        _userManager = userManager;
        
        // Initialize PayOS SDK
        _payOS = new Net.payOS.PayOS(
            _payOSSettings.ClientId,
            _payOSSettings.ApiKey,
            _payOSSettings.ChecksumKey
        );
    }

    /// <summary>
    /// Request DTO for starting checkout process
    /// </summary>
    /// <param name="UserId">User ID (optional, will use authenticated user if not provided)</param>
    /// <param name="BuyerName">Buyer's name (optional)</param>
    /// <param name="BuyerEmail">Buyer's email (optional)</param>
    /// <param name="BuyerPhone">Buyer's phone (optional)</param>
    /// <param name="Platform">Platform type: "web" or "mobile" (default: "web")</param>
    public record StartCheckoutDto(
        Guid? UserId = null,
        string? BuyerName = null,
        string? BuyerEmail = null,
        string? BuyerPhone = null,
        string? Platform = "web"
    );

    /// <summary>
    /// Start checkout process: Create order from basket and generate PayOS payment link
    /// Luồng: Basket (Redis) -> Create Order (DB) -> Clear Basket -> Create PayOS Payment Link
    /// </summary>
    /// <param name="dto">Checkout request data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Order details and PayOS checkout URL</returns>
    /// <response code="200">Checkout successful - Returns order ID and payment URL</response>
    /// <response code="400">Bad request - Basket empty or invalid data</response>
    /// <response code="401">Unauthorized - User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("start")]
    [ProducesResponseType(typeof(BaseResponse<PayOSPaymentResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Start([FromBody] StartCheckoutDto dto, CancellationToken ct)
    {
        try
        {
            // Get authenticated user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(BaseResponse<object>.Error("User not authenticated", 401));
            }

            var userId = dto.UserId ?? Guid.Parse(userIdClaim);

            _logger.LogInformation("Starting checkout for UserId: {UserId}", userId);

            // Step 1: Get basket from Redis
            var basketResponse = await _basket.GetAsync(userId, ct);
            if (basketResponse.Code != 200 || basketResponse.Data == null)
            {
                return BadRequest(BaseResponse<object>.Error("Basket not found or empty", 400));
            }

            var basket = basketResponse.Data;
            
            if (basket.Items.Count == 0)
            {
                return BadRequest(BaseResponse<object>.Error("Basket is empty", 400));
            }

            // Validate minimum amount (PayOS requires minimum 2000 VND)
            if (basket.Total < 2000)
            {
                return BadRequest(BaseResponse<object>.Error("Total amount must be at least 2000 VND", 400));
            }

            _logger.LogInformation("Basket retrieved: {ItemCount} items, Total: {Total} VND", 
                basket.Items.Count, basket.Total);

            // Step 2: Create Order from basket
            var orderResponse = await _orders.CreateFromBasketAsync(userId, ct);
            if (orderResponse.Code != 200 || orderResponse.Data == null)
            {
                return StatusCode(orderResponse.Code, orderResponse);
            }

            var order = orderResponse.Data;
            _logger.LogInformation("Order created successfully: OrderId={OrderId}, Amount={Amount}", 
                order.Id, order.TotalAmount);

            // Step 3: Clear basket from Redis after successful order creation
            await _basket.ClearAsync(userId, ct);
            _logger.LogInformation("Basket cleared for UserId: {UserId}", userId);

            // Step 4: Use OrderId as OrderCode for PayOS
            // ✅ FIXED: OrderCode = OrderId (auto-increment: 1, 2, 3, 4...)
            // This ensures OrderCode is sequential and predictable
            long orderCode = order.Id;
            
            _logger.LogInformation("✅ Using OrderId as OrderCode: {OrderCode} for OrderId: {OrderId}", orderCode, order.Id);

            // Prepare items for PayOS
            var paymentItems = new List<ItemData>
            {
                new ItemData(
                    name: $"Order #{order.Id} - {basket.Items.Count} items",
                    quantity: 1,
                    price: (int)order.TotalAmount  // Convert decimal to int (VND)
                )
            };

            // Determine return/cancel URLs based on platform
            var isMobile = dto.Platform?.ToLowerInvariant() == "mobile";
            var returnUrl = isMobile ? _payOSSettings.MobileReturnUrl : _payOSSettings.WebReturnUrl;
            var cancelUrl = isMobile ? _payOSSettings.MobileCancelUrl : _payOSSettings.WebCancelUrl;

            _logger.LogInformation("Creating payment for platform: {Platform}, ReturnUrl: {ReturnUrl}", 
                dto.Platform ?? "web", returnUrl);

            // Create PayOS payment data
            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: (int)order.TotalAmount,  // PayOS expects int amount in VND
                description: $"Payment for Order #{order.Id}",
                items: paymentItems,
                cancelUrl: cancelUrl,
                returnUrl: returnUrl,
                buyerName: dto.BuyerName,
                buyerEmail: dto.BuyerEmail,
                buyerPhone: dto.BuyerPhone
            );

            _logger.LogInformation("Creating PayOS payment link for OrderCode: {OrderCode}", orderCode);

            // Step 4b: Create PayOS payment link
            Net.payOS.Types.CreatePaymentResult createResult = null!;
            try
            {
                // Call PayOS API to create payment link
                createResult = await _payOS.createPaymentLink(paymentData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create PayOS payment for OrderCode: {OrderCode}", orderCode);
                throw;
            }

            _logger.LogInformation("PayOS payment link created successfully. CheckoutUrl: {CheckoutUrl}, OrderCode: {OrderCode}", 
                createResult.checkoutUrl, orderCode);

            // ✅ OrderCode = OrderId (already set above)
            // No need for mapping - they are the same value
            var payOSOrderCode = orderCode;  // Explicitly assign for clarity
            
            // Step 5: Save payment record to database with OrderCode = OrderId
            var paymentRequest = new CreatePaymentRequest
            {
                UserId = userId,
                OrderId = order.Id,
                Amount = order.TotalAmount,
                PaymentMethod = "PayOS",
                PaymentLinkId = createResult.paymentLinkId,
                OrderCode = payOSOrderCode  // ✅ OrderCode = OrderId
            };

            var paymentResponse = await _payments.CreateAsync(paymentRequest, ct);
            if (paymentResponse.Code != 200)
            {
                _logger.LogError("❌ CRITICAL: Failed to save payment record for OrderId: {OrderId}, PayOS OrderCode: {OrderCode}. Error: {Error}", 
                    order.Id, payOSOrderCode, paymentResponse.Message);
                // Continue anyway - payment link is already created
            }
            else
            {
                _logger.LogInformation("✅ Payment record saved successfully - OrderId: {OrderId}, PaymentId: {PaymentId}, OrderCode: {OrderCode}", 
                    order.Id, paymentResponse.Data?.Id, payOSOrderCode);
            }

            // Step 6: Return response with order details and payment URL
            var response = new PayOSPaymentResponse
            {
                CheckoutUrl = createResult.checkoutUrl,
                OrderCode = payOSOrderCode,  // ✅ Universal identifier for Order & Payment
                Amount = (int)order.TotalAmount,
                QrCode = createResult.qrCode,
                PaymentLinkId = createResult.paymentLinkId,
                Status = "PENDING"
            };

            _logger.LogInformation("✅ Checkout completed - OrderId: {OrderId}, OrderCode: {OrderCode}, Amount: {Amount}", 
                order.Id, payOSOrderCode, order.TotalAmount);

            return Ok(BaseResponse<PayOSPaymentResponse>.Success(response, 
                $"Order #{payOSOrderCode} created successfully. OrderCode: {payOSOrderCode}. Please complete payment."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during checkout process");
            return StatusCode(500, BaseResponse<object>.Error(
                $"Checkout failed: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Get checkout/payment status by order code
    /// </summary>
    /// <param name="orderCode">Order code (Order.Id)</param>
    /// <returns>Payment information</returns>
    [HttpGet("status/{orderCode}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCheckoutStatus(long orderCode)
    {
        try
        {
            _logger.LogInformation("Getting checkout status for OrderCode: {OrderCode}", orderCode);

            var paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);

            return Ok(BaseResponse<object>.Success(new
            {
                orderCode = paymentInfo.orderCode,
                amount = paymentInfo.amount,
                status = paymentInfo.status,
                transactions = paymentInfo.transactions
            }, "Payment information retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting checkout status for OrderCode: {OrderCode}", orderCode);
            return NotFound(BaseResponse<object>.Error("Payment not found", 404));
        }
    }

    /// <summary>
    /// Cancel a checkout/payment
    /// </summary>
    /// <param name="orderCode">Order code (Order.Id) to cancel</param>
    /// <param name="cancellationReason">Reason for cancellation (optional)</param>
    /// <returns>Cancellation result</returns>
    [HttpPost("cancel/{orderCode}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CancelCheckout(long orderCode, [FromBody] string? cancellationReason = null)
    {
        try
        {
            _logger.LogInformation("Cancelling checkout for OrderCode: {OrderCode}", orderCode);

            var cancelResult = await _payOS.cancelPaymentLink(orderCode, cancellationReason);

            // TODO: Update order status to cancelled in database
            // await _orders.UpdateStatusAsync((int)orderCode, "Cancelled");

            return Ok(BaseResponse<object>.Success(new
            {
                orderCode = cancelResult.orderCode,
                status = cancelResult.status
            }, "Checkout cancelled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling checkout for OrderCode: {OrderCode}", orderCode);
            return BadRequest(BaseResponse<object>.Error($"Failed to cancel checkout: {ex.Message}", 400));
        }
    }

    /// <summary>
    /// Helper method to send order confirmation email
    /// Used after successful payment from checkout or webhook
    /// </summary>
    private async Task SendOrderConfirmationEmailAsync(long orderCode, Guid? userId)
    {
        try
        {
            if (userId == null || userId == Guid.Empty)
            {
                _logger.LogWarning("⚠️ Cannot send email: UserId is missing for OrderCode {OrderCode}", orderCode);
                return;
            }

            // Get user details
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("⚠️ Cannot send email: User or email not found for OrderCode {OrderCode}", orderCode);
                return;
            }

            // Get order details using OrderService (OrderCode = OrderId)
            var orderResult = await _orders.GetByIdAsync((int)orderCode);
            if (orderResult.Code != 200 || orderResult.Data == null)
            {
                _logger.LogWarning("⚠️ Cannot send email: Order not found for OrderCode {OrderCode}", orderCode);
                return;
            }

            var order = orderResult.Data;

            // Build email model
            var emailModel = new OrderConfirmationEmailModel
            {
                OrderCode = (int)orderCode,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                PaymentStatus = "Paid",
                PaymentMethod = "PayOS",
                Items = order.OrderItems?.Select(oi => new OrderItemEmailModel
                {
                    BookingName = oi.BookingName ?? "Dịch vụ Trippio",
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList() ?? new List<OrderItemEmailModel>()
            };

            // Send email
            await _emailService.SendOrderConfirmationEmailAsync(
                user.Email,
                user.FirstName ?? user.UserName ?? "Quý khách",
                emailModel
            );

            _logger.LogInformation("✅ Order confirmation email sent successfully to {Email} for OrderCode {OrderCode}", 
                user.Email, orderCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending order confirmation email for OrderCode {OrderCode}: {Message}", 
                orderCode, ex.Message);
            // Don't throw - email failure shouldn't block payment process
        }
    }
}


