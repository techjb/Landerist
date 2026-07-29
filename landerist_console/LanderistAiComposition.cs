using landerist_domain.Parsing.Materialization;
using landerist_domain.Parsing.UserInput;
using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Infrastructure.Ai;
using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_library.Infrastructure.ListingMedia;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Websites;

namespace landerist_console;

internal sealed class LanderistAiComposition(
    LanderistRuntimeOptions runtimeOptions,
    LanderistListingParserProviderComposition providerComposition,
    WebsiteAccessServices websiteAccess,
    IApplicationLogger logger)
{
    public ParseListing CreateListingParser() => new(
        new ListingParserOrchestrationOptions(
            runtimeOptions.Batch.Enabled,
            runtimeOptions.Ai.Provider),
        providerComposition.Create(),
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