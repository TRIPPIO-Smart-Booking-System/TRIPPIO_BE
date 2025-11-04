using Trippio.Core.Models;
using Trippio.Core.Models.Common;
using Trippio.Core.Models.Payment;

namespace Trippio.Core.Services
{
    public interface IPaymentService
    {
        Task<BaseResponse<IEnumerable<PaymentDto>>> GetAllAsync();
        Task<BaseResponse<IEnumerable<PaymentDto>>> GetByUserIdAsync(Guid userId);
        Task<BaseResponse<PaymentDto>> GetByIdAsync(Guid id);
        Task<BaseResponse<PaymentDto>> GetByOrderCodeAsync(long orderCode);
        Task<BaseResponse<IEnumerable<PaymentDto>>> GetByOrderIdAsync(int orderId);
        Task<BaseResponse<IEnumerable<PaymentDto>>> GetByBookingIdAsync(Guid bookingId);
        Task<BaseResponse<PaymentDto>> RefundPaymentAsync(Guid paymentId, decimal amount);
        Task<BaseResponse<PaymentDto>> UpdatePaymentStatusAsync(Guid id, string status);
        Task<BaseResponse<decimal>> GetTotalPaymentAmountAsync(DateTime from, DateTime to);
        Task<string> CreatePaymentUrlAsync(CreatePaymentRequest request, string returnUrl, string ipAddress);
        Task<BaseResponse<PaymentDto>> CreateAsync(CreatePaymentRequest request, CancellationToken ct = default);
        Task<BaseResponse<PaymentDto>> UpdateStatusByOrderCodeAsync(long orderCode, string status, CancellationToken ct = default);
    }
}