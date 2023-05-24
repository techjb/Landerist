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
        public static Tuple<double, double>? Parse(string cadastralReference)
        {
            string requestUrl =
                "https://api.goolzoom.com/v1/cadastre/cadastralreference/" +
                cadastralReference + "/center";

            HttpClient client = new();
            client.DefaultRequestHeaders.Add("x-api-key", Configuration.Config.GOOLZOOM_API);
            try
            {
                var response = client.GetAsync(requestUrl).Result;
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;
                if (content != null)
                {
                    var goolzoomCenter = JsonConvert.DeserializeObject<GoolzoomCenter>(content);
                    if (goolzoomCenter != null)
                    {
                        return Tuple.Create(goolzoomCenter.lat, goolzoomCenter.lng);
                    }
                }
            }
            catch
            {

            }
            return null;
        }
    }
}
