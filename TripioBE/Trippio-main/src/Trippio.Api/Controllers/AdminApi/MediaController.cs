using Trippio.Core.ConfigOptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Trippio.Api.Controllers.AdminApi
{
    [Route("api/media")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnv;
        private readonly MediaSettings _settings;
        private readonly ILogger<MediaController> _logger;

        public MediaController(
            IWebHostEnvironment env, 
            IOptions<MediaSettings> settings,
            ILogger<MediaController> logger)
        {
            _hostingEnv = env;
            _settings = settings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Upload image file (avatar, product images, etc.)
        /// </summary>
        /// <param name="type">Type of image: avatar, product, banner, etc.</param>
        /// <returns>URL of uploaded image</returns>
        [HttpPost("upload")]
        [Authorize]
        [DisableRequestSizeLimit]
        public IActionResult UploadImage([FromQuery] string type = "avatar")
        {
            try
            {
                var allowImageTypes = _settings.AllowImageFileTypes?.Split(",") ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                
                var files = Request.Form.Files;
                if (files.Count == 0)
                {
                    return BadRequest(new { message = "No files uploaded" });
                }

                var file = files[0];
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition)?.FileName?.Trim('"');
                
                // Validate file type
                if (allowImageTypes?.Any(x => filename?.EndsWith(x, StringComparison.OrdinalIgnoreCase) == true) == false)
                {
                    return BadRequest(new { message = "File type not allowed. Only image files are permitted.", allowedTypes = string.Join(", ", allowImageTypes) });
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "File size exceeds 5MB limit" });
                }

                // Create unique filename to avoid conflicts
                var extension = Path.GetExtension(filename);
                var uniqueFilename = $"{Guid.NewGuid()}{extension}";
                
                var now = DateTime.Now;
                var imageFolder = $@"\{_settings.ImagePath}\images\{type}\{now:MMyyyy}";
                var folder = _hostingEnv.WebRootPath + imageFolder;

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var filePath = Path.Combine(folder, uniqueFilename);
                using (var fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }

                var path = Path.Combine(imageFolder, uniqueFilename).Replace(@"\", @"/");
                
                _logger.LogInformation("File uploaded successfully: {Path}", path);
                
                return Ok(new 
                { 
                    success = true,
                    path = path,
                    url = path,
                    fileName = uniqueFilename,
                    originalFileName = filename,
                    fileSize = file.Length,
                    contentType = file.ContentType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete uploaded image (Admin only)
        /// </summary>
        [HttpDelete("delete")]
        [Authorize]
        public IActionResult DeleteImage([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return BadRequest(new { message = "Path is required" });
                }

                var fullPath = _hostingEnv.WebRootPath + path.Replace("/", @"\");
                
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("File deleted successfully: {Path}", path);
                    return Ok(new { success = true, message = "File deleted successfully" });
                }
                
                return NotFound(new { message = "File not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return StatusCode(500, new { message = "Error deleting file", error = ex.Message });
            }
        }
    }
}