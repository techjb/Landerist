using landerist_library.Database;
using NetTopologySuite.Triangulate.Tri;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;
using System.Data;
using System.Globalization;

namespace landerist_library.Parse.Location.Goolzoom
{
    public class GoolzoomCenter
    {
        public double lat;
        public double lng;
    }

    public class GoolzoomApi
    {
        public (bool requestSucess, double? lat, double? lng)? GetLatLng(string cadastralReference)
        {
            if (string.IsNullOrEmpty(cadastralReference))
            {
                return null;
            }
            string url =
                "https://api.goolzoom.com/v1/cadastre/cadastralreference/" +
                cadastralReference + "/center";


            bool requestSucess = false;
            double? lat = null;
            double? lng = null;

            try
            {
                var httpClient = GetHttpClient();
                var response = httpClient.GetAsync(url).Result;
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

        public string? GetAddrees(string cadastralReference)
        {
            if (string.IsNullOrEmpty(cadastralReference))
            {
                return null;
            }
            bool isCadastraReference = cadastralReference.Length.Equals(20);
            string url =
                "https://api.goolzoom.com/v1/cadastre/" +
                (isCadastraReference ? "cadastralreference/" : "cadastralparcel/") +
                cadastralReference + "/dataTable?language=es";

            try
            {
                var httpClient = GetHttpClient();
                var response = httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(content))
                {
                    var data = JsonConvert.DeserializeObject<dynamic>(content);
                    return GetAddress(data);
                }
            }
            catch
            {
            }
            return null;
        }

        private static string? GetAddress(dynamic data)
        {
            if (data == null)
            {
                return null;
            }
            dynamic? registros = null;
            if (data.datos != null)
            {
                registros = data.datos[0].registros;
            }
            else if (data.registros != null)
            {
                registros = data.registros;
            }
            if (registros != null)
            {
                List<string> addressParts =
                [
                    (string)registros.finca.Dirección,
                    (string)registros.finca.Municipio,
                    (string)registros.finca.Provincia
                ];
                return string.Join(", ", addressParts.Where(part => !string.IsNullOrEmpty(part)));
            }
            return null;
        }

        private string? GetCadastalReference(double? latitude, double? longitude)
        {
            if (latitude == null || longitude == null)
            {
                return null;
            }
            string url =
                "https://api.goolzoom.com/v1/cadastre/latlng/" +
                ((double)latitude).ToString(CultureInfo.InvariantCulture) + "/" +
                ((double)longitude).ToString(CultureInfo.InvariantCulture) + "/" +
                "cadastralreferences";
            try
            {
                var httpClient = GetHttpClient();
                var response = httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(content))
                {
                    var data = JsonConvert.DeserializeObject<dynamic>(content);
                    return GetCadastalReference(data);
                }
            }
            catch
            {
            }
            return null;
        }
        private static string? GetCadastalReference(dynamic data)
        {
            if (data?.cadastralreferences is not JArray cadastralReferences || cadastralReferences.Count == 0)
            {
                return null;
            }

            var cadastralReference = (string?)cadastralReferences[0]?["cadatralreference"];
            if (string.IsNullOrEmpty(cadastralReference))
            {
                return null;
            }

            return cadastralReferences.Count == 1
                ? cadastralReference
                : cadastralReference.Length >= 14
                    ? cadastralReference[..14]
                    : null;
        }       

        private HttpClient GetHttpClient()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("x-api-key", Configuration.PrivateConfig.GOOLZOOM_API);
            return client;
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
                var latLng = new GoolzoomApi().GetLatLng(listing.cadastralReference);
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

        public static void UpdateAddressFromCadastralRef()
        {
            var listings = ES_Listings.GetListingWithCatastralReferenceAndNoAddress();
            int total = listings.Count;
            int processed = 0;
            int updated = 0;
            int errors = 0;
            foreach (var listing in listings)
            {
                processed++;
                var address = new GoolzoomApi().GetAddrees(listing.cadastralReference);
                if (address != null)
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
                Console.WriteLine($"Processed {processed}/{total}, Updated: {updated}, Errors: {errors}");
            }
        }

        public static void UpdateCadastralReferenceFromLocationIsAccurate()
        {
            var listings = ES_Listings.GetListingsWithoutCatastralReferenceAndLocationIsAccurate();
            int total = listings.Count;
            int processed = 0;
            int updated = 0;
            int errors = 0;

            DataTable dataTable = new();
            dataTable.Columns.Add("LocalId", typeof(string));
            dataTable.Columns.Add("Title", typeof(string));
            dataTable.Columns.Add("DocumentLink", typeof(string));
            dataTable.Columns.Add("Description", typeof(string));


            foreach (var listing in listings)
            {
                processed++;
                if (processed >= 100)
                {
                    break;
                }
                var cadastralReference = new GoolzoomApi().GetCadastalReference(listing.latitude, listing.longitude);
                if (cadastralReference != null)
                {

                    listing.cadastralReference = cadastralReference;
                    //if (ES_Listings.Update(listing))
                    if (true)
                    {
                        updated++;
                    }
                    else
                    {
                        errors++;
                    }

                    DataRow dataRow = dataTable.NewRow();
                    dataRow.ItemArray = [
                        listing.cadastralReference, 
                        listing.address, 
                        listing.sources.FirstOrDefault().sourceUrl,
                        listing.guid,
                        ];
                    dataTable.Rows.Add(dataRow);

                }
                Console.WriteLine($"Processed {processed}/{total}, Updated: {updated}, Errors: {errors}");
            }

            Tools.Csv.Write(dataTable, 
                Configuration.PrivateConfig.EXPORT_DIRECTORY_LOCAL + "CatastralReferencesAddress.csv", true);
        }
    }
}
