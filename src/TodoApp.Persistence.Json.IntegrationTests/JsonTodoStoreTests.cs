using System.Collections.Immutable;
using System.Text;
using FluentAssertions;
using TodoApp.Core.Results;
using TodoApp.Core.Todos;
using TodoApp.Persistence.Json;
using TodoApp.Persistence.Json.Configuration;
using Xunit;

namespace TodoApp.Persistence.Json.IntegrationTests;

public sealed class JsonTodoStoreTests
{
    [Fact]
    public async Task MissingFileReadsAsEmptyAndFirstCreateCreatesParentAndFile()
    {
        using var fixture = new TempDirectory();
        var path = Path.Combine(fixture.Path, "nested", "todos.json");
        var store = new JsonTodoStore(path);

        var before = await store.GetAll(TestContext.Current.CancellationToken);
        before.IsSuccess.Should().BeTrue();
        before.Value.Should().BeEmpty();
        File.Exists(path).Should().BeFalse();

        var todo = NewTodo();
        var created = await store.Create(todo, TestContext.Current.CancellationToken);
        created.IsSuccess.Should().BeTrue();
        File.Exists(path).Should().BeTrue();

        var loaded = await store.GetAll(TestContext.Current.CancellationToken);
        loaded.Value.Should().ContainSingle().Which.Should().BeEquivalentTo(todo);
    }

    [Fact]
    public async Task CreatePreservesArrayOrderAndRoundTripsAllFields()
    {
        using var fixture = new TempDirectory();
        var store = new JsonTodoStore(Path.Combine(fixture.Path, "todos.json"));
        var first = NewTodo(Guid.NewGuid(), "first");
        var second = NewTodo(Guid.NewGuid(), "second", completed: true);

        (await store.Create(first, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        (await store.Create(second, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();

        var loaded = await store.GetAll(TestContext.Current.CancellationToken);
        loaded.Value.Should().BeEquivalentTo(new[] { first, second });
    }

    [Fact]
    public async Task InvalidJsonIsReportedWithoutReplacingBytes()
    {
        using var fixture = new TempDirectory();
        var path = Path.Combine(fixture.Path, "todos.json");
        Directory.CreateDirectory(fixture.Path);
        var bytes = Encoding.UTF8.GetBytes("{\"schemaVersion\":1,\"todos\":[],\"unexpected\":true}");
        await File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
        var store = new JsonTodoStore(path);

        var result = await store.GetAll(TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(TodoErrorCode.InvalidStorage);
        (await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken)).Should().Equal(bytes);
    }

    [Fact]
    public async Task ReplaceAndDeleteUseCompareAndSwapVersions()
    {
        using var fixture = new TempDirectory();
        var store = new JsonTodoStore(Path.Combine(fixture.Path, "todos.json"));
        var original = NewTodo();
        (await store.Create(original, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();

        var replacement = original with { Title = "changed", UpdatedAt = original.UpdatedAt.AddTicks(1), Version = 2 };
        (await store.Replace(replacement, expectedVersion: 99, TestContext.Current.CancellationToken)).Error!.Code.Should().Be(TodoErrorCode.Conflict);
        (await store.Replace(replacement, expectedVersion: 1, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        (await store.Delete(original.Id, expectedVersion: 1, TestContext.Current.CancellationToken)).Error!.Code.Should().Be(TodoErrorCode.Conflict);
        (await store.Delete(original.Id, expectedVersion: 2, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConcurrentCreatesAreSerializedWithoutLostUpdates()
    {
        using var fixture = new TempDirectory();
        var store = new JsonTodoStore(Path.Combine(fixture.Path, "todos.json"));
        var todos = Enumerable.Range(0, 12).Select(index => NewTodo(Guid.NewGuid(), $"todo-{index}")).ToArray();

        var results = await Task.WhenAll(todos.Select(todo => store.Create(todo, TestContext.Current.CancellationToken)));

        results.Should().OnlyContain(result => result.IsSuccess);
        (await store.GetAll(TestContext.Current.CancellationToken)).Value.Should().HaveCount(todos.Length);
    }

    private static TodoItem NewTodo(Guid? id = null, string title = "example", bool completed = false) => new(
        id ?? Guid.NewGuid(),
        title,
        "description",
        completed,
        new DateOnly(2026, 9, 11),
        TodoPriority.High,
        ImmutableArray.Create("tag"),
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow.AddTicks(1),
        1);

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory() => Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"todo-json-{Guid.NewGuid():N}");

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
