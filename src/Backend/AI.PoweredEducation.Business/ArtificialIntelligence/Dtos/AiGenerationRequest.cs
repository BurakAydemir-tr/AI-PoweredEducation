using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;

public sealed record AiGenerationRequest(
    string GradeLevel,
    string Subject,
    string Topic,
    GameEnvironmentType EnvironmentType,
    int ExpectedStudentCount,
    int TaskCount)
{
    public AiGenerationContext ToContext() =>
        new(
            GradeLevel,
            Subject,
            Topic,
            EnvironmentType,
            ExpectedStudentCount);
}
