using AI.PoweredEducation.Business.LearningTasks.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.LearningTasks.Validators;

public sealed class ReorderLearningTasksRequestValidator : AbstractValidator<ReorderLearningTasksRequest>
{
    public ReorderLearningTasksRequestValidator()
    {
        RuleFor(request => request.TaskIds).NotNull().NotEmpty();
        RuleFor(request => request.TaskIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("TaskIds cannot contain duplicate identifiers.");
    }
}
