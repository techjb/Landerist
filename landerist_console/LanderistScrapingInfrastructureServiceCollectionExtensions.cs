using landerist_library.Application.Logging;
using landerist_library.Application.Scraping;
using landerist_library.Infrastructure.Browser;
using landerist_library.Infrastructure.Downloaders;
using landerist_library.Infrastructure.Downloaders.Multiple;
using landerist_library.Infrastructure.Downloaders.Puppeteer;
using landerist_library.Infrastructure.Http;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Websites;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistScrapingInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistScrapingInfrastructure(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        HttpClientTransportFactory httpClients = new(
            new HttpTransportOptions(
                runtimeOptions.Proxy.Host,
                runtimeOptions.Proxy.Port,
                runtimeOptions.Proxy.RandomizeStickyPorts,
                runtimeOptions.Proxy.StickyPortMin,
                runtimeOptions.Proxy.StickyPortMax,
                runtimeOptions.Proxy.Username,
                runtimeOptions.Proxy.Password));
        landerist_library.Tools.ScrapingBee.Configure(
            runtimeOptions.Integrations.ScrapingBeeApiKey,
            httpClients);
        landerist_library.Export.S3.Configure(
            new landerist_library.Export.S3Options(
                runtimeOptions.Integrations.AwsAccessKeyId,
                runtimeOptions.Integrations.AwsSecretAccessKey,
                runtimeOptions.Integrations.AwsDownloadsBucket,
                runtimeOptions.Integrations.AwsWebsiteBucket));
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
                TimeSpan.FromSeconds(
                    runtimeOptions.Scraping.HttpTimeoutSeconds),
                MaxRetryAttempts: 3));

        services.AddSingleton(httpClients);
        services.AddSingleton<IHttpClientTransportFactory>(httpClients);
        services.AddSingleton(browserOptions);
        services.AddSingleton(new ApplicationLoggerOptions(
            runtimeOptions.Execution.LogsEnabled,
            runtimeOptions.Execution.LogErrorsToConsole,
            runtimeOptions.Execution.LogInformationToConsole,
            runtimeOptions.Execution.MachineName));
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
        return services;
    }
}