namespace EncuestAPP__API.Services.Implementations;

public sealed class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "EncuestAPP";
    public string Audience { get; set; } = "EncuestAPP.Clients";
    public int ExpirationMinutes { get; set; } = 60;
}
