using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Sql;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.WebsiteServices;

public sealed class WebsiteMetricsService : IWebsiteMetricsService
{
    private readonly WebsitePageMetricsRepository _pages;
    private readonly ListingStatisticsRepository _listings;
    private readonly int _maximumPagesPerWebsite;

    public WebsiteMetricsService(
        WebsitePageMetricsRepository pages,
        ListingStatisticsRepository listings,
        int maximumPagesPerWebsite)
    {
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(listings);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumPagesPerWebsite);
        _pages = pages;
        _listings = listings;
        _maximumPagesPerWebsite = maximumPagesPerWebsite;
    }

    public int CountPages(Website website) => _pages.CountPages(website.Host);

    public int CountPagesScrapedSince(Website website, DateTime dateFrom) =>
        _pages.CountPagesSince(website.Host, "LastScrape", dateFrom);

    public int CountPagesInsertedSince(Website website, DateTime dateFrom) =>
        _pages.CountPagesSince(website.Host, "Inserted", dateFrom);

    public int CountPagesParsedSince(Website website, DateTime dateFrom) =>
        _pages.CountPagesSince(website.Host, "LastParseListing", dateFrom);

    public bool HasAchievedMaximumPages(Website website) =>
        CountPages(website) >= _maximumPagesPerWebsite;

    public int CountListings(Website website) => _listings.Count(website.Host);

    public int CountListingsSince(Website website, DateTime dateFrom) =>
        _listings.CountSinceListingDate(website.Host, dateFrom);

    public int CountPublishedListings(Website website) =>
        _listings.Count(website.Host, ListingStatus.published);

    public int CountPublishedListingsWithAddress(Website website) =>
        _listings.CountWithAddress(website.Host, ListingStatus.published);

    public int CountPublishedListingsWithCoordinates(Website website) =>
        _listings.CountWithCoordinates(website.Host, ListingStatus.published);

    public int CountPublishedListingsWithImages(Website website) =>
        _listings.CountWithImages(website.Host, ListingStatus.published);

    public int CountUnpublishedListings(Website website) =>
        _listings.Count(website.Host, ListingStatus.unpublished);

    public int CountListings(
        ListingStatus status,
        Operation operation,
        PropertyType propertyType) =>
        _listings.Count(status, operation, propertyType);
}
