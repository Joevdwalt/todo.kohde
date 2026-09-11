using TodoApp.Core.Results;
using TodoApp.Core.Todos;
using TodoApp.Persistence.Json.Configuration;
using TodoApp.Persistence.Json.Documents;
using TodoApp.Persistence.Json.Files;

namespace TodoApp.Persistence.Json;

public sealed class JsonTodoStore : ITodoStore
{
    private static readonly SemaphoreSlim MutationLock = new(1, 1);

    private readonly string _filePath;
    private readonly IAtomicFileCommitter _committer;
    private readonly TodoDocumentCodec _codec;

    public JsonTodoStore(string filePath, IAtomicFileCommitter? committer = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = Path.GetFullPath(filePath);
        _committer = committer ?? new PhysicalAtomicFileCommitter();
        _codec = new TodoDocumentCodec();
    }

    public JsonTodoStore(
        TodoStorageOptions options,
        string contentRootPath,
        IAtomicFileCommitter? committer = null)
        : this(TodoStoragePath.Resolve(options, contentRootPath), committer)
    {
    }

    public string FilePath => _filePath;

    public async Task<TodoResult<IReadOnlyList<TodoItem>>> GetAll(CancellationToken cancellationToken = default)
    {
        return await ReadCurrentAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<TodoResult<TodoItem>> Create(
        TodoItem todo,
        CancellationToken cancellationToken = default)
    {
        await MutationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var current = await ReadCurrentAsync(cancellationToken).ConfigureAwait(false);
            if (!current.IsSuccess)
            {
                return TodoResult<TodoItem>.Failure(current.Error!);
            }

            if (current.Value!.Any(item => item.Id == todo.Id))
            {
                return TodoResult<TodoItem>.Failure(TodoErrorCode.Conflict, "A todo with that identifier already exists.");
            }

            var validation = ValidateCandidate(todo, expectedVersion: 1);
            if (!validation.IsSuccess)
            {
                return TodoResult<TodoItem>.Failure(validation.Error!);
            }

            var next = current.Value!.ToList();
            next.Add(todo);
            var committed = await CommitAsync(next, cancellationToken).ConfigureAwait(false);
            return committed.IsSuccess
                ? TodoResult<TodoItem>.Success(todo)
                : TodoResult<TodoItem>.Failure(committed.Error!);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            return TodoResult<TodoItem>.Failure(TodoErrorCode.StorageUnavailable, StorageMessage(exception));
        }
        finally
        {
            MutationLock.Release();
        }
    }

