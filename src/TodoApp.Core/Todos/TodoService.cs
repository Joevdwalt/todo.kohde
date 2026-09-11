using TodoApp.Core.Results;

namespace TodoApp.Core.Todos;

public sealed class TodoService
{
    private readonly ITodoStore _store;
    private readonly IGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;

    public TodoService(ITodoStore store, IGuidGenerator guidGenerator, TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _guidGenerator = guidGenerator ?? throw new ArgumentNullException(nameof(guidGenerator));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public Task<TodoResult<IReadOnlyList<TodoItem>>> GetAll(CancellationToken cancellationToken = default) =>
        _store.GetAll(cancellationToken);

    public async Task<TodoResult<TodoItem>> Create(TodoDraft draft, CancellationToken cancellationToken = default)
    {
        var normalized = TodoRules.Normalize(draft);
        if (!normalized.IsSuccess)
        {
            return TodoResult<TodoItem>.Failure(normalized.Error!);
        }

        var now = _timeProvider.GetUtcNow().ToUniversalTime();
        var value = normalized.Value!;
        var todo = new TodoItem(
            _guidGenerator.NewGuid(),
            value.Title,
            value.Description,
            false,
            value.DueDate,
            value.Priority,
            value.Tags,
            now,
            now,
            1);

        return await _store.Create(todo, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TodoResult<TodoItem>> Update(
        Guid id,
        long expectedVersion,
        TodoDraft draft,
        CancellationToken cancellationToken = default)
    {
        var current = await Find(id, cancellationToken).ConfigureAwait(false);
        if (!current.IsSuccess)
        {
            return TodoResult<TodoItem>.Failure(current.Error!);
        }

        var existing = current.Value!;
        if (existing.Version != expectedVersion)
        {
            return TodoResult<TodoItem>.Failure(TodoErrorCode.Conflict, "The todo has changed since it was loaded.");
        }

        var normalized = TodoRules.Normalize(draft);
        if (!normalized.IsSuccess)
        {
            return TodoResult<TodoItem>.Failure(normalized.Error!);
        }

        var value = normalized.Value!;
        if (TodoRules.HasSameEditableValues(existing, value))
        {
            return TodoResult<TodoItem>.Success(existing);
        }

        var replacement = existing with
        {
            Title = value.Title,
            Description = value.Description,
            DueDate = value.DueDate,
            Priority = value.Priority,
            Tags = value.Tags,
            UpdatedAt = TodoRules.NextTimestamp(_timeProvider.GetUtcNow(), existing.UpdatedAt),
            Version = existing.Version + 1
        };

        return await _store.Replace(replacement, expectedVersion, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TodoResult<TodoItem>> SetCompletion(
        Guid id,
        long expectedVersion,
        bool isCompleted,
        CancellationToken cancellationToken = default)
    {
        var current = await Find(id, cancellationToken).ConfigureAwait(false);
        if (!current.IsSuccess)
        {
            return TodoResult<TodoItem>.Failure(current.Error!);
        }

        var existing = current.Value!;
        if (existing.Version != expectedVersion)
        {
            return TodoResult<TodoItem>.Failure(TodoErrorCode.Conflict, "The todo has changed since it was loaded.");
        }

        if (existing.IsCompleted == isCompleted)
        {
            return TodoResult<TodoItem>.Success(existing);
        }

        var replacement = existing with
        {
            IsCompleted = isCompleted,
            UpdatedAt = TodoRules.NextTimestamp(_timeProvider.GetUtcNow(), existing.UpdatedAt),
            Version = existing.Version + 1
        };

        return await _store.Replace(replacement, expectedVersion, cancellationToken).ConfigureAwait(false);
    }

    public Task<TodoResult<TodoItem>> Complete(Guid id, long expectedVersion, CancellationToken cancellationToken = default) =>
        SetCompletion(id, expectedVersion, true, cancellationToken);

    public Task<TodoResult<TodoItem>> Reopen(Guid id, long expectedVersion, CancellationToken cancellationToken = default) =>
        SetCompletion(id, expectedVersion, false, cancellationToken);

    public Task<TodoResult> Delete(Guid id, long expectedVersion, CancellationToken cancellationToken = default) =>
        _store.Delete(id, expectedVersion, cancellationToken);

    private async Task<TodoResult<TodoItem>> Find(Guid id, CancellationToken cancellationToken)
    {
        var all = await _store.GetAll(cancellationToken).ConfigureAwait(false);
        if (!all.IsSuccess)
        {
            return TodoResult<TodoItem>.Failure(all.Error!);
        }

        var todo = all.Value!.FirstOrDefault(item => item.Id == id);
        return todo is null
            ? TodoResult<TodoItem>.Failure(TodoErrorCode.NotFound, "The todo was not found.")
            : TodoResult<TodoItem>.Success(todo);
    }
}
