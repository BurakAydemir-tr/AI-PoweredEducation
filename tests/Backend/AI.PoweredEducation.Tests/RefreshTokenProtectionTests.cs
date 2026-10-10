using System.Net;
using System.Net.Http.Json;
using AI.PoweredEducation.Business.Authentication.Dtos;
using AI.PoweredEducation.Core.Security;
using AI.PoweredEducation.DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AI.PoweredEducation.Tests;

public sealed class RefreshTokenProtectionTests
{
    private const string Email = "teacher@example.invalid";
    private const string Password = "CorrectPassword1!";

    [Fact]
    public async Task LoginThenRefreshSucceeds()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = await CreateClientWithTeacherAsync(factory);
        var login = await LoginAsync(client);

        var rotated = await RefreshAsync(client, login.RefreshToken);

        Assert.False(string.IsNullOrWhiteSpace(rotated.AccessToken));
        Assert.NotEqual(login.RefreshToken, rotated.RefreshToken);
    }

    [Fact]
    public async Task ReusedRefreshTokenReturns401()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = await CreateClientWithTeacherAsync(factory);
        var login = await LoginAsync(client);
        await RefreshAsync(client, login.RefreshToken);

        using var replay = await PostRefreshAsync(client, login.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
    }

    [Fact]
    public async Task ReplayRevokesLatestActiveDescendant()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = await CreateClientWithTeacherAsync(factory);
        var first = await LoginAsync(client);
        var second = await RefreshAsync(client, first.RefreshToken);
        var third = await RefreshAsync(client, second.RefreshToken);

        using var replay = await PostRefreshAsync(client, first.RefreshToken);
        using var descendant = await PostRefreshAsync(client, third.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, descendant.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var latest = await database.RefreshTokens.SingleAsync(
            token => token.TokenHash == SecureToken.Hash(third.RefreshToken));
        var root = await database.RefreshTokens.SingleAsync(
            token => token.TokenHash == SecureToken.Hash(first.RefreshToken));
        Assert.NotNull(latest.RevokedAt);
        Assert.NotNull(root.FamilyRevokedAt);
    }

    [Fact]
    public async Task NormalRotationChainRemainsUsable()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = await CreateClientWithTeacherAsync(factory);
        var first = await LoginAsync(client);
        var second = await RefreshAsync(client, first.RefreshToken);
        var third = await RefreshAsync(client, second.RefreshToken);

        var fourth = await RefreshAsync(client, third.RefreshToken);

        Assert.NotEqual(third.RefreshToken, fourth.RefreshToken);
        Assert.False(string.IsNullOrWhiteSpace(fourth.AccessToken));
    }

    [Fact]
    public async Task LogoutRevokesCurrentRefreshToken()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = await CreateClientWithTeacherAsync(factory);
        var login = await LoginAsync(client);

        using var logout = await PostLogoutAsync(client, login.RefreshToken);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = await database.RefreshTokens.SingleAsync();
        Assert.NotNull(stored.RevokedAt);
        Assert.Equal(SecureToken.Hash(login.RefreshToken), stored.TokenHash);
        Assert.NotEqual(login.RefreshToken, stored.TokenHash);
    }

    [Fact]
    public async Task RefreshAfterLogoutReturns401()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = await CreateClientWithTeacherAsync(factory);
        var login = await LoginAsync(client);
        using var logout = await PostLogoutAsync(client, login.RefreshToken);

        using var refresh = await PostRefreshAsync(client, login.RefreshToken);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task RepeatedLogoutIsIdempotent()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = await CreateClientWithTeacherAsync(factory);
        var login = await LoginAsync(client);

        using var first = await PostLogoutAsync(client, login.RefreshToken);
        using var second = await PostLogoutAsync(client, login.RefreshToken);
        using var unknown = await PostLogoutAsync(client, "unknown-token");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, unknown.StatusCode);
    }

    [Fact]
    public async Task LogoutWithRotatedTokenRevokesCurrentDescendant()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = await CreateClientWithTeacherAsync(factory);
        var first = await LoginAsync(client);
        var second = await RefreshAsync(client, first.RefreshToken);

        using var logout = await PostLogoutAsync(client, first.RefreshToken);
        using var refresh = await PostRefreshAsync(client, second.RefreshToken);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task ReplayDoesNotRevokeAnotherLoginFamily()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = await CreateClientWithTeacherAsync(factory);
        var first = await LoginAsync(client);
        var independent = await LoginAsync(client);
        var rotated = await RefreshAsync(client, first.RefreshToken);

        using var replay = await PostRefreshAsync(client, first.RefreshToken);
        using var affected = await PostRefreshAsync(client, rotated.RefreshToken);
        var unaffected = await RefreshAsync(client, independent.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, affected.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(unaffected.RefreshToken));
    }

    private static async Task<HttpClient> CreateClientWithTeacherAsync(AuthenticationTestFactory factory)
    {
        var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);
        return client;
    }

    private static async Task<AuthenticationResponse> LoginAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = Email,
            password = Password
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(result);
        return result;
    }

    private static async Task<AuthenticationResponse> RefreshAsync(HttpClient client, string token)
    {
        using var response = await PostRefreshAsync(client, token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(result);
        return result;
    }

    private static Task<HttpResponseMessage> PostRefreshAsync(HttpClient client, string token) =>
        client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = token });

    private static Task<HttpResponseMessage> PostLogoutAsync(HttpClient client, string token) =>
        client.PostAsJsonAsync("/api/auth/logout", new { refreshToken = token });
}
