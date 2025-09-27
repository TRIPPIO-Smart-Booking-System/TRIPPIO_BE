using Trippio.Api.Service;
using Trippio.Core.ConfigOptions;
using Trippio.Core.Domain.Identity;
using Trippio.Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Trippio.Api.Controllers.AdminApi
{
    [Route("api/admin/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly JwtTokenSettings _jwtTokenSettings;

        public TokenController(UserManager<AppUser> userManager, ITokenService tokenService, IOptions<JwtTokenSettings> jwtTokenSettings)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _jwtTokenSettings = jwtTokenSettings.Value;
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthenticatedResult>> Refresh(TokenRequest tokenRequest)
        {
            if (tokenRequest == null)
            {
                return BadRequest("Invalid client request");
            }

            string accessToken = tokenRequest.AccessToken;
            string refreshToken = tokenRequest.RefreshToken;
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);

            if (principal == null || principal.Identity == null || principal.Identity.Name == null)
            {
                return BadRequest("Invalid token");
            }

            var userName = principal.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid client request");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims) ?? string.Empty;
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return Ok(new AuthenticatedResult()
            {
                RefreshToken = newRefreshToken,
                AccessToken = newAccessToken,
                ExpiresAt = DateTime.UtcNow.AddHours(_jwtTokenSettings.ExpireInHours),
                TokenType = "Bearer"
            });
        }

        [HttpPost]
        [Authorize]
        [Route("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("Invalid user identity.");

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound("User not found.");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return StatusCode(500, "Failed to update user token.");

            return NoContent();
        }
    }
}