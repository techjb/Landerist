using landerist_library.Database;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Landerist_com
{
    public class DownloadFilesUpdater : Landerist_com
    {
        private static DateTime Yesterday;
        public static void UpdateFiles()
        {
            Yesterday = DateTime.Now.AddDays(-1);
            try
            {
                UpdateListings();
                UpdateUpdates();
            }
            catch (Exception exception)
            {
                Log.WriteError("UpdateFiles", exception);
            }
        }

        public static void UpdateListings()
        {
            Console.WriteLine("Reading Listings ..");
            var listings = ES_Listings.GetAll(true);
            if (!Update(listings, CountryCode.ES, ExportType.Listings))
            {
                Log.WriteError("filesupdater", "Error updating Listings");
            }
        }

        public static void UpdateUpdates()
        {
            Console.WriteLine("Reading Updates ..");
            var listings = ES_Listings.GetListings(true, Yesterday);
            if (!Update(listings, CountryCode.ES, ExportType.Updates))
            {
                Log.WriteError("filesupdater", "Error updating Updates");
            }
        }

        private static bool Update(SortedSet<Listing> listings, CountryCode countryCode, ExportType exportType)
        {
            Console.WriteLine("Updating " + exportType.ToString() + "..");
            if (listings.Count.Equals(0))
            {
                return true;
            }

            string fileNameZip = GetFileName(countryCode, exportType, "zip");
            string subdirectory = GetLocalSubdirectory(countryCode, exportType);
            string filePathZip = GetFilePath(subdirectory, fileNameZip);
            string fileNameJson = GetFileName(countryCode, exportType, "json");
            string filePathJson = GetFilePath(subdirectory, fileNameJson);
            string subdirectoryInBucket = subdirectory.Replace("\\", "/");

            if (!Json.ExportListings(listings, filePathJson))
            {
                return false;
            }
            if (!Zip.Compress(filePathJson, filePathZip))
            {
                return false;
            }
            if (!new S3().UploadToDownloadsBucket(filePathZip, fileNameZip, subdirectoryInBucket))
            {
                return false;
            }
            if (!UploadHistoricFile(countryCode, exportType, filePathZip, subdirectoryInBucket))
            {
                return false;
            }

            Log.WriteInfo("filesupdater", fileNameZip);
            return true;
        }

        private static bool UploadHistoricFile(CountryCode countryCode, ExportType exportType, string filePathZip, string subdirectoryInBucket)
        {
            string fileNameWithoutExtension = GetFileName(countryCode, exportType);
            string newFileNameZip = GetFileNameWidhDate(Yesterday, fileNameWithoutExtension, "zip");
            string subdirectory = GetLocalSubdirectory(countryCode, exportType);
            string newFilePathZip = GetFilePath(subdirectory, newFileNameZip);

            File.Copy(filePathZip, newFilePathZip, true);

            bool sucess = new S3().UploadToDownloadsBucket(newFilePathZip, newFileNameZip, subdirectoryInBucket);
            File.Delete(newFilePathZip);
            return sucess;
        }
    }
}
