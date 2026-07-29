using landerist_console;
using landerist_library.Application.Persistence;
using landerist_library.Application.Tasks;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_unit_tests;

public sealed class LanderistServiceCompositionTests
{
    [Theory]
    [InlineData(LanderistExecutionRole.Principal)]
    [InlineData(LanderistExecutionRole.Scraper)]
    [InlineData(LanderistExecutionRole.LocalAi)]
    public void AddLanderist_BuildsServiceProviderForEveryRole(
        LanderistExecutionRole role)
    {
        ServiceCollection services = new();

        services.AddLanderist(CreateOptions(role));
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(TasksService));
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(ParseListing));
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(LanderistAiComposition));
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(LanderistBatchComposition));
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(LanderistDistributionComposition));
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(PagePersistenceService));
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(WebsitePersistenceService));

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.Same(
            provider.GetRequiredService<ParseListing>(),
            provider.GetRequiredService<ParseListing>());
        Assert.Equal(role, provider.GetRequiredService<LanderistRuntimeOptions>().Role);
        Assert.Same(
            provider.GetRequiredService<LanderistRuntimeOptions>().Ai,
            provider.GetRequiredService<AiRuntimeOptions>());
        Assert.Same(
            provider.GetRequiredService<LanderistRuntimeOptions>().Batch,
            provider.GetRequiredService<BatchRuntimeOptions>());
        Assert.Same(
            provider.GetRequiredService<LanderistRuntimeOptions>().Scraping,
            provider.GetRequiredService<ScrapingRuntimeOptions>());
        Assert.Same(
            provider.GetRequiredService<LanderistRuntimeOptions>().Integrations,
            provider.GetRequiredService<IntegrationRuntimeOptions>());
        Assert.Same(
            provider.GetRequiredService<LanderistRuntimeOptions>().Execution,
            provider.GetRequiredService<ExecutionRuntimeOptions>());
    }

    private static LanderistRuntimeOptions CreateOptions(
        LanderistExecutionRole role) => new(
            new DatabaseRuntimeOptions(
                "sql.example.test", "user", "password", "landerist",
                Encrypt: true, TrustServerCertificate: false),
            new ProxyRuntimeOptions(
                string.Empty, 0, false, 0, 0, string.Empty, string.Empty),
            new BrowserRuntimeOptions(
                true, false, 10_000, false, false),
            role)
        {            Ai = new AiRuntimeOptions(
                "sk-test-not-a-real-key",
                "test-vertex-credential",
                "test-project",
                "europe-west1",
                "google",
                "test-listing-model",
                "test-address-model",
                "localhost",
                false),
            Batch = new BatchRuntimeOptions(
                false, "batch", 100, 1, 1024, 1, false, 30, string.Empty)
        };
}