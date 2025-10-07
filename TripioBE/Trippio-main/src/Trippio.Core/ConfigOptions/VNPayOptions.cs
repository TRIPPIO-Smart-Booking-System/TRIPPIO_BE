namespace Trippio.Core.ConfigOptions;

public class VNPayOptions
{
    public string TMerchantId { get; set; } = string.Empty;
    public string THashSecret { get; set; } = string.Empty;
    public string TmnCode { get; set; } = string.Empty;
    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
    public string CurrCode { get; set; } = "VND";
    public string Locale { get; set; } = "vn";
    public string BaseUrl { get; set; } = string.Empty;
}

public class RedirectUrlsOptions
{
    public string Web { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
}