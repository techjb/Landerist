using landerist_domain.Parsing.Prompt;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_domain.Parsing.Tokenization;
using landerist_library.Application.Listings;
using landerist_library.Application.Statistics;
using landerist_library.Application.Scraping;
using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Application.Persistence;
using landerist_library.Infrastructure.Ai.Batch;
using landerist_library.Infrastructure.Ai.OpenAI;
using landerist_library.Infrastructure.Ai.OpenAI.Batch;
using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_library.Infrastructure.Ai.Vertex;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Parsing.Tokenization;
using landerist_library.Infrastructure.Parsing.UserInput;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Parsing;

namespace landerist_console;

internal sealed record LanderistBatchTasks(
    TenMinuteTaskJob TenMinute,
    TaskBatchCleaner Cleaner);

internal static class LanderistBatchComposition
{
    public static LanderistBatchTasks Create(
        LanderistRuntimeOptions runtimeOptions,
        LanderistDatabaseAdapterFactory databaseAdapters,
        IApplicationLogger logger,
        ParsedPageClassificationService parsedClassification,
        GlobalStatistics globalStatistics,
        SqlPageCatalog pageCatalog,
        PagePersistenceService pagePersistence,
        SqlPageWaitingStatusService waitingStatus,
        ParseListing listingParser)
    {
        Tokenizer tokenizer = new(
            TokenizerOptions.ForProvider(runtimeOptions.Ai.Provider));
        int maxInputTokens =
            TokenizerOptions.ForProvider(runtimeOptions.Ai.Provider).MaxContextWindow -
            tokenizer.CountSystemTokens();
        BatchProvider provider = runtimeOptions.Ai.Provider switch
        {
            LLMProvider.OpenAI => BatchProvider.OpenAI,
            LLMProvider.VertexAI => BatchProvider.VertexAI,
            _ => throw new InvalidOperationException(
                $"Batch upload is not supported for {runtimeOptions.Ai.Provider}.")
        };
        BatchUploadOptions uploadOptions = new(
            provider,
            runtimeOptions.Batch.MaxPages,
            runtimeOptions.Batch.MinPages,
            maxInputTokens,
            updateWaitingResponse: runtimeOptions.Batch.UpdateWaitingResponse,
            statusUpdateParallelism: runtimeOptions.Batch.StatusUpdateParallelism);
        VertexBatchOptions vertexOptions = new(
            runtimeOptions.Ai.VertexCredential,
            runtimeOptions.Ai.VertexProjectId,
            runtimeOptions.Ai.VertexLocation,
            runtimeOptions.Ai.VertexListingModel,
            runtimeOptions.Batch.VertexBucketName,
            runtimeOptions.Batch.Directory);
        VertexBatchJobClient vertexJobs = new(vertexOptions, logger);
        VertexCloudStorageClient vertexStorage = new(vertexOptions, logger);
        OpenAIBatchOptions openAiOptions = new(
            runtimeOptions.Ai.OpenAiApiKey,
            OpenAIListingParserOptions.DefaultModel,
            runtimeOptions.Batch.Directory);
        OpenAIBatchClient openAiClient = new(openAiOptions, logger);
        ListingBatchUploadProviderCatalog uploadProviders = new(
        [
            new OpenAIBatchUploadProvider(
                new OpenAIBatchUpload(
                    openAiOptions,
                    SystemPrompt.Text,
                    StructuredOutputSchema.GetJsonSchemaString()),
                openAiClient),
            new VertexAIBatchUploadProvider(
                SystemPrompt.Text,
                OpenApiSchemaSerializer.Serialize(VertexAIResponseSchema.ResponseSchema),
                vertexStorage.Upload,
                vertexJobs.Create)
        ]);
        IBatchInputWriter inputWriter = new JsonlBatchInputWriter(
            new BatchInputWriterOptions(
                provider,
                runtimeOptions.Batch.Directory,
                runtimeOptions.Batch.MaxFileSizeBytes,
                runtimeOptions.Batch.MinPages),
            uploadProviders.GetRequired(provider),
            new PageListingInputPreparer(logger),
            TimeProvider.System,
            logger);

        TenMinuteTaskJob tenMinute = new(
            new TaskBatchDownload(
                parsedClassification,
                databaseAdapters.CreateBatchStore(),
                globalStatistics,
                pageCatalog,
                pagePersistence,
                new BatchDownloadProviderCatalog(
                [
                    new BatchDownloadProvider(
                        BatchProvider.OpenAI,
                        new OpenAIBatchDownload(openAiClient, logger)),
                    new BatchDownloadProvider(
                        BatchProvider.VertexAI,
                        new VertexAIBatchDownload(vertexJobs, vertexStorage, logger))
                ]),
                new LegacyBatchListingResponseParser(listingParser),
                new BatchDownloadOptions(runtimeOptions.Batch.StatusUpdateParallelism),
                logger),
            new TaskBatchUpload(
                databaseAdapters.CreateBatchRegistrationStore(),
                waitingStatus,
                pagePersistence,
                uploadOptions,
                uploadProviders,
                inputWriter,
                logger));
        TaskBatchCleaner cleaner = new(
            databaseAdapters.CreateBatchStore(),
            new BatchCleanupOptions(runtimeOptions.Batch.Directory),
            new VertexBatchArtifactCleaner(
                vertexJobs,
                vertexStorage,
                runtimeOptions.Batch.CleanupAfterDays,
                TimeProvider.System));

        return new LanderistBatchTasks(tenMinute, cleaner);
    }
}