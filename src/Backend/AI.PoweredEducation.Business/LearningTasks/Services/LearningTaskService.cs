using AI.PoweredEducation.Business.Common.Exceptions;
using AI.PoweredEducation.Business.Common.Results;
using AI.PoweredEducation.Business.LearningGames.Dtos;
using AI.PoweredEducation.Business.LearningGames.Mappings;
using AI.PoweredEducation.Business.LearningTasks.Dtos;
using AI.PoweredEducation.Business.LearningTasks.Interfaces;
using AI.PoweredEducation.Core.Common;
using AI.PoweredEducation.Core.Security;
using AI.PoweredEducation.DataAccess.Repositories.Interfaces;
using AI.PoweredEducation.Entity.Entities;
using AI.PoweredEducation.Entity.Enums;
using FluentValidation;
using CoreResult = AI.PoweredEducation.Core.Common.Result;

namespace AI.PoweredEducation.Business.LearningTasks.Services;

public sealed class LearningTaskService : ILearningTaskService
{
    private readonly ILearningGameRepository _gameRepository;
    private readonly ILearningTaskRepository _taskRepository;
    private readonly IValidator<CreateQuizTaskRequest> _quizValidator;
    private readonly IValidator<CreateQrCodeTaskRequest> _qrValidator;
    private readonly IValidator<CreateGpsTaskRequest> _gpsValidator;
    private readonly IValidator<ReorderLearningTasksRequest> _reorderValidator;

    public LearningTaskService(
        ILearningGameRepository gameRepository,
        ILearningTaskRepository taskRepository,
        IValidator<CreateQuizTaskRequest> quizValidator,
        IValidator<CreateQrCodeTaskRequest> qrValidator,
        IValidator<CreateGpsTaskRequest> gpsValidator,
        IValidator<ReorderLearningTasksRequest> reorderValidator)
    {
        _gameRepository = gameRepository;
        _taskRepository = taskRepository;
        _quizValidator = quizValidator;
        _qrValidator = qrValidator;
        _gpsValidator = gpsValidator;
        _reorderValidator = reorderValidator;
    }

