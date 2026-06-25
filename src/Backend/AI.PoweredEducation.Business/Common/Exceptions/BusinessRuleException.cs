namespace AI.PoweredEducation.Business.Common.Exceptions;

public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}
