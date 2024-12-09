using landerist_library.Configuration;
using landerist_library.Websites;
using Newtonsoft.Json;

namespace landerist_library.Parse.Location.GoogleMaps
{
#pragma warning disable IDE1006
    public class GeocodeData
    {
        public Result[]? results { get; set; }
    }

    public class Result
    {
        public Geometry? geometry { get; set; }
    }

    public class Geometry
    {
        public Location? location { get; set; }

        public string? location_type { get; set; }
    }

    public class Location
    {
        public double? lat { get; set; }
        public double? lng { get; set; }
    }


    public class AddressToLatLng
    {
        public static (Tuple<double, double>? latLng, bool? isAccurate) Parse(string address, CountryCode countryCode)
        {
            if (Database.AddressLatLng.Select(address, GetRegion(countryCode)) is (double lat, double lng, bool isAccurate))
            {
                return (Tuple.Create(lat, lng), isAccurate);
            }
            return ParseInGoogle(address, countryCode);
        }

        private static (Tuple<double, double>? latLng, bool? isAccurate) ParseInGoogle(string address, CountryCode countryCode)
        {
            string uriAdress = Uri.EscapeDataString(address);
            var region = GetRegion(countryCode);
            string requestUrl =
                "https://maps.googleapis.com/maps/api/geocode/json?" +
                "address=" + uriAdress +
                "&region=" + region +
                "&key=" + PrivateConfig.GOOGLE_CLOUD_LANDERIST_API_KEY;

            try
            {
                HttpClient client = new();
                var response = client.GetAsync(requestUrl).Result;
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;
                if (content == null)
                {
                    return (null, null);
                }
                GeocodeData? geocodeData = JsonConvert.DeserializeObject<GeocodeData>(content);
                if (geocodeData == null || geocodeData.results == null)
                {
                    return (null, null);
                }
                if (geocodeData.results.Length.Equals(0))
                {
                    return (null, null);
                }
                var result = geocodeData.results[0];
                if (result.geometry != null &&
                    result.geometry.location != null &&
                    result.geometry.location.lat != null &&
                    result.geometry.location.lng != null
                    )
                {
                    var geometry = result.geometry;
                    double lat = (double)geometry.location.lat;
                    double lng = (double)geometry.location.lng;
                    var tuple = Tuple.Create(lat, lng);

                    bool isAccurate = false;
                    if (geometry.location_type != null)
                    {
                        isAccurate = geometry.location_type.Equals("ROOFTOP");
                    }

                    Database.AddressLatLng.Insert(address, region, lat, lng, isAccurate);
                    return (tuple, isAccurate);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return (null, null);
        }

        private static string GetRegion(CountryCode countryCode)
        {
            return countryCode switch
            {
                CountryCode.ES => "es",
                _ => string.Empty,
            };
        }
    }
#pragma warning restore IDE1006
}
