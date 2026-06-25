using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;

public sealed record AiGenerationContext(
    string GradeLevel,
    string Subject,
    string Topic,
    GameEnvironmentType EnvironmentType,
    int ExpectedStudentCount);
