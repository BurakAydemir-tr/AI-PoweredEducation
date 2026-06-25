using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.Results.Dtos;

public sealed record StudentResultResponse(
    int Rank,
    Guid StudentSessionId,
    string StudentName,
    SessionEndReason? EndReason,
    int TotalScore,
    int CompletedTaskCount,
    int UnfinishedTaskCount,
    int TimedOutTaskCount,
    TimeSpan CompletionTime,
    DateTimeOffset PlayedAt);
