using AI.PoweredEducation.API.Results;
using AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;
using AI.PoweredEducation.Business.ArtificialIntelligence.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.PoweredEducation.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ai")]
public sealed class ArtificialIntelligenceController : ControllerBase
{
    private readonly IAiService _aiService;

    public ArtificialIntelligenceController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("quiz-tasks")]
    public async Task<ActionResult<IReadOnlyCollection<GeneratedQuizTask>>> GenerateQuizTasks(
        AiGenerationRequest request,
        CancellationToken cancellationToken) =>
        (await _aiService.GenerateQuizTasksAsync(
            request.ToContext(),
            request.TaskCount,
            cancellationToken))
        .ToActionResult(this);

    [HttpPost("qr-code-tasks")]
    public async Task<ActionResult<IReadOnlyCollection<GeneratedQrCodeTask>>> GenerateQrCodeTasks(
        AiGenerationRequest request,
        CancellationToken cancellationToken) =>
        (await _aiService.GenerateQrCodeTasksAsync(
            request.ToContext(),
            request.TaskCount,
            cancellationToken))
        .ToActionResult(this);
}
