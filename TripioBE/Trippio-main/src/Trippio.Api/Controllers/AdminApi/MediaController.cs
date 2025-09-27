using Trippio.Core.ConfigOptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Trippio.Api.Controllers.AdminApi
{
    [Route("api/admin/media")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnv;
        private readonly MediaSettings _settings;

        public MediaController(IWebHostEnvironment env, IOptions<MediaSettings> settings)
        {
            _hostingEnv = env;
            _settings = settings.Value;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult UploadImage(string type)
        {
            var allowImageTypes = _settings.AllowImageFileTypes?.Split(",");

            var now = DateTime.Now;
            var files = Request.Form.Files;
            if (files.Count == 0)
            {
                return BadRequest("No files uploaded");
            }

            var file = files[0];
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition)?.FileName?.Trim('"');
            if (allowImageTypes?.Any(x => filename?.EndsWith(x, StringComparison.OrdinalIgnoreCase) == true) == false)
            {
                return BadRequest("File type not allowed. Only image files are permitted.");
            }

            var imageFolder = $@"\{_settings.ImagePath}\images\{type}\{now:MMyyyy}";
            var folder = _hostingEnv.WebRootPath + imageFolder;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var filePath = Path.Combine(folder, filename);
            using (var fs = System.IO.File.Create(filePath))
            {
                file.CopyTo(fs);
                fs.Flush();
            }

            var path = Path.Combine(imageFolder, filename).Replace(@"\", @"/");
            return Ok(new { path });
        }
    }
}