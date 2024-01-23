using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_orels.ES;
using landerist_library.Websites;

namespace landerist_library.Export
{
    public class FilesUpdater
    {
        private static DateTime Yesterday;
        public static void UpdateFiles()
        {
            Yesterday = DateTime.Now.AddDays(-1);
            try
            {
                UpdatFilesAllListings();
                UpdateFilesUpdatedYesterday();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("ExportAllListings", exception);
            }
        }

        private static void UpdatFilesAllListings()
        {
            Console.WriteLine("Exporting all listings ..");
            var listings = ES_Listings.GetListings(true);
            bool sucess = Update(listings, "es_listings", "ES\\listings");
            Log.WriteLogInfo("UpdatFilesAllListings", "Sucess: " + sucess.ToString());
        }

        private static void UpdateFilesUpdatedYesterday()
        {
            Console.WriteLine("Exporting updated yesterday ..");
            var listings = ES_Listings.GetListings(true, Yesterday);
            bool sucess = Update(listings, "es_listings_update", "ES\\updated");
            Log.WriteLogInfo("UpdateFilesUpdatedYesterday", "Sucess: " + sucess.ToString());
        }

        private static bool Update(SortedSet<Listing> listings, string fileNameWithoutExtension, string subdirectory)
        {
            if (listings.Count.Equals(0))
            {
                return false;
            }

            string fileNameZip = fileNameWithoutExtension + ".zip";
            string filePathZip = GetFilePath(subdirectory, fileNameZip);

            string filePathJson = GetFilePath(subdirectory, fileNameWithoutExtension + ".json");

            string subdirectoryInBucket = subdirectory.Replace("\\", "/");

            if (Json.ExportListings(listings, filePathJson))
            {
                if (Zip.Compress(filePathJson, filePathZip))
                {
                    if (new S3().UploadFilePublicBucket(filePathZip, fileNameZip, subdirectoryInBucket))
                    {
                        string newFileNameZip = GetFileNameWidhDate(fileNameWithoutExtension) + ".zip";
                        string newFilePathZip = GetFilePath(subdirectory, newFileNameZip);

                        File.Copy(filePathZip, newFilePathZip, true);

                        return new S3().UploadFilePublicBucket(newFilePathZip, newFileNameZip, subdirectoryInBucket);                        
                    }
                }
            }

            return false;
        }

        private static string GetFilePath(string subdirectory, string fileName)
        {
            return Config.EXPORT_DIRECTORY + subdirectory + "\\" + fileName;
        }

        private static string GetFileNameWidhDate(string prefix)
        {
            string datePart = Yesterday.ToString("yyyyMMdd");
            return prefix + "_" + datePart;
        }
    }
}
