using System.Collections.Immutable;

namespace TodoApp.Core.Todos;

public sealed record TodoItem
{
    public TodoItem(
        Guid id,
        string title,
        string? description,
        bool isCompleted,
        DateOnly? dueDate,
        TodoPriority priority,
        ImmutableArray<string> tags,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        long version)
    {
        Id = id;
        Title = title;
        Description = description;
        IsCompleted = isCompleted;
        DueDate = dueDate;
        Priority = priority;
        Tags = tags.IsDefault ? ImmutableArray<string>.Empty : tags;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Version = version;
    }

    public Guid Id { get; init; }

    public string Title { get; init; }

    public string? Description { get; init; }

    public bool IsCompleted { get; init; }

    public DateOnly? DueDate { get; init; }

    public TodoPriority Priority { get; init; }

    public ImmutableArray<string> Tags { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public long Version { get; init; }
}
