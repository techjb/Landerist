using landerist_domain.Parsing.Materialization;
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
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Websites;

namespace landerist_console;

internal sealed class LanderistAiComposition(
    LanderistRuntimeOptions runtimeOptions,
    WebsiteAccessServices websiteAccess,
    IApplicationLogger logger)
{
    public ParseListing CreateListingParser()
    {
        ListingParserClientCatalog clients = new(
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
                    ResolveHost: runtimeOptions.Ai.ResolveLocalAiHost),
                SystemPrompt.Text,
                StructuredOutputSchema.GetJsonSchemaString(),
                ListingImageUrlPlaceholderCodec.ReplaceImageUrls,
                logger)
        ]);

        return new ParseListing(
            new ListingParserOrchestrationOptions(
                runtimeOptions.Batch.Enabled,
                runtimeOptions.Ai.Provider),
            clients,
            new ListingParsingServices(
                ListingMaterializationRules.Default,
                websiteAccess,
                TimeProvider.System),
            new StructuredOutputMaterializationOperations(
                landerist_library.Tools.Strings.Clean,
                landerist_library.Tools.Strings.RemoveSpaces,
                landerist_library.Tools.Validate.Phone,
                landerist_library.Tools.Validate.Email,
                landerist_library.Tools.Validate.CadastralReference,
                (listing, page, access, images) =>
                    new MediaParser(
                            page,
                            access,
                            new ImageValidationCacheOperations(
                                landerist_library.Database.ValidInvalidImages.IsValid,
                                landerist_library.Database.ValidInvalidImages.IsInvalid,
                                landerist_library.Database.ValidInvalidImages.InsertValid,
                                landerist_library.Database.ValidInvalidImages.InsertInvalid))
                        .AddMediaImages(listing, images),
                (source, uri, exception) => logger.WriteError(
                    source,
                    uri + Environment.NewLine + exception)),
            logger);
    }

}