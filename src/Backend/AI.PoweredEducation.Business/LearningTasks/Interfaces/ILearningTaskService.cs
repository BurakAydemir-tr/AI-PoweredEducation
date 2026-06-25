using AI.PoweredEducation.Business.LearningGames.Dtos;
using AI.PoweredEducation.Business.LearningTasks.Dtos;

namespace AI.PoweredEducation.Business.LearningTasks.Interfaces;

public interface ILearningTaskService
{
    Task<LearningTaskResponse> CreateQuizAsync(Guid teacherId, Guid gameId, CreateQuizTaskRequest request, CancellationToken cancellationToken = default);
    Task<LearningTaskResponse> CreateQrCodeAsync(Guid teacherId, Guid gameId, CreateQrCodeTaskRequest request, CancellationToken cancellationToken = default);
    Task<LearningTaskResponse> CreateGpsAsync(Guid teacherId, Guid gameId, CreateGpsTaskRequest request, CancellationToken cancellationToken = default);
    Task<LearningTaskResponse> UpdateQuizAsync(Guid teacherId, Guid taskId, CreateQuizTaskRequest request, CancellationToken cancellationToken = default);
    Task<LearningTaskResponse> UpdateQrCodeAsync(Guid teacherId, Guid taskId, CreateQrCodeTaskRequest request, CancellationToken cancellationToken = default);
    Task<LearningTaskResponse> UpdateGpsAsync(Guid teacherId, Guid taskId, CreateGpsTaskRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid teacherId, Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LearningTaskResponse>> ReorderAsync(Guid teacherId, Guid gameId, ReorderLearningTasksRequest request, CancellationToken cancellationToken = default);
}
