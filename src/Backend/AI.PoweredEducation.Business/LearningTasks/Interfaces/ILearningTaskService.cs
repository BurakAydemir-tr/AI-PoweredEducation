using AI.PoweredEducation.Business.LearningGames.Dtos;
using AI.PoweredEducation.Business.LearningTasks.Dtos;
using AI.PoweredEducation.Core.Common;

namespace AI.PoweredEducation.Business.LearningTasks.Interfaces;

public interface ILearningTaskService
{
    Task<Result<LearningTaskResponse>> CreateQuizAsync(Guid teacherId, Guid gameId, CreateQuizTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<LearningTaskResponse>> CreateQrCodeAsync(Guid teacherId, Guid gameId, CreateQrCodeTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<LearningTaskResponse>> CreateGpsAsync(Guid teacherId, Guid gameId, CreateGpsTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<LearningTaskResponse>> UpdateQuizAsync(Guid teacherId, Guid taskId, CreateQuizTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<LearningTaskResponse>> UpdateQrCodeAsync(Guid teacherId, Guid taskId, CreateQrCodeTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<LearningTaskResponse>> UpdateGpsAsync(Guid teacherId, Guid taskId, CreateGpsTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid teacherId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<LearningTaskResponse>>> ReorderAsync(Guid teacherId, Guid gameId, ReorderLearningTasksRequest request, CancellationToken cancellationToken = default);
}
