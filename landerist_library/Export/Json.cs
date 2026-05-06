using landerist_orels.ES;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace landerist_library.Export
{
    public class Json
    {
        public static string SerializeListing(Listing listing)
        {
            using var textWriter = new StringWriter();
            using var writer = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented
            };

            var serializer = CreateSerializer();
            serializer.Serialize(writer, listing);
            return textWriter.ToString();
        }

        public static bool ExportListings(SortedSet<Listing> listings, string filePath)
        {
            try
            {
                var schema = new Schema(listings);
                File.Delete(filePath);
                using var file = File.CreateText(filePath);
                using var writer = new JsonTextWriter(file)
                {
                    Formatting = Formatting.Indented
                };
                var serializer = CreateSerializer();
                serializer.Serialize(writer, schema);
                return true;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("Json ExportListings", exception);
            }
            return false;
        }

        private static JsonSerializer CreateSerializer()
        {
            var serializer = new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MaxDepth = 5,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-ddTHH:mm:ssK",
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };
            serializer.Converters.Add(new StringEnumConverter());
            return serializer;
        }
    }
}
