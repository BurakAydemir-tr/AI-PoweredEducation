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
        if (user is null || await _userManager.IsLockedOutAsync(user))
        {
            throw InvalidCredentials();
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            var failureResult = await _userManager.AccessFailedAsync(user);
            if (!failureResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to record the login attempt.");
            }

            throw InvalidCredentials();
        }

        var resetResult = await _userManager.ResetAccessFailedCountAsync(user);
        if (!resetResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to reset the failed login count.");
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

        if (currentToken is null)
        {
            throw InvalidRefreshToken();
        }

        if (currentToken.RevokedAt is not null &&
            currentToken.ReplacedByTokenHash is not null)
        {
            await RevokeFamilyAsync(currentToken.FamilyId, now, cancellationToken);
            throw InvalidRefreshToken();
        }

        if (currentToken.RevokedAt is not null || currentToken.ExpiresAt <= now)
        {
            throw InvalidRefreshToken();
        }

        var familyRoot = await _dbContext.RefreshTokens.SingleOrDefaultAsync(
            token => token.Id == currentToken.FamilyId,
            cancellationToken);
        if (familyRoot is null || familyRoot.FamilyRevokedAt is not null)
        {
            throw InvalidRefreshToken();
        }

        var accessToken = _jwtTokenService.CreateAccessToken(currentToken.User, now);
        var replacement = _jwtTokenService.CreateRefreshToken(currentToken.User, now);
        replacement.Entity.FamilyId = currentToken.FamilyId;

        currentToken.RevokedAt = now;
        currentToken.ReplacedByTokenHash = replacement.Entity.TokenHash;
        currentToken.ConcurrencyStamp = Guid.NewGuid();
        familyRoot.ConcurrencyStamp = Guid.NewGuid();

        _dbContext.RefreshTokens.Add(replacement.Entity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RevokeFamilyAsync(currentToken.FamilyId, DateTimeOffset.UtcNow, cancellationToken);
            throw new AuthenticationServiceException(
                AuthenticationErrorCode.InvalidRefreshToken,
                "Refresh token is invalid or expired.",
                innerException: exception);
        }

        return CreateResponse(currentToken.User, accessToken, replacement);
    });

    public async Task LogoutAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        await _refreshTokenRequestValidator.ValidateAndThrowAsync(request, cancellationToken);
        var tokenHash = SecureToken.Hash(request.RefreshToken);
        var token = await _dbContext.RefreshTokens.SingleOrDefaultAsync(
            candidate => candidate.TokenHash == tokenHash,
            cancellationToken);

        if (token is not null)
        {
            await RevokeFamilyAsync(token.FamilyId, DateTimeOffset.UtcNow, cancellationToken);
        }
    }

    private async Task RevokeFamilyAsync(
        Guid familyId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        const int maximumAttempts = 5;
        for (var attempt = 0; attempt < maximumAttempts; attempt++)
        {
            _dbContext.ChangeTracker.Clear();
            var familyRoot = await _dbContext.RefreshTokens.SingleOrDefaultAsync(
                token => token.Id == familyId,
                cancellationToken);
            if (familyRoot is null)
            {
                return;
            }

            familyRoot.FamilyRevokedAt ??= now;
            familyRoot.ConcurrencyStamp = Guid.NewGuid();

            var activeTokens = await _dbContext.RefreshTokens
                .Where(token => token.FamilyId == familyId && token.RevokedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var activeToken in activeTokens)
            {
                activeToken.RevokedAt = now;
                activeToken.ConcurrencyStamp = Guid.NewGuid();
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return;
            }
            catch (DbUpdateConcurrencyException) when (attempt < maximumAttempts - 1)
            {
                // Reload the family after a concurrent refresh before retrying revocation.
            }
        }

        throw new InvalidOperationException("Refresh token family could not be revoked.");
    }

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

    private static AuthenticationServiceException InvalidCredentials()
    {
        return new AuthenticationServiceException(
            AuthenticationErrorCode.InvalidCredentials,
            "Email or password is invalid.");
    }
}
