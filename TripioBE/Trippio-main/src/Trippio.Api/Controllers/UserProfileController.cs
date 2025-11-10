using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
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
        /// Upload and update current user's avatar
        /// POST /api/user/avatar
        /// </summary>
        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UpdateAvatar([FromForm] IFormFile file)
        {
            try
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

                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "File is required." });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = "Only JPG, PNG, GIF, and WebP files are allowed." });
                }

                // Validate file size (max 5MB)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new { message = "File size must not exceed 5MB." });
                }

                // Create uploads directory if it doesn't exist
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Generate unique filename
                var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Build avatar URL
                var avatarUrl = $"/uploads/avatars/{fileName}";

                // Update user's avatar
                user.Avatar = avatarUrl;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    // Delete file if database update fails
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { message = "Failed to update avatar in database.", errors });
                }

                return Ok(new
                {
                    message = "Avatar uploaded and updated successfully",
                    avatar = avatarUrl,
                    fileName = fileName,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading the avatar.", error = ex.Message });
            }
        }
    }

    public class UpdateAvatarRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Avatar URL is required")]
        [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "Avatar URL is too long")]
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
