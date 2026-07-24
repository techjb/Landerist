using landerist_library.Application.Listings;
using landerist_library.Application.Pages;
using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;

namespace landerist_library.Application;

/// <summary>
/// Services used only by legacy static facades. New code should receive these
/// services through constructor injection.
/// </summary>
public sealed class LanderistApplicationServices
{
    private readonly IListingQueryService? _listingQueries;
    private readonly IListingMaintenanceService? _listingMaintenance;
    private readonly IPageQueryService? _pageQueries;
    private readonly IPageMaintenanceService? _pageMaintenance;
    private readonly IWebsiteCatalog? _websiteCatalog;
    private readonly IWebsiteMaintenanceService? _websiteMaintenance;
    private readonly IWebsiteMetricsService? _websiteMetrics;

    public LanderistApplicationServices(
        IPagePersistenceService pagePersistence,
        IWebsitePersistenceService websitePersistence,
        IWebsiteDeletionService websiteDeletion,
        IPageQueryService? pageQueries = null,
        IPageMaintenanceService? pageMaintenance = null,
        IWebsiteCatalog? websiteCatalog = null,
        IWebsiteMaintenanceService? websiteMaintenance = null,
        IWebsiteMetricsService? websiteMetrics = null,
        IListingQueryService? listingQueries = null,
        IListingMaintenanceService? listingMaintenance = null)
    {
        ArgumentNullException.ThrowIfNull(pagePersistence);
        ArgumentNullException.ThrowIfNull(websitePersistence);
        ArgumentNullException.ThrowIfNull(websiteDeletion);
        PagePersistence = pagePersistence;
        WebsitePersistence = websitePersistence;
        WebsiteDeletion = websiteDeletion;
        _listingQueries = listingQueries;
        _listingMaintenance = listingMaintenance;
        _pageQueries = pageQueries;
        _pageMaintenance = pageMaintenance;
        _websiteCatalog = websiteCatalog;
        _websiteMaintenance = websiteMaintenance;
        _websiteMetrics = websiteMetrics;
    }

    public IPagePersistenceService PagePersistence { get; }
    public IWebsitePersistenceService WebsitePersistence { get; }
    public IWebsiteDeletionService WebsiteDeletion { get; }

    public IListingQueryService ListingQueries =>
        GetRequiredService(_listingQueries, nameof(ListingQueries));

    public IListingMaintenanceService ListingMaintenance =>
        GetRequiredService(_listingMaintenance, nameof(ListingMaintenance));

    public IPageQueryService PageQueries =>
        GetRequiredService(_pageQueries, nameof(PageQueries));

    public IPageMaintenanceService PageMaintenance =>
        GetRequiredService(_pageMaintenance, nameof(PageMaintenance));

    public IWebsiteCatalog WebsiteCatalog =>
        GetRequiredService(_websiteCatalog, nameof(WebsiteCatalog));

    public IWebsiteMaintenanceService WebsiteMaintenance =>
        GetRequiredService(_websiteMaintenance, nameof(WebsiteMaintenance));

    public IWebsiteMetricsService WebsiteMetrics =>
        GetRequiredService(_websiteMetrics, nameof(WebsiteMetrics));

    private static T GetRequiredService<T>(T? service, string name) where T : class =>
        service ?? throw new InvalidOperationException(
            $"Legacy application service {name} has not been configured.");
}

/// <summary>
/// Transitional bridge for legacy static APIs. Executable projects must configure
/// it at their composition root before invoking legacy Pages or Websites facades.
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
