using AI.PoweredEducation.DataAccess.Persistence;
using AI.PoweredEducation.DataAccess.Repositories;
using AI.PoweredEducation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AI.PoweredEducation.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ILearningGameRepository, LearningGameRepository>();
        services.AddScoped<ILearningTaskRepository, LearningTaskRepository>();
        services.AddScoped<IStudentSessionRepository, StudentSessionRepository>();

        return services;
    }
}
