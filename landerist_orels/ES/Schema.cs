using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace landerist_orels.ES
{
    public class Schema
    {
        [JsonProperty(Order = 1)]
        public string schemaUrl { get; set; } = "https://github.com/techjb/Open-Real-Estate-Listings-Schema/blob/master/ES/1.0.json";

        [JsonProperty(Order = 2)]
        public DateTime created { get; set; } = DateTime.Now;

        [JsonProperty(Order = 3)]
        public List<Listing> listings = new List<Listing>();

        public Schema(List<Listing> listings)
        {
            foreach(Listing listing in listings)
            {
                AddListing(listing);
            }
        }

        public void AddListing(Listing listing)
        {
            if (!listings.Contains(listing))
            {
                listings.Add(listing);
            }
        }
    }
}
