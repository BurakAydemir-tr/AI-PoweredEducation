using AI.PoweredEducation.Entity.Common;
using AI.PoweredEducation.Entity.Enums;
using AI.PoweredEducation.Entity.Identity;

namespace AI.PoweredEducation.Entity.Entities;

public sealed class LearningGame : BaseEntity
{
    public Guid TeacherId { get; set; }

    public string GradeLevel { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Topic { get; set; } = string.Empty;

    public GameEnvironmentType EnvironmentType { get; set; }

    public int ExpectedStudentCount { get; set; }

    public LearningGameStatus Status { get; set; }

    public string GameCode { get; set; } = string.Empty;

    public ApplicationUser Teacher { get; set; } = null!;

    public ICollection<LearningTask> Tasks { get; set; } = new List<LearningTask>();

    public ICollection<StudentSession> StudentSessions { get; set; } = new List<StudentSession>();
}
