using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.StudentSessions.Dtos;

public sealed record StudentTaskResponse
{
    public Guid Id { get; init; }
    public LearningTaskType TaskType { get; init; }
    public int Order { get; init; }
    public string? Question { get; init; }
    public string? OptionA { get; init; }
    public string? OptionB { get; init; }
    public string? OptionC { get; init; }
    public string? OptionD { get; init; }
    public string? Instructions { get; init; }
    public double? TargetLatitude { get; init; }
    public double? TargetLongitude { get; init; }
    public int? TimeLimitMinutes { get; init; }
}
