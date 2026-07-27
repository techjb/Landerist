using landerist_library.Parse.Location.Candidates;
using landerist_library.Parse.Location.Providers.Goolzoom;
using landerist_library.Parse.Location.Validation;
using landerist_orels.ES;

namespace landerist_library.Parse.Location.Resolvers
{
    internal sealed class CadastralLocationResolver
    {
        private readonly CountryCoordinateValidator CoordinateValidator;
        private readonly IGoolzoomClient Goolzoom;

        public CadastralLocationResolver(CountryCoordinateValidator coordinateValidator, IGoolzoomClient goolzoom)
        {
            ArgumentNullException.ThrowIfNull(goolzoom);
            CoordinateValidator = coordinateValidator;
            Goolzoom = goolzoom;
        }

        public bool TryResolve(Listing listing, out CadastralLocationResolution? resolution)
        {
            resolution = null;
            if (string.IsNullOrWhiteSpace(listing.cadastralReference))
            {
                return false;
            }

            var result = Goolzoom.GetLatLng(listing.cadastralReference);
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

            string? resolvedAddress = null;
            if (string.IsNullOrWhiteSpace(listing.address))
            {
                resolvedAddress = Goolzoom.GetAddress(listing.cadastralReference);
            }

            var candidate = new LocationCandidate(
                result.Latitude.Value,
                result.Longitude.Value,
                true,
                LocationCandidateSources.CadastralReference);
            resolution = new CadastralLocationResolution(candidate, resolvedAddress);
            return true;
        }
    }
}
