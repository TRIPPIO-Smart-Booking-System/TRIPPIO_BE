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
        /// Upload và cập nhật avatar người dùng hiện tại.
        /// Nhận multipart/form-data chứa 1 file hình.
        /// Hỗ trợ: JPG, PNG, GIF, WebP. Giới hạn: 5MB.
        /// File lưu tại: wwwroot/uploads/avatars/
        /// </summary>
        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadAvatarResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateAvatar([FromForm] UploadAvatarDto request)
        {
            try
            {
                var file = request.File;

                var userId = User.GetUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User ID not found in token." });

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return NotFound(new { message = "User not found." });

                // Kiểm tra file tồn tại
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Vui lòng upload một hình ảnh." });

                // Kiểm tra loại file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new
                    {
                        message = "Chỉ chấp nhận định dạng JPG, PNG, GIF, WebP.",
                        supportedFormats = string.Join(", ", allowedExtensions)
                    });
                }

                // Kiểm tra kích thước file
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new
                    {
                        message = "Dung lượng file vượt quá giới hạn 5MB.",
                        fileSizeMB = file.Length / (1024.0 * 1024.0),
                        maxSizeMB = maxFileSize / (1024.0 * 1024.0)
                    });
                }

                // Tạo thư mục upload nếu chưa có
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                // Đặt tên file duy nhất theo userId + timestamp
                var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Lưu file vào disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Tạo URL public cho avatar
                var avatarUrl = $"/uploads/avatars/{fileName}";

                // Cập nhật user avatar
                user.Avatar = avatarUrl;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    // Rollback nếu update DB lỗi
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return BadRequest(new
                    {
                        message = "Không thể cập nhật avatar vào cơ sở dữ liệu.",
                        errors
                    });
                }

                return Ok(new UploadAvatarResponse
                {
                    Message = "Upload avatar thành công!",
                    AvatarUrl = avatarUrl,
                    FileName = fileName,
                    UserId = userId,
                    FileSize = file.Length,
                    UploadedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Đã xảy ra lỗi khi upload avatar.",
                    error = ex.Message
                });
            }
        }
    }

    public class UpdateAvatarRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Avatar URL is required")]
        [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "Avatar URL is too long")]
        public string AvatarUrl { get; set; } = string.Empty;
    }

    /// <summary>
/// DTO cho upload avatar qua Swagger.
/// </summary>
public class UploadAvatarDto
{
    [FromForm(Name = "file")]
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng chọn 1 file hình để upload.")]
    public IFormFile File { get; set; } = null!;
}
}
