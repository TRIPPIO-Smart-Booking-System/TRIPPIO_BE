using Trippio.Core.Services;

namespace Trippio.Data.Service
{
    public class SmsService : ISmsService
    {
        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            // Implementation using your preferred SMS service (Twilio, AWS SNS, etc.)
            // For now, just simulate sending
            await Task.Delay(100);
            Console.WriteLine($"SMS sent to {phoneNumber}: {message}");
        }

        public async Task SendPhoneOtpAsync(string phoneNumber, string name, string otp)
        {
            var message = $"Xin chao {name}! Ma OTP xac thuc tai khoan Trippio cua ban la: {otp}. Ma nay co hieu luc trong 5 phut.";
            await SendSmsAsync(phoneNumber, message);
        }
    }
}