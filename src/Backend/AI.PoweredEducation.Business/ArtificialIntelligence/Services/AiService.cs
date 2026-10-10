using AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;
using AI.PoweredEducation.Business.ArtificialIntelligence.Interfaces;
using AI.PoweredEducation.Core.Common;
using Microsoft.Extensions.Logging;

namespace AI.PoweredEducation.Business.ArtificialIntelligence.Services;

public sealed class AiService : IAiService
{
    private const int MinimumTaskCount = 1;
    private const int MaximumTaskCount = 20;
    private const int MaximumGradeLevelLength = 50;
    private const int MaximumSubjectLength = 100;
    private const int MaximumTopicLength = 200;

    private readonly IAiProvider _provider;
    private readonly ILogger<AiService> _logger;

    public AiService(IAiProvider provider, ILogger<AiService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyCollection<GeneratedQuizTask>>> GenerateQuizTasksAsync(
        AiGenerationContext context,
        int taskCount,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(context, taskCount);
        if (validationError is not null)
        {
            return Result.Failure<IReadOnlyCollection<GeneratedQuizTask>>(validationError);
        }

        try
        {
            return Result.Success(await _provider.GenerateQuizTasksAsync(
                Normalize(context),
                taskCount,
                cancellationToken));
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(exception, "AI quiz task generation failed.");
            return Result.Failure<IReadOnlyCollection<GeneratedQuizTask>>(Error.ExternalService(
                "AI.ProviderFailed",
                "AI generation is temporarily unavailable."));
        }
    }

    public async Task<Result<IReadOnlyCollection<GeneratedQrCodeTask>>> GenerateQrCodeTasksAsync(
        AiGenerationContext context,
        int taskCount,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(context, taskCount);
        if (validationError is not null)
        {
            return Result.Failure<IReadOnlyCollection<GeneratedQrCodeTask>>(validationError);
        }

        try
        {
            return Result.Success(await _provider.GenerateQrCodeTasksAsync(
                Normalize(context),
                taskCount,
                cancellationToken));
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(exception, "AI QR code task generation failed.");
            return Result.Failure<IReadOnlyCollection<GeneratedQrCodeTask>>(Error.ExternalService(
                "AI.ProviderFailed",
                "AI generation is temporarily unavailable."));
        }
    }

    private static Error? Validate(AiGenerationContext context, int taskCount)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(context.GradeLevel))
        {
            errors.Add("Grade level is required.");
        }
        else if (context.GradeLevel.Length > MaximumGradeLevelLength)
        {
            errors.Add($"Grade level must be at most {MaximumGradeLevelLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(context.Subject))
        {
            errors.Add("Subject is required.");
        }
        else if (context.Subject.Length > MaximumSubjectLength)
        {
            errors.Add($"Subject must be at most {MaximumSubjectLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(context.Topic))
        {
            errors.Add("Topic is required.");
        }
        else if (context.Topic.Length > MaximumTopicLength)
        {
            errors.Add($"Topic must be at most {MaximumTopicLength} characters.");
        }

        if (context.ExpectedStudentCount <= 0)
        {
            errors.Add("Expected student count must be greater than zero.");
        }

        if (taskCount is < MinimumTaskCount or > MaximumTaskCount)
        {
            errors.Add($"Task count must be between {MinimumTaskCount} and {MaximumTaskCount}.");
        }

        return errors.Count == 0
            ? null
            : Error.Validation("AI.ValidationFailed", "AI generation request is invalid.", errors);
    }

    private static AiGenerationContext Normalize(AiGenerationContext context) =>
        new(
            context.GradeLevel.Trim(),
            context.Subject.Trim(),
            context.Topic.Trim(),
            context.EnvironmentType,
            context.ExpectedStudentCount);
}
