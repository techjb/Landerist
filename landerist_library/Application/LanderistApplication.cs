using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;

namespace landerist_library.Application;

/// <summary>
/// Persistence services used only by the legacy Pages and Websites facades.
/// New code should receive persistence services through constructor injection.
/// </summary>
public sealed class LanderistApplicationServices
{
    public LanderistApplicationServices(
        IPagePersistenceService pagePersistence,
        IWebsitePersistenceService websitePersistence,
        IWebsiteDeletionService websiteDeletion)
    {
        ArgumentNullException.ThrowIfNull(pagePersistence);
        ArgumentNullException.ThrowIfNull(websitePersistence);
        ArgumentNullException.ThrowIfNull(websiteDeletion);
        PagePersistence = pagePersistence;
        WebsitePersistence = websitePersistence;
        WebsiteDeletion = websiteDeletion;
    }

    public IPagePersistenceService PagePersistence { get; }

    public IWebsitePersistenceService WebsitePersistence { get; }

    public IWebsiteDeletionService WebsiteDeletion { get; }
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
