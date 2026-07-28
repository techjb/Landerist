namespace landerist_library.Parse.Location.Extractors
{
    internal static class CoordinateKeyMatcher
    {
        public static bool IsLatitudeKey(string key)
        {
            return key.Equals("latitude", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("lat", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("latitud", StringComparison.OrdinalIgnoreCase) ||
                key.EndsWith(":latitude", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsLongitudeKey(string key)
        {
            return key.Equals("longitude", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("lng", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("lon", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("longitud", StringComparison.OrdinalIgnoreCase) ||
                key.EndsWith(":longitude", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsGeoPositionKey(string key)
        {
            return key.Equals("geo.position", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("icbm", StringComparison.OrdinalIgnoreCase);
        }
    }
}
