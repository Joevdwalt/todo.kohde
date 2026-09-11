namespace TodoApp.Core.Results;

public sealed record TodoError(TodoErrorCode Code, string Message);
