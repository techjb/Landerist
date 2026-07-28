using landerist_library.Websites;

namespace landerist_library.Pages;

public static class PageHtmlSignalExtensions
{
    private const string RemaxInvalidCanonicalPath =
        "/buscador-de-inmuebles/todos/todos/todos/todos/todos/todos";

    public static bool ContainsMetaRobotsNoIndex(this Page page) =>
        ContainsMetaRobots(page, "noindex");

    public static bool ContainsMetaRobotsNoFollow(this Page page) =>
        ContainsMetaRobots(page, "nofollow");

    public static bool ContainsMetaRobotsNoImageIndex(this Page page) =>
        ContainsMetaRobots(page, "noimageindex");

    public static bool NotCanonical(this Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        Uri? canonicalUri = page.GetCanonicalUri();
        return canonicalUri is not null && !page.Uri.Equals(canonicalUri);
    }

    public static bool IncorrectLanguage(this Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        var htmlNode = page.GetHtmlDocument()?
            .DocumentNode
            .SelectSingleNode("/html");
        var language = htmlNode?.Attributes["lang"];
        return language is not null &&
            !LanguageValidator.IsValidLanguageAndCountry(
                page.Website,
                language.Value);
    }

    public static Uri? GetCanonicalUri(this Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        var node = page.GetHtmlDocument()?
            .DocumentNode
            .SelectSingleNode("//link[@rel='canonical']");
        if (node is null)
        {
            return null;
        }

        string value = node.GetAttributeValue("href", string.Empty);
        Uri? canonicalUri = ResolveUri(page.Uri, value);
        return IsIgnoredCanonicalUri(page.Host, canonicalUri)
            ? null
            : canonicalUri;
    }

    private static Uri? ResolveUri(Uri pageUri, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !Uri.TryCreate(pageUri, value, out Uri? resolved) ||
            (resolved.Scheme != Uri.UriSchemeHttp &&
                resolved.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        return new UriBuilder(resolved) { Fragment = string.Empty }.Uri;
    }

    private static bool IsIgnoredCanonicalUri(
        string pageHost,
        Uri? canonicalUri)
    {
        return canonicalUri is not null &&
            IsRemaxHost(pageHost) &&
            IsRemaxHost(canonicalUri.Host) &&
            canonicalUri.AbsolutePath.TrimEnd('/').Equals(
                RemaxInvalidCanonicalPath,
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRemaxHost(string host) =>
        host.Equals("remax.es", StringComparison.OrdinalIgnoreCase) ||
        host.Equals("www.remax.es", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsMetaRobots(Page page, string content)
    {
        ArgumentNullException.ThrowIfNull(page);
        var node = page.GetHtmlDocument()?
            .DocumentNode
            .SelectSingleNode("//meta[@name='robots']");
        string? value = node?.GetAttributeValue("content", string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return value
            .Split(',')
            .Any(item => item.Equals(content) || item.Equals("none"));
    }
}