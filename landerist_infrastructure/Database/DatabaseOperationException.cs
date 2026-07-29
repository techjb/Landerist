namespace landerist_library.Database;

public sealed class DatabaseOperationException : Exception
{
    public DatabaseOperationException(
        string operationName,
        Exception innerException)
        : base($"Database operation '{operationName}' failed.", innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        OperationName = operationName;
    }

    public string OperationName { get; }
}