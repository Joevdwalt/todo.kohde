namespace TodoApp.Core.Todos;

public interface IGuidGenerator
{
    Guid NewGuid();
}

public sealed class SystemGuidGenerator : IGuidGenerator
{
    public Guid NewGuid() => Guid.NewGuid();
}
