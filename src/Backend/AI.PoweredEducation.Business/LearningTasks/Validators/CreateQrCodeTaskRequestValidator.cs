using AI.PoweredEducation.Business.LearningTasks.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.LearningTasks.Validators;

public sealed class CreateQrCodeTaskRequestValidator : AbstractValidator<CreateQrCodeTaskRequest>
{
    public CreateQrCodeTaskRequestValidator()
    {
        RuleFor(request => request.Instructions).NotEmpty();
        RuleFor(request => request.TimeLimitMinutes).GreaterThan(0);
        RuleFor(request => request.QrPayload)
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.QrPayload));
    }
}
