using landerist_library.Websites;

namespace landerist_library.Parse.Location.GoogleMaps
{
    internal static class GoogleMapsCountry
    {
        public static string GetRegion(CountryCode countryCode)
        {
            return countryCode switch
            {
                CountryCode.ES => "es",
                _ => string.Empty,
            };
        }
    }
}
