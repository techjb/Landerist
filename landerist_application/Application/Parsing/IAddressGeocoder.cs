using landerist_library.Websites;

namespace landerist_library.Application.Parsing;

public readonly record struct GeocodedLocation(
    double Latitude,
    double Longitude,
    bool IsAccurate);

public interface IAddressGeocoder
{
    GeocodedLocation? GetLatLng(
        string address,
        CountryCode countryCode);
}
