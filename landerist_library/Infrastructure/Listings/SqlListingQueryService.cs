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
