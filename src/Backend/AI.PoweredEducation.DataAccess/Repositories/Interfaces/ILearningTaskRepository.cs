using AI.PoweredEducation.Entity.Entities;

namespace AI.PoweredEducation.DataAccess.Repositories.Interfaces;

public interface ILearningTaskRepository
{
    Task<LearningTask?> GetOwnedAsync(
        Guid taskId,
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        LearningTask learningTask,
        CancellationToken cancellationToken = default);

    void Remove(LearningTask learningTask);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
