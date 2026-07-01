using AI.PoweredEducation.Business.Authentication.Dtos;
using AI.PoweredEducation.Business.Authentication.Exceptions;
using AI.PoweredEducation.Business.Authentication.Interfaces;
using AI.PoweredEducation.Business.Common.Results;
using AI.PoweredEducation.Core.Common;
using AI.PoweredEducation.Core.Security;
using AI.PoweredEducation.DataAccess.Persistence;
using AI.PoweredEducation.Entity.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AI.PoweredEducation.Business.Authentication.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<RegisterRequest> _registerRequestValidator;
    private readonly IValidator<LoginRequest> _loginRequestValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenRequestValidator;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IValidator<RegisterRequest> registerRequestValidator,
        IValidator<LoginRequest> loginRequestValidator,
        IValidator<RefreshTokenRequest> refreshTokenRequestValidator)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _registerRequestValidator = registerRequestValidator;
        _loginRequestValidator = loginRequestValidator;
        _refreshTokenRequestValidator = refreshTokenRequestValidator;
    }

    public Task<Result<AuthenticationResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _registerRequestValidator.ValidateAndThrowAsync(request, cancellationToken);
        var email = request.Email.Trim();

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };

        var creationResult = await _userManager.CreateAsync(user, request.Password);
        if (!creationResult.Succeeded)
        {
            throw new AuthenticationServiceException(
                AuthenticationErrorCode.RegistrationFailed,
                "Teacher registration failed.",
                creationResult.Errors.Select(error => error.Description).ToArray());
        }

        var response = await CreateAndPersistTokensAsync(user, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return response;
    });

    public Task<Result<AuthenticationResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _loginRequestValidator.ValidateAndThrowAsync(request, cancellationToken);
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new AuthenticationServiceException(
                AuthenticationErrorCode.InvalidCredentials,
                "Email or password is invalid.");
        }

        return await CreateAndPersistTokensAsync(user, cancellationToken);
    });

    public Task<Result<AuthenticationResponse>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _refreshTokenRequestValidator.ValidateAndThrowAsync(request, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var tokenHash = SecureToken.Hash(request.RefreshToken);

        var currentToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (currentToken is null ||
            currentToken.RevokedAt is not null ||
            currentToken.ExpiresAt <= now)
        {
            throw InvalidRefreshToken();
        }

        var accessToken = _jwtTokenService.CreateAccessToken(currentToken.User, now);
        var replacement = _jwtTokenService.CreateRefreshToken(currentToken.User, now);

        currentToken.RevokedAt = now;
        currentToken.ReplacedByTokenHash = replacement.Entity.TokenHash;
        currentToken.ConcurrencyStamp = Guid.NewGuid();

        _dbContext.RefreshTokens.Add(replacement.Entity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new AuthenticationServiceException(
                AuthenticationErrorCode.InvalidRefreshToken,
                "Refresh token has already been used.",
                innerException: exception);
        }

        return CreateResponse(currentToken.User, accessToken, replacement);
    });

    private async Task<AuthenticationResponse> CreateAndPersistTokensAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var accessToken = _jwtTokenService.CreateAccessToken(user, now);
        var refreshToken = _jwtTokenService.CreateRefreshToken(user, now);

        _dbContext.RefreshTokens.Add(refreshToken.Entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateResponse(user, accessToken, refreshToken);
    }

    private static AuthenticationResponse CreateResponse(
        ApplicationUser user,
        AccessToken accessToken,
        GeneratedRefreshToken refreshToken)
    {
        return new AuthenticationResponse(
            accessToken.Value,
            accessToken.ExpiresAt,
            refreshToken.Value,
            refreshToken.Entity.ExpiresAt,
            user.FirstName,
            user.LastName);
    }

    private static AuthenticationServiceException InvalidRefreshToken()
    {
        return new AuthenticationServiceException(
            AuthenticationErrorCode.InvalidRefreshToken,
            "Refresh token is invalid or expired.");
    }
}