    public Task<Result<LearningTaskResponse>> CreateQuizAsync(
        Guid teacherId, Guid gameId, CreateQuizTaskRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _quizValidator.ValidateAndThrowAsync(request, cancellationToken);
        var game = await GetEditableGameAsync(gameId, teacherId, cancellationToken);
        var task = new QuizTask
        {
            Id = Guid.NewGuid(),
            LearningGameId = game.Id,
            Order = NextOrder(game),
            Question = request.Question.Trim(),
            OptionA = request.OptionA.Trim(),
            OptionB = request.OptionB.Trim(),
            OptionC = request.OptionC.Trim(),
            OptionD = request.OptionD.Trim(),
            CorrectAnswer = request.CorrectAnswer
        };
        return await AddAsync(task, cancellationToken);
    });

    public Task<Result<LearningTaskResponse>> CreateQrCodeAsync(
        Guid teacherId, Guid gameId, CreateQrCodeTaskRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _qrValidator.ValidateAndThrowAsync(request, cancellationToken);
        var game = await GetEditableGameAsync(gameId, teacherId, cancellationToken);
        var task = new QrCodeTask
        {
            Id = Guid.NewGuid(),
            LearningGameId = game.Id,
            Order = NextOrder(game),
            Instructions = request.Instructions.Trim(),
            QrPayload = NormalizeQrPayload(request.QrPayload) ?? CreateQrPayload(),
            TimeLimitMinutes = request.TimeLimitMinutes
        };
        return await AddAsync(task, cancellationToken);
    });

    public Task<Result<LearningTaskResponse>> CreateGpsAsync(
        Guid teacherId, Guid gameId, CreateGpsTaskRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _gpsValidator.ValidateAndThrowAsync(request, cancellationToken);
        var game = await GetEditableGameAsync(gameId, teacherId, cancellationToken);
        var task = new GpsTask
        {
            Id = Guid.NewGuid(),
            LearningGameId = game.Id,
            Order = NextOrder(game),
            Instructions = request.Instructions.Trim(),
            TargetLatitude = request.TargetLatitude,
            TargetLongitude = request.TargetLongitude,
            GameAreaJson = request.GameAreaJson,
            TimeLimitMinutes = request.TimeLimitMinutes
        };
        return await AddAsync(task, cancellationToken);
    });

    public Task<Result<LearningTaskResponse>> UpdateQuizAsync(
        Guid teacherId, Guid taskId, CreateQuizTaskRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _quizValidator.ValidateAndThrowAsync(request, cancellationToken);
        var task = await GetEditableTaskAsync<QuizTask>(taskId, teacherId, cancellationToken);
        task.Question = request.Question.Trim();
        task.OptionA = request.OptionA.Trim();
        task.OptionB = request.OptionB.Trim();
        task.OptionC = request.OptionC.Trim();
        task.OptionD = request.OptionD.Trim();
        task.CorrectAnswer = request.CorrectAnswer;
        return await SaveAsync(task, cancellationToken);
    });

    public Task<Result<LearningTaskResponse>> UpdateQrCodeAsync(
        Guid teacherId, Guid taskId, CreateQrCodeTaskRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _qrValidator.ValidateAndThrowAsync(request, cancellationToken);
        var task = await GetEditableTaskAsync<QrCodeTask>(taskId, teacherId, cancellationToken);
        task.Instructions = request.Instructions.Trim();
        task.QrPayload = NormalizeQrPayload(request.QrPayload) ?? task.QrPayload;
        task.TimeLimitMinutes = request.TimeLimitMinutes;
        return await SaveAsync(task, cancellationToken);
    });

    public Task<Result<LearningTaskResponse>> UpdateGpsAsync(
        Guid teacherId, Guid taskId, CreateGpsTaskRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _gpsValidator.ValidateAndThrowAsync(request, cancellationToken);
        var task = await GetEditableTaskAsync<GpsTask>(taskId, teacherId, cancellationToken);
        task.Instructions = request.Instructions.Trim();
        task.TargetLatitude = request.TargetLatitude;
        task.TargetLongitude = request.TargetLongitude;
        task.GameAreaJson = request.GameAreaJson;
        task.TimeLimitMinutes = request.TimeLimitMinutes;
        return await SaveAsync(task, cancellationToken);
    });

    public Task<CoreResult> DeleteAsync(
        Guid teacherId, Guid taskId,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        var task = await GetEditableTaskAsync<LearningTask>(taskId, teacherId, cancellationToken);
        var game = await GetEditableGameAsync(task.LearningGameId, teacherId, cancellationToken);
        _taskRepository.Remove(task);
        foreach (var subsequent in game.Tasks.Where(candidate => candidate.Order > task.Order))
        {
            subsequent.Order--;
        }
        await _taskRepository.SaveChangesAsync(cancellationToken);
    });

    public Task<Result<IReadOnlyCollection<LearningTaskResponse>>> ReorderAsync(
        Guid teacherId, Guid gameId, ReorderLearningTasksRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _reorderValidator.ValidateAndThrowAsync(request, cancellationToken);
        var game = await GetEditableGameAsync(gameId, teacherId, cancellationToken);
        if (request.TaskIds.Count != game.Tasks.Count ||
            request.TaskIds.Any(id => game.Tasks.All(task => task.Id != id)))
        {
            throw new BusinessRuleException("TaskIds must contain every task in the game exactly once.");
        }

        for (var index = 0; index < request.TaskIds.Count; index++)
        {
            game.Tasks.Single(task => task.Id == request.TaskIds[index]).Order = index + 1;
        }

        await _gameRepository.SaveChangesAsync(cancellationToken);
        return (IReadOnlyCollection<LearningTaskResponse>)game.Tasks
            .OrderBy(task => task.Order)
            .Select(LearningGameMapper.ToTaskResponse)
            .ToArray();
    });

    private async Task<LearningTaskResponse> AddAsync(
        LearningTask task, CancellationToken cancellationToken)
    {
        await _taskRepository.AddAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);
        return LearningGameMapper.ToTaskResponse(task);
    }

    private async Task<LearningTaskResponse> SaveAsync(
        LearningTask task, CancellationToken cancellationToken)
    {
        await _taskRepository.SaveChangesAsync(cancellationToken);
        return LearningGameMapper.ToTaskResponse(task);
    }

    private async Task<TTask> GetEditableTaskAsync<TTask>(
        Guid taskId, Guid teacherId, CancellationToken cancellationToken)
        where TTask : LearningTask
    {
        var task = await _taskRepository.GetOwnedAsync(taskId, teacherId, cancellationToken)
            ?? throw new ResourceNotFoundException(nameof(LearningTask), taskId);

        if (task is not TTask typedTask)
        {
            throw new BusinessRuleException($"Task is not a {typeof(TTask).Name}.");
        }

        await EnsureEditableAsync(task.LearningGame, cancellationToken);
        return typedTask;
    }

    private async Task<LearningGame> GetEditableGameAsync(
        Guid gameId, Guid teacherId, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetOwnedWithTasksAsync(gameId, teacherId, cancellationToken)
            ?? throw new ResourceNotFoundException(nameof(LearningGame), gameId);
        await EnsureEditableAsync(game, cancellationToken);
        return game;
    }

    private async Task EnsureEditableAsync(
        LearningGame game, CancellationToken cancellationToken)
    {
        if (game.Status is not (LearningGameStatus.Draft or LearningGameStatus.Inactive))
            throw new BusinessRuleException("Tasks can only be edited in draft or inactive games.");
        if (await _gameRepository.HasResultsAsync(game.Id, cancellationToken))
            throw new BusinessRuleException("Games with results must be cloned before editing tasks.");
        if (await _gameRepository.HasUnfinishedSessionsAsync(game.Id, cancellationToken))
            throw new BusinessRuleException("Tasks cannot be edited while student sessions are unfinished.");
    }

    private static int NextOrder(LearningGame game) =>
        game.Tasks.Count == 0 ? 1 : game.Tasks.Max(task => task.Order) + 1;

    private static string CreateQrPayload() => $"TASK-{SecureToken.Generate()}";

    private static string? NormalizeQrPayload(string? qrPayload)
    {
        return string.IsNullOrWhiteSpace(qrPayload)
            ? null
            : qrPayload.Trim();
    }
}
