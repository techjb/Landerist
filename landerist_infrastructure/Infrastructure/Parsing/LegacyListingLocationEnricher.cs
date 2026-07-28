using landerist_library.Application.Listings;
using landerist_library.Application.Parsing;
using landerist_library.Database;
using landerist_library.Pages;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Infrastructure.Location.Parsing;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Parsing;

public sealed class LegacyListingLocationEnricher : IListingLocationEnricher
{
    private readonly IDatabase _database;
    private readonly IGoolzoomClient _goolzoom;
    private readonly IAddressGeocoder _geocoder;
    private readonly ICadastralReferenceProvider _cadastralReferences;

    public LegacyListingLocationEnricher(
        IDatabase database,
        IGoolzoomClient goolzoom,
        IAddressGeocoder geocoder,
        ICadastralReferenceProvider cadastralReferences)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(goolzoom);
        ArgumentNullException.ThrowIfNull(geocoder);
        ArgumentNullException.ThrowIfNull(cadastralReferences);
        _database = database;
        _goolzoom = goolzoom;
        _geocoder = geocoder;
        _cadastralReferences = cadastralReferences;
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
            _cadastralReferences,
            _goolzoom).SetLocation();
    }
}