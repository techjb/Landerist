using landerist_library.Database;
using landerist_orels.ES;

namespace landerist_library.Export
{
    public class Json
    {
        public static bool ExportListings(SortedSet<Listing> listings, string filePath)
        {
            try
            {
                var schema = new Schema(listings);
                string json = schema.Serialize();                
                File.Delete(filePath);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(exception);
            }
            return false;
        }
    }
}
