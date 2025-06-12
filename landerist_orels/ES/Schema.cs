using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace landerist_orels.ES
{
    public class Schema
    {
#pragma warning disable IDE1006

        [JsonProperty(Order = 1)]
        public string schemaUrl { get; set; } = "https://github.com/techjb/Open-Real-Estate-Listings-Schema/blob/master/ES/1.1.json";

        [JsonProperty(Order = 2)]
        public DateTime created { get; set; } = DateTime.Now;

        [JsonProperty(Order = 3)]
        public SortedSet<Listing> listings = new SortedSet<Listing>(new ListingComparer());

#pragma warning restore IDE1006
        public Schema()
        {

        }

        public Schema(Listing listing)
        {
            SortedSet<Listing> listings = new SortedSet<Listing>()
            {
                listing
            };
            this.listings = listings;
        }

        public Schema(SortedSet<Listing> listings)
        {
            this.listings = listings;
        }

        public void AddListing(Listing listing)
        {
            listings.Add(listing);
        }

        public string Serialize()
        {
            var jsonSerializerSettings = GetJsonSerializerSettings();
            return JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings);
        }

        public Schema Deserialize(string json)
        {
            var jsonSerializerSettings = GetJsonSerializerSettings();
            return JsonConvert.DeserializeObject<Schema>(json, jsonSerializerSettings);
        }

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
            };
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            return jsonSerializerSettings;
        }
    }
}
