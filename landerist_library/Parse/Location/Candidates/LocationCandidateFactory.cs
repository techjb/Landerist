using System.Globalization;
using landerist_library.Parse.Location.Validation;

namespace landerist_library.Parse.Location.Candidates
{
    internal sealed class LocationCandidateFactory
    {
        private readonly CountryCoordinateValidator CoordinateValidator;

        public LocationCandidateFactory(CountryCoordinateValidator coordinateValidator)
        {
            CoordinateValidator = coordinateValidator;
        }

        public bool TryCreate(
            string latitude,
            string longitude,
            bool isAccurate,
            string source,
            out LocationCandidate? candidate)
        {
            candidate = null;
            if (double.TryParse(longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lng) &&
                double.TryParse(latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
            {
                return TryCreate(lat, lng, isAccurate, source, out candidate);
            }

            return false;
        }

        public bool TryCreate(
            double latitude,
            double longitude,
            bool isAccurate,
            string source,
            out LocationCandidate? candidate)
        {
            candidate = null;
            if (!CoordinateValidator.Contains(latitude, longitude))
            {
                return false;
            }

            candidate = new LocationCandidate(latitude, longitude, isAccurate, source);
            return true;
        }
    }
}
