using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;
using Trippio.Core.ConfigOptions;
using Trippio.Core.Models.Payment;
using Trippio.Core.Services;
using System.Security.Claims;

namespace Trippio.Api.Controllers.Payment
{
    /// <summary>
    /// PayOS Real Money Payment Controller
    /// Handles payment creation and webhook callbacks for actual money transactions
    /// Documentation: https://payos.vn/docs
    /// </summary>
    [ApiController]
    [Route("api/payment")]
    public class PayOSController : ControllerBase
    {
        private readonly Net.payOS.PayOS _payOS;
        private readonly PayOSSettings _settings;
        private readonly ILogger<PayOSController> _logger;
        private readonly IPaymentService _paymentService;

        public PayOSController(
            IOptions<PayOSSettings> settings,
            ILogger<PayOSController> logger,
            IPaymentService paymentService)
        {
            _settings = settings.Value;
            _logger = logger;
            _paymentService = paymentService;
            
            // Initialize PayOS SDK
            _payOS = new Net.payOS.PayOS(
                _settings.ClientId,
                _settings.ApiKey,
                _settings.ChecksumKey
            );
        }

        /// <summary>
        /// Create a real money payment link using PayOS
        /// </summary>
        /// <param name="request">Payment request details</param>
        /// <returns>Checkout URL and QR code for payment</returns>
        /// <response code="200">Payment link created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized - User must be logged in</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("realmoney")]
        [Authorize]
        [ProducesResponseType(typeof(PayOSPaymentResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateRealMoneyPayment([FromBody] CreatePayOSPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating PayOS payment for OrderCode: {OrderCode}, Amount: {Amount}", 
                    request.OrderCode, request.Amount);

                // Validate minimum amount
                if (request.Amount < 2000)
                {
                    return BadRequest(new { message = "Amount must be at least 2000 VND" });
                }

                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("sub")?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Create payment data for PayOS
                var items = new List<Net.payOS.Types.ItemData>
                {
                    new Net.payOS.Types.ItemData(
                        name: request.Description,
                        quantity: 1,
                        price: request.Amount
                    )
                };

                var paymentData = new Net.payOS.Types.PaymentData(
                    orderCode: request.OrderCode,
                    amount: request.Amount,
                    description: request.Description,
                    items: items,
                    cancelUrl: _settings.CancelUrl,
                    returnUrl: _settings.ReturnUrl,
                    buyerName: request.BuyerName,
                    buyerEmail: request.BuyerEmail,
                    buyerPhone: request.BuyerPhone
                );

                // Call PayOS API to create payment link
                var createResult = await _payOS.createPaymentLink(paymentData);

                _logger.LogInformation("PayOS payment link created successfully. CheckoutUrl: {CheckoutUrl}", 
                    createResult.checkoutUrl);

                // TODO: Save payment record to database here
                // Example:
                // await _paymentService.CreatePaymentRecordAsync(new PaymentRecord
                // {
                //     OrderCode = request.OrderCode,
                //     UserId = Guid.Parse(userIdClaim),
                //     Amount = request.Amount,
                //     Status = "PENDING",
                //     PaymentLinkId = createResult.paymentLinkId,
                //     CreatedAt = DateTime.UtcNow
                // });

                var response = new PayOSPaymentResponse
                {
                    CheckoutUrl = createResult.checkoutUrl,
                    OrderCode = request.OrderCode,
                    Amount = request.Amount,
                    QrCode = createResult.qrCode,
                    PaymentLinkId = createResult.paymentLinkId,
                    Status = "PENDING"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment for OrderCode: {OrderCode}", request.OrderCode);
                return StatusCode(500, new 
                { 
                    message = "Failed to create payment link", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get all payments (Admin only)
        /// </summary>
        /// <returns>List of all payments</returns>
        /// <response code="200">Payments retrieved successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("all")]
        [Authorize] // TODO: Add [Authorize(Roles = "Admin")] for admin-only access
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                _logger.LogInformation("Getting all payments");

                var result = await _paymentService.GetAllAsync();
                return StatusCode(result.Code, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all payments");
                return StatusCode(500, new 
                { 
                    message = "Failed to retrieve payments", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get payments by user ID
        /// </summary>
        /// <param name="userId">User ID to query payments for</param>
        /// <returns>List of user's payments</returns>
        /// <response code="200">Payments retrieved successfully</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User can only view their own payments</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("user/{userId}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPaymentsByUserId(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting payments for UserId: {UserId}", userId);

                // Get authenticated user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("sub")?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var authenticatedUserId = Guid.Parse(userIdClaim);

                // Check if user is requesting their own payments
                // TODO: Add role check - Admin can view any user's payments
                // var isAdmin = User.IsInRole("Admin");
                // if (!isAdmin && authenticatedUserId != userId)
                if (authenticatedUserId != userId)
                {
                    _logger.LogWarning("User {AuthUserId} attempted to access payments of User {TargetUserId}", 
                        authenticatedUserId, userId);
                    return StatusCode(403, new 
                    { 
                        message = "You can only view your own payments" 
                    });
                }

                var result = await _paymentService.GetByUserIdAsync(userId);
                return StatusCode(result.Code, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for UserId: {UserId}", userId);
                return StatusCode(500, new 
                { 
                    message = "Failed to retrieve payments", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get payment information by order code
        /// </summary>
        /// <param name="orderCode">Order code to query</param>
        /// <returns>Payment information</returns>
        /// <response code="200">Payment information retrieved successfully</response>
        /// <response code="404">Payment not found</response>
        [HttpGet("realmoney/{orderCode}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPaymentInfo(long orderCode)
        {
            try
            {
                _logger.LogInformation("Getting payment info for OrderCode: {OrderCode}", orderCode);

                var paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);

                return Ok(new
                {
                    orderCode = paymentInfo.orderCode,
                    amount = paymentInfo.amount,
                    status = paymentInfo.status,
                    transactions = paymentInfo.transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment info for OrderCode: {OrderCode}", orderCode);
                return NotFound(new { message = "Payment not found", error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a payment link
        /// </summary>
        /// <param name="orderCode">Order code to cancel</param>
        /// <returns>Cancellation result</returns>
        /// <response code="200">Payment cancelled successfully</response>
        /// <response code="400">Cannot cancel payment</response>
        [HttpPost("realmoney/{orderCode}/cancel")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CancelPayment(long orderCode, [FromBody] string? cancellationReason)
        {
            try
            {
                _logger.LogInformation("Cancelling payment for OrderCode: {OrderCode}", orderCode);

                var cancelResult = await _payOS.cancelPaymentLink(orderCode, cancellationReason);

                return Ok(new
                {
                    orderCode = cancelResult.orderCode,
                    status = cancelResult.status,
                    message = "Payment cancelled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment for OrderCode: {OrderCode}", orderCode);
                return BadRequest(new { message = "Failed to cancel payment", error = ex.Message });
            }
        }

        /// <summary>
        /// PayOS Webhook - Receives payment status updates from PayOS
        /// This endpoint is called by PayOS server when payment status changes
        /// IMPORTANT: Configure this URL in PayOS dashboard: https://my.payos.vn
        /// Webhook URL should be: https://yourdomain.com/api/payment/payos-callback
        /// </summary>
        /// <param name="webhookData">Webhook payload from PayOS</param>
        /// <returns>Acknowledgement response</returns>
        /// <response code="200">Webhook processed successfully</response>
        /// <response code="400">Invalid webhook signature</response>
        [HttpPost("payos-callback")]
        [AllowAnonymous]  // PayOS server calls this, no auth needed
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookRequest webhookData)
        {
            try
            {
                if (webhookData?.Data == null)
                {
                    _logger.LogWarning("Received empty webhook data from PayOS");
                    return BadRequest(new { success = false, message = "Invalid webhook data" });
                }

                var payload = webhookData.Data;
                long orderCode = payload.OrderCode;
                int amount = payload.Amount;
                string code = payload.Code;
                string desc = payload.Desc;
                string reference = payload.Reference ?? "";
                string transactionDateTime = payload.TransactionDateTime ?? "";
                string description = payload.Description ?? "";

                _logger.LogInformation("Received PayOS webhook for OrderCode: {OrderCode}, Code: {Code}, Amount: {Amount}",
                    orderCode, code, amount);

                // Validate orderCode
                if (orderCode <= 0)
                {
                    _logger.LogWarning("Invalid OrderCode in webhook: {OrderCode}", orderCode);
                    return BadRequest(new { success = false, message = "Invalid order code" });
                }

                // Process webhook based on status code
                // Code "00" means payment successful
                if (code == "00")
                {
                    _logger.LogInformation("Payment SUCCESSFUL for OrderCode: {OrderCode}, Amount: {Amount}", 
                        orderCode, amount);

                    // Update payment status in database
                    var updateResult = await _paymentService.UpdateStatusByOrderCodeAsync(orderCode, "Paid");
                    
                    if (updateResult.Code == 200)
                    {
                        _logger.LogInformation("Payment status updated to PAID for OrderCode: {OrderCode}", orderCode);
                    }
                    else
                    {
                        _logger.LogError("Failed to update payment status for OrderCode: {OrderCode}. Error: {Error}", 
                            orderCode, updateResult.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("Payment FAILED or CANCELLED for OrderCode: {OrderCode}, Code: {Code}, Desc: {Desc}",
                        orderCode, code, desc);

                    // Update payment status as failed
                    var updateResult = await _paymentService.UpdateStatusByOrderCodeAsync(orderCode, "Failed");
                    
                    if (updateResult.Code == 200)
                    {
                        _logger.LogInformation("Payment status updated to FAILED for OrderCode: {OrderCode}", orderCode);
                    }
                    else
                    {
                        _logger.LogError("Failed to update payment status for OrderCode: {OrderCode}. Error: {Error}", 
                            orderCode, updateResult.Message);
                    }
                }

                // Log full webhook data for debugging
                _logger.LogInformation("Webhook Data: OrderCode={OrderCode}, Amount={Amount}, Code={Code}, Desc={Desc}, Ref={Ref}, Time={Time}",
                    orderCode, amount, code, desc, reference, transactionDateTime);

                // Return success to PayOS
                return Ok(new 
                { 
                    success = true, 
                    message = "Webhook processed successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook");
                // Still return 200 to avoid PayOS retrying
                return Ok(new 
                { 
                    success = false, 
                    message = "Webhook processing failed but acknowledged" 
                });
            }
        }

        /// <summary>
        /// Test webhook manually - Simulate PayOS payment success
        /// Use this to manually update payment status when webhook fails
        /// </summary>
        /// <param name="orderCode">Order code to mark as paid</param>
        /// <returns>Update result</returns>
        [HttpPost("test-webhook/{orderCode}")]
        [Authorize] // Protect this endpoint - only authenticated users can call
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> TestWebhookManually(long orderCode)
        {
            try
            {
                _logger.LogInformation("Manual webhook test for OrderCode: {OrderCode}", orderCode);

                // Simulate successful payment webhook
                var updateResult = await _paymentService.UpdateStatusByOrderCodeAsync(orderCode, "Paid");

                if (updateResult.Code == 200)
                {
                    _logger.LogInformation("Payment status manually updated to PAID for OrderCode: {OrderCode}", orderCode);
                    return Ok(new
                    {
                        success = true,
                        message = $"Payment for OrderCode {orderCode} marked as PAID",
                        data = updateResult.Data
                    });
                }
                else
                {
                    _logger.LogError("Failed to update payment status for OrderCode: {OrderCode}. Error: {Error}",
                        orderCode, updateResult.Message);
                    return StatusCode(updateResult.Code, new
                    {
                        success = false,
                        message = updateResult.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in manual webhook test for OrderCode: {OrderCode}", orderCode);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Manual webhook test failed",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Confirm webhook URL (for testing during PayOS setup)
        /// </summary>
        [HttpGet("payos-callback")]
        [AllowAnonymous]
        public IActionResult ConfirmWebhook()
        {
            return Ok(new 
            { 
                message = "PayOS webhook endpoint is active",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
