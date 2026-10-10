using AI.PoweredEducation.Business.ArtificialIntelligence.Interfaces;
using AI.PoweredEducation.DataAccess.Persistence;
using AI.PoweredEducation.Entity.Entities;
using AI.PoweredEducation.Entity.Enums;
using AI.PoweredEducation.Entity.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AI.PoweredEducation.Tests;

public sealed class AuthenticationTestFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly IAiProvider? _aiProvider;

    public AuthenticationTestFactory(IAiProvider? aiProvider = null)
    {
        _aiProvider = aiProvider;
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=unused;Database=unused");
        builder.UseSetting("Jwt:Issuer", "AI.PoweredEducation.Tests");
        builder.UseSetting("Jwt:Audience", "AI.PoweredEducation.Tests");
        builder.UseSetting("Jwt:Secret", "test-only-signing-key-0123456789abcdef0123456789abcdef");
        builder.UseSetting("Jwt:AccessTokenLifetimeMinutes", "15");
        builder.UseSetting("Jwt:RefreshTokenLifetimeDays", "7");
        builder.UseSetting("Authentication:Lockout:MaxFailedAccessAttempts", "5");
        builder.UseSetting("Authentication:Lockout:DurationMinutes", "15");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<IDatabaseProvider>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
            if (_aiProvider is not null)
            {
                services.RemoveAll<IAiProvider>();
                services.AddSingleton(_aiProvider);
            }
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(database.Database.IsSqlite());
        await database.Database.EnsureCreatedAsync();
    }

    public async Task SeedTeacherAsync(string email, string password)
    {
        await using var scope = Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var result = await users.CreateAsync(new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = "Test",
            LastName = "Teacher"
        }, password);
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(error => error.Description)));
    }

    public async Task SeedActiveGameAsync(string teacherEmail, string gameCode, int expectedStudentCount,
        int quizTaskCount = 1)
    {
        await using var scope = Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var teacher = await users.FindByEmailAsync(teacherEmail);
        Assert.NotNull(teacher);
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tasks = Enumerable.Range(1, quizTaskCount).Select(order => (LearningTask)new QuizTask
        {
            Id = Guid.NewGuid(),
            Order = order,
            Question = $"What grows? {order}",
            OptionA = "Plants",
            OptionB = "Rocks",
            OptionC = "Glass",
            OptionD = "Metal",
            CorrectAnswer = QuizAnswerOption.A
        }).ToList();
        database.LearningGames.Add(new LearningGame
        {
            Id = Guid.NewGuid(),
            TeacherId = teacher.Id,
            GradeLevel = "5",
            Subject = "Science",
            Topic = "Plants",
            EnvironmentType = GameEnvironmentType.Indoor,
            ExpectedStudentCount = expectedStudentCount,
            Status = LearningGameStatus.Active,
            GameCode = gameCode,
            Tasks = tasks
        });
        await database.SaveChangesAsync();
    }

    public async Task<(int FailedCount, bool LockedOut)> GetLockoutStateAsync(string email)
    {
        await using var scope = Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        return (await users.GetAccessFailedCountAsync(user), await users.IsLockedOutAsync(user));
    }

    public async Task<DateTimeOffset?> GetLockoutEndAsync(string email)
    {
        await using var scope = Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        return await users.GetLockoutEndDateAsync(user);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
