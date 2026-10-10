using AI.PoweredEducation.Business.StudentSessions.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.StudentSessions.Validators;

public sealed class CompleteGpsTaskRequestValidator : AbstractValidator<CompleteGpsTaskRequest>
{
    public CompleteGpsTaskRequestValidator()
    {
        RuleFor(request => request.Latitude).Must(double.IsFinite).InclusiveBetween(-90, 90);
        RuleFor(request => request.Longitude).Must(double.IsFinite).InclusiveBetween(-180, 180);
    }
}
