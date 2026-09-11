using System.Collections.Immutable;
using FluentAssertions;
using TodoApp.Core.Results;
using TodoApp.Core.Todos;
using TodoApp.Discovery.Todos;
using TodoApp.Web.Components.Todos;

namespace TodoApp.Web.Tests.Components;

public sealed class TodoPageStateTests
{
    [Fact]
    public async Task LoadClassifiesAnEmptyCollection()
    {
        var state = CreateState(new FakeStore());

        await state.LoadAsync(TestContext.Current.CancellationToken);

        state.Status.Should().Be(TodoPageStatus.Empty);
        state.Todos.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadMapsInvalidStorageToAnActionableState()
    {
        var state = CreateState(new FakeStore
        {
            GetAllResult = TodoResult<IReadOnlyList<TodoItem>>.Failure(
                TodoErrorCode.InvalidStorage, "The document is invalid.")
        });

        await state.LoadAsync(TestContext.Current.CancellationToken);

        state.Status.Should().Be(TodoPageStatus.InvalidStorage);
        state.Announcement.Should().Contain("invalid");
    }

    [Fact]
    public async Task SuccessfulCreateReloadsAuthoritativeStateBeforeAnnouncingSuccess()
    {
        var store = new FakeStore();
        var state = CreateState(store);
        await state.LoadAsync(TestContext.Current.CancellationToken);

        var result = await state.CreateAsync(new TodoDraft("  Buy milk  "), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        state.Todos.Should().ContainSingle(item => item.Title == "Buy milk");
        state.Announcement.Should().Be("Todo created.");
    }

    [Fact]
    public async Task ConflictRetainsAnActionableStateUntilExplicitReload()
    {
        var store = new FakeStore();
        var item = Item("Existing");
        store.Items = [item with { Version = 2 }];
        var state = CreateState(store);
        await state.LoadAsync(TestContext.Current.CancellationToken);

        var result = await state.UpdateAsync(item.Id, 1, new TodoDraft("Changed"), TestContext.Current.CancellationToken);

        result.Error!.Code.Should().Be(TodoErrorCode.Conflict);
        state.Status.Should().Be(TodoPageStatus.Conflict);
        state.Announcement.Should().Contain("changed");
    }

    private static TodoPageState CreateState(FakeStore store) =>
        new(new TodoService(store, new FixedGuidGenerator(), new FixedTimeProvider()), new TodoQueryService());

    private static TodoItem Item(string title) => new(
        Guid.NewGuid(), title, null, false, null, TodoPriority.None, ImmutableArray<string>.Empty,
        DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, 1);

    private sealed class FakeStore : ITodoStore
    {
        public TodoItem[] Items { get; set; } = [];
        public TodoResult<IReadOnlyList<TodoItem>>? GetAllResult { get; set; }

        public Task<TodoResult<IReadOnlyList<TodoItem>>> GetAll(CancellationToken cancellationToken = default) =>
            Task.FromResult(GetAllResult ?? TodoResult<IReadOnlyList<TodoItem>>.Success(Items));

        public Task<TodoResult<TodoItem>> Create(TodoItem todo, CancellationToken cancellationToken = default)
        {
            Items = Items.Append(todo).ToArray();
            return Task.FromResult(TodoResult<TodoItem>.Success(todo));
        }

        public Task<TodoResult<TodoItem>> Replace(TodoItem replacement, long expectedVersion, CancellationToken cancellationToken = default)
        {
            var index = Items.ToList().FindIndex(item => item.Id == replacement.Id);
            if (index < 0) return Task.FromResult(TodoResult<TodoItem>.Failure(TodoErrorCode.NotFound, "Missing."));
            if (Items[index].Version != expectedVersion) return Task.FromResult(TodoResult<TodoItem>.Failure(TodoErrorCode.Conflict, "Changed."));
            var next = Items.ToArray(); next[index] = replacement; Items = next;
            return Task.FromResult(TodoResult<TodoItem>.Success(replacement));
        }

        public Task<TodoResult> Delete(Guid id, long expectedVersion, CancellationToken cancellationToken = default) =>
            Task.FromResult(TodoResult.Success());
    }

    private sealed class FixedGuidGenerator : IGuidGenerator
    {
        public Guid NewGuid() => Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => DateTimeOffset.UnixEpoch.AddHours(1);
    }
}
