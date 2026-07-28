namespace landerist_library.Infrastructure.Logging;

public sealed record LegacyLogOptions(
    bool PersistenceEnabled,
    bool ErrorsInConsole,
    bool InformationInConsole,
    string MachineName)
{
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(MachineName);
    }
}
