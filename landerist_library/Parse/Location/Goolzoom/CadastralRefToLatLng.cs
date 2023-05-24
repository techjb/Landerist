using landerist_library.Parse.Location.GoogleMaps;
using Newtonsoft.Json;

namespace landerist_library.Parse.Location.Goolzoom
{

    //public class GoolzoomFeature
    //{

    //}

    //public class GoolzoomResponse
    //{
    //    public GoolzoomFeature[]? features { get; set; }
    //}

    public class CadastralRefToLatLng
    {
        public static Tuple<string, string>? Parse(string cadastralReference)
        {
            string requestUrl =
                "https://api.goolzoom.com/v1/cadastre/cadastralreference/" + cadastralReference + "/geo";

            try
            {
                HttpClient client = new();
                client.DefaultRequestHeaders.Add("x-api-key", "application/json");
                var response = client.GetAsync(requestUrl).Result;
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;
                if (content != null)
                {
                    //GoolzoomResponse? geocodeData = JsonConvert.DeserializeObject<GoolzoomResponse>(content);
                    //if (geocodeData != null && geocodeData.results != null)
                    //{
                    //    if (geocodeData.results.Count() > 0)
                    //    {
                    //        var result = geocodeData.results[0];
                    //        if (result.geometry != null &&
                    //            result.geometry.location != null &&
                    //            result.geometry.location.lat != null &&
                    //            result.geometry.location.lng != null
                    //            )
                    //        {
                    //            string lat = result.geometry.location.lat;
                    //            string lng = result.geometry.location.lng;
                    //            return Tuple.Create(lat, lng);
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

    }
}
