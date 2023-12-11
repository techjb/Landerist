using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_orels.ES;

namespace landerist_library.Export
{
    public class Exports
    {
        public static void Start()
        {
            try
            {
                ExportAllListings();
                ExportUpdatedYesterday();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("ExportAllListings", exception);
            }
        }

        private static void ExportAllListings()
        {
            Console.WriteLine("Exporting all listings ..");
            var listings = ES_Listings.GetListings(true);
            string fileName = JsonFileName("es_listings");
            bool sucess = Export(listings, fileName);
            Log.WriteLogInfo("ExportAllListings", "Sucess: " + sucess.ToString());
        }

        private static void ExportUpdatedYesterday()
        {
            Console.WriteLine("Exporting updated yesterday ..");
            DateTime dateTime = DateTime.Now.AddDays(-6);
            string fileName = JsonFileName("es_listings_update", dateTime);            
            var listings = ES_Listings.GetListings(true, dateTime);
            bool sucess = Export(listings, fileName);
            Log.WriteLogInfo("ExportUpdatedYesterday", "Sucess: " + sucess.ToString());
        }

        private static bool Export(SortedSet<Listing> listings, string fileNameJson)
        {
            string fileNameZip = fileNameJson + ".zip";
            string jsonFile = Config.EXPORT_DIRECTORY + fileNameJson;
            string zipFile = Config.EXPORT_DIRECTORY + fileNameZip;

            bool sucess = false;

            if (listings.Count > 0)
            {
                if (Json.ExportListings(listings, jsonFile))
                {
                    if (Zip.Compress(jsonFile, zipFile))
                    {
                        if (new S3().UploadFile(zipFile, fileNameZip))
                        {
                            sucess = true;
                        }
                    }
                }
            }

            File.Delete(jsonFile);
            File.Delete(zipFile);

            return sucess;
        }

        private static string JsonFileName(string prefix)
        {
            return JsonFileName(prefix, DateTime.Now);
        }

        private static string JsonFileName(string prefix, DateTime dateTime)
        {
            string datePart = dateTime.ToString("yyyyMMdd");
            return prefix + "_" + datePart + ".json";
        }
    }
}
