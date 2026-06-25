using AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;

namespace AI.PoweredEducation.Business.ArtificialIntelligence.Interfaces;

public interface IAiService
{
    Task<IReadOnlyCollection<GeneratedQuizTask>> GenerateQuizTasksAsync(
        AiGenerationContext context,
        int taskCount,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<GeneratedQrCodeTask>> GenerateQrCodeTasksAsync(
        AiGenerationContext context,
        int taskCount,
        CancellationToken cancellationToken = default);
}
