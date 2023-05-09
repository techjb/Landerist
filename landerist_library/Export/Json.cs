using landerist_library.Configuration;
using landerist_library.Database;
using landerist_orels.ES;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace landerist_library.Export
{
    public class Json
    {
        private const string JSON_FILE_NAME = "es_listings.json";

        private const string ZIP_FILE_NAME = JSON_FILE_NAME + ".zip";

        public static bool Export(bool makeZip)
        {
            var listings = ES_Listings.GetAll(true);
            var schema = new Schema(listings);
            string json = schema.Serialize();
            string jsonFile = Config.EXPORT_DIRECTORY + JSON_FILE_NAME;
            File.Delete(jsonFile);
            File.WriteAllText(jsonFile, json);

            if (makeZip)
            {
                string zipFile = Config.EXPORT_DIRECTORY + ZIP_FILE_NAME;
                File.Delete(zipFile);
                Zip.Export(jsonFile, zipFile);
            }
            return true;
        }
    }
}
