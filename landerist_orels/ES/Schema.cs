﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        public SortedSet<Listing> listings = new SortedSet<Listing>(new ListingComparer());

        public Schema()
        {

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
            var jsonSereializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };
            jsonSereializerSettings.Converters.Add(new StringEnumConverter());
            jsonSereializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ssZ";

            return JsonConvert.SerializeObject(this, Formatting.Indented, jsonSereializerSettings);
        }
    }
}
