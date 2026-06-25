using AI.PoweredEducation.Entity.Common;

namespace AI.PoweredEducation.Entity.Entities;

public sealed class Result : BaseEntity
{
    public Guid StudentSessionId { get; set; }

    public int TotalScore { get; set; }

    public TimeSpan CompletionTime { get; set; }

    public int CompletedTaskCount { get; set; }

    public int UnfinishedTaskCount { get; set; }

    public int TimedOutTaskCount { get; set; }

    public DateTimeOffset PlayedAt { get; set; }

    public StudentSession StudentSession { get; set; } = null!;
}
