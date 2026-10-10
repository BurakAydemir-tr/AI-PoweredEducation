using AI.PoweredEducation.Business.LearningTasks.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.LearningTasks.Validators;

public sealed class ReorderLearningTasksRequestValidator : AbstractValidator<ReorderLearningTasksRequest>
{
    public ReorderLearningTasksRequestValidator()
    {
        RuleFor(request => request.TaskIds)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(ids => ids.All(id => id != Guid.Empty) && ids.Distinct().Count() == ids.Count)
            .WithMessage("TaskIds must contain distinct, non-empty identifiers.");
    }
}
