using AI.PoweredEducation.Business.StudentSessions.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.StudentSessions.Validators;

public sealed class SubmitQuizAnswerRequestValidator : AbstractValidator<SubmitQuizAnswerRequest>
{
    public SubmitQuizAnswerRequestValidator()
    {
        RuleFor(request => request.Answer).IsInEnum();
    }
}
