using AI.PoweredEducation.API.Results;
using AI.PoweredEducation.API.Security;
using AI.PoweredEducation.Business.Authentication.Dtos;
using AI.PoweredEducation.Business.Authentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AI.PoweredEducation.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    [EnableRateLimiting(AuthenticationRateLimitPolicies.Register)]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthenticationResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        return (await _authenticationService.RegisterAsync(request, cancellationToken))
            .ToActionResult(this);
    }

    [HttpPost("login")]
    [EnableRateLimiting(AuthenticationRateLimitPolicies.Login)]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        return (await _authenticationService.LoginAsync(request, cancellationToken))
            .ToActionResult(this);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting(AuthenticationRateLimitPolicies.Refresh)]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        return (await _authenticationService.RefreshAsync(request, cancellationToken))
            .ToActionResult(this);
    }
}
