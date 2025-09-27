namespace Trippio.Core.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendOtpEmailAsync(string to, string name, string otp);
        Task SendWelcomeEmailAsync(string to, string name);
    }
}

namespace Trippio.Core.Services
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendPhoneOtpAsync(string phoneNumber, string name, string otp);
    }
}