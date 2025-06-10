using landerist_orels.ES;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace landerist_library.Export
{
    public class Json
    {
        public static bool ExportListings(SortedSet<Listing> listings, string filePath)
        {
            try
            {
                var schema = new Schema(listings);
                //string json = schema.Serialize();
                //if (File.Exists(filePath))
                //{
                //    File.Delete(filePath);
                //}
                //File.WriteAllText(filePath, json);

                File.Delete(filePath);
                using var file = File.CreateText(filePath);
                using var writer = new JsonTextWriter(file);
                var serializer = new JsonSerializer
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 5
                };
                serializer.Serialize(writer, schema);
                return true;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("Json ExportListings", exception);
            }
            return false;
        }
    }
}
