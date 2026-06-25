using AI.PoweredEducation.Entity.Entities;

namespace AI.PoweredEducation.DataAccess.Repositories.Interfaces;

public interface ILearningGameRepository
{
    Task<IReadOnlyList<LearningGame>> GetAllOwnedAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<LearningGame?> GetOwnedAsync(
        Guid gameId,
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<LearningGame?> GetOwnedWithTasksAsync(
        Guid gameId,
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<LearningGame?> GetActiveByCodeWithTasksAsync(
        string gameCode,
        CancellationToken cancellationToken = default);

    Task<bool> GameCodeExistsAsync(
        string gameCode,
        CancellationToken cancellationToken = default);

    Task<bool> HasResultsAsync(
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task<bool> HasUnfinishedSessionsAsync(
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        LearningGame learningGame,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