    public async Task<TodoResult<TodoItem>> Replace(
        TodoItem replacement,
        long expectedVersion,
        CancellationToken cancellationToken = default)
    {
        await MutationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var current = await ReadCurrentAsync(cancellationToken).ConfigureAwait(false);
            if (!current.IsSuccess)
            {
                return TodoResult<TodoItem>.Failure(current.Error!);
            }

            var todos = current.Value!.ToList();
            var index = todos.FindIndex(item => item.Id == replacement.Id);
            if (index < 0)
            {
                return TodoResult<TodoItem>.Failure(TodoErrorCode.NotFound, "The todo was not found.");
            }

            var existing = todos[index];
            if (existing.Version != expectedVersion)
            {
                return TodoResult<TodoItem>.Failure(TodoErrorCode.Conflict, "The todo has changed since it was loaded.");
            }

            if (replacement.CreatedAt != existing.CreatedAt)
            {
                return TodoResult<TodoItem>.Failure(TodoErrorCode.Validation, "A todo creation timestamp cannot change.");
            }

            var validation = ValidateCandidate(replacement, expectedVersion + 1);
            if (!validation.IsSuccess)
            {
                return TodoResult<TodoItem>.Failure(validation.Error!);
            }

            todos[index] = replacement;
            var committed = await CommitAsync(todos, cancellationToken).ConfigureAwait(false);
            return committed.IsSuccess
                ? TodoResult<TodoItem>.Success(replacement)
                : TodoResult<TodoItem>.Failure(committed.Error!);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            return TodoResult<TodoItem>.Failure(TodoErrorCode.StorageUnavailable, StorageMessage(exception));
        }
        finally
        {
            MutationLock.Release();
        }
    }

    public async Task<TodoResult> Delete(
        Guid id,
        long expectedVersion,
        CancellationToken cancellationToken = default)
    {
        await MutationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var current = await ReadCurrentAsync(cancellationToken).ConfigureAwait(false);
            if (!current.IsSuccess)
            {
                return TodoResult.Failure(current.Error!);
            }

            var todos = current.Value!.ToList();
            var index = todos.FindIndex(item => item.Id == id);
            if (index < 0)
            {
                return TodoResult.Failure(TodoErrorCode.NotFound, "The todo was not found.");
            }

            if (todos[index].Version != expectedVersion)
            {
                return TodoResult.Failure(TodoErrorCode.Conflict, "The todo has changed since it was loaded.");
            }

            todos.RemoveAt(index);
            return await CommitAsync(todos, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            return TodoResult.Failure(TodoErrorCode.StorageUnavailable, StorageMessage(exception));
        }
        finally
        {
            MutationLock.Release();
        }
    }

    private async Task<TodoResult<IReadOnlyList<TodoItem>>> ReadCurrentAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return TodoResult<IReadOnlyList<TodoItem>>.Success(Array.Empty<TodoItem>());
            }

            var bytes = await File.ReadAllBytesAsync(_filePath, cancellationToken).ConfigureAwait(false);
            return _codec.Decode(bytes);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            return TodoResult<IReadOnlyList<TodoItem>>.Failure(TodoErrorCode.StorageUnavailable, StorageMessage(exception));
        }
    }

    private async Task<TodoResult> CommitAsync(
        IReadOnlyList<TodoItem> todos,
        CancellationToken cancellationToken)
    {
        var bytes = _codec.Encode(todos);
        var roundTrip = _codec.Decode(bytes);
        if (!roundTrip.IsSuccess)
        {
            return TodoResult.Failure(TodoErrorCode.InvalidStorage, "The resulting storage document is invalid.");
        }

        await _committer.CommitAsync(_filePath, bytes, cancellationToken).ConfigureAwait(false);
        return TodoResult.Success();
    }

    private static TodoResult ValidateCandidate(TodoItem item, long expectedVersion)
    {
        if (item.Id == Guid.Empty)
        {
            return TodoResult.Failure(TodoErrorCode.Validation, "A todo identifier must be non-empty.");
        }

        if (item.Version != expectedVersion || item.Version <= 0)
        {
            return TodoResult.Failure(TodoErrorCode.Validation, "A todo version is invalid.");
        }

        if (item.CreatedAt.Offset != TimeSpan.Zero || item.UpdatedAt.Offset != TimeSpan.Zero)
        {
            return TodoResult.Failure(TodoErrorCode.Validation, "Todo timestamps must be UTC.");
        }

        if (item.CreatedAt > item.UpdatedAt)
        {
            return TodoResult.Failure(TodoErrorCode.Validation, "A todo creation timestamp cannot be later than its update timestamp.");
        }

        var normalized = TodoRules.Normalize(new TodoDraft(
            item.Title,
            item.Description,
            item.DueDate,
            item.Priority,
            item.Tags));
        if (!normalized.IsSuccess)
        {
            return TodoResult.Failure(TodoErrorCode.Validation, normalized.Error!.Message);
        }

        var canonical = normalized.Value!;
        return string.Equals(canonical.Title, item.Title, StringComparison.Ordinal)
            && string.Equals(canonical.Description, item.Description, StringComparison.Ordinal)
            && canonical.Tags.SequenceEqual(item.Tags, StringComparer.Ordinal)
            ? TodoResult.Success()
            : TodoResult.Failure(TodoErrorCode.Validation, "A todo contains non-canonical values.");
    }

    private static bool IsStorageFailure(Exception exception) =>
        exception is IOException
        or UnauthorizedAccessException
        or NotSupportedException
        or PathTooLongException
        or DirectoryNotFoundException
        or ArgumentException;

    private static string StorageMessage(Exception exception) =>
        $"Todo storage is unavailable: {exception.Message}";
}
