using AI.PoweredEducation.Business.StudentSessions.Dtos;

namespace AI.PoweredEducation.Business.StudentSessions.Interfaces;

public interface IStudentSessionService
{
    Task<JoinGameResponse> JoinAsync(JoinGameRequest request, CancellationToken cancellationToken = default);
    Task<StudentProgressResponse> GetProgressAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task<StudentProgressResponse> StartCurrentTaskAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task<StudentProgressResponse> SubmitQuizAnswerAsync(string sessionToken, SubmitQuizAnswerRequest request, CancellationToken cancellationToken = default);
    Task<StudentProgressResponse> ScanQrCodeAsync(string sessionToken, ScanQrCodeRequest request, CancellationToken cancellationToken = default);
    Task<StudentProgressResponse> CompleteGpsTaskAsync(string sessionToken, CompleteGpsTaskRequest request, CancellationToken cancellationToken = default);
    Task<StudentProgressResponse> TimeoutCurrentTaskAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task<ResultResponse> LeaveAsync(string sessionToken, CancellationToken cancellationToken = default);
}
