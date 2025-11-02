using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trippio.Api.Extensions;
using Trippio.Core.Domain.Identity;
using Trippio.Core.Models.System;

namespace Trippio.Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public UserProfileController(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Get current authenticated user information
        /// GET /api/user/me
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var userDto = _mapper.Map<AppUser, UserDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.ToList();

            return Ok(userDto);
        }

        /// <summary>
        /// Get current user's basic profile info
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult> GetProfile()
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                id = user.Id,
                userName = user.UserName,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phoneNumber = user.PhoneNumber,
                avatar = user.Avatar,
                balance = user.Balance,
                dateOfBirth = user.Dob,
                isEmailVerified = user.IsEmailVerified
            });
        }

        /// <summary>
        /// Update current user's avatar
        /// </summary>
        [HttpPut("avatar")]
        public async Task<ActionResult> UpdateAvatar([FromBody] UpdateAvatarRequest request)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.Avatar = request.AvatarUrl;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to update avatar", errors = result.Errors });
            }

            return Ok(new { message = "Avatar updated successfully", avatar = user.Avatar });
        }
    }

    public class UpdateAvatarRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Avatar URL is required")]
        [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "Avatar URL is too long")]
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
