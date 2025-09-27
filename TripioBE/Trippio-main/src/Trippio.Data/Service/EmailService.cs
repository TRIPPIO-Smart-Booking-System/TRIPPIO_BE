using Trippio.Core.Services;

namespace Trippio.Data.Service
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            // Implementation using your preferred email service (SendGrid, SMTP, etc.)
            // For now, just simulate sending
            await Task.Delay(100);
            Console.WriteLine($"Email sent to {to}: {subject}");
        }

        public async Task SendOtpEmailAsync(string to, string name, string otp)
        {
            var subject = "Xác thực tài khoản Trippio - Mã OTP";
            var htmlBody = $@"
                <html>
                <body>
                    <h2>Chào {name}!</h2>
                    <p>Chúc mừng bạn đã đăng ký thành công tài khoản Trippio!</p>
                    <p>Để hoàn tất quá trình đăng ký, vui lòng sử dụng mã OTP sau:</p>
                    <div style='background-color: #f4f4f4; padding: 20px; text-align: center; font-size: 24px; font-weight: bold; color: #333;'>
                        {otp}
                    </div>
                    <p>Mã OTP này sẽ hết hạn trong vòng 10 phút.</p>
                    <p>Nếu bạn không yêu cầu đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
                    <br>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Trippio</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, htmlBody);
        }

        public async Task SendWelcomeEmailAsync(string to, string name)
        {
            var subject = "Chào mừng bạn đến với Trippio!";
            var htmlBody = $@"
                <html>
                <body>
                    <h2>Chào mừng {name}!</h2>
                    <p>Tài khoản của bạn đã được xác thực thành công!</p>
                    <p>Bây giờ bạn có thể bắt đầu khám phá những trải nghiệm tuyệt vời trên Trippio.</p>
                    <p>Chúc bạn có những chuyến đi thú vị!</p>
                    <br>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Trippio</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, htmlBody);
        }
    }
}