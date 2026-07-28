using landerist_library.Application.Listings;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Infrastructure.Location.Candidates;

namespace landerist_library.Infrastructure.Parsing
{
    public sealed class GoolzoomListingsUpdater(IListingAdministrationService listings, IGoolzoomClient goolzoom)
    {
        public void UpdateLocationFromCadastralRef()
        {
            var items = listings.GetWithCadastralReference();
            int total = items.Count;
            int processed = 0;
            int updated = 0;
            int errors = 0;


            foreach (var listing in items)
            {
                processed++;

                var latLng = goolzoom.GetLatLng(listing.cadastralReference);
                if (latLng is { RequestSuccess: true, Latitude: double latitude, Longitude: double longitude })
                {
                    listing.latitude = latitude;
                    listing.longitude = longitude;
                    listing.locationIsAccurate = true;
                    listing.locationResolver = LocationCandidateSources.CadastralReference;

                    if (listings.Update(listing))
                    {
                        updated++;
                    }
                    else
                    {
                        errors++;
                    }
                }
                else
                {
                    errors++;
                }

                Console.WriteLine($"Processed {processed}/{total}, Updated: {updated}, Errors: {errors}");
            }
        }

        public void UpdateAddressFromCadastralRef()
        {
            var items = listings.GetWithCadastralReferenceAndNoAddress();
            int total = items.Count;
            int processed = 0;
            int updated = 0;
            int errors = 0;


            foreach (var listing in items)
            {
                processed++;

                var address = goolzoom.GetAddress(listing.cadastralReference);
                if (!string.IsNullOrWhiteSpace(address))
                {
                    listing.address = address;

                    if (listings.Update(listing))
                    {
                        updated++;
                    }
                    else
                    {
                        errors++;
                    }
                }
                else
                {
                    errors++;
                }

                Console.WriteLine($"Processed {processed}/{total}, Updated: {updated}, Errors: {errors}");
            }
        }
    }
}
