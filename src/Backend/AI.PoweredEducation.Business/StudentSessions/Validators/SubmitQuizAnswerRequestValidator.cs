using AI.PoweredEducation.Business.StudentSessions.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.StudentSessions.Validators;

public sealed class SubmitQuizAnswerRequestValidator : AbstractValidator<SubmitQuizAnswerRequest>
{
    public SubmitQuizAnswerRequestValidator()
    {
        RuleFor(request => request.TaskId).NotEmpty();
        RuleFor(request => request.ExpectedAttemptCount).InclusiveBetween(0, 2);
        RuleFor(request => request.Answer).IsInEnum();
    }
}
