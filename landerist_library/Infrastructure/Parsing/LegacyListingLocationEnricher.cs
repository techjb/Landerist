using landerist_library.Application.Listings;
using landerist_library.Application.Parsing;
using landerist_library.Database;
using landerist_library.Pages;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Parsing;

public sealed class LegacyListingLocationEnricher : IListingLocationEnricher
{
    private readonly IDatabase _database;
    private readonly IGoolzoomClient _goolzoom;
    private readonly IAddressGeocoder _geocoder;

    public LegacyListingLocationEnricher(
        IDatabase database,
        IGoolzoomClient goolzoom,
        IAddressGeocoder geocoder)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(goolzoom);
        ArgumentNullException.ThrowIfNull(geocoder);
        _database = database;
        _goolzoom = goolzoom;
        _geocoder = geocoder;
    }

    public void EnrichLocation(Page page, Listing listing)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(listing);
        new LocationParser(
            _database,
            page,
            listing,
            _geocoder,
            new AddressToCadastralReference(_database, _goolzoom),
            _goolzoom).SetLocation();
    }
}