namespace TodoApp.Persistence.Json.Configuration;

public sealed class TodoStorageOptions
{
    public const string SectionName = "TodoStorage";

    public string FilePath { get; set; } = "data/todos.json";
}
