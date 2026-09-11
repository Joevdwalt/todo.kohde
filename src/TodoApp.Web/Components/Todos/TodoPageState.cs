using TodoApp.Core.Results;
using TodoApp.Core.Todos;
using TodoApp.Discovery.Todos;

namespace TodoApp.Web.Components.Todos;

public enum TodoPageStatus
{
    Loading,
    Ready,
    Empty,
    NoResults,
    ValidationError,
    Conflict,
    NotFound,
    InvalidStorage,
    StorageUnavailable,
    UnexpectedError
}

/// <summary>Coordinates authoritative loads and user operations for the todo page.</summary>
public sealed class TodoPageState
{
    private readonly TodoService _todoService;
    private readonly TodoQueryService _queryService;
    private IReadOnlyList<TodoItem> _allTodos = [];

    public TodoPageState(TodoService todoService, TodoQueryService queryService)
    {
        _todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
    }

    public TodoPageStatus Status { get; private set; } = TodoPageStatus.Loading;

    public TodoQuery Query { get; private set; } = new();

    public IReadOnlyList<TodoItem> Todos { get; private set; } = [];

    public IReadOnlyList<TodoItem> AllTodos => _allTodos;

    public TodoError? Error { get; private set; }

    public string? Announcement { get; private set; }

    public bool IsLoading => Status == TodoPageStatus.Loading;

    public bool IsEmpty => Status == TodoPageStatus.Empty;

    public bool HasNoResults => Status == TodoPageStatus.NoResults;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Status = TodoPageStatus.Loading;
        Error = null;
        try
        {
            var result = await _todoService.GetAll(cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                ApplyFailure(result.Error);
                return;
            }

            _allTodos = result.Value ?? [];
            ApplyQuery();
            Announcement = null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            SetUnexpectedFailure(exception);
        }
    }

    public void SetQuery(TodoQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        Query = query;
        if (Status is not (TodoPageStatus.InvalidStorage or TodoPageStatus.StorageUnavailable))
        {
            ApplyQuery();
        }
    }

    public async Task<TodoResult<TodoItem>> CreateAsync(TodoDraft draft, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteMutation(
            token => _todoService.Create(draft, token),
            "Todo created.", cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task<TodoResult<TodoItem>> UpdateAsync(
        Guid id, long expectedVersion, TodoDraft draft, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteMutation(
            token => _todoService.Update(id, expectedVersion, draft, token),
            "Todo updated.", cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task<TodoResult<TodoItem>> SetCompletionAsync(
        Guid id, long expectedVersion, bool completed, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteMutation(
            token => _todoService.SetCompletion(id, expectedVersion, completed, token),
            completed ? "Todo completed." : "Todo reopened.", cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task<TodoResult> DeleteAsync(
        Guid id, long expectedVersion, CancellationToken cancellationToken = default)
    {
        Status = TodoPageStatus.Loading;
        Error = null;
        try
        {
            var result = await _todoService.Delete(id, expectedVersion, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                ApplyFailure(result.Error);
                return result;
            }

            await LoadAsync(cancellationToken).ConfigureAwait(false);
            Announcement = "Todo deleted.";
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            SetUnexpectedFailure(exception);
            return TodoResult.Failure(TodoErrorCode.StorageUnavailable, "The operation could not be completed.");
        }
    }

    public async Task ReloadAfterConflictAsync(CancellationToken cancellationToken = default)
    {
        await LoadAsync(cancellationToken).ConfigureAwait(false);
        Announcement = "The latest todo values have been reloaded.";
    }

    public static string MessageFor(TodoError? error) => error?.Code switch
    {
        TodoErrorCode.Validation => error.Message,
        TodoErrorCode.NotFound => "That todo could not be found. Reload the list and try again.",
        TodoErrorCode.Conflict => "That todo changed in another tab. Reload it before saving.",
        TodoErrorCode.InvalidStorage => "Todo storage is invalid. Correct the JSON file before retrying.",
        TodoErrorCode.StorageUnavailable => "Todo storage is unavailable. Check the file and retry.",
        _ => "The operation could not be completed. Please retry."
    };

    private async Task<TodoResult<TodoItem>> ExecuteMutation(
        Func<CancellationToken, Task<TodoResult<TodoItem>>> operation,
        string successMessage,
        CancellationToken cancellationToken)
    {
        Status = TodoPageStatus.Loading;
        Error = null;
        try
        {
            var result = await operation(cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                ApplyFailure(result.Error);
                return result;
            }

            await LoadAsync(cancellationToken).ConfigureAwait(false);
            Announcement = successMessage;
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            SetUnexpectedFailure(exception);
            return TodoResult<TodoItem>.Failure(
                TodoErrorCode.StorageUnavailable, "The operation could not be completed.");
        }
    }

    private void ApplyQuery()
    {
        Todos = _queryService.Apply(_allTodos, Query, DateOnly.FromDateTime(DateTime.Now));
        Status = _allTodos.Count == 0
            ? TodoPageStatus.Empty
            : Todos.Count == 0 ? TodoPageStatus.NoResults : TodoPageStatus.Ready;
    }

    private void ApplyFailure(TodoError? error)
    {
        Error = error ?? new TodoError(TodoErrorCode.StorageUnavailable, "The operation could not be completed.");
        Status = Error.Code switch
        {
            TodoErrorCode.Validation => TodoPageStatus.ValidationError,
            TodoErrorCode.Conflict => TodoPageStatus.Conflict,
            TodoErrorCode.NotFound => TodoPageStatus.NotFound,
            TodoErrorCode.InvalidStorage => TodoPageStatus.InvalidStorage,
            TodoErrorCode.StorageUnavailable => TodoPageStatus.StorageUnavailable,
            _ => TodoPageStatus.UnexpectedError
        };
        Announcement = MessageFor(Error);
    }

    private void SetUnexpectedFailure(Exception exception)
    {
        Error = new TodoError(TodoErrorCode.StorageUnavailable, "The operation could not be completed.");
        Status = TodoPageStatus.UnexpectedError;
        Announcement = "The operation could not be completed. Please retry.";
    }
}
