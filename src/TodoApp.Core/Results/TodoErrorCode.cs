namespace TodoApp.Core.Results;

public enum TodoErrorCode
{
    Validation,
    NotFound,
    Conflict,
    InvalidStorage,
    StorageUnavailable
}
