using AI.PoweredEducation.Business.Authentication.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.Authentication.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(request => request.Password)
            .NotEmpty();
    }
}
