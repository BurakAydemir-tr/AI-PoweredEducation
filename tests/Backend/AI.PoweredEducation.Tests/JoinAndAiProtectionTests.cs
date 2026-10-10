using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AI.PoweredEducation.Business.ArtificialIntelligence.Configuration;
using AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;
using AI.PoweredEducation.Business.ArtificialIntelligence.Interfaces;
using AI.PoweredEducation.Business.ArtificialIntelligence.Providers;
using AI.PoweredEducation.Entity.Enums;
using Microsoft.Extensions.Options;

namespace AI.PoweredEducation.Tests;

public sealed class JoinAndAiProtectionTests
{
    private const string Email = "teacher@example.invalid";
    private const string Password = "CorrectPassword1!";
    private const string GameCode = "ABC123";

    [Fact]
    public async Task NormalStudentJoinSucceeds()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);
        await factory.SeedActiveGameAsync(Email, GameCode, 30);

        using var response = await JoinAsync(client, GameCode, "Student One");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(
            body.RootElement.GetProperty("sessionToken").GetString()));
    }

    [Fact]
    public async Task ExpectedStudentCountDoesNotLimitJoining()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);
        await factory.SeedActiveGameAsync(Email, GameCode, 1);

        using var first = await JoinAsync(client, GameCode, "Student One");
        using var second = await JoinAsync(client, GameCode, "Student Two");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    [Fact]
    public async Task StudentJoinRateLimitReturns429()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();

        for (var request = 0; request < 120; request++)
        {
            using var response = await JoinAsync(client, "BAD123", "Student");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        using var rejected = await JoinAsync(client, "BAD123", "Student");
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
    }

    [Fact]
    public async Task NormalAiRequestSucceedsWithoutCallingExternalProvider()
    {
        var provider = new FakeAiProvider();
        using var factory = new AuthenticationTestFactory(provider);
        using var client = await CreateAuthenticatedClientAsync(factory);

        using var response = await GenerateQuizAsync(client, ValidAiRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task AiRateLimitReturns429()
    {
        var provider = new FakeAiProvider();
        using var factory = new AuthenticationTestFactory(provider);
        using var client = await CreateAuthenticatedClientAsync(factory);

        for (var request = 0; request < 10; request++)
        {
            using var response = await GenerateQuizAsync(client, ValidAiRequest());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using var rejected = await GenerateQuizAsync(client, ValidAiRequest());
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal(10, provider.CallCount);
    }

    [Fact]
    public async Task AiRateLimitIsPartitionedByTeacher()
    {
        var provider = new FakeAiProvider();
        using var factory = new AuthenticationTestFactory(provider);
        using var firstTeacher = await CreateAuthenticatedClientAsync(factory);
        await factory.SeedTeacherAsync("second@example.invalid", Password);

        for (var request = 0; request < 10; request++)
        {
            using var response = await GenerateQuizAsync(firstTeacher, ValidAiRequest());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using var secondTeacher = factory.CreateClient();
        using var login = await secondTeacher.PostAsJsonAsync("/api/auth/login", new
        {
            email = "second@example.invalid",
            password = Password
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var loginBody = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        secondTeacher.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginBody.RootElement.GetProperty("accessToken").GetString());

        using var accepted = await GenerateQuizAsync(secondTeacher, ValidAiRequest());
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        Assert.Equal(11, provider.CallCount);
    }

    [Fact]
    public async Task AiConcurrencyLimitReturns429()
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var provider = new FakeAiProvider { WaitForRelease = release.Task };
        using var factory = new AuthenticationTestFactory(provider);
        using var client = await CreateAuthenticatedClientAsync(factory);

        var firstRequest = GenerateQuizAsync(client, ValidAiRequest());
        await provider.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        using var rejected = await GenerateQuizAsync(client, ValidAiRequest());
        release.SetResult();
        using var firstResponse = await firstRequest;

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task OversizedAiInputIsRejectedByValidation()
    {
        var provider = new FakeAiProvider();
        using var factory = new AuthenticationTestFactory(provider);
        using var client = await CreateAuthenticatedClientAsync(factory);

        using var response = await GenerateQuizAsync(client, ValidAiRequest() with
        {
            GradeLevel = new string('G', 51),
            Subject = new string('S', 101),
            Topic = new string('T', 201)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task TaskCountAboveTwentyIsRejected()
    {
        var provider = new FakeAiProvider();
        using var factory = new AuthenticationTestFactory(provider);
        using var client = await CreateAuthenticatedClientAsync(factory);

        using var response = await GenerateQuizAsync(client, ValidAiRequest() with { TaskCount = 21 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task AiProviderErrorDoesNotExposeSensitiveDetails()
    {
        var provider = new FakeAiProvider { Failure = new InvalidOperationException("provider-secret-marker") };
        using var factory = new AuthenticationTestFactory(provider);
        using var client = await CreateAuthenticatedClientAsync(factory);

        using var response = await GenerateQuizAsync(client, ValidAiRequest());
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.DoesNotContain("provider-secret-marker", body);
    }

    [Theory]
    [InlineData("OpenAI")]
    [InlineData("Gemini")]
    public async Task OversizedProviderResponseIsRejected(string providerName)
    {
        var handler = new LargeValidResponseHandler(providerName);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://provider.example.invalid/")
        };
        IAiProvider provider = providerName == "OpenAI"
            ? new OpenAiProvider(httpClient, Options.Create(new OpenAiOptions { ApiKey = "test-key" }))
            : new GeminiProvider(httpClient, Options.Create(new GeminiOptions { ApiKey = "test-key" }));

        await Assert.ThrowsAnyAsync<Exception>(() => provider.GenerateQuizTasksAsync(
            ValidAiRequest().ToContext(), 1));
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(8192, handler.MaxOutputTokens);
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(AuthenticationTestFactory factory)
    {
        var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);
        using var login = await client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var body = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var accessToken = body.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    private static Task<HttpResponseMessage> JoinAsync(HttpClient client, string gameCode, string studentName) =>
        client.PostAsJsonAsync("/api/student-sessions/join", new { gameCode, studentName });

    private static Task<HttpResponseMessage> GenerateQuizAsync(HttpClient client, AiGenerationRequest request) =>
        client.PostAsJsonAsync("/api/ai/quiz-tasks", request);

    private static AiGenerationRequest ValidAiRequest() =>
        new("5", "Science", "Plants", GameEnvironmentType.Indoor, 30, 1);

    private sealed class FakeAiProvider : IAiProvider
    {
        private int _callCount;

        public string Name => "Fake";

        public int CallCount => Volatile.Read(ref _callCount);

        public TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task? WaitForRelease { get; init; }

        public Exception? Failure { get; init; }

        public async Task<IReadOnlyCollection<GeneratedQuizTask>> GenerateQuizTasksAsync(
            AiGenerationContext context,
            int taskCount,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            Started.TrySetResult();
            if (WaitForRelease is not null)
            {
                await WaitForRelease.WaitAsync(cancellationToken);
            }

            if (Failure is not null)
            {
                throw Failure;
            }

            return new[]
            {
                new GeneratedQuizTask("Question", "A", "B", "C", "D", QuizAnswerOption.A)
            };
        }

        public Task<IReadOnlyCollection<GeneratedQrCodeTask>> GenerateQrCodeTasksAsync(
            AiGenerationContext context,
            int taskCount,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<GeneratedQrCodeTask>>(new[]
            {
                new GeneratedQrCodeTask("Scan", 5, "Plants")
            });
    }

    private sealed class LargeValidResponseHandler : HttpMessageHandler
    {
        private readonly string _providerName;

        public LargeValidResponseHandler(string providerName) => _providerName = providerName;

        public int CallCount { get; private set; }

        public int MaxOutputTokens { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            using var requestBody = JsonDocument.Parse(
                await request.Content!.ReadAsStringAsync(cancellationToken));
            MaxOutputTokens = _providerName == "OpenAI"
                ? requestBody.RootElement.GetProperty("max_output_tokens").GetInt32()
                : requestBody.RootElement.GetProperty("generation_config")
                    .GetProperty("max_output_tokens").GetInt32();
            var tasks = new
            {
                tasks = new[]
                {
                    new
                    {
                        question = "Question",
                        optionA = "A",
                        optionB = "B",
                        optionC = "C",
                        optionD = "D",
                        correctAnswer = "A"
                    }
                }
            };
            var padding = new string('x', 256 * 1024);
            var responseJson = _providerName == "OpenAI"
                ? JsonSerializer.Serialize(new { output_text = JsonSerializer.Serialize(tasks), padding })
                : JsonSerializer.Serialize(new { tasks.tasks, padding });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };
        }
    }
}
