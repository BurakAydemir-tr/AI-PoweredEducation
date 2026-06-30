using AI.PoweredEducation.Business.Results.Dtos;
using AI.PoweredEducation.Core.Common;

namespace AI.PoweredEducation.Business.Results.Interfaces;

public interface IResultService
{
    Task<Result<GameResultsResponse>> GetGameResultsAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default);
}
