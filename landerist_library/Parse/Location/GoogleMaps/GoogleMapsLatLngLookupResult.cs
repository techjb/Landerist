namespace landerist_library.Parse.Location.GoogleMaps
{
    public readonly record struct GoogleMapsLatLngResult(double Latitude, double Longitude, bool IsAccurate);

    internal enum GoogleMapsLatLngLookupStatus
    {
        Found,
        NotFound,
        Error
    }

    internal readonly record struct GoogleMapsLatLngLookupResult(
        GoogleMapsLatLngLookupStatus Status,
        GoogleMapsLatLngResult? Coordinates);
}
