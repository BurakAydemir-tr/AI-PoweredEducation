using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Entity.Entities;

public sealed class QrCodeTask : LearningTask
{
    public QrCodeTask()
        : base(LearningTaskType.QrCode)
    {
    }

    public string Instructions { get; set; } = string.Empty;

    public string QrPayload { get; set; } = string.Empty;

    public int TimeLimitMinutes { get; set; }
}
