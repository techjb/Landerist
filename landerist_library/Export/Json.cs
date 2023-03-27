using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using landerist_orels.ES;
using landerist_library.ES;

namespace landerist_library.Export
{
    public class Json
    {
        private const string FILE_NAME = "es_listings.json";
        public void Export()
        {
            var listings = new Listings().GetAll(true);
            var schema = new Schema(listings);
            var jsonSereializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };
            jsonSereializerSettings.Converters.Add(new StringEnumConverter());

            string json = JsonConvert.SerializeObject(schema, Formatting.Indented, jsonSereializerSettings);
            string file = Config.EXPORT_DIRECTORY + FILE_NAME;
            File.Delete(file);
            File.WriteAllText(file, json);
        }
    }
}
