using AI.PoweredEducation.Business.Authentication.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.Authentication.Validators;

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty();
    }
}
