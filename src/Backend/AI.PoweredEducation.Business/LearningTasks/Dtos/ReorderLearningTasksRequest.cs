namespace AI.PoweredEducation.Business.LearningTasks.Dtos;

public sealed record ReorderLearningTasksRequest(IReadOnlyList<Guid> TaskIds);
