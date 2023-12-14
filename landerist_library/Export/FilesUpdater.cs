using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_orels.ES;

namespace landerist_library.Export
{
    public class FilesUpdater
    {
        public static void Update()
        {
            try
            {
                FileAllListings();
                FileUpdatedYesterday();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("ExportAllListings", exception);
            }
        }

        private static void FileAllListings()
        {
            Console.WriteLine("Exporting all listings ..");
            var listings = ES_Listings.GetListings(true);
            DateTime dateTime = DateTime.Now.AddDays(-1);
            bool sucess = Export(listings, "es_listings", dateTime);
            Log.WriteLogInfo("ExportAllListings", "Sucess: " + sucess.ToString());
        }

        private static void FileUpdatedYesterday()
        {
            Console.WriteLine("Exporting updated yesterday ..");
            DateTime dateTime = DateTime.Now.AddDays(-1);
            var listings = ES_Listings.GetListings(true, dateTime);
            bool sucess = Export(listings, "es_listings_update", dateTime);
            Log.WriteLogInfo("ExportUpdatedYesterday", "Sucess: " + sucess.ToString());
        }

        private static bool Export(SortedSet<Listing> listings, string fileName, DateTime dateTime)
        {
            string filePathJson = Config.EXPORT_DIRECTORY + fileName + ".json";
            string fileNameZip = fileName + ".zip";
            string filePathZip = Config.EXPORT_DIRECTORY + fileNameZip;

            File.Delete(filePathJson);
            File.Delete(filePathZip);

            bool sucess = false;

            if (listings.Count > 0)
            {
                if (Json.ExportListings(listings, filePathJson))
                {
                    if (Zip.Compress(filePathJson, filePathZip))
                    {
                        if (new S3().UploadFile(filePathZip, fileNameZip))
                        {
                            string newFileNameZip = GetFileName(fileName, dateTime) + ".zip";
                            string newFilePathZip = Config.EXPORT_DIRECTORY + newFileNameZip;

                            File.Copy(filePathZip, newFilePathZip, true);

                            if (new S3().UploadFile(newFilePathZip, newFileNameZip))
                            {
                                sucess = true;
                            }
                        }
                    }
                }
            }

            return sucess;
        }

        private static string GetFileName(string prefix, DateTime dateTime)
        {
            string datePart = dateTime.ToString("yyyyMMdd");
            return prefix + "_" + datePart;
        }
    }
}
