using AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;
using AI.PoweredEducation.Core.Common;

namespace AI.PoweredEducation.Business.ArtificialIntelligence.Interfaces;

public interface IAiService
{
    Task<Result<IReadOnlyCollection<GeneratedQuizTask>>> GenerateQuizTasksAsync(
        AiGenerationContext context,
        int taskCount,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<GeneratedQrCodeTask>>> GenerateQrCodeTasksAsync(
        AiGenerationContext context,
        int taskCount,
        CancellationToken cancellationToken = default);
}
