using Newtonsoft.Json;

namespace landerist_orels
{
    public class ORELS
    {
        [JsonProperty(Order = 1)]
        public double schemaVersion { get; set; } = 1.0;

        [JsonProperty(Order = 2)]
        public string schemaUrl { get; set; } = "https://github.com/techjb/Open-Real-Estate-Listings-Schema/blob/master/ES/1.0.json";

        [JsonProperty(Order = 3)]
        public DateTime updated { get; set; } = DateTime.Now;

        [JsonProperty(Order = 4)]
        public List<Listing> listings = new();

        public void AddListing(Listing listing)
        {
            if (!listings.Contains(listing))
            {
                listings.Add(listing);
            }            
        }
    }
}
