using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_orels.ES;

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
                Listings();
                UpdatedYesterday();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("UpdateFiles", exception);
            }
        }

        private static void Listings()
        {
            Console.WriteLine("Exporting listings ..");
            var listings = ES_Listings.GetListings(true);
            Update(listings, "es_listings", "ES\\listings");
        }

        private static void UpdatedYesterday()
        {
            Console.WriteLine("Exporting updated yesterday ..");
            var listings = ES_Listings.GetListings(true, Yesterday);
            Update(listings, "es_listings_update", "ES\\updated");            
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

            bool sucess = false;

            if (Json.ExportListings(listings, filePathJson))
            {
                if (Zip.Compress(filePathJson, filePathZip))
                {
                    if (new S3().UploadFilePublicBucket(filePathZip, fileNameZip, subdirectoryInBucket))
                    {
                        string newFileNameZip = GetFileNameWidhDate(fileNameWithoutExtension) + ".zip";
                        string newFilePathZip = GetFilePath(subdirectory, newFileNameZip);

                        File.Copy(filePathZip, newFilePathZip, true);

                        sucess = new S3().UploadFilePublicBucket(newFilePathZip, newFileNameZip, subdirectoryInBucket);                        
                    }
                }
            }

            Log.WriteLogInfo("filesupdater", fileNameZip + " Sucess: " + sucess.ToString());
            return sucess;
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
