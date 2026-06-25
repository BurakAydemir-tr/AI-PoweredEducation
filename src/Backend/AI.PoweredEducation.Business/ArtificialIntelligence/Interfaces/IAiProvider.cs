using AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;

namespace AI.PoweredEducation.Business.ArtificialIntelligence.Interfaces;

public interface IAiProvider
{
    string Name { get; }

    Task<IReadOnlyCollection<GeneratedQuizTask>> GenerateQuizTasksAsync(
        AiGenerationContext context,
        int taskCount,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<GeneratedQrCodeTask>> GenerateQrCodeTasksAsync(
        AiGenerationContext context,
        int taskCount,
        CancellationToken cancellationToken = default);
}
