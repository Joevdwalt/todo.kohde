using System.Text.Json.Serialization;

namespace TodoApp.Persistence.Json.Documents;

public sealed class TodoItemDtoV1
{
    [JsonPropertyName("id")]
    [JsonRequired]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    [JsonRequired]
    public string? Title { get; init; }

    [JsonPropertyName("description")]
    [JsonRequired]
    public string? Description { get; init; }

    [JsonPropertyName("isCompleted")]
    [JsonRequired]
    public bool IsCompleted { get; init; }

    [JsonPropertyName("dueDate")]
    [JsonRequired]
    public DateOnly? DueDate { get; init; }

    [JsonPropertyName("priority")]
    [JsonRequired]
    public string? Priority { get; init; }

    [JsonPropertyName("tags")]
    [JsonRequired]
    public List<string>? Tags { get; init; }

    [JsonPropertyName("createdAt")]
    [JsonRequired]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    [JsonRequired]
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonPropertyName("version")]
    [JsonRequired]
    public long Version { get; init; }
}
