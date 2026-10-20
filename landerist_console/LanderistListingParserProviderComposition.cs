using landerist_domain.Parsing.Prompt;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_domain.Parsing.UserInput;
using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Infrastructure.Ai.LocalAI;
using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_library.Infrastructure.Runtime;

namespace landerist_console;

internal sealed class LanderistListingParserProviderComposition(
    LanderistRuntimeOptions runtimeOptions,
    IApplicationLogger logger)
{
    public ListingParserClientCatalog Create() => new(
    [
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
