using AI.PoweredEducation.Business.Results.Dtos;

namespace AI.PoweredEducation.Business.Results.Interfaces;

public interface IResultService
{
    Task<GameResultsResponse> GetGameResultsAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default);
}
