using AI.PoweredEducation.Business.LearningGames.Dtos;
using AI.PoweredEducation.Core.Common;

namespace AI.PoweredEducation.Business.LearningGames.Interfaces;

public interface ILearningGameService
{
    Task<Result<IReadOnlyCollection<LearningGameResponse>>> GetAllAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<Result<LearningGameResponse>> CreateAsync(
        Guid teacherId,
        CreateLearningGameRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<LearningGameResponse>> GetAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task<Result<LearningGameResponse>> UpdateAsync(
        Guid teacherId,
        Guid gameId,
        UpdateLearningGameRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<LearningGameResponse>> PublishAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task<Result<LearningGameResponse>> ActivateAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task<Result<LearningGameResponse>> DeactivateAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task<Result<LearningGameResponse>> ArchiveAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task<Result<LearningGameResponse>> RestoreArchivedAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task<Result<LearningGameResponse>> CloneAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default);
}
