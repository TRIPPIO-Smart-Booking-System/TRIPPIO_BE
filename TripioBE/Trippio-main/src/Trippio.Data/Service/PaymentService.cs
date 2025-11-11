using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Models.Common;
using Trippio.Core.Models.Payment;
using Trippio.Core.Repositories;
using Trippio.Core.SeedWorks;
using Trippio.Core.Services;
using Trippio.Data.Repositories;
using Trippio.Core.ConfigOptions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Trippio.Data.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PaymentService(
            IPaymentRepository paymentRepo,
            IOrderRepository orderRepo,
            IBookingRepository bookingRepo,
            IUnitOfWork uow,
            IMapper mapper
            )
        {
            _paymentRepo = paymentRepo;
            _orderRepo = orderRepo;
            _bookingRepo = bookingRepo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<BaseResponse<IEnumerable<PaymentDto>>> GetAllAsync()
        {
            var payments = await _paymentRepo.GetAllAsync();
            var data = payments.Select(p => _mapper.Map<PaymentDto>(p));
            return BaseResponse<IEnumerable<PaymentDto>>.Success(data, "Payments retrieved successfully");
        }

        public async Task<BaseResponse<IEnumerable<PaymentDto>>> GetByUserIdAsync(Guid userId)
        {
            var payments = await _paymentRepo.GetByUserIdAsync(userId);
            var data = payments.Select(p => _mapper.Map<PaymentDto>(p));
            return BaseResponse<IEnumerable<PaymentDto>>.Success(data);
        }

        public async Task<BaseResponse<PaymentDto>> GetByIdAsync(Guid id)
        {
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null)
                return BaseResponse<PaymentDto>.NotFound("Payment not found");

            return BaseResponse<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment));
        }

        public async Task<BaseResponse<PaymentDto>> GetByOrderCodeAsync(long orderCode)
        {
            var payment = await _paymentRepo.GetByOrderCodeAsync(orderCode);
            if (payment == null)
                return BaseResponse<PaymentDto>.NotFound($"Payment not found for OrderCode: {orderCode}");

            return BaseResponse<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment));
        }

        public async Task<BaseResponse<IEnumerable<PaymentDto>>> GetByOrderIdAsync(int orderId)
        {
            var payments = await _paymentRepo.GetByOrderIdAsync(orderId);
            var data = payments.Select(p => _mapper.Map<PaymentDto>(p));
            return BaseResponse<IEnumerable<PaymentDto>>.Success(data);
        }

        public async Task<BaseResponse<IEnumerable<PaymentDto>>> GetByBookingIdAsync(Guid bookingId)
        {
            var payments = await _paymentRepo.GetByBookingIdAsync(bookingId);
            var data = payments.Select(p => _mapper.Map<PaymentDto>(p));
            return BaseResponse<IEnumerable<PaymentDto>>.Success(data);
        }

        public async Task<BaseResponse<PaymentDto>> UpdatePaymentStatusAsync(Guid id, string status)
        {
            if (!Enum.TryParse(status, true, out PaymentStatus parsedStatus))
                return BaseResponse<PaymentDto>.Error($"Unknown status: {status}", 400);

            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null)
                return BaseResponse<PaymentDto>.NotFound("Payment not found");

            await _uow.BeginTransactionAsync();
            try
            {
                payment.Status = parsedStatus;
                payment.ModifiedDate = DateTime.UtcNow;

                if (payment.OrderId.HasValue)
                {
                    var order = await _orderRepo.GetByIdAsync(payment.OrderId.Value);
                    if (order != null)
                    {
                        if (parsedStatus == PaymentStatus.Paid)
                            order.Status = OrderStatus.Confirmed;
                        else if (parsedStatus is PaymentStatus.Failed or PaymentStatus.Refunded)
                            order.Status = OrderStatus.Cancelled;
                    }
                }

                if (payment.BookingId.HasValue)
                {
                    var booking = await _bookingRepo.GetWithDetailsAsync(payment.BookingId.Value)
                                  ?? await _bookingRepo.GetByIdAsync(payment.BookingId.Value);
                    if (booking != null)
                    {
                        if (parsedStatus == PaymentStatus.Paid)
                            booking.Status = "Confirmed";
                        else if (parsedStatus is PaymentStatus.Failed or PaymentStatus.Refunded)
                            booking.Status = "Cancelled";

                        booking.ModifiedDate = DateTime.UtcNow;

                    }
                }

                await _uow.CompleteAsync();
                await _uow.CommitTransactionAsync();

                return BaseResponse<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment), "Payment status updated");
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<BaseResponse<PaymentDto>> RefundPaymentAsync(Guid paymentId, decimal amount)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment == null) return BaseResponse<PaymentDto>.NotFound("Payment not found");
            if (payment.Status != PaymentStatus.Paid) return BaseResponse<PaymentDto>.Error("Only paid payments can be refunded", 409);
            if (amount > payment.Amount) return BaseResponse<PaymentDto>.Error("Refund amount exceeds payment total", 400);

            await _uow.BeginTransactionAsync();
            try
            {
                payment.Status = PaymentStatus.Refunded;
                payment.ModifiedDate = DateTime.UtcNow;

                if (payment.OrderId.HasValue)
                {
                    var order = await _orderRepo.GetByIdAsync(payment.OrderId.Value);
                    if (order != null) order.Status = OrderStatus.Cancelled;
                }

                if (payment.BookingId.HasValue)
                {
                    var booking = await _bookingRepo.GetByIdAsync(payment.BookingId.Value);
                    if (booking != null)
                    {
                        booking.Status = "Cancelled";
                        booking.ModifiedDate = DateTime.UtcNow;
                    }
                }

                await _uow.CompleteAsync();
                await _uow.CommitTransactionAsync();

                return BaseResponse<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment), "Payment refunded");
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<BaseResponse<decimal>> GetTotalPaymentAmountAsync(DateTime from, DateTime to)
        {
            if (to < from)
                return BaseResponse<decimal>.Error("End date must be after start date", 400);

            var total = await _paymentRepo.GetTotalPaymentAmountAsync(from, to);
            return BaseResponse<decimal>.Success(total, "Total payment amount calculated");
        }

        public async Task<string> CreatePaymentUrlAsync(CreatePaymentRequest request, string returnUrl, string ipAddress)
        {
            // VNPay payment method has been discontinued.
            // Please use PayOS for real money payments instead.
            throw new NotSupportedException("VNPay payment method is no longer supported. Please use PayOS instead.");
        }

        public async Task<BaseResponse<PaymentDto>> CreateAsync(CreatePaymentRequest request, CancellationToken ct = default)
        {
            try
            {
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    OrderId = request.OrderId,
                    BookingId = request.BookingId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    Status = PaymentStatus.Pending,
                    PaidAt = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow,
                    PaymentLinkId = request.PaymentLinkId,
                    OrderCode = request.OrderCode
                };

                await _paymentRepo.Add(payment);
                await _uow.CompleteAsync();

                return BaseResponse<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment), "Payment record created successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<PaymentDto>.Error($"Failed to create payment: {ex.Message}", 500);
            }
        }

        public async Task<BaseResponse<PaymentDto>> UpdateStatusByOrderCodeAsync(long orderCode, string status, CancellationToken ct = default)
        {
            try
            {
                if (!Enum.TryParse(status, true, out PaymentStatus parsedStatus))
                    return BaseResponse<PaymentDto>.Error($"Unknown status: {status}", 400);

                // Find payment by OrderCode - FIXED: Use direct query instead of GetAllAsync
                var payment = await _paymentRepo.GetByOrderCodeAsync(orderCode);

                if (payment == null)
                    return BaseResponse<PaymentDto>.NotFound($"Payment not found for OrderCode: {orderCode}");

                await _uow.BeginTransactionAsync();
                try
                {
                    payment.Status = parsedStatus;
                    payment.ModifiedDate = DateTime.UtcNow;
                    
                    if (parsedStatus == PaymentStatus.Paid)
                    {
                        payment.PaidAt = DateTime.UtcNow;
                    }

                    // ✅ FIXED: Explicitly update payment in repository
                    _paymentRepo.Update(payment);

                    // Update related Order status
                    if (payment.OrderId.HasValue)
                    {
                        var order = await _orderRepo.GetByIdAsync(payment.OrderId.Value);
                        if (order != null)
                        {
                            if (parsedStatus == PaymentStatus.Paid)
                                order.Status = OrderStatus.Confirmed;
                            else if (parsedStatus is PaymentStatus.Failed or PaymentStatus.Refunded)
                                order.Status = OrderStatus.Cancelled;
                        }
                    }

                    // Update related Booking status
                    if (payment.BookingId.HasValue)
                    {
                        var booking = await _bookingRepo.GetWithDetailsAsync(payment.BookingId.Value)
                                      ?? await _bookingRepo.GetByIdAsync(payment.BookingId.Value);
                        if (booking != null)
                        {
                            if (parsedStatus == PaymentStatus.Paid)
                                booking.Status = "Confirmed";
                            else if (parsedStatus is PaymentStatus.Failed or PaymentStatus.Refunded)
                                booking.Status = "Cancelled";

                            booking.ModifiedDate = DateTime.UtcNow;
                        }
                    }

                    await _uow.CompleteAsync();
                    await _uow.CommitTransactionAsync();

                    return BaseResponse<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment), "Payment status updated successfully");
                }
                catch
                {
                    await _uow.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return BaseResponse<PaymentDto>.Error($"Failed to update payment status: {ex.Message}", 500);
            }
        }
    }
}