namespace Trippio.Core.ConfigOptions
{
    public class MediaSettings
    {
        public string? AllowImageFileTypes { get; set; }
        public string ImagePath { get; set; } = "media";
        public string ImageUrl { get; set; } = "/images/no-image.png";
    }
}
