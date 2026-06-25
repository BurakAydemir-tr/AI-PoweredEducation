using AI.PoweredEducation.Business.StudentSessions.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.StudentSessions.Validators;

public sealed class CompleteGpsTaskRequestValidator : AbstractValidator<CompleteGpsTaskRequest>
{
    public CompleteGpsTaskRequestValidator()
    {
        RuleFor(request => request.Latitude).InclusiveBetween(-90, 90);
        RuleFor(request => request.Longitude).InclusiveBetween(-180, 180);
    }
}
