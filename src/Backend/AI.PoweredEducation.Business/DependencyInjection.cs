using System.Text;
using AI.PoweredEducation.Business.ArtificialIntelligence.Configuration;
using AI.PoweredEducation.Business.ArtificialIntelligence.Interfaces;
using AI.PoweredEducation.Business.ArtificialIntelligence.Providers;
using AI.PoweredEducation.Business.ArtificialIntelligence.Services;
using AI.PoweredEducation.Business.Authentication.Configuration;
using AI.PoweredEducation.Business.Authentication.Interfaces;
using AI.PoweredEducation.Business.Authentication.Services;
using AI.PoweredEducation.Business.Authentication.Validators;
using AI.PoweredEducation.Business.LearningGames.Interfaces;
using AI.PoweredEducation.Business.LearningGames.Services;
using AI.PoweredEducation.Business.LearningTasks.Interfaces;
using AI.PoweredEducation.Business.LearningTasks.Services;
using AI.PoweredEducation.Business.StudentSessions.Interfaces;
using AI.PoweredEducation.Business.StudentSessions.Services;
using AI.PoweredEducation.Business.Results.Interfaces;
using AI.PoweredEducation.Business.Results.Services;
using AI.PoweredEducation.DataAccess;
using AI.PoweredEducation.DataAccess.Persistence;
using AI.PoweredEducation.Entity.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.PoweredEducation.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services.AddDataAccess(connectionString);

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "JWT issuer is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "JWT audience is required.")
            .Validate(
                options => Encoding.UTF8.GetByteCount(options.Secret) >=
                    JwtOptions.MinimumSecretLengthInBytes,
                "JWT secret must be at least 32 bytes.")
            .Validate(
                options => options.AccessTokenLifetimeMinutes == 15,
                "JWT access token lifetime must be 15 minutes.")
            .Validate(
                options => options.RefreshTokenLifetimeDays == 7,
                "Refresh token lifetime must be 7 days.")
            .ValidateOnStart();

        services.AddOptions<OpenAiOptions>()
            .Bind(configuration.GetSection(OpenAiOptions.SectionName));
        services.AddOptions<GeminiOptions>()
            .Bind(configuration.GetSection(GeminiOptions.SectionName));
        services.AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName));

        services.AddHttpClient<OpenAiProvider>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAiOptions>>()
                .Value;
            httpClient.BaseAddress = options.BaseUrl;
            httpClient.Timeout = TimeSpan.FromSeconds(60);
        });
        services.AddHttpClient<GeminiProvider>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<GeminiOptions>>()
                .Value;
            httpClient.BaseAddress = options.BaseUrl;
            httpClient.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<IAiProvider>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<AiOptions>>()
                .Value;

            return options.Provider.Trim().ToUpperInvariant() switch
            {
                "GEMINI" => serviceProvider.GetRequiredService<GeminiProvider>(),
                "OPENAI" => serviceProvider.GetRequiredService<OpenAiProvider>(),
                _ => throw new InvalidOperationException(
                    $"Unsupported AI provider '{options.Provider}'.")
            };
        });
        services.AddScoped<IAiService, AiService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ILearningGameService, LearningGameService>();
        services.AddScoped<ILearningTaskService, LearningTaskService>();
        services.AddScoped<IStudentSessionService, StudentSessionService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}
