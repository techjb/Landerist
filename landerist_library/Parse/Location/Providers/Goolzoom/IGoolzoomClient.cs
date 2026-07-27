namespace landerist_library.Parse.Location.Providers.Goolzoom;

public interface IGoolzoomClient
{
    GoolzoomLatLngResult? GetLatLng(string cadastralReference);

    string? GetAddress(string cadastralReference);

    string? GetAddresses(double latitude, double longitude, int radius);
}
