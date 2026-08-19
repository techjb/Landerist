using landerist_domain.Parsing.Prompt;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_domain.Parsing.UserInput;
using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Infrastructure.Ai;
using landerist_library.Infrastructure.Ai.LocalAI;
using landerist_library.Infrastructure.Ai.OpenAI;
using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_library.Infrastructure.Ai.Vertex;
using landerist_library.Infrastructure.ListingMedia;
using landerist_library.Infrastructure.Runtime;

namespace landerist_console;

internal sealed class LanderistListingParserProviderComposition(
    LanderistRuntimeOptions runtimeOptions,
    IApplicationLogger logger)
{
    public ListingParserClientCatalog Create() => new(
    [
        new OpenAIListingParserClient(
            new OpenAIListingParserOptions(runtimeOptions.Ai.OpenAiApiKey),
            SystemPrompt.Text,
            StructuredOutputSchema.GetJsonSchemaString(),
            logger),
        new VertexListingParserClient(
            new VertexListingParserOptions(
                runtimeOptions.Ai.VertexCredential,
                runtimeOptions.Ai.VertexProjectId,
                runtimeOptions.Ai.VertexLocation,
                runtimeOptions.Ai.VertexPublisher,
                runtimeOptions.Ai.VertexListingModel),
            SystemPrompt.Text,
            VertexAIResponseSchema.ResponseSchema,
            logger),
        new LocalAIListingParserClient(
            new LocalAIListingParserOptions(
                runtimeOptions.Ai.LocalAiHost,
                MaxCompletionTokens: 4000,
                MaxContextWindow: runtimeOptions.Execution.LocalAiMaxModelLength,
                ResolveHost: runtimeOptions.Ai.ResolveLocalAiHost),
            SystemPrompt.Text,
            StructuredOutputSchema.GetJsonSchemaString(),
            ListingImageUrlPlaceholderCodec.ReplaceImageUrls,
            logger)
    ]);
}
