using landerist_library.Application.Logging;
using landerist_library.Configuration;
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
                LanderistSettings.Current.GetString("GOOLZOOM_API"),
                TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT),
                MaxRetryAttempts: 3));

        services.AddSingleton(httpClients);
        services.AddSingleton<IHttpClientTransportFactory>(httpClients);
        services.AddSingleton(browserOptions);
        ApplicationLoggerOptions loggerOptions = new(
            Config.LOGS_ENABLED,
            Config.LOGS_ERRORS_IN_CONSOLE,
            Config.LOGS_INFO_IN_CONSOLE,
            Config.MACHINE_NAME);
        services.AddSingleton(loggerOptions);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<SqlApplicationLogger>();
        services.AddSingleton<IApplicationLogger>(serviceProvider =>
            serviceProvider.GetRequiredService<SqlApplicationLogger>());
        services.AddSingleton<DownloadersPool>(serviceProvider =>
            new DownloadersPool(
                Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER,
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
        services.AddSingleton<LanderistScrapingPipelineFactory>();

        return services;
    }
}
