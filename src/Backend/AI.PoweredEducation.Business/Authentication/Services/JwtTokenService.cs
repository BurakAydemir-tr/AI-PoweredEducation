using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AI.PoweredEducation.Business.Authentication.Configuration;
using AI.PoweredEducation.Business.Authentication.Interfaces;
using AI.PoweredEducation.Core.Security;
using AI.PoweredEducation.Entity.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AI.PoweredEducation.Business.Authentication.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public AccessToken CreateAccessToken(ApplicationUser user, DateTimeOffset issuedAt)
    {
        var expiresAt = issuedAt.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }

    public GeneratedRefreshToken CreateRefreshToken(
        ApplicationUser user,
        DateTimeOffset issuedAt)
    {
        var rawToken = SecureToken.Generate();
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = SecureToken.Hash(rawToken),
            ExpiresAt = issuedAt.AddDays(_options.RefreshTokenLifetimeDays),
            ConcurrencyStamp = Guid.NewGuid()
        };

        return new GeneratedRefreshToken(rawToken, entity);
    }
}
