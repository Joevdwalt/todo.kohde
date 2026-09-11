namespace TodoApp.Persistence.Json.Configuration;

public static class TodoStoragePath
{
    public static string Resolve(TodoStorageOptions options, string contentRootPath)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);

        if (string.IsNullOrWhiteSpace(options.FilePath))
        {
            throw new ArgumentException("TodoStorage:FilePath must not be empty.", nameof(options));
        }

        return Path.GetFullPath(options.FilePath, contentRootPath);
    }

    public static string Resolve(string? configuredPath, string contentRootPath)
    {
        var options = new TodoStorageOptions { FilePath = configuredPath ?? string.Empty };
        return Resolve(options, contentRootPath);
    }
}
