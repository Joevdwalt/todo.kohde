namespace TodoApp.Core.Results;

public readonly record struct TodoResult
{
    private TodoResult(bool isSuccess, TodoError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool Succeeded => IsSuccess;

    public TodoError? Error { get; }

    public static TodoResult Success() => new(true, null);

    public static TodoResult Failure(TodoError error) => new(false, error);

    public static TodoResult Failure(TodoErrorCode code, string message) =>
        Failure(new TodoError(code, message));
}

#pragma warning disable CA1000
public readonly record struct TodoResult<T>
{
    private TodoResult(bool isSuccess, T? value, TodoError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool Succeeded => IsSuccess;

    public T? Value { get; }

    public TodoError? Error { get; }

    public static TodoResult<T> Success(T value) => new(true, value, null);

    public static TodoResult<T> Failure(TodoError error) => new(false, default, error);

    public static TodoResult<T> Failure(TodoErrorCode code, string message) =>
        Failure(new TodoError(code, message));

    public TodoResult AsResult() => IsSuccess
        ? TodoResult.Success()
        : TodoResult.Failure(Error ?? new TodoError(TodoErrorCode.StorageUnavailable, "The operation failed."));
}
#pragma warning restore CA1000
