using AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;
using AI.PoweredEducation.Business.ArtificialIntelligence.Exceptions;
using AI.PoweredEducation.Business.ArtificialIntelligence.Interfaces;
using AI.PoweredEducation.Core.Common;

namespace AI.PoweredEducation.Business.ArtificialIntelligence.Services;

public sealed class AiService : IAiService
{
    private const int MinimumTaskCount = 1;
    private const int MaximumTaskCount = 20;

    private readonly IAiProvider _provider;

    public AiService(IAiProvider provider)
    {
        _provider = provider;
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
        catch (AiProviderException exception)
        {
            return Result.Failure<IReadOnlyCollection<GeneratedQuizTask>>(Error.ExternalService(
                "AI.ProviderFailed",
                exception.Message));
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
        catch (AiProviderException exception)
        {
            return Result.Failure<IReadOnlyCollection<GeneratedQrCodeTask>>(Error.ExternalService(
                "AI.ProviderFailed",
                exception.Message));
        }
    }

    private static Error? Validate(AiGenerationContext context, int taskCount)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(context.GradeLevel))
        {
            errors.Add("Grade level is required.");
        }

        if (string.IsNullOrWhiteSpace(context.Subject))
        {
            errors.Add("Subject is required.");
        }

        if (string.IsNullOrWhiteSpace(context.Topic))
        {
            errors.Add("Topic is required.");
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
