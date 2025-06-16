namespace Company.Application.Common;

/// <summary>
/// Generic result pattern for application layer responses
/// </summary>
/// <typeparam name="T">The type of the result value</typeparam>
public class ServiceResult<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Error message in case of failure
    /// </summary>
    public string Error { get; private set; }

    /// <summary>
    /// The result value (only valid if IsSuccess is true)
    /// </summary>
    public T? Value { get; private set; }

    /// <summary>
    /// Indicates if the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    private ServiceResult(bool isSuccess, string error, T? value)
    {
        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    /// <summary>
    /// Creates a successful result with the specified value
    /// </summary>
    public static ServiceResult<T> Success(T value) => new ServiceResult<T>(true, string.Empty, value);

    /// <summary>
    /// Creates a failed result with the specified error message
    /// </summary>
    public static ServiceResult<T> Failure(string error) => new ServiceResult<T>(false, error, default);
}