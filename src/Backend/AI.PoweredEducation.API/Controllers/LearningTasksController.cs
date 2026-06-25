using AI.PoweredEducation.API.Security;
using AI.PoweredEducation.Business.LearningGames.Dtos;
using AI.PoweredEducation.Business.LearningTasks.Dtos;
using AI.PoweredEducation.Business.LearningTasks.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.PoweredEducation.API.Controllers;

[ApiController]
[Authorize]
public sealed class LearningTasksController : ControllerBase
{
    private readonly ILearningTaskService _service;

    public LearningTasksController(ILearningTaskService service) => _service = service;

    [HttpPost("api/games/{gameId:guid}/tasks/quiz")]
    public async Task<ActionResult<LearningTaskResponse>> CreateQuiz(
        Guid gameId, CreateQuizTaskRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateQuizAsync(User.GetRequiredUserId(), gameId, request, cancellationToken));

    [HttpPost("api/games/{gameId:guid}/tasks/qr")]
    public async Task<ActionResult<LearningTaskResponse>> CreateQr(
        Guid gameId, CreateQrCodeTaskRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateQrCodeAsync(User.GetRequiredUserId(), gameId, request, cancellationToken));

    [HttpPost("api/games/{gameId:guid}/tasks/gps")]
    public async Task<ActionResult<LearningTaskResponse>> CreateGps(
        Guid gameId, CreateGpsTaskRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateGpsAsync(User.GetRequiredUserId(), gameId, request, cancellationToken));

    [HttpPut("api/tasks/{taskId:guid}/quiz")]
    public async Task<ActionResult<LearningTaskResponse>> UpdateQuiz(
        Guid taskId, CreateQuizTaskRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateQuizAsync(User.GetRequiredUserId(), taskId, request, cancellationToken));

    [HttpPut("api/tasks/{taskId:guid}/qr")]
    public async Task<ActionResult<LearningTaskResponse>> UpdateQr(
        Guid taskId, CreateQrCodeTaskRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateQrCodeAsync(User.GetRequiredUserId(), taskId, request, cancellationToken));

    [HttpPut("api/tasks/{taskId:guid}/gps")]
    public async Task<ActionResult<LearningTaskResponse>> UpdateGps(
        Guid taskId, CreateGpsTaskRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateGpsAsync(User.GetRequiredUserId(), taskId, request, cancellationToken));

    [HttpDelete("api/tasks/{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(User.GetRequiredUserId(), taskId, cancellationToken);
        return NoContent();
    }

    [HttpPut("api/games/{gameId:guid}/tasks/order")]
    public async Task<ActionResult<IReadOnlyCollection<LearningTaskResponse>>> Reorder(
        Guid gameId, ReorderLearningTasksRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.ReorderAsync(User.GetRequiredUserId(), gameId, request, cancellationToken));
}
