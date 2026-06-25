using AI.PoweredEducation.DataAccess.Persistence;
using AI.PoweredEducation.DataAccess.Repositories.Interfaces;
using AI.PoweredEducation.Entity.Entities;
using AI.PoweredEducation.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace AI.PoweredEducation.DataAccess.Repositories;

public sealed class LearningGameRepository : ILearningGameRepository
{
    private readonly ApplicationDbContext _dbContext;

    public LearningGameRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LearningGame>> GetAllOwnedAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LearningGames
            .AsNoTracking()
            .Include(game => game.Tasks.OrderBy(task => task.Order))
            .Where(game => game.TeacherId == teacherId)
            .OrderByDescending(game => game.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<LearningGame?> GetOwnedAsync(
        Guid gameId,
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningGames.SingleOrDefaultAsync(
            game => game.Id == gameId && game.TeacherId == teacherId,
            cancellationToken);
    }

    public Task<LearningGame?> GetOwnedWithTasksAsync(
        Guid gameId,
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningGames
            .Include(game => game.Tasks.OrderBy(task => task.Order))
            .SingleOrDefaultAsync(
                game => game.Id == gameId && game.TeacherId == teacherId,
                cancellationToken);
    }

    public Task<LearningGame?> GetActiveByCodeWithTasksAsync(
        string gameCode,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningGames
            .Include(game => game.Tasks.OrderBy(task => task.Order))
            .SingleOrDefaultAsync(
                game => game.GameCode == gameCode &&
                    game.Status == LearningGameStatus.Active,
                cancellationToken);
    }

    public Task<bool> GameCodeExistsAsync(
        string gameCode,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningGames.AnyAsync(
            game => game.GameCode == gameCode,
            cancellationToken);
    }

    public Task<bool> HasResultsAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Results.AnyAsync(
            result => result.StudentSession.LearningGameId == gameId,
            cancellationToken);
    }

    public Task<bool> HasUnfinishedSessionsAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.StudentSessions.AnyAsync(
            session => session.LearningGameId == gameId && session.FinishedAt == null,
            cancellationToken);
    }

    public async Task AddAsync(
        LearningGame learningGame,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.LearningGames.AddAsync(learningGame, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
