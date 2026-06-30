namespace AI.PoweredEducation.Business.ArtificialIntelligence.Exceptions;

public sealed class AiProviderException : Exception
{
    public AiProviderException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
