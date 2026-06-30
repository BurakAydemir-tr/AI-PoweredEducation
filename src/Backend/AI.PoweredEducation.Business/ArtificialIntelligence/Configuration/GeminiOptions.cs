namespace AI.PoweredEducation.Business.ArtificialIntelligence.Configuration;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gemini-2.5-flash";
    public Uri BaseUrl { get; init; } = new("https://generativelanguage.googleapis.com/v1beta/");
}
