namespace SkyOS.Shared.Results;

/// <summary>
/// Non-generic result of an operation. Encapsulates success/failure without throwing.
/// Carries a primary <see cref="Error"/> plus optional field-level validation errors.
/// </summary>
public class Result
{
    private static readonly IReadOnlyDictionary<string, string[]> EmptyValidationErrors =
        new Dictionary<string, string[]>();

    protected Result(bool isSuccess, Error error, IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A successful result cannot carry an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("A failed result must carry an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? EmptyValidationErrors;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new(false, Error.Validation("Bir veya birden fazla doğrulama hatası oluştu."), errors);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);
}
