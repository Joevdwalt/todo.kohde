using TodoApp.Core.Todos;

namespace TodoApp.Discovery.Todos;

/// <summary>
/// The read-only controls used to select and order todos.
/// </summary>
public sealed record TodoQuery
{
    public TodoQuery()
    {
    }

    public TodoQuery(
        TodoView view = TodoView.All,
        string? searchText = null,
        bool? isCompleted = null,
        TodoPriority? priority = null,
        string? tag = null,
        TodoDueFilter dueDateFilter = TodoDueFilter.Any,
        TodoSortField sortField = TodoSortField.Default,
        SortDirection sortDirection = SortDirection.Ascending)
    {
        View = view;
        SearchText = searchText;
        IsCompleted = isCompleted;
        Priority = priority;
        Tag = tag;
        DueDateFilter = dueDateFilter;
        SortField = sortField;
        SortDirection = sortDirection;
    }

    public TodoView View { get; init; } = TodoView.All;

    public string? SearchText { get; init; }

    public bool? IsCompleted { get; init; }

    public TodoPriority? Priority { get; init; }

    public string? Tag { get; init; }

    public TodoDueFilter DueDateFilter { get; init; } = TodoDueFilter.Any;

    public TodoSortField SortField { get; init; } = TodoSortField.Default;

    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;

    // Short aliases keep query construction convenient for callers that use
    // the terminology from the corresponding filter controls.
    public string? Search
    {
        get => SearchText;
        init => SearchText = value;
    }

    public bool? Completed
    {
        get => IsCompleted;
        init => IsCompleted = value;
    }

    public string? TagFilter
    {
        get => Tag;
        init => Tag = value;
    }

    public TodoDueFilter DueFilter
    {
        get => DueDateFilter;
        init => DueDateFilter = value;
    }
}
