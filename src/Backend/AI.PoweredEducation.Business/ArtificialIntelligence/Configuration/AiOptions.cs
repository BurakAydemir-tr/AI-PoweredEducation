namespace AI.PoweredEducation.Business.ArtificialIntelligence.Configuration;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public string Provider { get; init; } = "OpenAI";
}
