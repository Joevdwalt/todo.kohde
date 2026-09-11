using System.Collections.Immutable;
using FluentAssertions;
using TodoApp.Core.Todos;
using TodoApp.Discovery.Todos;

namespace TodoApp.Discovery.Tests.Todos;

public sealed class TodoQueryServiceTests
{
    private static readonly DateOnly Today = new(2026, 9, 11);

    private readonly TodoQueryService _service = new();

    [Theory]
    [InlineData(TodoView.All, "active-overdue", "active-today", "active-undated", "completed-today")]
    [InlineData(TodoView.Active, "active-overdue", "active-today", "active-undated")]
    [InlineData(TodoView.Completed, "completed-today")]
    [InlineData(TodoView.DueToday, "active-today", "completed-today")]
    [InlineData(TodoView.Overdue, "active-overdue")]
    public void ApplySelectsTheRequestedView(TodoView view, params string[] expectedTitles)
    {
        var items = SampleTodos();

        var result = _service.Apply(items, new TodoQuery(view: view), Today);

        result.Select(item => item.Title).Should().Equal(expectedTitles);
    }

    [Theory]
    [InlineData("PROJECT", "active-today")]
    [InlineData("details", "active-today")]
    [InlineData("home", "active-overdue")]
    [InlineData("  ", "active-overdue", "active-today", "active-undated", "completed-today")]
    public void ApplySearchesTitleDescriptionAndTags(string search, params string[] expectedTitles)
    {
        var result = _service.Apply(SampleTodos(), new TodoQuery(searchText: search), Today);

        result.Select(item => item.Title).Should().Equal(expectedTitles);
    }

    [Fact]
    public void ApplyCombinesCompletionPriorityTagAndDueFiltersWithAnd()
    {
        var items = SampleTodos().Append(
            Todo("high-home", isCompleted: false, dueDate: Today, priority: TodoPriority.High, tags: ["Home"])).ToArray();

        var query = new TodoQuery(
            isCompleted: false,
            priority: TodoPriority.High,
            tag: " home ",
            dueDateFilter: TodoDueFilter.DueToday);

        _service.Apply(items, query, Today).Select(item => item.Title).Should().Equal("high-home");
    }

    [Theory]
    [InlineData(TodoDueFilter.Any, "active-overdue", "active-today", "active-undated", "completed-today")]
    [InlineData(TodoDueFilter.DueToday, "active-today", "completed-today")]
    [InlineData(TodoDueFilter.Overdue, "active-overdue")]
    [InlineData(TodoDueFilter.NoDueDate, "active-undated")]
    public void ApplySupportsDueDateFilters(TodoDueFilter filter, params string[] expectedTitles)
    {
        var result = _service.Apply(SampleTodos(), new TodoQuery(dueDateFilter: filter), Today);

        result.Select(item => item.Title).Should().Equal(expectedTitles);
    }

    [Theory]
    [InlineData(TodoSortField.CreatedAt, SortDirection.Ascending, "active-overdue", "active-today", "active-undated", "completed-today")]
    [InlineData(TodoSortField.CreatedAt, SortDirection.Descending, "active-overdue", "active-today", "active-undated", "completed-today")]
    [InlineData(TodoSortField.DueDate, SortDirection.Ascending, "active-overdue", "active-today", "completed-today", "active-undated")]
    [InlineData(TodoSortField.DueDate, SortDirection.Descending, "active-today", "completed-today", "active-overdue", "active-undated")]
    [InlineData(TodoSortField.Priority, SortDirection.Ascending, "active-overdue", "active-today", "active-undated", "completed-today")]
    [InlineData(TodoSortField.Priority, SortDirection.Descending, "completed-today", "active-undated", "active-overdue", "active-today")]
    [InlineData(TodoSortField.Title, SortDirection.Ascending, "active-overdue", "active-today", "active-undated", "completed-today")]
    [InlineData(TodoSortField.Title, SortDirection.Descending, "completed-today", "active-undated", "active-today", "active-overdue")]
    public void ApplySupportsExplicitSorts(
        TodoSortField sortField,
        SortDirection direction,
        params string[] expectedTitles)
    {
        var result = _service.Apply(
            SampleTodos(),
            new TodoQuery(sortField: sortField, sortDirection: direction),
            Today);

        result.Select(item => item.Title).Should().Equal(expectedTitles);
    }

