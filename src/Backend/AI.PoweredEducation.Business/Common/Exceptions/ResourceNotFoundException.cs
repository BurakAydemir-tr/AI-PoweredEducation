namespace AI.PoweredEducation.Business.Common.Exceptions;

public sealed class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string resourceName, object key)
        : base($"{resourceName} with identifier '{key}' was not found.")
    {
    }
}
