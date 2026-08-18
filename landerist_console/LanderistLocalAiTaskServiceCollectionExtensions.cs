using landerist_domain.Parsing.Tokenization;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Statistics;
using landerist_library.Application.Tasks;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Parsing.Tokenization;
using landerist_library.Infrastructure.Parsing.UserInput;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Sql.Statistics;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistLocalAiTaskServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistLocalAiTasks(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        services.AddSingleton<LocalAiTaskJob>(serviceProvider => new(() =>
        {
            LanderistScrapingPipeline pipeline =
                serviceProvider.GetRequiredService<LanderistScrapingPipeline>();
            IApplicationLogger logger =
                serviceProvider.GetRequiredService<IApplicationLogger>();
            return new TaskLocalAIParsing(
                pipeline.ParsedClassification,
                serviceProvider.GetRequiredService<GlobalStatistics>(),
                serviceProvider.GetRequiredService<SqlPageWaitingStatusService>(),
                serviceProvider.GetRequiredService<SqlPageCatalog>(),
                serviceProvider.GetRequiredService<PagePersistenceService>(),
                new LegacyLocalAiListingParser(
                    serviceProvider.GetRequiredService<ParseListing>(),
                    serviceProvider.GetRequiredService<HostStatistics>()),
                new PageListingInputPreparer(logger),
                new LocalAiParsingTaskOptions(
                    modelMaxTokens: runtimeOptions.Execution.LocalAiMaxModelLength,
                    runSequentially: runtimeOptions.Execution.IsLocal,
                    updateWaitingStatusOnStart: runtimeOptions.Execution.IsProduction),
                new LegacyLocalAiTokenBudget(
                    new Tokenizer(TokenizerOptions.ForProvider(LLMProvider.LocalAI))),
                logger);
        }));
        return services;
    }
}
