using landerist_domain.Parsing.Prompt;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_domain.Parsing.Tokenization;
using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Application.Tasks;
using landerist_library.Infrastructure.Ai.Batch;
using landerist_library.Infrastructure.Ai.OpenAI;
using landerist_library.Infrastructure.Ai.OpenAI.Batch;
using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_library.Infrastructure.Ai.Vertex;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Parsing.Tokenization;
using landerist_library.Infrastructure.Parsing.UserInput;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Parsing;

namespace landerist_console;

internal sealed record LanderistBatchProviderServices(
    BatchUploadOptions UploadOptions,
    ListingBatchUploadProviderCatalog UploadProviders,
    IBatchInputWriter InputWriter,
    BatchDownloadProviderCatalog DownloadProviders,
    IBatchArtifactCleaner ArtifactCleaner);

internal sealed class LanderistBatchProviderComposition(
    LanderistRuntimeOptions runtimeOptions,
    IApplicationLogger logger)
{
    public LanderistBatchProviderServices Create()
    {
        TokenizerOptions tokenizerOptions =
            TokenizerOptions.ForProvider(runtimeOptions.Ai.Provider);
        Tokenizer tokenizer = new(tokenizerOptions);
        BatchProvider provider = GetBatchProvider(runtimeOptions.Ai.Provider);
        BatchUploadOptions uploadOptions = new(
            provider,
            runtimeOptions.Batch.MaxPages,
            runtimeOptions.Batch.MinPages,
            tokenizerOptions.MaxContextWindow - tokenizer.CountSystemTokens(),
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
                OpenApiSchemaSerializer.Serialize(
                    VertexAIResponseSchema.ResponseSchema),
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
        BatchDownloadProviderCatalog downloadProviders = new(
        [
            new BatchDownloadProvider(
                BatchProvider.OpenAI,
                new OpenAIBatchDownload(openAiClient, logger)),
            new BatchDownloadProvider(
                BatchProvider.VertexAI,
                new VertexAIBatchDownload(
                    vertexJobs,
                    vertexStorage,
                    logger))
        ]);
        IBatchArtifactCleaner artifactCleaner = new VertexBatchArtifactCleaner(
            vertexJobs,
            vertexStorage,
            runtimeOptions.Batch.CleanupAfterDays,
            TimeProvider.System);

        return new LanderistBatchProviderServices(
            uploadOptions,
            uploadProviders,
            inputWriter,
            downloadProviders,
            artifactCleaner);
    }

    private static BatchProvider GetBatchProvider(LLMProvider provider) =>
        provider switch
        {
            LLMProvider.OpenAI => BatchProvider.OpenAI,
            LLMProvider.VertexAI => BatchProvider.VertexAI,
            _ => throw new InvalidOperationException(
                $"Batch upload is not supported for {provider}.")
        };
}
