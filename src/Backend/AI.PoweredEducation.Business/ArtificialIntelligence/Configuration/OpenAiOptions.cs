namespace AI.PoweredEducation.Business.ArtificialIntelligence.Configuration;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gpt-4.1-mini";
    public Uri BaseUrl { get; init; } = new("https://api.openai.com/v1/");
}
