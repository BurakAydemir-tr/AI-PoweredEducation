using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AI.PoweredEducation.DataAccess.Persistence;
using AI.PoweredEducation.Entity.Entities;
using AI.PoweredEducation.Entity.Enums;
using AI.PoweredEducation.Entity.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AI.PoweredEducation.Tests;

public sealed class PostgresQuizConcurrencyTests
{
    [Fact]
    public async Task ConcurrentAnswersAndFinalCompletionAreSerializedOnPostgres()
    {
        var adminConnectionString = Environment.GetEnvironmentVariable(
            "AI_EDUCATION_TEST_POSTGRES_ADMIN");
        if (string.IsNullOrWhiteSpace(adminConnectionString)) return;

        var admin = new NpgsqlConnectionStringBuilder(adminConnectionString);
        Assert.Contains(admin.Host, new[] { "localhost", "127.0.0.1", "::1" });
        Assert.Equal("ai_powered_education", admin.Database);

        var testDatabase = "ai_powered_education_phase2d_" +
            Guid.NewGuid().ToString("N");
        await ExecuteAdminAsync(admin.ConnectionString,
            $"CREATE DATABASE \"{testDatabase}\"");
        try
        {
            admin.Database = testDatabase;
            using var factory = new PostgresTestFactory(admin.ConnectionString);
            using var client = factory.CreateClient();
            await SeedAsync(factory);

            var first = await JoinAsync(client, "Concurrent Student");
            var wrongRequests = Enumerable.Range(0, 8)
                .Select(_ => AnswerAsync(client, first.Token, first.TaskId, 0, 1));
            var wrongResponses = await Task.WhenAll(wrongRequests);
            Assert.Single(wrongResponses, response => response.StatusCode == HttpStatusCode.OK);
            Assert.Equal(7, wrongResponses.Count(response => response.StatusCode == HttpStatusCode.Conflict));
            foreach (var response in wrongResponses) response.Dispose();
            Assert.Equal((1, 0), await ReadAttemptAsync(factory, first.TaskId, first.Token));

            using var secondWrong = await AnswerAsync(client, first.Token, first.TaskId, 1, 1);
            using var thirdWrong = await AnswerAsync(client, first.Token, first.TaskId, 2, 1);
            Assert.Equal(HttpStatusCode.OK, secondWrong.StatusCode);
            Assert.Equal(HttpStatusCode.OK, thirdWrong.StatusCode);
            Assert.Equal((3, 0), await ReadAttemptAsync(factory, first.TaskId, first.Token));

            var second = await JoinAsync(client, "Final Student");
            var finalRequests = Enumerable.Range(0, 8)
                .Select(_ => AnswerAsync(client, second.Token, second.TaskId, 0, 0));
            var finalResponses = await Task.WhenAll(finalRequests);
            Assert.Single(finalResponses, response => response.StatusCode == HttpStatusCode.OK);
            Assert.Equal(7, finalResponses.Count(response => response.StatusCode == HttpStatusCode.Conflict));
            foreach (var response in finalResponses) response.Dispose();
            Assert.Equal((1, 100), await ReadAttemptAsync(factory, second.TaskId, second.Token));

            await using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(2, await db.Results.CountAsync());
        }
        finally
        {
            admin.Database = "ai_powered_education";
            await ExecuteAdminAsync(admin.ConnectionString,
                $"DROP DATABASE \"{testDatabase}\" WITH (FORCE)");
        }
    }

    private static async Task ExecuteAdminAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task SeedAsync(PostgresTestFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var teacher = new ApplicationUser
        {
            Id = Guid.NewGuid(), Email = "postgres-quiz@example.invalid",
            UserName = "postgres-quiz@example.invalid", FirstName = "Test", LastName = "Teacher"
        };
        var created = await users.CreateAsync(teacher, "CorrectPassword1!");
        Assert.True(created.Succeeded);
        db.LearningGames.Add(new LearningGame
        {
            Id = Guid.NewGuid(), TeacherId = teacher.Id,
            GradeLevel = "5", Subject = "Science", Topic = "Plants",
            EnvironmentType = GameEnvironmentType.Indoor,
            ExpectedStudentCount = 30,
            Status = LearningGameStatus.Active,
            GameCode = "PGQ123",
            Tasks = new List<LearningTask>
            {
                new QuizTask
                {
                    Id = Guid.NewGuid(), Order = 1, Question = "What grows?",
                    OptionA = "Plants", OptionB = "Rocks", OptionC = "Glass",
                    OptionD = "Metal", CorrectAnswer = QuizAnswerOption.A
                }
            }
        });
        await db.SaveChangesAsync();
    }

    private static async Task<(string Token, Guid TaskId)> JoinAsync(
        HttpClient client, string studentName)
    {
        using var response = await client.PostAsJsonAsync("/api/student-sessions/join",
            new { gameCode = "PGQ123", studentName });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (body.RootElement.GetProperty("sessionToken").GetString()!,
            body.RootElement.GetProperty("currentTask").GetProperty("id").GetGuid());
    }

    private static async Task<HttpResponseMessage> AnswerAsync(
        HttpClient client, string token, Guid taskId, int expectedAttemptCount, int answer)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post,
            "/api/student-sessions/tasks/current/quiz-answer")
        {
            Content = JsonContent.Create(new { taskId, expectedAttemptCount, answer })
        };
        request.Headers.Add("X-Session-Token", token);
        return await client.SendAsync(request);
    }

    private static async Task<(int AttemptCount, int Score)> ReadAttemptAsync(
        PostgresTestFactory factory, Guid taskId, string token)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var attempt = await db.TaskAttempts.SingleAsync(item => item.LearningTaskId == taskId &&
            item.StudentSession.SessionTokenHash ==
            AI.PoweredEducation.Core.Security.SecureToken.Hash(token));
        return (attempt.AttemptCount, attempt.ScoreEarned);
    }

    private sealed class PostgresTestFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
            builder.UseSetting("Jwt:Issuer", "AI.PoweredEducation.Tests");
            builder.UseSetting("Jwt:Audience", "AI.PoweredEducation.Tests");
            builder.UseSetting("Jwt:Secret", "test-only-signing-key-0123456789abcdef0123456789abcdef");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.RemoveAll<IDatabaseProvider>();
                services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
            });
        }
    }
}
