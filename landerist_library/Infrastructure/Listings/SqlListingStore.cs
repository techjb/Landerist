using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_library.Statistics;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Listings;

public sealed class SqlListingStore : IListingStore
{
    private readonly ListingRepository _listings;
    private readonly ListingQueryRepository _queries;
    private readonly MediaRepository _media;
    private readonly SourceRepository _sources;
    private readonly GlobalStatisticsRepository _statistics;
    private readonly IApplicationLogger _logger;

    public SqlListingStore(IDatabase database, IApplicationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(logger);
        _listings = new ListingRepository(database);
        _queries = new ListingQueryRepository(database);
        _media = new MediaRepository(database);
        _sources = new SourceRepository(database);
        _statistics = new GlobalStatisticsRepository(database);
        _logger = logger;
    }

    public Listing? Get(Page page, bool loadMedia, bool loadSources)
    {
        ArgumentNullException.ThrowIfNull(page);
        DataTable rows = _queries.GetListing(page.UriHash);
        if (rows.Rows.Count != 1)
        {
            return null;
        }

        Listing listing = ES_Listings.GetListingData(rows.Rows[0]);
        if (loadMedia)
        {
            listing.SetMedia(ReadMedia(listing));
        }
        if (loadSources)
        {
            listing.SetSources(ReadSources(listing));
        }
        return listing;
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

    private SortedSet<Media> ReadMedia(Listing listing)
    {
        SortedSet<Media> result = new(new MediaComparer());
        foreach (DataRow row in _media.GetMedia(listing).Rows)
        {
            Media? item = ES_Media.GetMedia(row);
            if (item is not null)
            {
                result.Add(item);
            }
        }
        return result;
    }

    private SortedSet<Source> ReadSources(Listing listing)
    {
        SortedSet<Source> result = new(new SourceComparer());
        foreach (DataRow row in _sources.GetSources(listing).Rows)
        {
            Source? item = ES_Sources.GetSource(row);
            if (item is not null)
            {
                result.Add(item);
            }
        }
        return result;
    }

    private static bool SetEquals<T>(SortedSet<T>? left, SortedSet<T>? right) =>
        ReferenceEquals(left, right) ||
        (left is not null && right is not null && left.SetEquals(right));
}
