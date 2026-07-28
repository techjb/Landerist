using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using landerist_library.Pages;

namespace landerist_library.Pages;

public static class PageHtmlDocumentExtensions
{
    private static readonly ConditionalWeakTable<Page, CachedDocument> Cache = new();

    public static HtmlDocument? GetHtmlDocument(this Page page)
    {
        ArgumentNullException.ThrowIfNull(page);

        string? responseBody = page.GetResponseBody();
        if (string.IsNullOrEmpty(responseBody))
        {
            Cache.Remove(page);
            return null;
        }

        if (Cache.TryGetValue(page, out CachedDocument? cached) &&
            string.Equals(cached.ResponseBody, responseBody, StringComparison.Ordinal) &&
            string.Equals(
                cached.OriginalOuterHtml,
                cached.Document.DocumentNode.OuterHtml,
                StringComparison.Ordinal))
        {
            return cached.Document;
        }

        try
        {
            HtmlDocument document = new();
            document.LoadHtml(responseBody);
            Cache.Remove(page);
            Cache.Add(page, new CachedDocument(
                responseBody,
                document.DocumentNode.OuterHtml,
                document));
            return document;
        }
        catch (Exception)
        {
            Cache.Remove(page);
            return null;
        }
    }

    private sealed record CachedDocument(
        string ResponseBody,
        string OriginalOuterHtml,
        HtmlDocument Document);
}