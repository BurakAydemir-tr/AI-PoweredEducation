using AI.PoweredEducation.Business.LearningGames.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.LearningGames.Validators;

public sealed class CreateLearningGameRequestValidator
    : AbstractValidator<CreateLearningGameRequest>
{
    public CreateLearningGameRequestValidator()
    {
        RuleFor(request => request.GradeLevel).NotEmpty();
        RuleFor(request => request.Subject).NotEmpty();
        RuleFor(request => request.Topic).NotEmpty();
        RuleFor(request => request.EnvironmentType).IsInEnum();
        RuleFor(request => request.ExpectedStudentCount).GreaterThan(0);
    }
}
