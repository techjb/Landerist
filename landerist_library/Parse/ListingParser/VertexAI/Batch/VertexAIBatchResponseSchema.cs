using landerist_library.Parse.ListingParser.StructuredOutputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
    public class VertexAIBatchResponseSchema
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "object";

        [JsonPropertyName("properties")]
        public Properties Properties { get; set; } = new();
    }

    public class Properties
    {
        [JsonPropertyName("es_un_anuncio")]
        public EsUnAnuncio EsUnAnuncio { get; set; } = new();
    }

    public class EsUnAnuncio
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "boolean";

        [JsonPropertyName("description")]
        public string Description { get; set; } = StructuredOutputEsParse.FunctionNameIsListingDescription;
    }
}
