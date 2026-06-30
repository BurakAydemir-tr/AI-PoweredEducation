namespace AI.PoweredEducation.Core.Common;

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value)
        : base(true, null)
    {
        _value = value;
    }

    private Result(Error error)
        : base(false, error)
    {
    }

    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("A failed result does not contain a value.");

    public static Result<TValue> Success(TValue value) => new(value);

    public static new Result<TValue> Failure(Error error) => new(error);
}
