using AI.PoweredEducation.Business.Authentication.Dtos;

namespace AI.PoweredEducation.Business.Authentication.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);
}
