using System.Text.Json;
using AI.PoweredEducation.Business.LearningTasks.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.LearningTasks.Validators;

public sealed class CreateGpsTaskRequestValidator : AbstractValidator<CreateGpsTaskRequest>
{
    public CreateGpsTaskRequestValidator()
    {
        RuleFor(request => request.Instructions).NotEmpty();
        RuleFor(request => request.TargetLatitude).InclusiveBetween(-90, 90);
        RuleFor(request => request.TargetLongitude).InclusiveBetween(-180, 180);
        RuleFor(request => request.TimeLimitMinutes).GreaterThan(0);
        RuleFor(request => request.GameAreaJson)
            .NotEmpty()
            .Must(BePolygonGeoJson)
            .WithMessage("GameAreaJson must contain a valid GeoJSON Polygon.");
    }

    private static bool BePolygonGeoJson(string value)
    {
        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.TryGetProperty("type", out var type) &&
                string.Equals(type.GetString(), "Polygon", StringComparison.OrdinalIgnoreCase) &&
                document.RootElement.TryGetProperty("coordinates", out var coordinates) &&
                coordinates.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
