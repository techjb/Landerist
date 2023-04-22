using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using landerist_orels.ES;
using landerist_library.Database;
using landerist_library.Configuration;

namespace landerist_library.Export
{
    public class Json
    {
        private const string JSON_FILE_NAME = "es_listings.json";

        private const string ZIP_FILE_NAME = JSON_FILE_NAME + ".zip";

        public static bool Export(bool makeZip)
        {
            var listings = new ES_Listings().GetAll(true);
            var schema = new Schema(listings);
            var jsonSereializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };
            jsonSereializerSettings.Converters.Add(new StringEnumConverter());

            string json = JsonConvert.SerializeObject(schema, Formatting.Indented, jsonSereializerSettings);
            string jsonFile = Config.EXPORT_DIRECTORY + JSON_FILE_NAME;
            File.Delete(jsonFile);
            File.WriteAllText(jsonFile, json);

            if (makeZip)
            {
                string zipFile = Config.EXPORT_DIRECTORY + ZIP_FILE_NAME;
                File.Delete(zipFile);
                return new Zip().Export(jsonFile, zipFile);
            }
            return true;
        }
    }
}
