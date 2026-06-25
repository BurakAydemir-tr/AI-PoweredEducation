using AI.PoweredEducation.Entity.Entities;

namespace AI.PoweredEducation.DataAccess.Repositories.Interfaces;

public interface IStudentSessionRepository
{
    Task<bool> ActiveStudentNameExistsAsync(
        Guid learningGameId,
        string normalizedStudentName,
        CancellationToken cancellationToken = default);

    Task<StudentSession?> GetByTokenHashWithProgressAsync(
        string sessionTokenHash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentSession>> GetFinishedForOwnedGameAsync(
        Guid learningGameId,
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentSession>> GetAllForOwnedGameAsync(
        Guid learningGameId,
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StudentSession studentSession,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
