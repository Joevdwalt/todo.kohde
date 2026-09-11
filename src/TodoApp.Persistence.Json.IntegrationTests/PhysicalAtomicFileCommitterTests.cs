using System.Text;
using FluentAssertions;
using TodoApp.Persistence.Json.Files;
using Xunit;

namespace TodoApp.Persistence.Json.IntegrationTests;

public sealed class PhysicalAtomicFileCommitterTests
{
    [Fact]
    public async Task CommitWritesBytesAndLeavesNoTemporaryFiles()
    {
        var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"todo-commit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var destination = System.IO.Path.Combine(directory, "todos.json");
            await new PhysicalAtomicFileCommitter().CommitAsync(destination, Encoding.UTF8.GetBytes("updated"), TestContext.Current.CancellationToken);

            (await File.ReadAllTextAsync(destination, TestContext.Current.CancellationToken)).Should().Be("updated");
            Directory.EnumerateFiles(directory, ".todos.json.*.tmp").Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
