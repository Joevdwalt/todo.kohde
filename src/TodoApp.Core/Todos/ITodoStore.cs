using TodoApp.Core.Results;

namespace TodoApp.Core.Todos;

public interface ITodoStore
{
    Task<TodoResult<IReadOnlyList<TodoItem>>> GetAll(CancellationToken cancellationToken = default);

    Task<TodoResult<TodoItem>> Create(TodoItem todo, CancellationToken cancellationToken = default);

    Task<TodoResult<TodoItem>> Replace(
        TodoItem replacement,
        long expectedVersion,
        CancellationToken cancellationToken = default);

    Task<TodoResult> Delete(
        Guid id,
        long expectedVersion,
        CancellationToken cancellationToken = default);
}
