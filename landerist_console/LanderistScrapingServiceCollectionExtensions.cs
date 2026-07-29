using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Sql;using landerist_library.Application.Logging;
using landerist_library.Application.Scraping;
using landerist_library.Infrastructure.Browser;
using landerist_library.Infrastructure.Downloaders;
using landerist_library.Infrastructure.Http;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Infrastructure.Downloaders.Multiple;
using landerist_library.Infrastructure.Downloaders.Puppeteer;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Websites;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistScrapingServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistScraping(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(runtimeOptions);
        runtimeOptions.Proxy.Validate();
        runtimeOptions.Browser.Validate();

        HttpClientTransportFactory httpClients = new(
            new HttpTransportOptions(
                runtimeOptions.Proxy.Host,
                runtimeOptions.Proxy.Port,
                runtimeOptions.Proxy.RandomizeStickyPorts,
                runtimeOptions.Proxy.StickyPortMin,
                runtimeOptions.Proxy.StickyPortMax,
                runtimeOptions.Proxy.Username,
                runtimeOptions.Proxy.Password));
        PuppeteerBrowserOptions browserOptions = new(
            runtimeOptions.Browser.Headless,
            runtimeOptions.Browser.IsLocal,
            runtimeOptions.Browser.TimeoutMilliseconds,
            runtimeOptions.Proxy.Host,
            runtimeOptions.Proxy.Port,
            runtimeOptions.Proxy.RandomizeStickyPorts,
            runtimeOptions.Proxy.StickyPortMin,
            runtimeOptions.Proxy.StickyPortMax,
            runtimeOptions.Proxy.Username,
            runtimeOptions.Proxy.Password);
        WebsiteRobotsPolicy robotsPolicy = new();
        GoolzoomApi goolzoom = new(
            httpClients,
            new GoolzoomOptions(
                runtimeOptions.Integrations.GoolzoomApiKey,
                TimeSpan.FromSeconds(runtimeOptions.Scraping.HttpTimeoutSeconds),
                MaxRetryAttempts: 3));

        services.AddSingleton(httpClients);
        services.AddSingleton<IHttpClientTransportFactory>(httpClients);
        services.AddSingleton(browserOptions);
        ApplicationLoggerOptions loggerOptions = new(
            runtimeOptions.Execution.LogsEnabled,
            runtimeOptions.Execution.LogErrorsToConsole,
            runtimeOptions.Execution.LogInformationToConsole,
            runtimeOptions.Execution.MachineName);
        services.AddSingleton(loggerOptions);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<SqlApplicationLogger>();
        services.AddSingleton<IApplicationLogger>(serviceProvider =>
            serviceProvider.GetRequiredService<SqlApplicationLogger>());
        services.AddSingleton<DownloadersPool>(serviceProvider =>
            new DownloadersPool(
                runtimeOptions.Scraping.MaxDegreeOfParallelism,
                new PuppeteerDownloaderFactory(
                    browserOptions,
                    serviceProvider.GetRequiredService<IApplicationLogger>()),
                serviceProvider.GetRequiredService<IApplicationLogger>()));
        services.AddSingleton<LegacyDownloadersPoolAdapter>(serviceProvider =>
            new LegacyDownloadersPoolAdapter(
                serviceProvider.GetRequiredService<DownloadersPool>()));
        services.AddSingleton<IDownloaderPool>(serviceProvider =>
            serviceProvider.GetRequiredService<LegacyDownloadersPoolAdapter>());
        services.AddSingleton<ChromeMaintenanceService>(serviceProvider =>
            new ChromeMaintenanceService(
                new ChromeMaintenanceOptions(
                    runtimeOptions.Browser.ProcessCleanupEnabled,
                    runtimeOptions.Browser.UseTaskKillFallback),
                new SystemChromeProcessController(
                    serviceProvider.GetRequiredService<IApplicationLogger>()),
                new PuppeteerChromeBrowserInstaller()));
        services.AddSingleton(robotsPolicy);
        services.AddSingleton<IWebsiteRobotsPolicy>(robotsPolicy);
        services.AddSingleton(goolzoom);
        services.AddSingleton(new WebsiteNetworkService(httpClients, TimeProvider.System));
        services.AddSingleton(new WebsiteAccessServices(robotsPolicy, httpClients));
        services.AddSingleton<PooledPageDownloader>(serviceProvider =>
            new PooledPageDownloader(
                serviceProvider.GetRequiredService<IDownloaderPool>()));
        services.AddSingleton(new HttpConditionalPageHeaderService(httpClients));
        services.AddSingleton<ScrapeBrowserManager>(serviceProvider =>
            new ScrapeBrowserManager(
                serviceProvider.GetRequiredService<IDownloaderPool>(),
                serviceProvider.GetRequiredService<ChromeMaintenanceService>(),
                serviceProvider.GetRequiredService<IApplicationLogger>()));
        services.AddSingleton<SqlPageLinkService>(serviceProvider => new(
            serviceProvider.GetRequiredService<PagePersistenceService>(),
            serviceProvider.GetRequiredService<WebsitePageMetricsRepository>(),
            serviceProvider.GetRequiredService<WebsiteRobotsPolicy>(),
            runtimeOptions.Scraping.MaxPagesPerWebsite));
        services.AddSingleton<ListingLifecycleService>(serviceProvider =>
        {
            IApplicationLogger logger =
                serviceProvider.GetRequiredService<IApplicationLogger>();
            return new ListingLifecycleService(
                serviceProvider.GetRequiredService<SqlListingStore>(),
                serviceProvider.GetRequiredService<SqlNotListingCacheService>(),
                serviceProvider.GetRequiredService<SqlPageLinkService>(),
                serviceProvider.GetRequiredService<LanderistDatabaseAdapterFactory>()
                    .CreateListingEnricher(
                        serviceProvider.GetRequiredService<GoolzoomApi>(),
                        runtimeOptions.Integrations.GoogleCloudLanderistApiKey,
                        serviceProvider.GetRequiredService<LanderistAiComposition>()
                            .CreateAddressSelectorOptions(),
                        logger),
                new LegacyListingUnpublishPolicy(
                    serviceProvider.GetRequiredService<SqlListingQueryService>()),
                logger,
                new HtmlPageContentInspector());
        });
        services.AddSingleton<LanderistScrapingPipelineFactory>();

        return services;
    }
}
