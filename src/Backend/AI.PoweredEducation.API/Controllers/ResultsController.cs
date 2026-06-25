using AI.PoweredEducation.API.Security;
using AI.PoweredEducation.Business.Results.Dtos;
using AI.PoweredEducation.Business.Results.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.PoweredEducation.API.Controllers;

[ApiController]
[Authorize]
[Route("api/games/{gameId:guid}/results")]
public sealed class ResultsController : ControllerBase
{
    private readonly IResultService _service;

    public ResultsController(IResultService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<GameResultsResponse>> Get(
        Guid gameId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetGameResultsAsync(
            User.GetRequiredUserId(),
            gameId,
            cancellationToken));
}
