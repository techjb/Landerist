using landerist_library.Database;
using landerist_library.Application.Listings;
using landerist_library.Parse.Location.Providers.GoogleMaps;
using landerist_library.Parse.Location.Candidates;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Parsing
{
    public sealed class GoogleMapsListingsLocationUpdater(IListingAdministrationService listings)
    {
        public void UpdateListingsLocationIsAccurate(IDatabase database)
        {
            var items = listings.GetAccurateLocationWithoutCadastralReference();
            if (listings == null || items.Count == 0)
            {
                return;
            }

            var googleMapsApi = new GoogleMapsApi(database);

            int total = items.Count;
            int processed = 0;
            int latLngFound = 0;
            int latLngNotFound = 0;
            int accurate = 0;
            int notAccurate = 0;
            int errors = 0;

            Parallel.ForEach(items,
                new ParallelOptions() { MaxDegreeOfParallelism = 10 },
                listing =>
            {
                double? lat = null;
                double? lng = null;
                bool? locationIsAccurate = null;
                string? locationResolver = null;
                bool updateAddress = true;

                if (listing.address != null)
                {
                    var result = googleMapsApi.GetLatLngLookup(listing.address, CountryCode.ES);
                    if (result.Status == GoogleMapsLatLngLookupStatus.Found && result.Coordinates != null)
                    {
                        lat = result.Coordinates.Value.Latitude;
                        lng = result.Coordinates.Value.Longitude;
                        locationIsAccurate = result.Coordinates.Value.IsAccurate;
                        locationResolver = LocationCandidateSources.GoogleMapsAddress;
                    }
                    else if (result.Status == GoogleMapsLatLngLookupStatus.Error)
                    {
                        updateAddress = false;
                    }
                }

                if (!updateAddress || !listings.UpdateAddress(listing.guid, lat, lng, locationIsAccurate, locationResolver))
                {
                    Interlocked.Increment(ref errors);
                }

                Interlocked.Increment(ref processed);

                if (locationIsAccurate is true)
                {
                    Interlocked.Increment(ref accurate);
                }
                else if (locationIsAccurate is false)
                {
                    Interlocked.Increment(ref notAccurate);
                }

                if (lat is not null && lng is not null)
                {
                    Interlocked.Increment(ref latLngFound);
                }
                else if (updateAddress)
                {
                    Interlocked.Increment(ref latLngNotFound);
                }

                var accuratePercentage = total > 0 ? (double)accurate / total * 100 : 0;
                var notAccuratePercentage = total > 0 ? (double)notAccurate / total * 100 : 0;
                var latLngFoundPercentage = total > 0 ? (double)latLngFound / total * 100 : 0;
                var latLngNotFoundPercentage = total > 0 ? (double)latLngNotFound / total * 100 : 0;

                Console.WriteLine(
                    $"{processed}/{total}. " +
                    $"latLngFound: {latLngFound} ({(int)latLngFoundPercentage}%) " +
                    $"latLngNotFound: {latLngNotFound} ({(int)latLngNotFoundPercentage}%) " +
                    $"Accurate: {accurate} ({(int)accuratePercentage}%) " +
                    $"NotAccurate: {notAccurate} ({(int)notAccuratePercentage}%) " +
                    $"Errors: {errors} "
                    );
            });
        }
    }
}
