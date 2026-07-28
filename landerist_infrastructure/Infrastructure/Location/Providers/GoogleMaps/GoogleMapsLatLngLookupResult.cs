namespace landerist_library.Infrastructure.Location.Providers.GoogleMaps
{
    public readonly record struct GoogleMapsLatLngResult(double Latitude, double Longitude, bool IsAccurate);

    public enum GoogleMapsLatLngLookupStatus
    {
        Found,
        NotFound,
        Error
    }

    public readonly record struct GoogleMapsLatLngLookupResult(
        GoogleMapsLatLngLookupStatus Status,
        GoogleMapsLatLngResult? Coordinates);
}

