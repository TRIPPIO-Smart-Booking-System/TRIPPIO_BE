namespace Trippio.Core.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendOtpEmailAsync(string to, string name, string otp);
        Task SendWelcomeEmailAsync(string to, string name);
        Task SendPasswordResetOtpEmailAsync(string to, string name, string otp);
        Task SendOrderConfirmationEmailAsync(string to, string customerName, OrderConfirmationEmailModel order);
    }

    /// <summary>
    /// Model for order confirmation email
    /// </summary>
    public class OrderConfirmationEmailModel
    {
        public int OrderCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime OrderDate { get; set; }
        public List<OrderItemEmailModel> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = "PayOS";
    }

    public class OrderItemEmailModel
    {
        public string BookingName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}