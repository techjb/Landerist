using landerist_library.Parse.Location.Candidates;
using landerist_library.Parse.Location.Providers.GoogleMaps;
using landerist_library.Parse.Location.Validation;
using landerist_library.Websites;

namespace landerist_library.Parse.Location.Resolvers
{
    internal sealed class GoogleMapsAddressLocationResolver
    {
        private readonly CountryCode CountryCode;
        private readonly CountryCoordinateValidator CoordinateValidator;

        public GoogleMapsAddressLocationResolver(
            CountryCode countryCode,
            CountryCoordinateValidator coordinateValidator)
        {
            CountryCode = countryCode;
            CoordinateValidator = coordinateValidator;
        }

        public bool TryResolve(string? address, out LocationCandidate? candidate)
        {
            candidate = null;
            if (string.IsNullOrEmpty(address))
            {
                return false;
            }

            var result = new GoogleMapsApi().GetLatLng(address, CountryCode);
            if (result == null ||
                !CoordinateValidator.Contains(result.Value.Latitude, result.Value.Longitude))
            {
                return false;
            }

            candidate = new LocationCandidate(
                result.Value.Latitude,
                result.Value.Longitude,
                result.Value.IsAccurate,
                LocationCandidateSources.GoogleMapsAddress);
            return true;
        }
    }
}
