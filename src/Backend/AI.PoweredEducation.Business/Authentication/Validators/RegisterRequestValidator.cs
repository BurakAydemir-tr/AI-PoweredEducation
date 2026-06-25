using AI.PoweredEducation.Business.Authentication.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.Authentication.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(request => request.Password)
            .NotEmpty();
    }
}
