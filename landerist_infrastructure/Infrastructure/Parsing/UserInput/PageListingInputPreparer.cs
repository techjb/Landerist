using HtmlAgilityPack;
using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Parsing.UserInput;

public sealed class PageListingInputPreparer : IListingInputPreparer
{
    private readonly ParseListingUserInput _parser;

    public PageListingInputPreparer(IApplicationLogger logger)
    {
        _parser = new ParseListingUserInput(logger);
    }

    public void Prepare(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (page.GetHtmlDocument() is null)
        {
            page.ApplyListingParserInput(null, contentAvailable: false);
            return;
        }

        string? input = !string.IsNullOrEmpty(page.ListingParserInput)
            ? page.ListingParserInput
            : _parser.GetHtml(page);
        page.ApplyListingParserInput(input);
    }

    public bool MatchesUnavailableRule(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return page.Website.MatchesListingUnavailableRegex(page.ListingParserInput) ||
            page.Website.MatchesListingUnavailableRegex(GetVisibleText(page.ListingParserInput));
    }

    private static string? GetVisibleText(string? parserInput)
    {
        if (string.IsNullOrWhiteSpace(parserInput))
        {
            return null;
        }

        try
        {
            HtmlDocument document = new();
            document.LoadHtml(parserInput);
            return HtmlEntity.DeEntitize(document.DocumentNode.InnerText);
        }
        catch (Exception)
        {
            return null;
        }
    }
}