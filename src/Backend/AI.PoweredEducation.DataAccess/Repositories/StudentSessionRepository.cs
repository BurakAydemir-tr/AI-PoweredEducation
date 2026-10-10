using AI.PoweredEducation.DataAccess.Persistence;
using AI.PoweredEducation.DataAccess.Repositories.Interfaces;
using AI.PoweredEducation.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

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

    public async Task<T> ExecuteWithSessionLockAsync<T>(
        string sessionTokenHash,
        Func<Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            _dbContext.Database.IsNpgsql() ? IsolationLevel.ReadCommitted : IsolationLevel.Serializable,
            cancellationToken);

        if (_dbContext.Database.IsNpgsql())
        {
            await using var command = _dbContext.Database.GetDbConnection().CreateCommand();
            command.Transaction = transaction.GetDbTransaction();
            command.CommandText = "SELECT \"Id\" FROM \"StudentSessions\" WHERE \"SessionTokenHash\" = @tokenHash FOR UPDATE";
            var tokenParameter = command.CreateParameter();
            tokenParameter.ParameterName = "tokenHash";
            tokenParameter.Value = sessionTokenHash;
            command.Parameters.Add(tokenParameter);
            await command.ExecuteScalarAsync(cancellationToken);
        }

        var result = await action();
        await transaction.CommitAsync(cancellationToken);
        return result;
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

    public async Task AddResultAsync(
        Result result,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Results.AddAsync(result, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
