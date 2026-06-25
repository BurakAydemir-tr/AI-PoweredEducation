using AI.PoweredEducation.Business.StudentSessions.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.StudentSessions.Validators;

public sealed class JoinGameRequestValidator : AbstractValidator<JoinGameRequest>
{
    public JoinGameRequestValidator()
    {
        RuleFor(request => request.GameCode).NotEmpty().Length(6);
        RuleFor(request => request.StudentName).NotEmpty();
    }
}
