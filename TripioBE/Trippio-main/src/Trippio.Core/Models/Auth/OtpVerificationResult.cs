namespace Trippio.Core.Models.Auth
{
    public class OtpVerificationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequireEmailVerification { get; set; }
        public bool RequirePhoneVerification { get; set; }
        public LoginResponse? LoginResponse { get; set; }

    }
}