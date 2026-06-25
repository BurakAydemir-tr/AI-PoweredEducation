using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.LearningGames.Dtos;

public sealed record LearningTaskResponse
{
    public Guid Id { get; init; }
    public LearningTaskType TaskType { get; init; }
    public int Order { get; init; }
    public string? Question { get; init; }
    public string? OptionA { get; init; }
    public string? OptionB { get; init; }
    public string? OptionC { get; init; }
    public string? OptionD { get; init; }
    public QuizAnswerOption? CorrectAnswer { get; init; }
    public string? Instructions { get; init; }
    public string? QrPayload { get; init; }
    public double? TargetLatitude { get; init; }
    public double? TargetLongitude { get; init; }
    public string? GameAreaJson { get; init; }
    public int? TimeLimitMinutes { get; init; }
}
