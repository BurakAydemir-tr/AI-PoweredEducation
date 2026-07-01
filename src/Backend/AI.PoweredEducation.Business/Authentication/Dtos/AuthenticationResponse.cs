namespace AI.PoweredEducation.Business.Authentication.Dtos;

public sealed record AuthenticationResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    string FirstName,
    string LastName);
