namespace Trippio.Core.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendOtpEmailAsync(string to, string name, string otp);
        Task SendWelcomeEmailAsync(string to, string name);
        Task SendPasswordResetOtpEmailAsync(string to, string name, string otp);
    }
}