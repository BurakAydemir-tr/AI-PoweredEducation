namespace AI.PoweredEducation.Business.LearningTasks.Dtos;

public sealed record CreateGpsTaskRequest(
    string Instructions,
    double TargetLatitude,
    double TargetLongitude,
    string GameAreaJson,
    int TimeLimitMinutes);
