using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AI.PoweredEducation.Tests;

public sealed class AuthenticationProtectionTests
{
    private const string Email = "teacher@example.invalid";
    private const string Password = "CorrectPassword1!";
    private const string WrongPassword = "WrongPassword1!";

    [Fact]
    public async Task CorrectPasswordAllowsLogin()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);

        using var response = await LoginAsync(client, Email, Password);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(content?.AccessToken));
    }

    [Fact]
    public async Task WrongPasswordIncrementsFailedCount()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);

        using var response = await LoginAsync(client, Email, WrongPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(1, (await factory.GetLockoutStateAsync(Email)).FailedCount);
    }

    [Fact]
    public async Task FiveFailedAttemptsLockTheTeacher()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var response = await LoginAsync(client, Email, WrongPassword);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        Assert.True((await factory.GetLockoutStateAsync(Email)).LockedOut);
        var remainingLockout = (await factory.GetLockoutEndAsync(Email)) - DateTimeOffset.UtcNow;
        Assert.InRange(remainingLockout!.Value.TotalMinutes, 14, 15);
    }

    [Fact]
    public async Task LockedTeacherCannotLoginWithCorrectPassword()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var failed = await LoginAsync(client, Email, WrongPassword);
            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        using var response = await LoginAsync(client, Email, Password);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SuccessfulLoginResetsFailedCount()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            using var failed = await LoginAsync(client, Email, WrongPassword);
            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        using var response = await LoginAsync(client, Email, Password);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, (await factory.GetLockoutStateAsync(Email)).FailedCount);
    }

    [Fact]
    public async Task EleventhLoginWithinOneMinuteReturns429()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();

        for (var attempt = 0; attempt < 10; attempt++)
        {
            using var response = await LoginAsync(client, "absent@example.invalid", WrongPassword);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        using var rejected = await LoginAsync(client, "absent@example.invalid", WrongPassword);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
    }

    [Fact]
    public async Task NormalRepeatedLoginsStayWithinLimit()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);

        for (var attempt = 0; attempt < 3; attempt++)
        {
            using var response = await LoginAsync(client, Email, Password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task UnknownAndIncorrectCredentialsReturnSameMessage()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);

        using var incorrect = await LoginAsync(client, Email, WrongPassword);
        using var unknown = await LoginAsync(client, "absent@example.invalid", WrongPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, incorrect.StatusCode);
        Assert.Equal(incorrect.StatusCode, unknown.StatusCode);
        using var incorrectBody = JsonDocument.Parse(await incorrect.Content.ReadAsStringAsync());
        using var unknownBody = JsonDocument.Parse(await unknown.Content.ReadAsStringAsync());
        Assert.Equal(
            incorrectBody.RootElement.GetProperty("title").GetString(),
            unknownBody.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task SixthRegistrationWithinOneHourReturns429()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                firstName = "Test",
                lastName = "Teacher",
                email = $"teacher{attempt}@example.invalid",
                password = Password
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using var rejected = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "Teacher",
            email = "teacher6@example.invalid",
            password = Password
        });
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
    }

    [Fact]
    public async Task ThirtyFirstRefreshWithinOneMinuteReturns429()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();

        for (var attempt = 0; attempt < 30; attempt++)
        {
            using var response = await client.PostAsJsonAsync("/api/auth/refresh", new
            {
                refreshToken = "invalid-token"
            });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        using var rejected = await client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = "invalid-token"
        });
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
    }

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("/api/auth/login", new { email, password });

    private sealed record LoginResponse(string AccessToken);
}
