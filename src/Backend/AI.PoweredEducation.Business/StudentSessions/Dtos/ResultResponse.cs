namespace AI.PoweredEducation.Business.StudentSessions.Dtos;

public sealed record ResultResponse(
    Guid Id,
    int TotalScore,
    TimeSpan CompletionTime,
    int CompletedTaskCount,
    int UnfinishedTaskCount,
    int TimedOutTaskCount,
    DateTimeOffset PlayedAt);
