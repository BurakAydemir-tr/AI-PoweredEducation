using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Entity.Entities;

public sealed class GpsTask : LearningTask
{
    public GpsTask()
        : base(LearningTaskType.Gps)
    {
    }

    public string Instructions { get; set; } = string.Empty;

    public double TargetLatitude { get; set; }

    public double TargetLongitude { get; set; }

    public string GameAreaJson { get; set; } = string.Empty;

    public int TimeLimitMinutes { get; set; }
}
