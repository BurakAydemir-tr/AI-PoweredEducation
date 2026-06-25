using AI.PoweredEducation.Entity.Identity;

namespace AI.PoweredEducation.Business.Authentication.Interfaces;

public interface IJwtTokenService
{
    AccessToken CreateAccessToken(ApplicationUser user, DateTimeOffset issuedAt);

    GeneratedRefreshToken CreateRefreshToken(ApplicationUser user, DateTimeOffset issuedAt);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

public sealed record GeneratedRefreshToken(
    string Value,
    RefreshToken Entity);
