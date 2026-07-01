namespace AI.PoweredEducation.Business.LearningTasks.Dtos;

public sealed record CreateQrCodeTaskRequest(
    string Instructions,
    int TimeLimitMinutes,
    string? QrPayload = null);
