using AI.PoweredEducation.Business.LearningTasks.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.LearningTasks.Validators;

public sealed class CreateQuizTaskRequestValidator : AbstractValidator<CreateQuizTaskRequest>
{
    public CreateQuizTaskRequestValidator()
    {
        RuleFor(request => request.Question).NotEmpty();
        RuleFor(request => request.OptionA).NotEmpty();
        RuleFor(request => request.OptionB).NotEmpty();
        RuleFor(request => request.OptionC).NotEmpty();
        RuleFor(request => request.OptionD).NotEmpty();
        RuleFor(request => request.CorrectAnswer).IsInEnum();
    }
}
