namespace SkyOS.Shared.Results;

/// <summary>
/// Generic result carrying a value on success. Accessing <see cref="Value"/> on a failed
/// result throws, forcing callers to check <see cref="Result.IsSuccess"/> first.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value)
        : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error, IReadOnlyDictionary<string, string[]>? validationErrors = null)
        : base(false, error, validationErrors)
    {
        _value = default;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    public static Result<TValue> Success(TValue value) => new(value);

    public static new Result<TValue> Failure(Error error) => new(error);

    public static new Result<TValue> ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new(Error.Validation("Bir veya birden fazla doğrulama hatası oluştu."), errors);

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
