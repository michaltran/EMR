namespace EMR.Api.Auth;

public class JwtSettings
{
    public string Issuer { get; set; } = "emr-lienchieu";
    public string Audience { get; set; } = "emr-lienchieu";
    public string Key { get; set; } = "DEV_ONLY_CHANGE_ME_emr-lienchieu-2026-secret-min-32-chars!!";
    public int ExpiryMinutes { get; set; } = 480;
}
