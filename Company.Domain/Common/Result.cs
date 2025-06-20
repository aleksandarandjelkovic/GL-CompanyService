namespace Company.Domain.Common
{
    /// <summary>
    /// Represents the result of an operation, encapsulating success or failure and an optional value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Gets the error message if the operation failed; otherwise, an empty string.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Gets the value returned by the operation if successful; otherwise, the default value of <typeparamref name="T"/>.
        /// </summary>
        public T? Value { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class.
        /// </summary>
        /// <param name="isSuccess">Indicates whether the operation was successful.</param>
        /// <param name="error">The error message.</param>
        /// <param name="value">The result value.</param>
        private Result(bool isSuccess, string error, T? value)
        {
            IsSuccess = isSuccess;
            Error = error;
            Value = value;
        }

        /// <summary>
        /// Creates a successful result containing the specified value.
        /// </summary>
        /// <param name="value">The value to return.</param>
        /// <returns>A successful result.</returns>
        public static Result<T> Success(T value) => new Result<T>(true, string.Empty, value);

        /// <summary>
        /// Creates a failed result with the specified error message.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <returns>A failed result.</returns>
        public static Result<T> Failure(string error) => new Result<T>(false, error, default);
    }
}
