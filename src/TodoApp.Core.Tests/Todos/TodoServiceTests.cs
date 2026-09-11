using System.Collections.Immutable;
using FluentAssertions;
using TodoApp.Core.Results;
using TodoApp.Core.Todos;

namespace TodoApp.Core.Tests.Todos;

public sealed class TodoServiceTests
{
    [Fact]
    public async Task CreateNormalizesValuesAndStartsAtVersionOne()
    {
        var clock = new TestTimeProvider(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var store = new MemoryTodoStore();
        var service = new TodoService(store, new FixedGuidGenerator(id), clock);

        var result = await service.Create(new TodoDraft(" title ", tags: [" work "]), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new
        {
            Id = id,
            Title = "title",
            Description = (string?)null,
            IsCompleted = false,
            Version = 1L
        });
        store.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task UpdateIsNoopForSameValuesAndConflictsForStaleVersion()
    {
        var original = Todo(1);
        var store = new MemoryTodoStore(original);
        var clock = new TestTimeProvider(original.UpdatedAt);
        var service = new TodoService(store, new FixedGuidGenerator(original.Id), clock);

        var noOp = await service.Update(original.Id, 1, new TodoDraft(original.Title), TestContext.Current.CancellationToken);
        noOp.IsSuccess.Should().BeTrue();
        noOp.Value!.Version.Should().Be(1);
        store.ReplaceCalls.Should().Be(0);

        var updated = await service.Update(original.Id, 1, new TodoDraft("changed"), TestContext.Current.CancellationToken);
        updated.IsSuccess.Should().BeTrue();
        updated.Value!.Version.Should().Be(2);
        updated.Value.UpdatedAt.Should().Be(original.UpdatedAt.AddTicks(1));

        var stale = await service.Update(original.Id, 1, new TodoDraft("stale"), TestContext.Current.CancellationToken);
        stale.IsSuccess.Should().BeFalse();
        stale.Error!.Code.Should().Be(TodoErrorCode.Conflict);
    }

    [Fact]
    public async Task CompletionIsIdempotentAndChangesVersionWhenStateChanges()
    {
        var original = Todo(4);
        var store = new MemoryTodoStore(original);
        var service = new TodoService(store, new FixedGuidGenerator(original.Id), new TestTimeProvider(original.UpdatedAt));

        var noOp = await service.SetCompletion(original.Id, original.Version, false, TestContext.Current.CancellationToken);
        noOp.Value!.IsCompleted.Should().BeFalse();
        store.ReplaceCalls.Should().Be(0);

        var completed = await service.Complete(original.Id, original.Version, TestContext.Current.CancellationToken);
        completed.Value!.IsCompleted.Should().BeTrue();
        completed.Value.Version.Should().Be(5);
    }

    [Fact]
    public async Task UpdateReturnsNotFoundWithoutWriting()
    {
        var store = new MemoryTodoStore();
        var service = new TodoService(store, new FixedGuidGenerator(Guid.NewGuid()), TimeProvider.System);

        var result = await service.Update(Guid.NewGuid(), 1, new TodoDraft("title"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(TodoErrorCode.NotFound);
        store.ReplaceCalls.Should().Be(0);
    }

    private static TodoItem Todo(long version) => new(
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        "title",
        null,
        false,
        null,
        TodoPriority.None,
        ImmutableArray<string>.Empty,
        new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
        version);

    private sealed class FixedGuidGenerator(Guid id) : IGuidGenerator
    {
        public Guid NewGuid() => id;
    }

    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class MemoryTodoStore(TodoItem? initial = null) : ITodoStore
    {
        private readonly List<TodoItem> _items = initial is null ? [] : [initial];

        public IReadOnlyList<TodoItem> Items => _items;

        public int ReplaceCalls { get; private set; }

        public Task<TodoResult<IReadOnlyList<TodoItem>>> GetAll(CancellationToken cancellationToken = default) =>
            Task.FromResult(TodoResult<IReadOnlyList<TodoItem>>.Success(_items.ToArray()));

        public Task<TodoResult<TodoItem>> Create(TodoItem todo, CancellationToken cancellationToken = default)
        {
            _items.Add(todo);
            return Task.FromResult(TodoResult<TodoItem>.Success(todo));
        }

        public Task<TodoResult<TodoItem>> Replace(TodoItem replacement, long expectedVersion, CancellationToken cancellationToken = default)
        {
            ReplaceCalls++;
            var index = _items.FindIndex(item => item.Id == replacement.Id);
            if (index < 0)
            {
                return Task.FromResult(TodoResult<TodoItem>.Failure(TodoErrorCode.NotFound, "not found"));
            }

            if (_items[index].Version != expectedVersion)
            {
                return Task.FromResult(TodoResult<TodoItem>.Failure(TodoErrorCode.Conflict, "conflict"));
            }

            _items[index] = replacement;
            return Task.FromResult(TodoResult<TodoItem>.Success(replacement));
        }

        public Task<TodoResult> Delete(Guid id, long expectedVersion, CancellationToken cancellationToken = default) =>
            Task.FromResult(TodoResult.Success());
    }
}
