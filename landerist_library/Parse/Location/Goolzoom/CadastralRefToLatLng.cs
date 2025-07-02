using landerist_library.Database;
using Newtonsoft.Json;

namespace landerist_library.Parse.Location.Goolzoom
{
    public class GoolzoomCenter
    {
        public double lat;
        public double lng;
    }

    public class CadastralRefToLatLng
    {
        public (bool requestSucess, double? lat, double? lng)? GetLatLng(string cadastralReference)
        {
            if (string.IsNullOrEmpty(cadastralReference))
            {
                return null;
            }
            string requestUrl =
                "https://api.goolzoom.com/v1/cadastre/cadastralreference/" +
                cadastralReference + "/center";


            bool requestSucess = false;
            double? lat = null;
            double? lng = null;

            try
            {
                HttpClient client = new();
                client.DefaultRequestHeaders.Add("x-api-key", Configuration.PrivateConfig.GOOLZOOM_API);
                var response = client.GetAsync(requestUrl).Result;
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;
                requestSucess = true;
                if (!string.IsNullOrEmpty(content))
                {
                    var goolzoomCenter = JsonConvert.DeserializeObject<GoolzoomCenter>(content);
                    if (goolzoomCenter != null)
                    {
                        lng = goolzoomCenter.lng;
                        lat = goolzoomCenter.lat;
                    }
                }
            }
            catch
            {

            }
            return (requestSucess, lat, lng);
        }

        public static void UpdateLocationFromCadastralRef()
        {
            var listings = ES_Listings.GetListingWithCatastralReference();
            int total = listings.Count;
            int processed = 0;
            int updated = 0;
            int errors = 0;
            foreach (var listing in listings)
            {
                processed++;
                var latLng = new CadastralRefToLatLng().GetLatLng(listing.cadastralReference);
                if (latLng != null)
                {
                    listing.latitude = latLng.Value.lat;
                    listing.longitude = latLng.Value.lng;
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
                Console.WriteLine($"Processed {processed}/{total}, Updated: {updated}, Errors: {errors}");
            }
        }
    }
}
