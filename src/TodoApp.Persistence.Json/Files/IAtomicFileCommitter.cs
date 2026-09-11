namespace TodoApp.Persistence.Json.Files;

public interface IAtomicFileCommitter
{
    Task CommitAsync(
        string destinationPath,
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default);
}
