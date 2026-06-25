using AI.PoweredEducation.DataAccess.Persistence;
using AI.PoweredEducation.DataAccess.Repositories.Interfaces;
using AI.PoweredEducation.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace AI.PoweredEducation.DataAccess.Repositories;

public sealed class StudentSessionRepository : IStudentSessionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StudentSessionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ActiveStudentNameExistsAsync(
        Guid learningGameId,
        string normalizedStudentName,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.StudentSessions.AnyAsync(
            session => session.LearningGameId == learningGameId &&
                session.NormalizedStudentName == normalizedStudentName &&
                session.FinishedAt == null,
            cancellationToken);
    }

    public Task<StudentSession?> GetByTokenHashWithProgressAsync(
        string sessionTokenHash,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.StudentSessions
            .Include(session => session.LearningGame)
                .ThenInclude(game => game.Tasks.OrderBy(task => task.Order))
            .Include(session => session.TaskAttempts)
                .ThenInclude(attempt => attempt.LearningTask)
            .Include(session => session.Result)
            .SingleOrDefaultAsync(
                session => session.SessionTokenHash == sessionTokenHash,
                cancellationToken);
    }

    public async Task<IReadOnlyList<StudentSession>> GetFinishedForOwnedGameAsync(
        Guid learningGameId,
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.StudentSessions
            .AsNoTracking()
            .Include(session => session.Result)
            .Where(session => session.LearningGameId == learningGameId &&
                session.LearningGame.TeacherId == teacherId &&
                session.FinishedAt != null)
            .OrderByDescending(session => session.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentSession>> GetAllForOwnedGameAsync(
        Guid learningGameId,
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.StudentSessions
            .AsNoTracking()
            .Include(session => session.Result)
            .Include(session => session.TaskAttempts)
            .Where(session => session.LearningGameId == learningGameId &&
                session.LearningGame.TeacherId == teacherId)
            .OrderByDescending(session => session.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        StudentSession studentSession,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.StudentSessions.AddAsync(studentSession, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
