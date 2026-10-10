using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.StudentSessions.Dtos;

public sealed record SubmitQuizAnswerRequest(Guid TaskId, int ExpectedAttemptCount, QuizAnswerOption Answer);
