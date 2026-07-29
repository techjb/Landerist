using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Parsing;

namespace landerist_console;

internal sealed record LanderistBatchTasks(
    TenMinuteTaskJob TenMinute,
    TaskBatchCleaner Cleaner);

internal sealed class LanderistBatchComposition(
    LanderistRuntimeOptions runtimeOptions,
    LanderistDatabaseAdapterFactory databaseAdapters,
    LanderistBatchProviderComposition providerComposition,
    IApplicationLogger logger)
{
    public LanderistBatchTasks Create(
        ParsedPageClassificationService parsedClassification,
        GlobalStatistics globalStatistics,
        SqlPageCatalog pageCatalog,
        PagePersistenceService pagePersistence,
        SqlPageWaitingStatusService waitingStatus,
        ParseListing listingParser)
    {
        LanderistBatchProviderServices providers = providerComposition.Create();
        TenMinuteTaskJob tenMinute = new(
            new TaskBatchDownload(
                parsedClassification,
                databaseAdapters.CreateBatchStore(),
                globalStatistics,
                pageCatalog,
                pagePersistence,
                providers.DownloadProviders,
                new LegacyBatchListingResponseParser(listingParser),
                new BatchDownloadOptions(
                    runtimeOptions.Batch.StatusUpdateParallelism),
                logger),
            new TaskBatchUpload(
                databaseAdapters.CreateBatchRegistrationStore(),
                waitingStatus,
                pagePersistence,
                providers.UploadOptions,
                providers.UploadProviders,
                providers.InputWriter,
                logger));
        TaskBatchCleaner cleaner = new(
            databaseAdapters.CreateBatchStore(),
            new BatchCleanupOptions(runtimeOptions.Batch.Directory),
            providers.ArtifactCleaner);

        return new LanderistBatchTasks(tenMinute, cleaner);
    }
}