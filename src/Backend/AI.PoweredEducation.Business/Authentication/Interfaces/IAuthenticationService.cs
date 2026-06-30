using AI.PoweredEducation.Business.Authentication.Dtos;
using AI.PoweredEducation.Core.Common;

namespace AI.PoweredEducation.Business.Authentication.Interfaces;

public interface IAuthenticationService
{
    Task<Result<AuthenticationResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResponse>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);
}
