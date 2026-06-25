using AI.PoweredEducation.DataAccess.Persistence;
using AI.PoweredEducation.DataAccess.Repositories.Interfaces;
using AI.PoweredEducation.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace AI.PoweredEducation.DataAccess.Repositories;

public sealed class LearningTaskRepository : ILearningTaskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public LearningTaskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LearningTask?> GetOwnedAsync(
        Guid taskId,
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningTasks
            .Include(task => task.LearningGame)
            .SingleOrDefaultAsync(
                task => task.Id == taskId && task.LearningGame.TeacherId == teacherId,
                cancellationToken);
    }

    public async Task AddAsync(
        LearningTask learningTask,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.LearningTasks.AddAsync(learningTask, cancellationToken);
    }

    public void Remove(LearningTask learningTask)
    {
        _dbContext.LearningTasks.Remove(learningTask);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
