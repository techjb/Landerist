using landerist_library.Database;

namespace landerist_library.Parse.Location.Providers.Goolzoom
{
    public static class GoolzoomListingsUpdater
    {
        public static void UpdateLocationFromCadastralRef()
        {
            var listings = ES_Listings.GetListingWithCatastralReference();
            int total = listings.Count;
            int processed = 0;
            int updated = 0;
            int errors = 0;

            var api = new GoolzoomApi();

            foreach (var listing in listings)
            {
                processed++;

                var latLng = api.GetLatLng(listing.cadastralReference);
                if (latLng is { RequestSuccess: true, Latitude: double latitude, Longitude: double longitude })
                {
                    listing.latitude = latitude;
                    listing.longitude = longitude;
                    listing.locationIsAccurate = true;

                    if (ES_Listings.Update(listing))
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

        public static void UpdateAddressFromCadastralRef()
        {
            var listings = ES_Listings.GetListingWithCatastralReferenceAndNoAddress();
            int total = listings.Count;
            int processed = 0;
            int updated = 0;
            int errors = 0;

            var api = new GoolzoomApi();

            foreach (var listing in listings)
            {
                processed++;

                var address = api.GetAddress(listing.cadastralReference);
                if (!string.IsNullOrWhiteSpace(address))
                {
                    listing.address = address;

                    if (ES_Listings.Update(listing))
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

