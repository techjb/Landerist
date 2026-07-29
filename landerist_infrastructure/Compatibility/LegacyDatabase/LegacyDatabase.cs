namespace landerist_library.Database;

/// <summary>
/// Transitional bridge for legacy static database helpers.
/// New code should receive IDatabase or IDatabaseFactory through injection.
/// </summary>
public static class LegacyDatabase
{
    private static IDatabaseFactory? _factory;

    public static void Configure(IDatabaseFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        Interlocked.Exchange(ref _factory, factory);
    }

    public static IDatabase Create() =>
        Volatile.Read(ref _factory)?.Create()
        ?? throw new InvalidOperationException(
            "The legacy database factory has not been configured. " +
            "Configure it at the executable composition root.");
}
