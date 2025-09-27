using AutoMapper;
using Trippio.Api.Extensions;
using Trippio.Api.Service;
using Trippio.Core.ConfigOptions;
using Trippio.Core.Domain.Identity;
using Trippio.Core.Models.Auth;
using Trippio.Core.Models.System;
using Trippio.Core.SeedWorks.Constants;
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

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            RoleManager<AppRole> roleManager,
            IMapper mapper,
            IOptions<JwtTokenSettings> jwtTokenSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _roleManager = roleManager;
            _mapper = mapper;
            _jwtTokenSettings = jwtTokenSettings.Value;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticatedResult>> Login([FromBody] LoginRequest request)
        {
            // Authentication
            if (request == null)
            {
                return BadRequest("Invalid request");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || user.IsActive == false || user.LockoutEnabled)
            {
                return Unauthorized();
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, request.Password, false, true);

            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            // Authorization
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetPermissionsByUserIdAsync(user.Id.ToString());

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(UserClaims.Id, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Name, user.UserName),
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

            var userDto = _mapper.Map<Trippio.Core.Models.Auth.UserDto>(user);
            userDto.Roles = roles.ToList();

            return Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(_jwtTokenSettings.ExpireInHours),
                User = userDto
            });
        }

        private async Task<List<string>> GetPermissionsByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
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
    }
}