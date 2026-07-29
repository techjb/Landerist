using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Infrastructure.Ai;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.WebsiteServices;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistListingScrapingServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistListingScraping(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        services.AddSingleton(new VertexAddressSelectorOptions(
            runtimeOptions.Ai.VertexCredential,
            runtimeOptions.Ai.VertexProjectId,
            runtimeOptions.Ai.VertexLocation,
            runtimeOptions.Ai.VertexPublisher,
            runtimeOptions.Ai.VertexAddressModel));
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
                        serviceProvider.GetRequiredService<VertexAddressSelectorOptions>(),
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