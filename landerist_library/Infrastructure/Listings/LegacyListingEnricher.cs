using landerist_library.Application.Listings;
using landerist_library.Pages;
using landerist_library.Parse.Location;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Listings;

public sealed class LegacyListingEnricher : IListingEnricher
{
    public void Enrich(Page page, Listing listing)
    {
        new LocationParser(page, listing).SetLocation();
        new LauIdParser(page.Website.CountryCode, listing).SetLauIdAndLauName();
    }
}
