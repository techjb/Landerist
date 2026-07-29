using landerist_library.Application.Logging;
using landerist_library.Application.Scraping;
using landerist_library.Infrastructure.Indexing;
using landerist_library.Application.Persistence;
using landerist_library.Infrastructure.Browser;
using landerist_library.Infrastructure.Downloaders;
using landerist_library.Infrastructure.Http;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Websites;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistWebsiteScrapingServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistWebsiteScraping(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        services.AddSingleton<WebsiteNetworkService>(serviceProvider => new(
            serviceProvider.GetRequiredService<HttpClientTransportFactory>(),
            TimeProvider.System));
        services.AddSingleton<WebsiteAccessServices>(serviceProvider => new(
            serviceProvider.GetRequiredService<WebsiteRobotsPolicy>(),
            serviceProvider.GetRequiredService<HttpClientTransportFactory>()));
        services.AddSingleton<WebsiteSitemapService>(serviceProvider => new(
            runtimeOptions.Scraping.IndexerEnabled,
            serviceProvider.GetRequiredService<WebsiteRobotsPolicy>(),
            TimeProvider.System,
            new LegacyWebsiteSitemapIndexerFactory(
                serviceProvider.GetRequiredService<WebsiteRobotsPolicy>(),
                serviceProvider.GetRequiredService<HttpClientTransportFactory>(),
                serviceProvider.GetRequiredService<PagePersistenceService>(),
                serviceProvider.GetRequiredService<WebsiteMetricsService>()),
            serviceProvider.GetRequiredService<IApplicationLogger>()));
        services.AddSingleton<PooledPageDownloader>(serviceProvider =>
            new PooledPageDownloader(
                serviceProvider.GetRequiredService<IDownloaderPool>()));
        services.AddSingleton<HttpConditionalPageHeaderService>(serviceProvider =>
            new HttpConditionalPageHeaderService(
                serviceProvider.GetRequiredService<HttpClientTransportFactory>()));
        services.AddSingleton<ScrapeBrowserManager>(serviceProvider =>
            new ScrapeBrowserManager(
                serviceProvider.GetRequiredService<IDownloaderPool>(),
                serviceProvider.GetRequiredService<ChromeMaintenanceService>(),
                serviceProvider.GetRequiredService<IApplicationLogger>()));
        return services;
    }
}