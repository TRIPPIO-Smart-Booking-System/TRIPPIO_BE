using AutoMapper;
using Trippio.Api.Extensions;
using Trippio.Api.Service;
using Trippio.Core.ConfigOptions;
using Trippio.Core.Domain.Identity;
using Trippio.Core.Models.Auth;
using Trippio.Core.Models.System;
using Trippio.Core.SeedWorks.Constants;
using Trippio.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Trippio.Api.Controllers.AdminApi
{
    [Route("api/admin/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly JwtTokenSettings _jwtTokenSettings;
        private readonly IEmailService _emailService;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            RoleManager<AppRole> roleManager,
            IMapper mapper,
            IOptions<JwtTokenSettings> jwtTokenSettings,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _roleManager = roleManager;
            _mapper = mapper;
            _jwtTokenSettings = jwtTokenSettings.Value;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<OtpVerificationResult>> Login([FromBody] LoginRequest request)
        {
            // Authentication
            if (request == null)
            {
                return BadRequest("Invalid request");
            }

            // Find user by username or phone number
            AppUser? user = null;

            // Try to find by username first
            user = await _userManager.FindByNameAsync(request.UsernameOrPhone);

            // If not found, try to find by phone number
            if (user == null)
            {
                var users = _userManager.Users.Where(u => u.PhoneNumber == request.UsernameOrPhone).ToList();
                user = users.FirstOrDefault();
            }

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, request.Password, false, true);

            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Check if this is first login and email is not verified
            if (!user.IsEmailVerified)
            {
                // Generate OTP for email verification
                var otp = GenerateOtp();
                user.EmailOtp = otp;
                user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(10);
                await _userManager.UpdateAsync(user);

                // Send OTP email
                await _emailService.SendOtpEmailAsync(user.Email!, user.GetFullName()!, otp);

                return Ok(new OtpVerificationResult
                {
                    IsSuccess = false,
                    Message = "Email verification required for first login. OTP has been sent to your email.",
                    RequireEmailVerification = true,
                    RequirePhoneVerification = false,
                    Email = user.Email
                });
            }

            // Generate tokens
            var loginResponse = await GenerateLoginResponse(user);

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            user.IsFirstLogin = false;
            await _userManager.UpdateAsync(user);

            return Ok(new OtpVerificationResult
            {
                IsSuccess = true,
                Message = "Login successful",
                RequireEmailVerification = false,
                RequirePhoneVerification = false,
                LoginResponse = loginResponse
            });
        }

        private async Task<List<string>> GetPermissionsByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();
            var allPermissions = new List<RoleClaimsDto>();

            if (roles.Contains(Roles.Admin))
            {
                var types = typeof(Permissions).GetNestedTypes();
                foreach (var type in types)
                {
                    allPermissions.GetPermissions(type);
                }
                permissions.AddRange(allPermissions.Select(x => x.Value));
            }
            else
            {
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        var claims = await _roleManager.GetClaimsAsync(role);
                        var roleClaimsValues = claims.Select(x => x.Value).ToList();
                        permissions.AddRange(roleClaimsValues);
                    }
                }
            }
            return permissions.Distinct().ToList();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if username already exists
            var existingUserByUsername = await _userManager.FindByNameAsync(request.Username);
            if (existingUserByUsername != null)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Check if email already exists
            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Check if phone number already exists
            var existingUserByPhone = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == request.PhoneNumber);
            if (existingUserByPhone != null)
            {
                return BadRequest(new { message = "Phone number already exists" });
            }

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Username,
                NormalizedUserName = request.Username.ToUpper(),
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpper(),
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = false, // ✅ Chỉ active sau khi verify email
                DateCreated = DateTime.UtcNow,
                Dob = request.DateOfBirth,
                IsFirstLogin = true,
                IsEmailVerified = false,
                Balance = 0,
                LoyaltyAmountPerPost = 1000
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                // Assign default role (chỉ assign nếu role tồn tại)
                var customerRole = await _roleManager.FindByNameAsync("Customer");
                if (customerRole != null)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                }

                // Generate OTP for email verification only
                var otp = GenerateOtp();
                user.EmailOtp = otp;
                user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(10); // Email OTP expires in 10 minutes
                await _userManager.UpdateAsync(user);

                // Send OTP email
                await _emailService.SendOtpEmailAsync(user.Email!, user.GetFullName()!, otp);

                return Ok(new OtpVerificationResult
                {
                    IsSuccess = true, // ✅ Registration thành công
                    Message = "Registration successful. Please verify your email with the OTP sent to your email address to activate your account.",
                    RequireEmailVerification = true,
                    RequirePhoneVerification = false,
                });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult<LoginResponse>> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            if (user.IsEmailVerified)
            {
                return BadRequest(new { message = "Email already verified" });
            }

            if (user.EmailOtp != request.Otp || user.EmailOtpExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired OTP" });
            }

            // Verify email and activate account
            user.IsEmailVerified = true;
            user.EmailOtp = null;
            user.EmailOtpExpiry = null;
            user.IsActive = true; // ✅ Activate account sau khi verify email
            user.IsFirstLogin = false;
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(user.Email!, user.GetFullName()!);

            // Generate login response
            var loginResponse = await GenerateLoginResponse(user);

            return Ok(loginResponse);
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            if (user.IsEmailVerified)
            {
                return BadRequest(new { message = "Email already verified" });
            }

            // Generate new OTP
            var otp = GenerateOtp();
            user.EmailOtp = otp;
            user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            // Send OTP email
            await _emailService.SendOtpEmailAsync(user.Email!, user.GetFullName()!, otp);

            return Ok(new { message = "OTP has been resent to your email" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist for security reasons
                return Ok(new { message = "If your email exists in our system, you will receive a password reset OTP shortly." });
            }

            // Generate OTP for password reset
            var otp = GenerateOtp();
            user.PasswordResetOtp = otp;
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(10); // OTP expires in 10 minutes
            await _userManager.UpdateAsync(user);

            // Send OTP email for password reset
            await _emailService.SendPasswordResetOtpEmailAsync(user.Email!, user.GetFullName()!, otp);

            return Ok(new { message = "If your email exists in our system, you will receive a password reset OTP shortly." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            // Verify OTP
            if (user.PasswordResetOtp != request.Otp || user.PasswordResetOtpExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired OTP" });
            }

            // Reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to reset password", errors = result.Errors.Select(e => e.Description) });
            }

            // Clear OTP fields
            user.PasswordResetOtp = null;
            user.PasswordResetOtpExpiry = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Password has been reset successfully. You can now login with your new password." });
        }

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private async Task<LoginResponse> GenerateLoginResponse(AppUser user)
        {
            // Authorization
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetPermissionsByUserIdAsync(user.Id.ToString());

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(UserClaims.Id, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(UserClaims.FirstName, user.FirstName),
                new Claim(UserClaims.Roles, string.Join(";", roles)),
                new Claim(UserClaims.Permissions, JsonSerializer.Serialize(permissions)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await _userManager.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles.ToList();

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(_jwtTokenSettings.ExpireInHours),
                User = userDto
            };
        }
    }
}