using Newtonsoft.Json;
using System;

namespace landerist_orels.ES
{
    public enum MediaType
    {
        image,
        video,
        floor_plan,
        text,
        other
    }
    public class Media
    {
        [JsonProperty(Order = 1)]
        public MediaType? mediaType { get; set; }

        [JsonProperty(Order = 2)]
        public string title { get; set; }

        [JsonProperty(Order = 3)]
        public Uri url { get; set; }
    }
}
