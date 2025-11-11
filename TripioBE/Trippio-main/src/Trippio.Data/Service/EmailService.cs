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

        /// <summary>
        /// Send order confirmation email after successful payment
        /// </summary>
        public async Task SendOrderConfirmationEmailAsync(string to, string customerName, OrderConfirmationEmailModel order)
        {
            var subject = $"Xác nhận đơn hàng #{order.OrderCode} - Trippio";
            
            // Format amount to VND
            var formattedAmount = order.TotalAmount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
            var orderDate = order.OrderDate.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
            
            // Build items table
            var itemsHtml = string.Empty;
            foreach (var item in order.Items)
            {
                var itemTotal = item.Quantity * item.Price;
                var itemPrice = item.Price.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
                var itemTotalFormatted = itemTotal.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
                
                itemsHtml += $@"
                    <tr style='border-bottom: 1px solid #e0e0e0;'>
                        <td style='padding: 12px; text-align: left;'>{item.BookingName}</td>
                        <td style='padding: 12px; text-align: center;'>{item.Quantity}</td>
                        <td style='padding: 12px; text-align: right;'>{itemPrice} ₫</td>
                        <td style='padding: 12px; text-align: right; font-weight: bold;'>{itemTotalFormatted} ₫</td>
                    </tr>";
            }

            // Determine status badge color
            var statusColor = order.PaymentStatus switch
            {
                "Paid" => "#22c55e",
                "Pending" => "#f59e0b",
                "Failed" => "#ef4444",
                "Refunded" => "#3b82f6",
                _ => "#6b7280"
            };
            var statusVietnamese = order.PaymentStatus switch
            {
                "Paid" => "Đã thanh toán",
                "Pending" => "Đang xử lý",
                "Failed" => "Thất bại",
                "Refunded" => "Đã hoàn tiền",
                _ => order.PaymentStatus
            };

            var htmlBody = $@"<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Xác nhận đơn hàng</title>
</head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", ""Roboto"", ""Oxygen"", ""Ubuntu"", ""Cantarell"", sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #f9fafb;'>
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #0b2749 0%, #0e315c 100%); color: white; padding: 40px 20px; text-align: center;'>
            <div style='font-size: 28px; font-weight: bold; margin-bottom: 8px;'>Trippio</div>
            <div style='font-size: 14px; opacity: 0.9;'>Xác nhận đơn hàng</div>
        </div>

        <!-- Main Content -->
        <div style='padding: 40px 20px; background-color: white; margin: 20px;'>
            <h2 style='color: #0b2749; margin-bottom: 8px;'>Chào {customerName}!</h2>
            <p style='color: #6b7280; margin-bottom: 24px;'>Cảm ơn bạn đã đặt hàng tại Trippio. Dưới đây là chi tiết đơn hàng của bạn:</p>

            <!-- Order Summary Box -->
            <div style='background-color: #f3f4f6; border-radius: 8px; padding: 20px; margin-bottom: 24px;'>
                <div style='margin-bottom: 12px;'>
                    <span style='color: #6b7280; font-size: 12px;'>MÃ ĐƠN HÀNG</span>
                    <div style='font-size: 24px; font-weight: bold; color: #0b2749;'>#{order.OrderCode}</div>
                </div>
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 12px;'>
                    <div>
                        <span style='color: #6b7280; font-size: 12px;'>NGÀY ĐẶT</span>
                        <div style='font-weight: bold; color: #0b2749;'>{orderDate}</div>
                    </div>
                    <div>
                        <span style='color: #6b7280; font-size: 12px;'>TRẠNG THÁI THANH TOÁN</span>
                        <div style='background-color: {statusColor}; color: white; padding: 4px 12px; border-radius: 4px; display: inline-block; font-weight: bold; margin-top: 4px;'>{statusVietnamese}</div>
                    </div>
                </div>
            </div>

            <!-- Items Table -->
            <div style='margin-bottom: 24px;'>
                <h3 style='color: #0b2749; margin-bottom: 16px; font-size: 16px;'>Chi tiết sản phẩm</h3>
                <table style='width: 100%; border-collapse: collapse; background-color: #f9fafb;'>
                    <thead>
                        <tr style='background-color: #e5e7eb;'>
                            <th style='padding: 12px; text-align: left; font-weight: bold; color: #0b2749;'>Sản phẩm</th>
                            <th style='padding: 12px; text-align: center; font-weight: bold; color: #0b2749;'>Số lượng</th>
                            <th style='padding: 12px; text-align: right; font-weight: bold; color: #0b2749;'>Đơn giá</th>
                            <th style='padding: 12px; text-align: right; font-weight: bold; color: #0b2749;'>Thành tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                </table>
            </div>

            <!-- Total Amount -->
            <div style='text-align: right; margin-bottom: 24px;'>
                <div style='font-size: 18px; font-weight: bold; color: #0b2749;'>
                    Tổng cộng: <span style='color: #22c55e;'>{formattedAmount} ₫</span>
                </div>
                <div style='font-size: 12px; color: #6b7280; margin-top: 8px;'>Phương thức thanh toán: {order.PaymentMethod}</div>
            </div>

            <!-- Next Steps -->
            <div style='background-color: #f0fdf4; border-left: 4px solid #22c55e; padding: 16px; margin-bottom: 24px; border-radius: 4px;'>
                <h3 style='color: #15803d; margin-top: 0;'>Bước tiếp theo</h3>
                <ul style='color: #15803d; margin: 8px 0; padding-left: 20px;'>
                    <li>Chúng tôi sẽ chuẩn bị dịch vụ của bạn</li>
                    <li>Bạn sẽ nhận được thông báo khi tất cả đã sẵn sàng</li>
                    <li>Vui lòng kiểm tra email và số điện thoại để cập nhật từ chúng tôi</li>
                </ul>
            </div>

            <!-- Support -->
            <div style='text-align: center; padding: 16px; background-color: #f3f4f6; border-radius: 8px;'>
                <p style='color: #6b7280; margin-bottom: 8px;'>Nếu bạn có câu hỏi, vui lòng liên hệ với chúng tôi:</p>
                <a href='mailto:support@trippio.vn' style='color: #0b2749; font-weight: bold; text-decoration: none;'>support@trippio.vn</a>
            </div>
        </div>

        <!-- Footer -->
        <div style='background-color: #0b2749; color: white; text-align: center; padding: 20px;'>
            <p style='margin: 0; font-size: 12px;'>© 2025 Trippio. Tất cả quyền được bảo lưu.</p>
            <p style='margin: 8px 0 0 0; font-size: 11px; opacity: 0.8;'>Email này được gửi vì bạn đã đặt hàng tại Trippio.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(to, subject, htmlBody);
            _logger.LogInformation("Order confirmation email sent to {To} for OrderCode {OrderCode}", to, order.OrderCode);
        }

    }
}

