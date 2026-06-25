using AI.PoweredEducation.Business.Authentication.Dtos;
using AI.PoweredEducation.Business.Authentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthenticationResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _authenticationService.RegisterAsync(request, cancellationToken));
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _authenticationService.LoginAsync(request, cancellationToken));
    }

    [HttpPost("refresh")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _authenticationService.RefreshAsync(request, cancellationToken));
    }
}
