using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AI.PoweredEducation.DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AI.PoweredEducation.Tests;

public sealed class QuizProgressionTests
{
    private const string Email = "quiz-teacher@example.invalid";
    private const string Password = "CorrectPassword1!";
    private const string GameCode = "QUIZ42";

    [Fact]
    public async Task CorrectAnswerAwardsExistingQuizScore()
    {
        using var factory = await CreateGameAsync();
        using var client = factory.CreateClient();
        var (token, taskId) = await JoinAsync(client);
        client.DefaultRequestHeaders.Add("X-Session-Token", token);

        using var response = await AnswerAsync(client, taskId, 0, 0);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var state = await GetStateAsync(factory);
        Assert.Equal(1, state.AttemptCount);
        Assert.Equal(100, state.Score);
        Assert.Equal(1, state.ResultCount);
    }

    [Fact]
    public async Task ThreeWrongAnswersRevealCorrectAnswerAndAwardZero()
    {
        using var factory = await CreateGameAsync();
        using var client = factory.CreateClient();
        var (token, taskId) = await JoinAsync(client);
        client.DefaultRequestHeaders.Add("X-Session-Token", token);

        for (var expected = 0; expected < 3; expected++)
        {
            using var response = await AnswerAsync(client, taskId, expected, 1);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            if (expected == 2)
            {
                using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal(0, body.RootElement.GetProperty("revealedCorrectAnswer").GetInt32());
            }
        }

        var state = await GetStateAsync(factory);
        Assert.Equal(3, state.AttemptCount);
        Assert.Equal(0, state.Score);
        Assert.Equal(1, state.ResultCount);
    }

    [Fact]
    public async Task CorrectAnswerAfterTwoWrongAnswersAwardsFifty()
    {
        using var factory = await CreateGameAsync();
        using var client = factory.CreateClient();
        var (token, taskId) = await JoinAsync(client);
        client.DefaultRequestHeaders.Add("X-Session-Token", token);

        using var firstWrong = await AnswerAsync(client, taskId, 0, 1);
        using var secondWrong = await AnswerAsync(client, taskId, 1, 1);
        using var correct = await AnswerAsync(client, taskId, 2, 0);

        Assert.Equal(HttpStatusCode.OK, firstWrong.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondWrong.StatusCode);
        Assert.Equal(HttpStatusCode.OK, correct.StatusCode);
        var state = await GetStateAsync(factory);
        Assert.Equal(3, state.AttemptCount);
        Assert.Equal(50, state.Score);
        Assert.Equal(1, state.ResultCount);
    }

    [Fact]
    public async Task RepeatedWrongAnswerDoesNotIncrementAttemptCount()
    {
        using var factory = await CreateGameAsync();
        using var client = factory.CreateClient();
        var (token, taskId) = await JoinAsync(client);
        client.DefaultRequestHeaders.Add("X-Session-Token", token);

        using var first = await AnswerAsync(client, taskId, 0, 1);
        using var repeated = await AnswerAsync(client, taskId, 0, 1);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);
        var state = await GetStateAsync(factory);
        Assert.Equal(1, state.AttemptCount);
        Assert.Equal(0, state.Score);
    }

    [Fact]
    public async Task LateAnswerCannotApplyToNextQuizTask()
    {
        using var factory = await CreateGameAsync(2);
        using var client = factory.CreateClient();
        var (token, firstTaskId) = await JoinAsync(client);
        client.DefaultRequestHeaders.Add("X-Session-Token", token);

        using var advance = await AnswerAsync(client, firstTaskId, 0, 0);
        Assert.Equal(HttpStatusCode.OK, advance.StatusCode);
        using var delayed = await AnswerAsync(client, firstTaskId, 0, 1);
        Assert.Equal(HttpStatusCode.Conflict, delayed.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var attempts = await db.TaskAttempts.Include(attempt => attempt.LearningTask)
            .OrderBy(attempt => attempt.LearningTask.Order).ToListAsync();
        Assert.Equal(1, attempts[0].AttemptCount);
        Assert.Equal(0, attempts[1].AttemptCount);
        Assert.Empty(await db.Results.ToListAsync());
    }

    [Fact]
    public async Task RepeatedFinalAnswerDoesNotCreateSecondResult()
    {
        using var factory = await CreateGameAsync();
        using var client = factory.CreateClient();
        var (token, taskId) = await JoinAsync(client);
        client.DefaultRequestHeaders.Add("X-Session-Token", token);

        using var first = await AnswerAsync(client, taskId, 0, 0);
        using var repeated = await AnswerAsync(client, taskId, 0, 0);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);
        Assert.Equal(1, (await GetStateAsync(factory)).ResultCount);
    }

    private static async Task<AuthenticationTestFactory> CreateGameAsync(int taskCount = 1)
    {
        var factory = new AuthenticationTestFactory();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync(Email, Password);
        await factory.SeedActiveGameAsync(Email, GameCode, 30, taskCount);
        return factory;
    }

    private static async Task<(string Token, Guid TaskId)> JoinAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync("/api/student-sessions/join",
            new { gameCode = GameCode, studentName = "Quiz Student" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (body.RootElement.GetProperty("sessionToken").GetString()!,
            body.RootElement.GetProperty("currentTask").GetProperty("id").GetGuid());
    }

    private static Task<HttpResponseMessage> AnswerAsync(HttpClient client, Guid taskId,
        int expectedAttemptCount, int answer) =>
        client.PostAsJsonAsync("/api/student-sessions/tasks/current/quiz-answer",
            new { taskId, expectedAttemptCount, answer });

    private static async Task<(int AttemptCount, int Score, int ResultCount)> GetStateAsync(
        AuthenticationTestFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var attempt = await db.TaskAttempts.SingleAsync();
        return (attempt.AttemptCount, attempt.ScoreEarned, await db.Results.CountAsync());
    }
}
