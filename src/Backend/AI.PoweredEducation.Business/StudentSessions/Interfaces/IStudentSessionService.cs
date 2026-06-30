using AI.PoweredEducation.Business.StudentSessions.Dtos;
using AI.PoweredEducation.Core.Common;

namespace AI.PoweredEducation.Business.StudentSessions.Interfaces;

public interface IStudentSessionService
{
    Task<Result<JoinGameResponse>> JoinAsync(JoinGameRequest request, CancellationToken cancellationToken = default);
    Task<Result<StudentProgressResponse>> GetProgressAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task<Result<StudentProgressResponse>> StartCurrentTaskAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task<Result<StudentProgressResponse>> SubmitQuizAnswerAsync(string sessionToken, SubmitQuizAnswerRequest request, CancellationToken cancellationToken = default);
    Task<Result<StudentProgressResponse>> ScanQrCodeAsync(string sessionToken, ScanQrCodeRequest request, CancellationToken cancellationToken = default);
    Task<Result<StudentProgressResponse>> CompleteGpsTaskAsync(string sessionToken, CompleteGpsTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<StudentProgressResponse>> TimeoutCurrentTaskAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task<Result<ResultResponse>> LeaveAsync(string sessionToken, CancellationToken cancellationToken = default);
}
