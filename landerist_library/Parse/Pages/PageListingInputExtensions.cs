using HtmlAgilityPack;
using landerist_library.Parse.ListingParser.UserInput;

namespace landerist_library.Pages;

public static class PageListingInputExtensions
{
    public static void SetListingParserInput(this Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (page.GetHtmlDocument() is null)
        {
            page.ApplyListingParserInput(
                parserInput: null,
                contentAvailable: false);
            return;
        }

        page.ApplyListingParserInput(page.GetListingParserInput());
    }

    public static string? GetListingParserInput(this Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (!string.IsNullOrEmpty(page.ListingParserInput))
        {
            return page.ListingParserInput;
        }

        page.ListingParserInput = ParseListingUserInput.GetHtml(page);
        return page.ListingParserInput;
    }

    public static bool MatchesWebsiteListingUnavailableRule(this Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return page.Website.MatchesListingUnavailableRegex(
                page.ListingParserInput) ||
            page.Website.MatchesListingUnavailableRegex(
                GetListingParserInputText(page.ListingParserInput));
    }

    private static string? GetListingParserInputText(string? parserInput)
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