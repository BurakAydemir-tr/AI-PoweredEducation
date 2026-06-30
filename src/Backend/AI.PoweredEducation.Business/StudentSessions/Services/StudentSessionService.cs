using AI.PoweredEducation.Business.Common.Exceptions;
using AI.PoweredEducation.Business.Common.Results;
using AI.PoweredEducation.Business.StudentSessions.Dtos;
using AI.PoweredEducation.Business.StudentSessions.Interfaces;
using AI.PoweredEducation.Business.StudentSessions.Mappings;
using AI.PoweredEducation.Core.Common;
using AI.PoweredEducation.Core.Security;
using AI.PoweredEducation.DataAccess.Repositories.Interfaces;
using AI.PoweredEducation.Entity.Entities;
using AI.PoweredEducation.Entity.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using DomainResult = AI.PoweredEducation.Entity.Entities.Result;

namespace AI.PoweredEducation.Business.StudentSessions.Services;

public sealed class StudentSessionService : IStudentSessionService
{
    private const double GpsCompletionRadiusMeters = 5;

    private readonly ILearningGameRepository _gameRepository;
    private readonly IStudentSessionRepository _sessionRepository;
    private readonly IValidator<JoinGameRequest> _joinValidator;
    private readonly IValidator<SubmitQuizAnswerRequest> _quizValidator;
    private readonly IValidator<ScanQrCodeRequest> _qrValidator;
    private readonly IValidator<CompleteGpsTaskRequest> _gpsValidator;

    public StudentSessionService(
        ILearningGameRepository gameRepository,
        IStudentSessionRepository sessionRepository,
        IValidator<JoinGameRequest> joinValidator,
        IValidator<SubmitQuizAnswerRequest> quizValidator,
        IValidator<ScanQrCodeRequest> qrValidator,
        IValidator<CompleteGpsTaskRequest> gpsValidator)
    {
        _gameRepository = gameRepository;
        _sessionRepository = sessionRepository;
        _joinValidator = joinValidator;
        _quizValidator = quizValidator;
        _qrValidator = qrValidator;
        _gpsValidator = gpsValidator;
    }

