using AI.PoweredEducation.Business.LearningTasks.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.LearningTasks.Validators;

public sealed class CreateQuizTaskRequestValidator : AbstractValidator<CreateQuizTaskRequest>
{
    public CreateQuizTaskRequestValidator()
    {
        RuleFor(request => request.Question).NotEmpty().MaximumLength(1000);
        RuleFor(request => request.OptionA).NotEmpty().MaximumLength(500);
        RuleFor(request => request.OptionB).NotEmpty().MaximumLength(500);
        RuleFor(request => request.OptionC).NotEmpty().MaximumLength(500);
        RuleFor(request => request.OptionD).NotEmpty().MaximumLength(500);
        RuleFor(request => request.CorrectAnswer).IsInEnum();
    }
}
