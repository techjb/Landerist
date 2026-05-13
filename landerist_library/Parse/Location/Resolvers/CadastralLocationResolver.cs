using landerist_library.Parse.Location.Candidates;
using landerist_library.Parse.Location.Providers.Goolzoom;
using landerist_library.Parse.Location.Validation;
using landerist_orels.ES;

namespace landerist_library.Parse.Location.Resolvers
{
    internal sealed class CadastralLocationResolver
    {
        private readonly CountryCoordinateValidator CoordinateValidator;

        public CadastralLocationResolver(CountryCoordinateValidator coordinateValidator)
        {
            CoordinateValidator = coordinateValidator;
        }

        public bool TryResolve(Listing listing, out LocationCandidate? candidate)
        {
            candidate = null;
            if (string.IsNullOrEmpty(listing.cadastralReference))
            {
                return false;
            }

            var goolzoomApi = new GoolzoomApi();
            var result = goolzoomApi.GetLatLng(listing.cadastralReference);
            if (result == null || !result.RequestSuccess)
            {
                return false;
            }
            if (result.Latitude == null || result.Longitude == null)
            {
                return false;
            }
            if (!CoordinateValidator.Contains(result.Latitude.Value, result.Longitude.Value))
            {
                return false;
            }
            if (string.IsNullOrEmpty(listing.address))
            {
                var address = goolzoomApi.GetAddress(listing.cadastralReference);
                if (!string.IsNullOrEmpty(address))
                {
                    listing.address = address;
                }
            }

            candidate = new LocationCandidate(
                result.Latitude.Value,
                result.Longitude.Value,
                true,
                LocationCandidateSources.CadastralReference);
            return true;
        }
    }
}
