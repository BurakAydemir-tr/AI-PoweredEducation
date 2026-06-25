using AI.PoweredEducation.Business.Common.Exceptions;
using AI.PoweredEducation.Business.Results.Dtos;
using AI.PoweredEducation.Business.Results.Interfaces;
using AI.PoweredEducation.DataAccess.Repositories.Interfaces;
using AI.PoweredEducation.Entity.Entities;
using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.Results.Services;

public sealed class ResultService : IResultService
{
    private readonly ILearningGameRepository _gameRepository;
    private readonly IStudentSessionRepository _sessionRepository;

    public ResultService(
        ILearningGameRepository gameRepository,
        IStudentSessionRepository sessionRepository)
    {
        _gameRepository = gameRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<GameResultsResponse> GetGameResultsAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetOwnedAsync(gameId, teacherId, cancellationToken)
            ?? throw new ResourceNotFoundException(nameof(LearningGame), gameId);
        var sessions = await _sessionRepository.GetAllForOwnedGameAsync(
            gameId,
            teacherId,
            cancellationToken);

        var ranked = sessions
            .Where(session => session.Result is not null)
            .OrderByDescending(session => session.Result!.TotalScore)
            .ThenBy(session => session.Result!.CompletionTime)
            .ThenBy(session => session.Result!.PlayedAt)
            .Select((session, index) => new StudentResultResponse(
                index + 1,
                session.Id,
                session.StudentName,
                session.EndReason,
                session.Result!.TotalScore,
                session.Result.CompletedTaskCount,
                session.Result.UnfinishedTaskCount,
                session.Result.TimedOutTaskCount,
                session.Result.CompletionTime,
                session.Result.PlayedAt))
            .ToArray();

        return new GameResultsResponse(
            game.Id,
            game.ExpectedStudentCount,
            sessions.Count,
            sessions.Count(session => session.EndReason == SessionEndReason.Completed),
            ranked);
    }
}
