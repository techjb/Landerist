using landerist_library.Application.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Pages;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Listings;

public sealed class SqlListingQueryService : IListingQueryService
{
    private readonly ListingQueryRepository _listings;
    private readonly IListingMediaRepository _media;
    private readonly IListingSourceRepository _sources;

    public SqlListingQueryService(
        ListingQueryRepository listings,
        IListingMediaRepository media,
        IListingSourceRepository sources)
    {
        ArgumentNullException.ThrowIfNull(listings);
        ArgumentNullException.ThrowIfNull(media);
        ArgumentNullException.ThrowIfNull(sources);
        _listings = listings;
        _media = media;
        _sources = sources;
    }

    public Listing? Get(Page page, bool loadMedia, bool loadSources)
    {
        ArgumentNullException.ThrowIfNull(page);
        DataTable rows = _listings.GetListing(page.UriHash);
        return rows.Rows.Count == 1
            ? Map(rows.Rows[0], loadMedia, loadSources)
            : null;
    }

    public async Task<Listing?> GetAsync(
        Page page,
        bool loadMedia,
        bool loadSources,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(page);
        DataTable rows = await _listings
            .GetListingAsync(page.UriHash, cancellationToken)
            .ConfigureAwait(false);
        if (rows.Rows.Count != 1)
        {
            return null;
        }

        Listing listing = ListingDataMapper.Map(rows.Rows[0]);
        Task<DataTable>? media = loadMedia
            ? _media.GetMediaAsync(listing, cancellationToken)
            : null;
        Task<DataTable>? sources = loadSources
            ? _sources.GetSourcesAsync(listing, cancellationToken)
            : null;

        if (media is not null)
        {
            listing.SetMedia(MapMedia(await media.ConfigureAwait(false)));
        }
        if (sources is not null)
        {
            listing.SetSources(MapSources(await sources.ConfigureAwait(false)));
        }

        return listing;
    }
    public IReadOnlyCollection<Listing> GetUnpublishedBefore(DateTime unlistingDate) =>
        Map(_listings.GetUnpublishedListings(unlistingDate), loadMedia: false, loadSources: true);

    private SortedSet<Listing> Map(
        DataTable rows,
        bool loadMedia,
        bool loadSources)
    {
        SortedSet<Listing> listings = new(new ListingComparer());
        foreach (DataRow row in rows.Rows)
        {
            listings.Add(Map(row, loadMedia, loadSources));
        }
        return listings;
    }

    private Listing Map(DataRow row, bool loadMedia, bool loadSources)
    {
        Listing listing = ListingDataMapper.Map(row);
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

    private static SortedSet<Media> MapMedia(DataTable rows)
    {
        SortedSet<Media> result = new(new MediaComparer());
        foreach (DataRow row in rows.Rows)
        {
            Media? media = MediaDataMapper.Map(row);
            if (media is not null)
            {
                result.Add(media);
            }
        }
        return result;
    }

    private static SortedSet<Source> MapSources(DataTable rows)
    {
        SortedSet<Source> result = new(new SourceComparer());
        foreach (DataRow row in rows.Rows)
        {
            Source? source = SourceDataMapper.Map(row);
            if (source is not null)
            {
                result.Add(source);
            }
        }
        return result;
    }
    private SortedSet<Media> ReadMedia(Listing listing)
    {
        SortedSet<Media> result = new(new MediaComparer());
        foreach (DataRow row in _media.GetMedia(listing).Rows)
        {
            Media? media = MediaDataMapper.Map(row);
            if (media is not null)
            {
                result.Add(media);
            }
        }
        return result;
    }

    private SortedSet<Source> ReadSources(Listing listing)
    {
        SortedSet<Source> result = new(new SourceComparer());
        foreach (DataRow row in _sources.GetSources(listing).Rows)
        {
            Source? source = SourceDataMapper.Map(row);
            if (source is not null)
            {
                result.Add(source);
            }
        }
        return result;
    }
}
