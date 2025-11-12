using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using System.Security.Claims;
using Trippio.Api.Service;
using Trippio.Core.Domain.Identity;

namespace Trippio.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<GoogleAuthController> _logger;
        private readonly IConfiguration _configuration;

        public GoogleAuthController(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            ILogger<GoogleAuthController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// POST /api/auth/google-verify
        /// Verify Google JWT token from Frontend
        /// Frontend sends Google JWT → Backend validates → creates/gets user → returns JWT
        /// </summary>
        [HttpPost("google-verify")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleVerify([FromBody] GoogleTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Token))
                {
                    _logger.LogWarning("Google verify called without token");
                    return BadRequest(new { isSuccess = false, message = "Token không được cung cấp" });
                }

                var googleClientId = _configuration["Authentication:Google:ClientId"];
                if (string.IsNullOrEmpty(googleClientId))
                {
                    _logger.LogError("Google Client ID not configured");
                    return StatusCode(500, new { isSuccess = false, message = "Lỗi cấu hình máy chủ" });
                }

                // ✅ Verify Google JWT signature
                GoogleJsonWebSignature.Payload payload;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(
                        request.Token,
                        new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = new[] { googleClientId }
                        }
                    );
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning($"Invalid Google token: {ex.Message}");
                    return Unauthorized(new { isSuccess = false, message = "Token Google không hợp lệ" });
                }

                _logger.LogInformation($"Google user verified: {payload.Email}");

                // ✅ Find or create user
                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    // Create new user from Google info
                    var firstName = payload.Name?.Split(' ').FirstOrDefault() ?? payload.Email.Split('@')[0];
                    var lastName = payload.Name?.Contains(' ') == true 
                        ? string.Join(" ", payload.Name.Split(' ').Skip(1)) 
                        : "";

                    user = new AppUser
                    {
                        Id = Guid.NewGuid(),
                        Email = payload.Email,
                        UserName = payload.Email,
                        FirstName = firstName,
                        LastName = lastName,
                        PhoneNumber = "+84900000000", // Default phone for Google users
                        GoogleId = payload.Subject,
                        Picture = payload.Picture,
                        OAuthProvider = "google",
                        IsEmailVerified = true,
                        IsActive = true,
                        DateCreated = DateTime.UtcNow,
                        Dob = DateTime.Now.AddYears(-25)
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        _logger.LogError($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                        return BadRequest(new { isSuccess = false, message = "Không thể tạo tài khoản" });
                    }

                    // After user is created, update with Google fields to ensure they're saved
                    user.GoogleId = payload.Subject;
                    user.Picture = payload.Picture;
                    user.OAuthProvider = "google";
                    user.IsEmailVerified = true;
                    user.LastLoginDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation($"Created new user from Google: {user.Email}, GoogleId: {user.GoogleId}");

                    // Assign default role (customer)
                    await _userManager.AddToRoleAsync(user, "customer");
                }
                else
                {
                    // Update existing user with Google info if not already linked
                    bool needsUpdate = false;
                    
                    if (string.IsNullOrEmpty(user.GoogleId))
                    {
                        user.GoogleId = payload.Subject;
                        needsUpdate = true;
                    }

                    if (string.IsNullOrEmpty(user.Picture) && !string.IsNullOrEmpty(payload.Picture))
                    {
                        user.Picture = payload.Picture;
                        needsUpdate = true;
                    }

                    if (user.OAuthProvider != "google")
                    {
                        user.OAuthProvider = "google";
                        needsUpdate = true;
                    }

                    if (!user.IsEmailVerified)
                    {
                        user.IsEmailVerified = true;
                        needsUpdate = true;
                    }

                    user.LastLoginDate = DateTime.UtcNow;
                    needsUpdate = true;

                    if (needsUpdate)
                    {
                        await _userManager.UpdateAsync(user);
                        _logger.LogInformation($"Updated existing user with Google info: {user.Email}, GoogleId: {user.GoogleId}");
                    }
                }

                // ✅ Generate JWT tokens
                var claims = await GetUserClaims(user);
                var accessToken = _tokenService.GenerateAccessToken(claims);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Save refresh token và update LastLoginDate
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                user.LastLoginDate = DateTime.UtcNow;
                var updateResult = await _userManager.UpdateAsync(user);
                
                if (!updateResult.Succeeded)
                {
                    _logger.LogWarning($"Failed to save refresh token for user {user.Email}");
                }

                _logger.LogInformation($"✅ Google login successful for user: {user.Email}, Id: {user.Id}");

                return Ok(new
                {
                    isSuccess = true,
                    message = "Đăng nhập Google thành công",
                    accessToken = accessToken,
                    refreshToken = refreshToken,
                    user = new
                    {
                        id = user.Id.ToString(),
                        email = user.Email,
                        userName = user.UserName,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        picture = user.Picture,
                        roles = (await _userManager.GetRolesAsync(user)).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Google verify error: {ex.Message}", ex);
                return StatusCode(500, new { isSuccess = false, message = "Lỗi xác thực Google" });
            }
        }

        private async Task<IEnumerable<Claim>> GetUserClaims(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("email_verified", user.IsEmailVerified ? "true" : "false"),
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
    }

    /// <summary>
    /// DTO: Request to verify Google token
    /// </summary>
    public class GoogleTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
