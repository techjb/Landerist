using landerist_library.Infrastructure.Location.Candidates;
using landerist_library.Application.Parsing;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Location.Resolvers
{
    public sealed class GoogleMapsAddressLocationResolver
    {
        private readonly CountryCode CountryCode;
        private readonly ICoordinateValidator CoordinateValidator;
        private readonly IAddressGeocoder Geocoder;

        public GoogleMapsAddressLocationResolver(
            CountryCode countryCode,
            ICoordinateValidator coordinateValidator,
            IAddressGeocoder geocoder)
        {
            CountryCode = countryCode;
            CoordinateValidator = coordinateValidator;
            Geocoder = geocoder;
        }

        public bool TryResolve(string? address, out LocationCandidate? candidate)
        {
            candidate = null;
            if (string.IsNullOrWhiteSpace(address))
            {
                return false;
            }

            var result = Geocoder.GetLatLng(address, CountryCode);
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