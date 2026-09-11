using System.Collections.Immutable;

namespace TodoApp.Core.Todos;

public sealed record TodoDraft
{
    public TodoDraft(
        string title,
        string? description = null,
        DateOnly? dueDate = null,
        TodoPriority priority = TodoPriority.None,
        IEnumerable<string>? tags = null)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        Priority = priority;
        Tags = tags is null ? ImmutableArray<string>.Empty : [.. tags];
    }

    public string Title { get; init; }

    public string? Description { get; init; }

    public DateOnly? DueDate { get; init; }

    public TodoPriority Priority { get; init; }

    public ImmutableArray<string> Tags { get; init; }
}