    [Fact]
    public void DefaultSortPlacesActiveAndDatedTodosFirstThenUsesNewestCreationTimeAndId()
    {
        var sameDueDate = Today.AddDays(1);
        var older = Todo("older", dueDate: sameDueDate, createdAt: Today.ToDateTime(TimeOnly.MinValue).AddDays(-2));
        var newer = Todo("newer", dueDate: sameDueDate, createdAt: Today.ToDateTime(TimeOnly.MinValue).AddDays(-1));
        var completed = Todo("completed", isCompleted: true, dueDate: Today.AddDays(-10));

        var result = _service.Apply([completed, older, newer], new TodoQuery(), Today);

        result.Select(item => item.Title).Should().Equal("newer", "older", "completed");
    }

    [Fact]
    public void ApplyDoesNotMutateInputAndReturnsIndependentMaterializedResult()
    {
        var items = SampleTodos().ToList();
        var originalOrder = items.Select(item => item.Id).ToArray();

        var result = _service.Apply(items, new TodoQuery(sortField: TodoSortField.Title), Today);

        items.Select(item => item.Id).Should().Equal(originalOrder);
        result.Should().NotBeSameAs(items);
        result.Should().BeAssignableTo<IReadOnlyList<TodoItem>>();
    }

    private static IReadOnlyList<TodoItem> SampleTodos() =>
    [
        Todo("active-today", dueDate: Today, description: "Project details", tags: ["Work"]),
        Todo("completed-today", isCompleted: true, dueDate: Today, priority: TodoPriority.High),
        Todo("active-overdue", dueDate: Today.AddDays(-1), tags: ["Home"]),
        Todo("active-undated", priority: TodoPriority.Low)
    ];

    private static TodoItem Todo(
        string title,
        bool isCompleted = false,
        DateOnly? dueDate = null,
        TodoPriority priority = TodoPriority.None,
        IEnumerable<string>? tags = null,
        string? description = null,
        DateTime? createdAt = null)
    {
        var created = createdAt ?? Today.ToDateTime(TimeOnly.MinValue);
        var id = GuidUtility.Create(GuidUtility.UrlNamespace, title);
        return new TodoItem(
            id,
            title,
            description,
            isCompleted,
            dueDate,
            priority,
            tags?.ToImmutableArray() ?? ImmutableArray<string>.Empty,
            new DateTimeOffset(created, TimeSpan.Zero),
            new DateTimeOffset(created, TimeSpan.Zero),
            1);
    }

    private static class GuidUtility
    {
        public static readonly Guid UrlNamespace = new("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

        public static Guid Create(Guid namespaceId, string name)
        {
            var namespaceBytes = namespaceId.ToByteArray();
            SwapByteOrder(namespaceBytes);
            var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
            var data = new byte[namespaceBytes.Length + nameBytes.Length];
            Buffer.BlockCopy(namespaceBytes, 0, data, 0, namespaceBytes.Length);
            Buffer.BlockCopy(nameBytes, 0, data, namespaceBytes.Length, nameBytes.Length);

            var hash = System.Security.Cryptography.SHA256.HashData(data);
            var guidBytes = hash[..16];
            guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50);
            guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);
            SwapByteOrder(guidBytes);
            return new Guid(guidBytes);
        }

        private static void SwapByteOrder(Span<byte> bytes)
        {
            (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
            (bytes[1], bytes[2]) = (bytes[2], bytes[1]);
            (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
            (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
        }
    }
}
