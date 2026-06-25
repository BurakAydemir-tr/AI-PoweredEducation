namespace AI.PoweredEducation.Business.Authentication.Exceptions;

public sealed class AuthenticationServiceException : Exception
{
    public AuthenticationServiceException(
        AuthenticationErrorCode errorCode,
        string message,
        IReadOnlyCollection<string>? details = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details ?? Array.Empty<string>();
    }

    public AuthenticationErrorCode ErrorCode { get; }

    public IReadOnlyCollection<string> Details { get; }
}
