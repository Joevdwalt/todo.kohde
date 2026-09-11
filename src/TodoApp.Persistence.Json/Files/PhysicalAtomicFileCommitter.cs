namespace TodoApp.Persistence.Json.Files;

public sealed class PhysicalAtomicFileCommitter : IAtomicFileCommitter
{
    public async Task CommitAsync(
        string destinationPath,
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        var parentDirectory = Path.GetDirectoryName(destinationPath);
        if (string.IsNullOrEmpty(parentDirectory))
        {
            throw new ArgumentException("The destination path must include a directory.", nameof(destinationPath));
        }

        Directory.CreateDirectory(parentDirectory);

        var temporaryPath = Path.Combine(
            parentDirectory,
            $".{Path.GetFileName(destinationPath)}.{Guid.NewGuid():N}.tmp");
        var moved = false;
        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                options: FileOptions.SequentialScan | FileOptions.Asynchronous))
            {
                await stream.WriteAsync(content, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.Flush(flushToDisk: true);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, destinationPath, overwrite: true);
            moved = true;
        }
        finally
        {
            if (!moved)
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (IOException)
                {
                    // Cleanup is best effort; preserve the original failure.
                }
                catch (UnauthorizedAccessException)
                {
                    // Cleanup is best effort; preserve the original failure.
                }
            }
        }
    }
}
