namespace FiapNext.Infrastructure.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string SecretKey { get; set; } = "FiapNext_SuperSecretKey_2TDS_2026_Minimum32CharsLong!";
    public string Issuer { get; set; } = "FiapNextApi";
    public string Audience { get; set; } = "FiapNextClient";
    public int ExpirationInHours { get; set; } = 4;
}
