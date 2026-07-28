using landerist_library.Application.Listings;
using landerist_library.Database;
using landerist_library.Pages;
using landerist_library.Parse.Location.Providers.GoogleMaps;
using landerist_library.Parse.Location.Providers.Goolzoom;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Parsing;

public sealed class LegacyListingLocationEnricher : IListingLocationEnricher
{
    private readonly IDatabase _database;
    private readonly IGoolzoomClient _goolzoom;

    public LegacyListingLocationEnricher(
        IDatabase database,
        IGoolzoomClient goolzoom)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(goolzoom);
        _database = database;
        _goolzoom = goolzoom;
    }

    public void EnrichLocation(Page page, Listing listing)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(listing);
        new LocationParser(
            page,
            listing,
            new GoogleMapsApi(_database),
            new AddressToCadastralReference(_database, _goolzoom),
            _goolzoom).SetLocation();
    }
}