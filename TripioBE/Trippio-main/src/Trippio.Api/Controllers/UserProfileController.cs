using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trippio.Api.Extensions;
using Trippio.Core.Domain.Identity;
using Trippio.Core.Models.System;
using System.IO;

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
        /// Update current user's avatar by uploading an image file
        /// </summary>
        [HttpPost("avatar")]
        public async Task<ActionResult> UpdateAvatar([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "File is required" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = $"File type not allowed. Allowed types: {string.Join(", ", allowedExtensions)}" });
            }

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds 5MB limit" });
            }

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

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsDir = Path.Combine("wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Generate unique filename
                var fileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update user avatar URL
                var avatarUrl = $"/uploads/avatars/{fileName}";
                user.Avatar = avatarUrl;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    // Delete uploaded file if user update fails
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    return BadRequest(new { message = "Failed to update avatar", errors = result.Errors });
                }

                return Ok(new
                {
                    message = "Avatar uploaded successfully",
                    avatar = avatarUrl,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
            }
        }

        /// <summary>
        /// Update current user's avatar with URL (legacy method)
        /// </summary>
        [HttpPut("avatar")]
        [Obsolete("Use POST /api/user/avatar with file upload instead")]
        public async Task<ActionResult> UpdateAvatarUrl([FromBody] UpdateAvatarRequest request)
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
