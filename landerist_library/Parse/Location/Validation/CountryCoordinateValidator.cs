using landerist_library.Parse.Location.Delimitations;
using landerist_library.Websites;

namespace landerist_library.Parse.Location.Validation
{
    internal sealed class CountryCoordinateValidator
    {
        private readonly CountryCode CountryCode;

        public CountryCoordinateValidator(CountryCode countryCode)
        {
            CountryCode = countryCode;
        }

        public bool Contains(double latitude, double longitude)
        {
            return CountriesParser.ContainsCountry(CountryCode, latitude, longitude);
        }
    }
}
