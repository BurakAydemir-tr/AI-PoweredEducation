using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.StudentSessions.Dtos;

public sealed record StudentProgressResponse(
    Guid StudentSessionId,
    StudentTaskResponse? CurrentTask,
    ResultResponse? Result,
    string? Feedback = null,
    QuizAnswerOption? RevealedCorrectAnswer = null);
