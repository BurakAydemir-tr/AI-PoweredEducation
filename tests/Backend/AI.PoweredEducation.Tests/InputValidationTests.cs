using AI.PoweredEducation.Business.LearningTasks.Dtos;
using AI.PoweredEducation.Business.LearningTasks.Validators;
using AI.PoweredEducation.Business.StudentSessions.Dtos;
using AI.PoweredEducation.Business.StudentSessions.Validators;
using AI.PoweredEducation.Entity.Enums;
using AI.PoweredEducation.DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AI.PoweredEducation.Tests;

public sealed class InputValidationTests
{
    private const string ValidPolygon = """
        {"type":"Polygon","coordinates":[[[29,41],[30,41],[30,42],[29,41]]]}
        """;

    [Theory]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("{\"type\":\"Polygon\",\"coordinates\":[]}")]
    [InlineData("{\"type\":\"Polygon\",\"coordinates\":[[]]}")]
    [InlineData("{\"type\":\"Point\",\"coordinates\":[29,41]}")]
    [InlineData("{\"type\":\"Polygon\",\"coordinates\":[[[\"x\",41],[30,41],[30,42],[\"x\",41]]]}")]
    public void MalformedOrEmptyPolygonIsRejected(string polygon)
    {
        var result = new CreateGpsTaskRequestValidator().Validate(
            new CreateGpsTaskRequest("Go", 41, 29, polygon, 5));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidPolygonRemainsAccepted()
    {
        var result = new CreateGpsTaskRequestValidator().Validate(
            new CreateGpsTaskRequest("Go", 41, 29, ValidPolygon, 5));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NullAndDuplicateTaskIdsAreRejectedWithoutThrowing()
    {
        var validator = new ReorderLearningTasksRequestValidator();
        var taskId = Guid.NewGuid();
        Assert.False(validator.Validate(new ReorderLearningTasksRequest(null!)).IsValid);
        Assert.False(validator.Validate(new ReorderLearningTasksRequest([taskId, taskId])).IsValid);
        Assert.False(validator.Validate(new ReorderLearningTasksRequest([Guid.Empty])).IsValid);
    }

    [Fact]
    public void InvalidEnumsAndCoordinatesAreRejected()
    {
        Assert.False(new SubmitQuizAnswerRequestValidator().Validate(
            new SubmitQuizAnswerRequest(Guid.NewGuid(), 0, (QuizAnswerOption)99)).IsValid);
        Assert.False(new CreateQuizTaskRequestValidator().Validate(
            new CreateQuizTaskRequest("Q", "A", "B", "C", "D", (QuizAnswerOption)99)).IsValid);
        Assert.False(new CompleteGpsTaskRequestValidator().Validate(
            new CompleteGpsTaskRequest(91, 0)).IsValid);
        Assert.False(new CompleteGpsTaskRequestValidator().Validate(
            new CompleteGpsTaskRequest(double.NaN, 0)).IsValid);
        Assert.False(new CreateGpsTaskRequestValidator().Validate(
            new CreateGpsTaskRequest("Go", 0, 181, ValidPolygon, 5)).IsValid);
    }

    [Fact]
    public void ExcessiveTextAndBlankStudentNameAreRejected()
    {
        Assert.False(new JoinGameRequestValidator().Validate(
            new JoinGameRequest("ABC123", "   ")).IsValid);
        Assert.False(new JoinGameRequestValidator().Validate(
            new JoinGameRequest("ABC123", new string('x', 101))).IsValid);
        Assert.False(new CreateQuizTaskRequestValidator().Validate(
            new CreateQuizTaskRequest(new string('Q', 1001), "A", "B", "C", "D", QuizAnswerOption.A)).IsValid);
        Assert.False(new CreateQuizTaskRequestValidator().Validate(
            new CreateQuizTaskRequest("Q", new string('A', 501), "B", "C", "D", QuizAnswerOption.A)).IsValid);
        Assert.False(new CreateQrCodeTaskRequestValidator().Validate(
            new CreateQrCodeTaskRequest(new string('Q', 2001), 5)).IsValid);
        Assert.False(new CreateGpsTaskRequestValidator().Validate(
            new CreateGpsTaskRequest(new string('G', 2001), 41, 29, ValidPolygon, 5)).IsValid);
    }

    [Fact]
    public async Task InvalidRequestsReturnHttp400()
    {
        using var factory = new AuthenticationTestFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.SeedTeacherAsync("validator@example.invalid", "CorrectPassword1!");
        await factory.SeedActiveGameAsync("validator@example.invalid", "VAL123", 30);

        using var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "validator@example.invalid", password = "CorrectPassword1!" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var loginBody = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            loginBody.RootElement.GetProperty("accessToken").GetString());

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var game = await db.LearningGames.Include(item => item.Tasks).SingleAsync();
        var taskId = game.Tasks.Single().Id;

        using var malformedPolygon = await client.PostAsJsonAsync($"/api/games/{game.Id}/tasks/gps",
            new { instructions = "Go", targetLatitude = 41, targetLongitude = 29,
                gameAreaJson = "{", timeLimitMinutes = 5 });
        using var emptyPolygon = await client.PostAsJsonAsync($"/api/games/{game.Id}/tasks/gps",
            new { instructions = "Go", targetLatitude = 41, targetLongitude = 29,
                gameAreaJson = "{\"type\":\"Polygon\",\"coordinates\":[]}", timeLimitMinutes = 5 });
        using var nullTaskIds = await client.PutAsJsonAsync($"/api/games/{game.Id}/tasks/order",
            new { taskIds = (Guid[]?)null });
        using var duplicateTaskIds = await client.PutAsJsonAsync($"/api/games/{game.Id}/tasks/order",
            new { taskIds = new[] { taskId, taskId } });
        using var invalidEnum = await client.PostAsJsonAsync($"/api/games/{game.Id}/tasks/quiz",
            new { question = "Q", optionA = "A", optionB = "B", optionC = "C",
                optionD = "D", correctAnswer = 99 });
        using var longQuestion = await client.PostAsJsonAsync($"/api/games/{game.Id}/tasks/quiz",
            new { question = new string('Q', 1001), optionA = "A", optionB = "B",
                optionC = "C", optionD = "D", correctAnswer = 0 });
        using var longStudentName = await client.PostAsJsonAsync("/api/student-sessions/join",
            new { gameCode = "VAL123", studentName = new string('S', 101) });

        Assert.All(new[] { malformedPolygon, emptyPolygon, nullTaskIds,
            duplicateTaskIds, invalidEnum, longQuestion, longStudentName },
            response => Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode));
    }
}
