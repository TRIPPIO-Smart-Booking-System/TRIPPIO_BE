using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Trippio.Core.Services;

namespace Trippio.Data.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(
                _config["Smtp:FromName"] ?? "Trippio",
                _config["Smtp:FromEmail"] ?? ""
            ));
            emailMessage.To.Add(MailboxAddress.Parse(to));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                var host = _config["Smtp:Host"];
                var port = int.Parse(_config["Smtp:Port"] ?? "587");
                var user = _config["Smtp:User"];
                var pass = _config["Smtp:Pass"];
                var useSsl = bool.Parse(_config["Smtp:UseSsl"] ?? "false");

                var socketOptions = useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                await client.ConnectAsync(host, port, socketOptions);
                await client.AuthenticateAsync(user, pass);

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {To} with subject {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
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

        public async Task SendPasswordResetOtpEmailAsync(string to, string name, string otp)
        {
            var subject = "Đặt lại mật khẩu Trippio - Mã OTP";
            var htmlBody = $@"
                <html>
                <body>
                    <h2>Xin chào {name}!</h2>
                    <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản Trippio của bạn.</p>
                    <p>Để đặt lại mật khẩu, vui lòng sử dụng mã OTP sau:</p>
                    <div style='background-color: #f4f4f4; padding: 20px; text-align: center; font-size: 24px; font-weight: bold; color: #333;'>
                        {otp}
                    </div>
                    <p>Mã OTP này sẽ hết hạn trong vòng 10 phút.</p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này và mật khẩu của bạn sẽ không thay đổi.</p>
                    <br>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Trippio</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, htmlBody);
        }
    }
}
