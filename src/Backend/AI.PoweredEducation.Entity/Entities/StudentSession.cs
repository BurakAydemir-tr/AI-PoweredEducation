using AI.PoweredEducation.Entity.Common;
using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Entity.Entities;

public sealed class StudentSession : BaseEntity
{
    public Guid LearningGameId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string NormalizedStudentName { get; set; } = string.Empty;

    public string SessionTokenHash { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public SessionEndReason? EndReason { get; set; }

    public LearningGame LearningGame { get; set; } = null!;

    public ICollection<TaskAttempt> TaskAttempts { get; set; } = new List<TaskAttempt>();

    public Result? Result { get; set; }
}
