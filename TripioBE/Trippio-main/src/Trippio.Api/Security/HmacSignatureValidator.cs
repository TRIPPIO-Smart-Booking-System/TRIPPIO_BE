using System.Security.Cryptography;
using System.Text;

namespace Trippio.Api.Security
{
    public static class HmacSignatureValidator
    {
        public static bool IsValid(string payload, string signatureHeader, string secret)
        {
            if (string.IsNullOrWhiteSpace(signatureHeader)) return false;
            var parts = signatureHeader.Split('=', 2);
            if (parts.Length != 2 || !parts[0].Equals("sha256", StringComparison.OrdinalIgnoreCase)) return false;

            var expected = parts[1];
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var actual = Convert.ToHexString(hash).ToLowerInvariant();
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(actual), Encoding.UTF8.GetBytes(expected));
        }
    }
}
