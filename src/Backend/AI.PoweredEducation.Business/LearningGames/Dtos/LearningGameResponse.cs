using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.LearningGames.Dtos;

public sealed record LearningGameResponse(
    Guid Id,
    string GradeLevel,
    string Subject,
    string Topic,
    GameEnvironmentType EnvironmentType,
    int ExpectedStudentCount,
    LearningGameStatus Status,
    string GameCode,
    IReadOnlyCollection<LearningTaskResponse> Tasks);
