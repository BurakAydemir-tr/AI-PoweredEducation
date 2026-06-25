using AI.PoweredEducation.Entity.Common;
using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Entity.Entities;

public abstract class LearningTask : BaseEntity
{
    protected LearningTask(LearningTaskType taskType)
    {
        TaskType = taskType;
    }

    public Guid LearningGameId { get; set; }

    public int Order { get; set; }

    public LearningTaskType TaskType { get; protected set; }

    public LearningGame LearningGame { get; set; } = null!;

    public ICollection<TaskAttempt> TaskAttempts { get; set; } = new List<TaskAttempt>();
}
