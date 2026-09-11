using System.Collections.Immutable;
using TodoApp.Core.Results;

namespace TodoApp.Core.Todos;

public static class TodoRules
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2_000;
    public const int MaxTagCount = 10;
    public const int MaxTagLength = 40;

    public static TodoResult<TodoDraft> Normalize(TodoDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var title = draft.Title?.Trim() ?? string.Empty;
        if (title.Length == 0)
        {
            return TodoResult<TodoDraft>.Failure(TodoErrorCode.Validation, "A title is required.");
        }

        if (title.Length > MaxTitleLength)
        {
            return TodoResult<TodoDraft>.Failure(TodoErrorCode.Validation, $"A title cannot exceed {MaxTitleLength} characters.");
        }

        var description = draft.Description?.Trim();
        if (string.IsNullOrEmpty(description))
        {
            description = null;
        }
        else if (description.Length > MaxDescriptionLength)
        {
            return TodoResult<TodoDraft>.Failure(TodoErrorCode.Validation, $"A description cannot exceed {MaxDescriptionLength} characters.");
        }

        if (!Enum.IsDefined(draft.Priority))
        {
            return TodoResult<TodoDraft>.Failure(TodoErrorCode.Validation, "The priority is invalid.");
        }

        var tags = new List<string>();
        var seenTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rawTag in draft.Tags.IsDefault ? [] : draft.Tags)
        {
            var tag = rawTag?.Trim() ?? string.Empty;
            if (tag.Length == 0)
            {
                continue;
            }

            if (tag.Length > MaxTagLength)
            {
                return TodoResult<TodoDraft>.Failure(TodoErrorCode.Validation, $"A tag cannot exceed {MaxTagLength} characters.");
            }

            if (seenTags.Add(tag))
            {
                tags.Add(tag);
            }
        }

        if (tags.Count > MaxTagCount)
        {
            return TodoResult<TodoDraft>.Failure(TodoErrorCode.Validation, $"A todo cannot have more than {MaxTagCount} tags.");
        }

        return TodoResult<TodoDraft>.Success(new TodoDraft(title, description, draft.DueDate, draft.Priority, tags));
    }

    public static TodoResult<TodoDraft> NormalizeAndValidate(TodoDraft draft) => Normalize(draft);

    public static DateTimeOffset NextTimestamp(DateTimeOffset nowUtc, DateTimeOffset previous)
    {
        var now = nowUtc.ToUniversalTime();
        var prior = previous.ToUniversalTime();
        return now > prior ? now : prior.AddTicks(1);
    }

    public static bool HasSameEditableValues(TodoItem todo, TodoDraft draft) =>
        string.Equals(todo.Title, draft.Title, StringComparison.Ordinal)
        && string.Equals(todo.Description, draft.Description, StringComparison.Ordinal)
        && todo.DueDate == draft.DueDate
        && todo.Priority == draft.Priority
        && todo.Tags.SequenceEqual(draft.Tags, StringComparer.Ordinal);
}
