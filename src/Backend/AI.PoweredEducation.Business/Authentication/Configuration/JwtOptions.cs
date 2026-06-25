namespace AI.PoweredEducation.Business.Authentication.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinimumSecretLengthInBytes = 32;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string Secret { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    public int RefreshTokenLifetimeDays { get; init; } = 7;
}
