using AI.PoweredEducation.Business.StudentSessions.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.StudentSessions.Validators;

public sealed class ScanQrCodeRequestValidator : AbstractValidator<ScanQrCodeRequest>
{
    public ScanQrCodeRequestValidator()
    {
        RuleFor(request => request.QrPayload).NotEmpty();
    }
}
