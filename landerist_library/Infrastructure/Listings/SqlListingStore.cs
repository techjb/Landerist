using landerist_library.Infrastructure.Statistics;
using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_library.Application.Statistics;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Listings;

public sealed class SqlListingStore : IListingStore
{
    private readonly ListingRepository _listings;
    private readonly IListingQueryService _queries;
    private readonly MediaRepository _media;
    private readonly SourceRepository _sources;
    private readonly GlobalStatisticsRepository _statistics;
    private readonly IApplicationLogger _logger;

    public SqlListingStore(IDatabase database, IApplicationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(logger);
        _listings = new ListingRepository(database);
        _media = new MediaRepository(database);
        _sources = new SourceRepository(database);
        _queries = new SqlListingQueryService(
            new ListingQueryRepository(database),
            _media,
            _sources);
        _statistics = new GlobalStatisticsRepository(database);
        _logger = logger;
    }

    public Listing? Get(Page page, bool loadMedia, bool loadSources)
    {
        ArgumentNullException.ThrowIfNull(page);
        return _queries.Get(page, loadMedia, loadSources);
    }

    public void Upsert(
        Page page,
        Listing listing,
        ListingUnpublishDecision? unpublishDecision = null)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(listing);

        Listing? existing = Get(page, loadMedia: true, loadSources: true);
        if (existing is null)
        {
            Insert(page, listing, unpublishDecision);
            return;
        }

        if (!existing.Equals(listing))
        {
            Update(existing, listing, unpublishDecision);
        }
    }

    private void Insert(
        Page page,
        Listing listing,
        ListingUnpublishDecision? unpublishDecision)
    {
        bool inserted = _listings.Insert(
            listing,
            page.Website.Host,
            unpublishDecision,
            out Exception? exception);
        if (!inserted)
        {
            _logger.WriteError(
                "SqlListingStore Insert",
                exception is null
                    ? $"Could not insert listing {listing.guid}."
                    : $"Could not insert listing {listing.guid}: {exception}");
            return;
        }

        _media.Insert(listing);
        _sources.Insert(listing);
        _statistics.InsertDailyCounter(StatisticsKey.ListingInsert.ToString(), 1);
    }

    private void Update(
        Listing existing,
        Listing listing,
        ListingUnpublishDecision? unpublishDecision)
    {
        if (!_listings.Update(listing, unpublishDecision))
        {
            _logger.WriteError(
                "SqlListingStore Update",
                $"Could not update listing {listing.guid}.");
            return;
        }

        if (!SetEquals(existing.media, listing.media))
        {
            _media.Delete(listing.guid);
            _media.Insert(listing);
        }
        if (!SetEquals(existing.sources, listing.sources))
        {
            if (_sources.Delete(listing.guid))
            {
                _sources.Insert(listing);
            }
        }
        _statistics.InsertDailyCounter(StatisticsKey.ListingUpdate.ToString(), 1);
    }

    private static bool SetEquals<T>(SortedSet<T>? left, SortedSet<T>? right) =>
        ReferenceEquals(left, right) ||
        (left is not null && right is not null && left.SetEquals(right));
}
