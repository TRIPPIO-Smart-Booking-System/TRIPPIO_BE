//using Microsoft.Extensions.Options;
//using Trippio.Core.ConfigOptions;
//using Trippio.Core.Services;
//using Twilio;
//using Twilio.Rest.Api.V2010.Account;
//namespace Trippio.Data.Service
//{
//    public class SmsService : ISmsService
//    {
//        private readonly TwilioSettings _twilioSettings;

//        public SmsService(IOptions<TwilioSettings> twilioSettings)
//        {
//            _twilioSettings = twilioSettings.Value;
//            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
//        }

//        public async Task SendSmsAsync(string phoneNumber, string message)
//        {
//            var msg = await MessageResource.CreateAsync(
//                body: message,
//                from: new Twilio.Types.PhoneNumber(_twilioSettings.PhoneNumber),
//                to: new Twilio.Types.PhoneNumber(phoneNumber)
//            );

//            Console.WriteLine($"SMS sent to {phoneNumber}. SID: {msg.Sid}");
//        }

//        public async Task SendPhoneOtpAsync(string phoneNumber, string name, string otp)
//        {
//            var message = $"Xin chào {name}! Mã OTP xác thực tài khoản Trippio của bạn là: {otp}. Mã này có hiệu lực trong 5 phút.";
//            await SendSmsAsync(phoneNumber, message);
//        }
//    }
//}