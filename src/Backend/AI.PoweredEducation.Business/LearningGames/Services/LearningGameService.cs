using AI.PoweredEducation.Business.Common.Exceptions;
using AI.PoweredEducation.Business.Common.Results;
using AI.PoweredEducation.Business.LearningGames.Dtos;
using AI.PoweredEducation.Business.LearningGames.Interfaces;
using AI.PoweredEducation.Business.LearningGames.Mappings;
using AI.PoweredEducation.Core.Common;
using AI.PoweredEducation.Core.Security;
using AI.PoweredEducation.DataAccess.Repositories.Interfaces;
using AI.PoweredEducation.Entity.Entities;
using AI.PoweredEducation.Entity.Enums;
using FluentValidation;

namespace AI.PoweredEducation.Business.LearningGames.Services;

public sealed class LearningGameService : ILearningGameService
{
    private const int MaximumGameCodeGenerationAttempts = 10;

    private readonly ILearningGameRepository _repository;
    private readonly IValidator<CreateLearningGameRequest> _createValidator;
    private readonly IValidator<UpdateLearningGameRequest> _updateValidator;

    public LearningGameService(
        ILearningGameRepository repository,
        IValidator<CreateLearningGameRequest> createValidator,
        IValidator<UpdateLearningGameRequest> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public Task<Result<IReadOnlyCollection<LearningGameResponse>>> GetAllAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        var games = await _repository.GetAllOwnedAsync(teacherId, cancellationToken);
        return (IReadOnlyCollection<LearningGameResponse>)games.Select(LearningGameMapper.ToResponse).ToArray();
    });

    public Task<Result<LearningGameResponse>> CreateAsync(
        Guid teacherId,
        CreateLearningGameRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var game = LearningGameMapper.ToEntity(request);
        game.Id = Guid.NewGuid();
        game.TeacherId = teacherId;
        game.Status = LearningGameStatus.Draft;
        game.GameCode = await GenerateUniqueGameCodeAsync(cancellationToken);

        await _repository.AddAsync(game, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return LearningGameMapper.ToResponse(game);
    });

    public Task<Result<LearningGameResponse>> GetAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        var game = await GetOwnedWithTasksAsync(gameId, teacherId, cancellationToken);
        return LearningGameMapper.ToResponse(game);
    });

    public Task<Result<LearningGameResponse>> UpdateAsync(
        Guid teacherId,
        Guid gameId,
        UpdateLearningGameRequest request,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var game = await GetOwnedWithTasksAsync(gameId, teacherId, cancellationToken);
        await EnsureEditableAsync(game, cancellationToken);

        LearningGameMapper.Apply(request, game);
        await _repository.SaveChangesAsync(cancellationToken);

        return LearningGameMapper.ToResponse(game);
    });

    public Task<Result<LearningGameResponse>> PublishAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        var game = await GetOwnedWithTasksAsync(gameId, teacherId, cancellationToken);

        if (game.Status != LearningGameStatus.Draft)
        {
            throw new BusinessRuleException("Only draft games can be published.");
        }

        if (game.Tasks.Count == 0)
        {
            throw new BusinessRuleException("A game must contain at least one task before publication.");
        }

        game.Status = LearningGameStatus.Inactive;
        await _repository.SaveChangesAsync(cancellationToken);

        return LearningGameMapper.ToResponse(game);
    });

    public Task<Result<LearningGameResponse>> ActivateAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default)
        => BusinessResult.FromAsync(async () =>
    {
        var game = await GetOwnedWithTasksAsync(gameId, teacherId, cancellationToken);

        if (game.Status is not (LearningGameStatus.Draft or LearningGameStatus.Inactive))
        {
            throw new BusinessRuleException("Only draft or inactive games can be activated.");
        }

        if (game.Tasks.Count == 0)
        {
            throw new BusinessRuleException("A game must contain at least one task before activation.");
        }

        game.Status = LearningGameStatus.Active;
        await _repository.SaveChangesAsync(cancellationToken);

        return LearningGameMapper.ToResponse(game);
    });

    public Task<Result<LearningGameResponse>> DeactivateAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        return BusinessResult.FromAsync(() => ChangeStatusAsync(
            teacherId,
            gameId,
            LearningGameStatus.Active,
            LearningGameStatus.Inactive,
            cancellationToken));
    }

    public Task<Result<LearningGameResponse>> ArchiveAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        var game = await GetOwnedWithTasksAsync(gameId, teacherId, cancellationToken);

        if (game.Status == LearningGameStatus.Active)
        {
            throw new BusinessRuleException("An active game must be deactivated before it can be archived.");
        }

        game.Status = LearningGameStatus.Archived;
        await _repository.SaveChangesAsync(cancellationToken);

        return LearningGameMapper.ToResponse(game);
    });

    public Task<Result<LearningGameResponse>> RestoreArchivedAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        return BusinessResult.FromAsync(() => ChangeStatusAsync(
            teacherId,
            gameId,
            LearningGameStatus.Archived,
            LearningGameStatus.Inactive,
            cancellationToken));
    }

    public Task<Result<LearningGameResponse>> CloneAsync(
        Guid teacherId,
        Guid gameId,
        CancellationToken cancellationToken = default) =>
        BusinessResult.FromAsync(async () =>
    {
        var source = await GetOwnedWithTasksAsync(gameId, teacherId, cancellationToken);
        var clone = new LearningGame
        {
            Id = Guid.NewGuid(),
            TeacherId = teacherId,
            GradeLevel = source.GradeLevel,
            Subject = source.Subject,
            Topic = source.Topic,
            EnvironmentType = source.EnvironmentType,
            ExpectedStudentCount = source.ExpectedStudentCount,
            Status = LearningGameStatus.Draft,
            GameCode = await GenerateUniqueGameCodeAsync(cancellationToken)
        };

        foreach (var task in source.Tasks.OrderBy(task => task.Order))
        {
            clone.Tasks.Add(CloneTask(task, clone.Id));
        }

        await _repository.AddAsync(clone, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return LearningGameMapper.ToResponse(clone);
    });

    private async Task<LearningGameResponse> ChangeStatusAsync(
        Guid teacherId,
        Guid gameId,
        LearningGameStatus requiredStatus,
        LearningGameStatus targetStatus,
        CancellationToken cancellationToken)
    {
        var game = await GetOwnedWithTasksAsync(gameId, teacherId, cancellationToken);

        if (game.Status != requiredStatus)
        {
            throw new BusinessRuleException(
                $"Game must be {requiredStatus} before changing to {targetStatus}.");
        }

        game.Status = targetStatus;
        await _repository.SaveChangesAsync(cancellationToken);

        return LearningGameMapper.ToResponse(game);
    }

    private async Task EnsureEditableAsync(
        LearningGame game,
        CancellationToken cancellationToken)
    {
        if (game.Status is not (LearningGameStatus.Draft or LearningGameStatus.Inactive))
        {
            throw new BusinessRuleException("Only draft or inactive games can be edited.");
        }

        if (await _repository.HasResultsAsync(game.Id, cancellationToken))
        {
            throw new BusinessRuleException("Games with results must be cloned before editing.");
        }

        if (await _repository.HasUnfinishedSessionsAsync(game.Id, cancellationToken))
        {
            throw new BusinessRuleException("A game cannot be edited while student sessions are unfinished.");
        }
    }

    private async Task<LearningGame> GetOwnedWithTasksAsync(
        Guid gameId,
        Guid teacherId,
        CancellationToken cancellationToken)
    {
        return await _repository.GetOwnedWithTasksAsync(gameId, teacherId, cancellationToken)
            ?? throw new ResourceNotFoundException(nameof(LearningGame), gameId);
    }

    private async Task<string> GenerateUniqueGameCodeAsync(
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaximumGameCodeGenerationAttempts; attempt++)
        {
            var gameCode = GameCodeGenerator.Generate();
            if (!await _repository.GameCodeExistsAsync(gameCode, cancellationToken))
            {
                return gameCode;
            }
        }

        throw new InvalidOperationException("A unique game code could not be generated.");
    }

    private static LearningTask CloneTask(LearningTask source, Guid learningGameId)
    {
        LearningTask clone = source switch
        {
            QuizTask quiz => new QuizTask
            {
                Question = quiz.Question,
                OptionA = quiz.OptionA,
                OptionB = quiz.OptionB,
                OptionC = quiz.OptionC,
                OptionD = quiz.OptionD,
                CorrectAnswer = quiz.CorrectAnswer
            },
            QrCodeTask qr => new QrCodeTask
            {
                Instructions = qr.Instructions,
                QrPayload = $"TASK-{SecureToken.Generate(16)}",
                TimeLimitMinutes = qr.TimeLimitMinutes
            },
            GpsTask gps => new GpsTask
            {
                Instructions = gps.Instructions,
                TargetLatitude = gps.TargetLatitude,
                TargetLongitude = gps.TargetLongitude,
                GameAreaJson = gps.GameAreaJson,
                TimeLimitMinutes = gps.TimeLimitMinutes
            },
            _ => throw new InvalidOperationException($"Unsupported task type: {source.GetType().Name}.")
        };

        clone.Id = Guid.NewGuid();
        clone.LearningGameId = learningGameId;
        clone.Order = source.Order;

        return clone;
    }
}
