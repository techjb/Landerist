using Newtonsoft.Json;

namespace landerist_orels
{
    public enum MediaType
    {
        image, 
        video, 
        floorplan, 
        other
    }
    public class Media
    {
        [JsonProperty(Order = 1)]
        public MediaType type { get; set; }

        [JsonProperty(Order = 2)]
        public string title { get; set; }

        [JsonProperty(Order = 3)]
        public Uri url { get; set; }
    }
}