    public Task<Result<JoinGameResponse>> JoinAsync(
        JoinGameRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _joinValidator.ValidateAndThrowAsync(request, cancellationToken);
        var gameCode = request.GameCode.Trim().ToUpperInvariant();
        var studentName = request.StudentName.Trim();
        var normalizedName = studentName.ToUpperInvariant();

        var game = await _gameRepository.GetActiveByCodeWithTasksAsync(gameCode, cancellationToken)
            ?? throw new ResourceNotFoundException(nameof(LearningGame), gameCode);

        if (game.Tasks.Count == 0)
            throw new BusinessRuleException("The active game has no tasks.");
        if (await _sessionRepository.ActiveStudentNameExistsAsync(game.Id, normalizedName, cancellationToken))
            throw new BusinessRuleException("Student name is already in use in this game.");

        var now = DateTimeOffset.UtcNow;
        var rawSessionToken = SecureToken.Generate();
        var session = new StudentSession
        {
            Id = Guid.NewGuid(),
            LearningGameId = game.Id,
            StudentName = studentName,
            NormalizedStudentName = normalizedName,
            SessionTokenHash = SecureToken.Hash(rawSessionToken),
            StartedAt = now
        };

        foreach (var task in game.Tasks.OrderBy(task => task.Order))
        {
            session.TaskAttempts.Add(new TaskAttempt
            {
                Id = Guid.NewGuid(),
                StudentSessionId = session.Id,
                LearningTaskId = task.Id,
                LearningTask = task,
                Status = TaskAttemptStatus.NotStarted
            });
        }

        var firstAttempt = GetCurrentAttempt(session)!;
        firstAttempt.StartedAt = now;

        await _sessionRepository.AddAsync(session, cancellationToken);
        try
        {
            await _sessionRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new BusinessRuleException(
                "The student name is already in use in this game.");
        }

        return new JoinGameResponse(
            session.Id,
            session.StudentName,
            rawSessionToken,
            StudentSessionMapper.ToTaskResponse(firstAttempt.LearningTask));
    });

    public Task<Result<StudentProgressResponse>> GetProgressAsync(
        string sessionToken,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        var session = await GetSessionAsync(sessionToken, cancellationToken);
        return BuildProgress(session);
    });

    public Task<Result<StudentProgressResponse>> StartCurrentTaskAsync(
        string sessionToken,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        var session = await GetActiveSessionAsync(sessionToken, cancellationToken);
        var attempt = GetCurrentAttempt(session)
            ?? throw new BusinessRuleException("The session has no unfinished task.");
        attempt.StartedAt ??= DateTimeOffset.UtcNow;
        await _sessionRepository.SaveChangesAsync(cancellationToken);
        return BuildProgress(session);
    });

    public Task<Result<StudentProgressResponse>> SubmitQuizAnswerAsync(
        string sessionToken,
        SubmitQuizAnswerRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _quizValidator.ValidateAndThrowAsync(request, cancellationToken);
        var session = await GetActiveSessionAsync(sessionToken, cancellationToken);
        var attempt = RequireCurrentAttempt<QuizTask>(session);
        var quiz = (QuizTask)attempt.LearningTask;
        var now = DateTimeOffset.UtcNow;
        attempt.StartedAt ??= now;
        attempt.AttemptCount++;

        if (request.Answer == quiz.CorrectAnswer)
        {
            attempt.ScoreEarned = Math.Max(100 - ((attempt.AttemptCount - 1) * 25), 0);
            CompleteAttempt(attempt, now);
            return await AdvanceAndSaveAsync(session, cancellationToken, "Correct answer.");
        }

        if (attempt.AttemptCount >= 4)
        {
            attempt.ScoreEarned = 0;
            CompleteAttempt(attempt, now);
            return await AdvanceAndSaveAsync(
                session,
                cancellationToken,
                "Incorrect answer. The correct answer is shown.",
                quiz.CorrectAnswer);
        }

        await _sessionRepository.SaveChangesAsync(cancellationToken);
        return BuildProgress(session, "Incorrect answer. Try again.");
    });

    public Task<Result<StudentProgressResponse>> ScanQrCodeAsync(
        string sessionToken,
        ScanQrCodeRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _qrValidator.ValidateAndThrowAsync(request, cancellationToken);
        var session = await GetActiveSessionAsync(sessionToken, cancellationToken);
        var attempt = RequireCurrentAttempt<QrCodeTask>(session);
        var task = (QrCodeTask)attempt.LearningTask;
        var now = DateTimeOffset.UtcNow;
        attempt.StartedAt ??= now;

        if (HasTimedOut(attempt, task.TimeLimitMinutes, now))
        {
            TimeoutAttempt(attempt, now);
            return await AdvanceAndSaveAsync(session, cancellationToken, "Time is up.");
        }

        attempt.AttemptCount++;
        if (!string.Equals(request.QrPayload, task.QrPayload, StringComparison.Ordinal))
        {
            await _sessionRepository.SaveChangesAsync(cancellationToken);
            return BuildProgress(session, "Wrong QR code.");
        }

        attempt.ScoreEarned = 100;
        CompleteAttempt(attempt, now);
        return await AdvanceAndSaveAsync(session, cancellationToken, "QR task completed.");
    });

    public Task<Result<StudentProgressResponse>> CompleteGpsTaskAsync(
        string sessionToken,
        CompleteGpsTaskRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _gpsValidator.ValidateAndThrowAsync(request, cancellationToken);
        var session = await GetActiveSessionAsync(sessionToken, cancellationToken);
        var attempt = RequireCurrentAttempt<GpsTask>(session);
        var task = (GpsTask)attempt.LearningTask;
        var now = DateTimeOffset.UtcNow;
        attempt.StartedAt ??= now;

        if (HasTimedOut(attempt, task.TimeLimitMinutes, now))
        {
            TimeoutAttempt(attempt, now);
            return await AdvanceAndSaveAsync(session, cancellationToken, "Time is up.");
        }

        attempt.AttemptCount++;
        var distance = CalculateDistanceMeters(
            request.Latitude, request.Longitude,
            task.TargetLatitude, task.TargetLongitude);
        if (distance > GpsCompletionRadiusMeters)
        {
            await _sessionRepository.SaveChangesAsync(cancellationToken);
            return BuildProgress(session, "Target location has not been reached.");
        }

        attempt.ScoreEarned = 100;
        CompleteAttempt(attempt, now);
        return await AdvanceAndSaveAsync(session, cancellationToken, "Target reached.");
    });

    public Task<Result<StudentProgressResponse>> TimeoutCurrentTaskAsync(
        string sessionToken,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        var session = await GetActiveSessionAsync(sessionToken, cancellationToken);
        var attempt = GetCurrentAttempt(session)
            ?? throw new BusinessRuleException("The session has no unfinished task.");
        if (attempt.LearningTask is QuizTask)
            throw new BusinessRuleException("Quiz tasks cannot be timed out.");
        TimeoutAttempt(attempt, DateTimeOffset.UtcNow);
        return await AdvanceAndSaveAsync(session, cancellationToken, "Task timed out.");
    });

    public Task<Result<ResultResponse>> LeaveAsync(
        string sessionToken,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        var session = await GetActiveSessionAsync(sessionToken, cancellationToken);
        var result = await FinishSessionAsync(
            session,
            SessionEndReason.Left,
            DateTimeOffset.UtcNow,
            cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);
        return StudentSessionMapper.ToResultResponse(result);
    });

    private async Task<StudentProgressResponse> AdvanceAndSaveAsync(
        StudentSession session,
        CancellationToken cancellationToken,
        string feedback,
        QuizAnswerOption? revealedCorrectAnswer = null)
    {
        var now = DateTimeOffset.UtcNow;
        var nextAttempt = GetCurrentAttempt(session);
        if (nextAttempt is null)
        {
            await FinishSessionAsync(session, SessionEndReason.Completed, now, cancellationToken);
        }
        else
        {
            nextAttempt.StartedAt ??= now;
        }

        await _sessionRepository.SaveChangesAsync(cancellationToken);
        return BuildProgress(session, feedback, revealedCorrectAnswer);
    }

    private StudentProgressResponse BuildProgress(
        StudentSession session,
        string? feedback = null,
        QuizAnswerOption? revealedCorrectAnswer = null)
    {
        var currentAttempt = GetCurrentAttempt(session);
        return new StudentProgressResponse(
            session.Id,
            currentAttempt is null ? null : StudentSessionMapper.ToTaskResponse(currentAttempt.LearningTask),
            session.Result is null ? null : StudentSessionMapper.ToResultResponse(session.Result),
            feedback,
            revealedCorrectAnswer);
    }

    private async Task<StudentSession> GetSessionAsync(
        string rawToken,
        CancellationToken cancellationToken)
    {
        var tokenHash = SecureToken.Hash(rawToken);
        return await _sessionRepository.GetByTokenHashWithProgressAsync(tokenHash, cancellationToken)
            ?? throw new ResourceNotFoundException(nameof(StudentSession), "session token");
    }

    private async Task<StudentSession> GetActiveSessionAsync(
        string rawToken,
        CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(rawToken, cancellationToken);
        if (session.FinishedAt is not null)
            throw new BusinessRuleException("The student session has ended.");
        return session;
    }

    private static TaskAttempt? GetCurrentAttempt(StudentSession session) =>
        session.TaskAttempts
            .Where(attempt => attempt.Status == TaskAttemptStatus.NotStarted)
            .OrderBy(attempt => attempt.LearningTask.Order)
            .FirstOrDefault();

    private static TaskAttempt RequireCurrentAttempt<TTask>(StudentSession session)
        where TTask : LearningTask
    {
        var attempt = GetCurrentAttempt(session)
            ?? throw new BusinessRuleException("The session has no unfinished task.");
        if (attempt.LearningTask is not TTask)
            throw new BusinessRuleException($"The current task is not a {typeof(TTask).Name}.");
        return attempt;
    }

    private static void CompleteAttempt(TaskAttempt attempt, DateTimeOffset completedAt)
    {
        attempt.Status = TaskAttemptStatus.Completed;
        attempt.CompletedAt = completedAt;
    }

    private static void TimeoutAttempt(TaskAttempt attempt, DateTimeOffset completedAt)
    {
        attempt.Status = TaskAttemptStatus.TimedOut;
        attempt.ScoreEarned = 0;
        attempt.CompletedAt = completedAt;
        attempt.StartedAt ??= completedAt;
    }

    private static bool HasTimedOut(
        TaskAttempt attempt,
        int timeLimitMinutes,
        DateTimeOffset now) =>
        attempt.StartedAt.HasValue &&
        now - attempt.StartedAt.Value >= TimeSpan.FromMinutes(timeLimitMinutes);

    private async Task<DomainResult> FinishSessionAsync(
        StudentSession session,
        SessionEndReason reason,
        DateTimeOffset finishedAt,
        CancellationToken cancellationToken)
    {
        session.FinishedAt = finishedAt;
        session.EndReason = reason;
        var result = new DomainResult
        {
            Id = Guid.NewGuid(),
            StudentSessionId = session.Id,
            TotalScore = session.TaskAttempts.Sum(attempt => attempt.ScoreEarned),
            CompletionTime = finishedAt - session.StartedAt,
            CompletedTaskCount = session.TaskAttempts.Count(attempt => attempt.Status == TaskAttemptStatus.Completed),
            UnfinishedTaskCount = session.TaskAttempts.Count(attempt => attempt.Status == TaskAttemptStatus.NotStarted),
            TimedOutTaskCount = session.TaskAttempts.Count(attempt => attempt.Status == TaskAttemptStatus.TimedOut),
            PlayedAt = session.StartedAt
        };
        session.Result = result;
        await _sessionRepository.AddResultAsync(result, cancellationToken);
        return result;
    }

    private static double CalculateDistanceMeters(
        double latitude1, double longitude1,
        double latitude2, double longitude2)
    {
        const double earthRadiusMeters = 6_371_000;
        var latitudeDelta = DegreesToRadians(latitude2 - latitude1);
        var longitudeDelta = DegreesToRadians(longitude2 - longitude1);
        var firstLatitude = DegreesToRadians(latitude1);
        var secondLatitude = DegreesToRadians(latitude2);
        var haversine = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
            Math.Cos(firstLatitude) * Math.Cos(secondLatitude) *
            Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
        return earthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
