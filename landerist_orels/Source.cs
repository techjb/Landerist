using Newtonsoft.Json;
using System;

namespace landerist_orels
{
    public class Source
    {
#pragma warning disable IDE1006 // Naming Styles
        [JsonProperty(Order = 1)]
        public string sourceName { get; set; }

        [JsonProperty(Order = 2)]
        public Uri sourceUrl { get; set; }

        [JsonProperty(Order = 3)]
        public string sourceGuid { get; set; }

#pragma warning restore IDE1006
    }
}
