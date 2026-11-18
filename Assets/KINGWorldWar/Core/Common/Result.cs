using System;

namespace KingWorldWar.Core.Common
{
    /// <summary>
    /// Functional result representing either success or an error.
    /// </summary>
    public readonly struct Result<T, TError>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T Value { get; }
        public TError Error { get; }

        private Result(T value)
        {
            IsSuccess = true;
            Value = value;
            Error = default;
        }

        private Result(TError error)
        {
            IsSuccess = false;
            Value = default;
            Error = error;
        }

        public static Result<T, TError> Ok(T value) => new Result<T, TError>(value);
        public static Result<T, TError> Fail(TError error) => new Result<T, TError>(error);

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<TError, TResult> onFailure)
        {
            if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));
            return IsSuccess ? onSuccess(Value) : onFailure(Error);
        }

        public static implicit operator Result<T, TError>(T value) => Ok(value);
    }

    /// <summary>
    /// Functional result for void operations representing either success or an error.
    /// </summary>
    public readonly struct Result<TError>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public TError Error { get; }

        private Result(bool isSuccess, TError error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result<TError> Ok() => new Result<TError>(true, default);
        public static Result<TError> Fail(TError error) => new Result<TError>(false, error);

        public void Match(Action onSuccess, Action<TError> onFailure)
        {
            if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));
            if (IsSuccess) onSuccess(); else onFailure(Error);
        }
    }
}
