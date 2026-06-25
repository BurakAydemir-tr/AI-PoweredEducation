using AI.PoweredEducation.Business.LearningGames.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.LearningGames.Validators;

public sealed class UpdateLearningGameRequestValidator
    : AbstractValidator<UpdateLearningGameRequest>
{
    public UpdateLearningGameRequestValidator()
    {
        RuleFor(request => request.GradeLevel).NotEmpty();
        RuleFor(request => request.Subject).NotEmpty();
        RuleFor(request => request.Topic).NotEmpty();
        RuleFor(request => request.EnvironmentType).IsInEnum();
        RuleFor(request => request.ExpectedStudentCount).GreaterThan(0);
    }
}
