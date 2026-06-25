namespace AI.PoweredEducation.Business.StudentSessions.Dtos;

public sealed record JoinGameResponse(
    Guid StudentSessionId,
    string StudentName,
    string SessionToken,
    StudentTaskResponse CurrentTask);
