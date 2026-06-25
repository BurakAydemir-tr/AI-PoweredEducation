using AI.PoweredEducation.Entity.Common;
using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Entity.Entities;

public sealed class TaskAttempt : BaseEntity
{
    public Guid StudentSessionId { get; set; }

    public Guid LearningTaskId { get; set; }

    public int AttemptCount { get; set; }

    public int ScoreEarned { get; set; }

    public TaskAttemptStatus Status { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public StudentSession StudentSession { get; set; } = null!;

    public LearningTask LearningTask { get; set; } = null!;
}
