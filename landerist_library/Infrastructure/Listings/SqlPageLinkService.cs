using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Index;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_library.Tools;

namespace landerist_library.Infrastructure.Listings;

public sealed class SqlPageLinkService : IPageLinkService
{
    private readonly IPagePersistenceService _pages;
    private readonly WebsitePageMetricsRepository _metrics;
    private readonly int _maximumPagesPerWebsite;

    public SqlPageLinkService(
        IPagePersistenceService pages,
        WebsitePageMetricsRepository metrics,
        int maximumPagesPerWebsite)
    {
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(metrics);
        if (maximumPagesPerWebsite <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumPagesPerWebsite));
        }
        _pages = pages;
        _metrics = metrics;
        _maximumPagesPerWebsite = maximumPagesPerWebsite;
    }

    public Uri? Resolve(Page sourcePage, string? url)
    {
        ArgumentNullException.ThrowIfNull(sourcePage);
        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(sourcePage.Uri, url, out Uri? uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        UriBuilder builder = new(uri) { Fragment = string.Empty };
        return builder.Uri;
    }

    public void Index(Page sourcePage, Uri destinationUri)
    {
        ArgumentNullException.ThrowIfNull(sourcePage);
        ArgumentNullException.ThrowIfNull(destinationUri);
        var website = sourcePage.Website;
        Uri uri = Uris.CleanUri(destinationUri);
        if (_metrics.CountPages(website.Host) >= _maximumPagesPerWebsite ||
            website.IsDiscardedByIndexUrlRegex(uri) ||
            ProhibitedUrls.IsProhibited(uri, website.LanguageCode) ||
            !Indexer.IsWebPage(uri) ||
            !uri.Host.Equals(sourcePage.Host, StringComparison.OrdinalIgnoreCase) ||
            uri.Equals(sourcePage.Uri) ||
            !website.IsAllowedByRobotsTxt(uri) ||
            website.MainUri.Equals(uri))
        {
            return;
        }

        _pages.Insert(new Page(website, uri));
    }
}
