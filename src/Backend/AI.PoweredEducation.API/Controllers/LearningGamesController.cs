using AI.PoweredEducation.API.Security;
using AI.PoweredEducation.Business.LearningGames.Dtos;
using AI.PoweredEducation.Business.LearningGames.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.PoweredEducation.API.Controllers;

[ApiController]
[Authorize]
[Route("api/games")]
public sealed class LearningGamesController : ControllerBase
{
    private readonly ILearningGameService _service;

    public LearningGamesController(ILearningGameService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LearningGameResponse>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(await _service.GetAllAsync(User.GetRequiredUserId(), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<LearningGameResponse>> Create(
        CreateLearningGameRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateAsync(User.GetRequiredUserId(), request, cancellationToken));

    [HttpGet("{gameId:guid}")]
    public async Task<ActionResult<LearningGameResponse>> Get(
        Guid gameId, CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(User.GetRequiredUserId(), gameId, cancellationToken));

    [HttpPut("{gameId:guid}")]
    public async Task<ActionResult<LearningGameResponse>> Update(
        Guid gameId, UpdateLearningGameRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(User.GetRequiredUserId(), gameId, request, cancellationToken));

    [HttpPost("{gameId:guid}/publish")]
    public async Task<ActionResult<LearningGameResponse>> Publish(Guid gameId, CancellationToken cancellationToken) =>
        Ok(await _service.PublishAsync(User.GetRequiredUserId(), gameId, cancellationToken));

    [HttpPost("{gameId:guid}/activate")]
    public async Task<ActionResult<LearningGameResponse>> Activate(Guid gameId, CancellationToken cancellationToken) =>
        Ok(await _service.ActivateAsync(User.GetRequiredUserId(), gameId, cancellationToken));

    [HttpPost("{gameId:guid}/deactivate")]
    public async Task<ActionResult<LearningGameResponse>> Deactivate(Guid gameId, CancellationToken cancellationToken) =>
        Ok(await _service.DeactivateAsync(User.GetRequiredUserId(), gameId, cancellationToken));

    [HttpPost("{gameId:guid}/archive")]
    public async Task<ActionResult<LearningGameResponse>> Archive(Guid gameId, CancellationToken cancellationToken) =>
        Ok(await _service.ArchiveAsync(User.GetRequiredUserId(), gameId, cancellationToken));

    [HttpPost("{gameId:guid}/restore")]
    public async Task<ActionResult<LearningGameResponse>> Restore(Guid gameId, CancellationToken cancellationToken) =>
        Ok(await _service.RestoreArchivedAsync(User.GetRequiredUserId(), gameId, cancellationToken));

    [HttpPost("{gameId:guid}/clone")]
    public async Task<ActionResult<LearningGameResponse>> Clone(Guid gameId, CancellationToken cancellationToken) =>
        Ok(await _service.CloneAsync(User.GetRequiredUserId(), gameId, cancellationToken));
}
