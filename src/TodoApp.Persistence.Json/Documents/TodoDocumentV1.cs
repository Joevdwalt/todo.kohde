using System.Text.Json.Serialization;

namespace TodoApp.Persistence.Json.Documents;

public sealed class TodoDocumentV1
{
    [JsonPropertyName("schemaVersion")]
    [JsonRequired]
    public int SchemaVersion { get; init; }

    [JsonPropertyName("todos")]
    [JsonRequired]
    public List<TodoItemDtoV1>? Todos { get; init; }
}
