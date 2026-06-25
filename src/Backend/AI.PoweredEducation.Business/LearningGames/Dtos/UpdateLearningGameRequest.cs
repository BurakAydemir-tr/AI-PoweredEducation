using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.LearningGames.Dtos;

public sealed record UpdateLearningGameRequest(
    string GradeLevel,
    string Subject,
    string Topic,
    GameEnvironmentType EnvironmentType,
    int ExpectedStudentCount);
