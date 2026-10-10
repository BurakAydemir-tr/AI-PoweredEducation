using System.Text.Json;
using AI.PoweredEducation.Business.LearningTasks.Dtos;
using FluentValidation;

namespace AI.PoweredEducation.Business.LearningTasks.Validators;

public sealed class CreateGpsTaskRequestValidator : AbstractValidator<CreateGpsTaskRequest>
{
    public CreateGpsTaskRequestValidator()
    {
        RuleFor(request => request.Instructions).NotEmpty().MaximumLength(2000);
        RuleFor(request => request.TargetLatitude).Must(double.IsFinite).InclusiveBetween(-90, 90);
        RuleFor(request => request.TargetLongitude).Must(double.IsFinite).InclusiveBetween(-180, 180);
        RuleFor(request => request.TimeLimitMinutes).GreaterThan(0);
        RuleFor(request => request.GameAreaJson)
            .NotEmpty()
            .MaximumLength(100_000)
            .Must(BePolygonGeoJson)
            .WithMessage("GameAreaJson must contain a valid GeoJSON Polygon.");
    }

    private static bool BePolygonGeoJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        try
        {
            using var document = JsonDocument.Parse(value);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("type", out var type) ||
                type.ValueKind != JsonValueKind.String ||
                !string.Equals(type.GetString(), "Polygon", StringComparison.Ordinal) ||
                !root.TryGetProperty("coordinates", out var coordinates) ||
                coordinates.ValueKind != JsonValueKind.Array ||
                coordinates.GetArrayLength() == 0)
                return false;

            foreach (var ring in coordinates.EnumerateArray())
            {
                if (ring.ValueKind != JsonValueKind.Array || ring.GetArrayLength() < 4)
                    return false;

                double firstLongitude = 0, firstLatitude = 0;
                double lastLongitude = 0, lastLatitude = 0;
                var index = 0;
                foreach (var point in ring.EnumerateArray())
                {
                    if (point.ValueKind != JsonValueKind.Array || point.GetArrayLength() < 2 ||
                        point[0].ValueKind != JsonValueKind.Number ||
                        point[1].ValueKind != JsonValueKind.Number ||
                        !point[0].TryGetDouble(out var longitude) ||
                        !point[1].TryGetDouble(out var latitude) ||
                        !double.IsFinite(longitude) || !double.IsFinite(latitude) ||
                        longitude is < -180 or > 180 || latitude is < -90 or > 90)
                        return false;

                    if (index++ == 0)
                        (firstLongitude, firstLatitude) = (longitude, latitude);
                    (lastLongitude, lastLatitude) = (longitude, latitude);
                }

                if (firstLongitude != lastLongitude || firstLatitude != lastLatitude)
                    return false;
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
