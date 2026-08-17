using landerist_domain.Parsing.Materialization;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Ai.StructuredOutputs;

public sealed record StructuredOutputMaterializationOperations(
    Func<string, string> Clean,
    Func<string, string> RemoveSpaces,
    Func<string?, bool> ValidatePhone,
    Func<string?, bool> ValidateEmail,
    Func<string?, bool> ValidateCadastralReference,
    Action<Listing, Page, WebsiteAccessServices, List<(string url, string? title)>> AddMediaImages,
    Action<string, Uri, Exception> LogError);

public class StructuredOutputEsParser
{
    private readonly ListingMaterializationRules Rules;
    private readonly TimeProvider TimeProvider;
    private readonly StructuredOutputMaterializationOperations Operations;

    public StructuredOutputEsParser(
        StructuredOutputEs structuredOutputEs,
        ListingMaterializationRules rules,
        TimeProvider timeProvider,
        StructuredOutputMaterializationOperations operations)
    {
        ArgumentNullException.ThrowIfNull(structuredOutputEs);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(operations);
        Anuncio = structuredOutputEs.Anuncio;
        Rules = rules;
        TimeProvider = timeProvider;
        Operations = operations;
    }

    public Anuncio? Anuncio { get; }

    public (PageType pageType, Listing? listing) Parse(
        Page page,
        WebsiteAccessServices websiteAccess)
    {
        if (Anuncio is null)
        {
            return (PageType.NotListingByParser, null);
        }

        try
        {
            Listing listing = new StructuredOutputListingMapper(
                Anuncio,
                Rules,
                TimeProvider,
                Operations).Create(page);
            StructuredOutputListingRelations.Attach(
                listing,
                Anuncio,
                page,
                websiteAccess,
                Rules,
                Operations);
            return (PageType.Listing, listing);
        }
        catch (Exception exception)
        {
            Operations.LogError("StructuredOutputEsParser.Parse", page.Uri, exception);
            return (PageType.MayBeListing, null);
        }
    }
}
