using landerist_library.Infrastructure.Location.Candidates;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Application.Parsing;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Location.Resolvers
{
    public sealed class CadastralLocationResolver
    {
        private readonly ICoordinateValidator CoordinateValidator;
        private readonly IGoolzoomClient Goolzoom;

        public CadastralLocationResolver(ICoordinateValidator coordinateValidator, IGoolzoomClient goolzoom)
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
