using landerist_library.Application.Parsing;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Location.Parsing;

public sealed class LauIdParser(
    ILocalAdministrativeAreaLookup areas,
    CountryCode countryCode,
    Listing listing)
{
    public void SetLauIdAndLauName()
    {
        if (listing.latitude is not double latitude ||
            listing.longitude is not double longitude)
        {
            return;
        }

        LocalAdministrativeArea? area = areas.Find(
            countryCode,
            latitude,
            longitude);
        if (area is null)
        {
            return;
        }

        listing.lauId = area.Id;
        listing.lauName = area.Name;
    }
}
