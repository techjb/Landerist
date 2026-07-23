using landerist_library.Application.Persistence;

namespace landerist_library.Application;

/// <summary>
/// Application services used by the legacy static facades.
/// New code should receive these services through constructor injection.
/// </summary>
public sealed class LanderistApplicationServices
{
    public LanderistApplicationServices(
        IPagePersistenceService pagePersistence,
        IWebsitePersistenceService websitePersistence)
    {
        ArgumentNullException.ThrowIfNull(pagePersistence);
        ArgumentNullException.ThrowIfNull(websitePersistence);

        PagePersistence = pagePersistence;
        WebsitePersistence = websitePersistence;
    }

    public IPagePersistenceService PagePersistence { get; }

    public IWebsitePersistenceService WebsitePersistence { get; }
}

/// <summary>
/// Transitional bridge for legacy static APIs. Executable projects must configure
/// it at their composition root before invoking Pages or Websites persistence.
/// </summary>
public static class LanderistApplication
{
    private static LanderistApplicationServices? _services;

    public static LanderistApplicationServices Services =>
        Volatile.Read(ref _services)
        ?? throw new InvalidOperationException(
            "Landerist application services have not been configured. " +
            "Configure them in the executable composition root.");

    public static void Configure(LanderistApplicationServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Interlocked.Exchange(ref _services, services);
    }
}
