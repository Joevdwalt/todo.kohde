using System.Collections.Immutable;
using TodoApp.Core.Todos;

namespace TodoApp.Discovery.Todos;

/// <summary>
/// Applies a discovery query without changing or retaining todo state.
/// </summary>
public sealed class TodoQueryService
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822",
        Justification = "The service is registered as an application service and intentionally exposes instance behavior.")]
    public IReadOnlyList<TodoItem> Apply(
        IReadOnlyCollection<TodoItem> items,
        TodoQuery query,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(query);

        var matching = items
            .Where(item => MatchesView(item, query.View, today))
            .Where(item => MatchesSearch(item, query.SearchText))
            .Where(item => MatchesFilters(item, query, today));

        var ordered = query.SortField == TodoSortField.Default
            ? ApplyDefaultOrder(matching)
            : ApplyExplicitOrder(matching, query.SortField, query.SortDirection);

        // ImmutableArray ensures the result cannot be a caller-owned mutable
        // collection, while preserving IReadOnlyList for the public contract.
        return ordered.ToImmutableArray();
    }

    private static bool MatchesView(TodoItem item, TodoView view, DateOnly today) =>
        view switch
        {
            TodoView.All => true,
            TodoView.Active => !item.IsCompleted,
            TodoView.Completed => item.IsCompleted,
            TodoView.DueToday => item.DueDate == today,
            TodoView.Overdue => IsOverdue(item, today),
            _ => throw new ArgumentOutOfRangeException(nameof(view), view, "The todo view is invalid.")
        };

    private static bool MatchesSearch(TodoItem item, string? searchText)
    {
        var search = searchText?.Trim();
        if (string.IsNullOrEmpty(search))
        {
            return true;
        }

        return item.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
            || (item.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
            || item.Tags.Any(tag => tag.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesFilters(TodoItem item, TodoQuery query, DateOnly today)
    {
        if (query.IsCompleted.HasValue && item.IsCompleted != query.IsCompleted.Value)
        {
            return false;
        }

        if (query.Priority.HasValue && item.Priority != query.Priority.Value)
        {
            return false;
        }

        var tag = query.Tag?.Trim();
        if (!string.IsNullOrEmpty(tag)
            && !item.Tags.Any(itemTag => string.Equals(itemTag, tag, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return query.DueDateFilter switch
        {
            TodoDueFilter.Any => true,
            TodoDueFilter.DueToday => item.DueDate == today,
            TodoDueFilter.Overdue => IsOverdue(item, today),
            TodoDueFilter.NoDueDate => item.DueDate is null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(query), query.DueDateFilter, "The due-date filter is invalid.")
        };
    }

    private static bool IsOverdue(TodoItem item, DateOnly today) =>
        !item.IsCompleted && item.DueDate is DateOnly dueDate && dueDate < today;

    private static IOrderedEnumerable<TodoItem> ApplyDefaultOrder(IEnumerable<TodoItem> items) =>
        items.OrderBy(item => item.IsCompleted)
            .ThenBy(item => item.DueDate is null)
            .ThenBy(item => item.DueDate)
            .ThenByDescending(item => item.CreatedAt)
            .ThenBy(item => item.Id);

    private static IEnumerable<TodoItem> ApplyExplicitOrder(
        IEnumerable<TodoItem> items,
        TodoSortField sortField,
        SortDirection direction)
    {
        var descending = direction switch
        {
            SortDirection.Ascending => false,
            SortDirection.Descending => true,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "The sort direction is invalid.")
        };

        return sortField switch
        {
            TodoSortField.CreatedAt => descending
                ? items.OrderByDescending(item => item.CreatedAt).ThenBy(item => item.Id)
                : items.OrderBy(item => item.CreatedAt).ThenBy(item => item.Id),
            TodoSortField.DueDate => OrderByDueDate(items, descending),
            TodoSortField.Priority => descending
                ? items.OrderByDescending(item => PriorityRank(item.Priority)).ThenBy(item => item.Id)
                : items.OrderBy(item => PriorityRank(item.Priority)).ThenBy(item => item.Id),
            TodoSortField.Title => descending
                ? items.OrderByDescending(item => item.Title, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id)
                : items.OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(sortField), sortField, "The sort field is invalid.")
        };
    }

    private static IEnumerable<TodoItem> OrderByDueDate(IEnumerable<TodoItem> items, bool descending)
    {
        // Null dates are deliberately sorted last for both directions.
        var dated = descending
            ? items.Where(item => item.DueDate is not null).OrderByDescending(item => item.DueDate).ThenBy(item => item.Id)
            : items.Where(item => item.DueDate is not null).OrderBy(item => item.DueDate).ThenBy(item => item.Id);

        return dated.Concat(items.Where(item => item.DueDate is null).OrderBy(item => item.Id));
    }

    private static int PriorityRank(TodoPriority priority) => priority switch
    {
        TodoPriority.None => 0,
        TodoPriority.Low => 1,
        TodoPriority.Medium => 2,
        TodoPriority.High => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "The priority is invalid.")
    };
}
