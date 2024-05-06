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
                Listings();
                Updates();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("UpdateFiles", exception);
            }
        }

        private static void Listings()
        {
            Console.WriteLine("Exporting Listings ..");
            var listings = ES_Listings.GetListings(true);
            if (!Update(listings, CountryCode.ES, ExportType.Listings))
            {
                Log.WriteLogErrors("filesupdater", "Error updating Listings");
            }
        }

        private static void Updates()
        {
            Console.WriteLine("Exporting Updates ..");
            var listings = ES_Listings.GetListings(true, Yesterday);
            if (!Update(listings, CountryCode.ES, ExportType.Updates))
            {
                Log.WriteLogErrors("filesupdater", "Error updating Updates");
            }
        }

        private static bool Update(SortedSet<Listing> listings, CountryCode countryCode, ExportType exportType)
        {
            if (listings.Count.Equals(0))
            {
                return false;
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

            Log.WriteLogInfo("filesupdater", fileNameZip);
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
