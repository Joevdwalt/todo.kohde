using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using TodoApp.Core.Results;
using TodoApp.Core.Todos;

namespace TodoApp.Persistence.Json.Documents;

public sealed class TodoDocumentCodec
{
    public const int SupportedSchemaVersion = 1;

    private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();
    private readonly JsonSerializerOptions _serializerOptions = SerializerOptions;

    public TodoResult<IReadOnlyList<TodoItem>> Decode(ReadOnlySpan<byte> json)
    {
        try
        {
            var document = JsonSerializer.Deserialize<TodoDocumentV1>(json, _serializerOptions);
            if (document is null)
            {
                return Invalid("The storage document is empty.");
            }

            if (document.SchemaVersion != SupportedSchemaVersion)
            {
                return Invalid("The storage document schema version is not supported.");
            }

            if (document.Todos is null)
            {
                return Invalid("The storage document must contain a todos array.");
            }

            var result = new List<TodoItem>(document.Todos.Count);
            var ids = new HashSet<Guid>();
            foreach (var dto in document.Todos)
            {
                var item = DecodeItem(dto);
                if (!item.IsSuccess)
                {
                    return TodoResult<IReadOnlyList<TodoItem>>.Failure(item.Error!);
                }

                if (!ids.Add(item.Value!.Id))
                {
                    return Invalid("The storage document contains duplicate todo identifiers.");
                }

                result.Add(item.Value);
            }

            return TodoResult<IReadOnlyList<TodoItem>>.Success(result);
        }
        catch (JsonException exception)
        {
            return Invalid($"The storage document is invalid: {exception.Message}");
        }
        catch (NotSupportedException exception)
        {
            return Invalid($"The storage document is invalid: {exception.Message}");
        }
    }

    public byte[] Encode(IReadOnlyList<TodoItem> todos)
    {
        ArgumentNullException.ThrowIfNull(todos);

        var document = new TodoDocumentV1
        {
            SchemaVersion = SupportedSchemaVersion,
            Todos = todos.Select(EncodeItem).ToList()
        };

        return JsonSerializer.SerializeToUtf8Bytes(document, _serializerOptions);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822", Justification = "The instance API keeps codec operations consistent for callers.")]
    public TodoResult<TodoItem> DecodeItem(TodoItemDtoV1? dto)
    {
        if (dto is null)
        {
            return TodoResult<TodoItem>.Failure(TodoErrorCode.InvalidStorage, "The storage document contains a null todo.");
        }

        if (dto.Id == Guid.Empty)
        {
            return InvalidItem("A todo identifier must be non-empty.");
        }

        if (dto.Title is null)
        {
            return InvalidItem("A todo title is required.");
        }

        if (dto.Tags is null)
        {
            return InvalidItem("A todo tags array is required.");
        }

        if (dto.Priority is null || !Enum.TryParse<TodoPriority>(dto.Priority, ignoreCase: false, out var priority)
            || !Enum.IsDefined(priority)
            || !string.Equals(dto.Priority, priority.ToString(), StringComparison.Ordinal))
        {
            return InvalidItem("A todo priority is invalid.");
        }

        if (dto.Version <= 0)
        {
            return InvalidItem("A todo version must be positive.");
        }

        var createdAt = dto.CreatedAt;
        var updatedAt = dto.UpdatedAt;
        if (createdAt.Offset != TimeSpan.Zero || updatedAt.Offset != TimeSpan.Zero)
        {
            return InvalidItem("Todo timestamps must be UTC.");
        }

        if (createdAt > updatedAt)
        {
            return InvalidItem("A todo creation timestamp cannot be later than its update timestamp.");
        }

        var draft = new TodoDraft(dto.Title, dto.Description, dto.DueDate, priority, dto.Tags);
        var normalized = TodoRules.Normalize(draft);
        if (!normalized.IsSuccess)
        {
            return InvalidItem(normalized.Error!.Message);
        }

        var canonical = normalized.Value!;
        if (!string.Equals(canonical.Title, dto.Title, StringComparison.Ordinal)
            || !string.Equals(canonical.Description, dto.Description, StringComparison.Ordinal)
            || canonical.Priority != priority
            || !canonical.Tags.SequenceEqual(dto.Tags, StringComparer.Ordinal))
        {
            return InvalidItem("A todo contains non-canonical title, description, or tags.");
        }

        return TodoResult<TodoItem>.Success(new TodoItem(
            dto.Id,
            dto.Title,
            dto.Description,
            dto.IsCompleted,
            dto.DueDate,
            priority,
            [.. dto.Tags],
            createdAt.ToUniversalTime(),
            updatedAt.ToUniversalTime(),
            dto.Version));
    }

    private static TodoItemDtoV1 EncodeItem(TodoItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        IsCompleted = item.IsCompleted,
        DueDate = item.DueDate,
        Priority = item.Priority.ToString(),
        Tags = (item.Tags.IsDefault ? [] : item.Tags).ToList(),
        CreatedAt = item.CreatedAt.ToUniversalTime(),
        UpdatedAt = item.UpdatedAt.ToUniversalTime(),
        Version = item.Version
    };

    private static TodoResult<IReadOnlyList<TodoItem>> Invalid(string message) =>
        TodoResult<IReadOnlyList<TodoItem>>.Failure(TodoErrorCode.InvalidStorage, message);

    private static TodoResult<TodoItem> InvalidItem(string message) =>
        TodoResult<TodoItem>.Failure(TodoErrorCode.InvalidStorage, message);

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            WriteIndented = true
        };
        options.Converters.Add(new StrictDateOnlyConverter());
        options.Converters.Add(new StrictDateTimeOffsetConverter());
        return options;
    }

    private sealed class StrictDateOnlyConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String
                || !DateOnly.TryParseExact(reader.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
            {
                throw new JsonException("Date values must use YYYY-MM-DD.");
            }

            return date;
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    private sealed class StrictDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String
                || reader.GetString() is not { Length: > 0 } timestamp
                || timestamp[^1] != 'Z'
                || !DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var value)
                || value.Offset != TimeSpan.Zero)
            {
                throw new JsonException("Timestamp values must be ISO-8601 UTC values.");
            }

            return value;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            var text = value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
            writer.WriteStringValue(text.EndsWith("+00:00", StringComparison.Ordinal)
                ? string.Concat(text.AsSpan(0, text.Length - 6), "Z")
                : text);
        }
    }
}
