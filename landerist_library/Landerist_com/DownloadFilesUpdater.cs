using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Globalization;

namespace landerist_library.Landerist_com
{
    public class DownloadFilesUpdater : Landerist_com
    {

        private const string keyDateFrom = "dateFrom";
        private const string keyDateTo = "dateTo";

        public static void UpdateListingsAndUpdates()
        {
            try
            {
                UpdateListings();
                UpdateUpdates();
            }
            catch (Exception exception)
            {
                Log.WriteError("UpdateListingsAndUpdates", exception);
            }
        }

        public static void UpdateListings()
        {
            Console.WriteLine("Reading Listings ..");
            var listings = ES_Listings.GetAll(true);
            if (!Update(listings, CountryCode.ES, ExportType.Listings, null, null))
            {
                Log.WriteError("filesupdater", "Error updating Listings");
            }
        }

        public static void UpdateUpdates()
        {
            Console.WriteLine("Reading Updates ..");
            DateOnly dateFrom = GetDateFrom();            
            DateOnly dateTo = Yesterday();
            var listings = ES_Listings.GetListings(true, dateFrom, dateTo);
            if (!Update(listings, CountryCode.ES, ExportType.Updates, dateFrom, dateTo))
            {
                Log.WriteError("filesupdater", "Error updating Updates");
            }
        }

        private static bool Update(SortedSet<Listing> listings, CountryCode countryCode, ExportType exportType, DateOnly? dateFrom, DateOnly? dateTo)
        {
            Console.WriteLine("Updating " + exportType.ToString() + "..");
            if (listings.Count.Equals(0))
            {
                return true;
            }

            string fileNameZip = GetFileName(countryCode, exportType, "zip");
            string subdirectory = GetLocalSubdirectory(countryCode, exportType);
            string filePath = GetFilePath(subdirectory);
            string filePathZip = GetFilePath(subdirectory, fileNameZip);
            string fileNameJson = GetFileName(countryCode, exportType, "json");
            string filePathJson = GetFilePath(subdirectory, fileNameJson);
            string subdirectoryInBucket = subdirectory.Replace("\\", "/");

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            if (!Json.ExportListings(listings, filePathJson))
            {
                return false;
            }
            if (!Zip.Compress(filePathJson, filePathZip))
            {
                return false;
            }

            var metadata = GetMetadata(dateFrom, dateTo);
            if (!new S3().UploadToDownloadsBucket(filePathZip, fileNameZip, subdirectoryInBucket, metadata))
            {
                return false;
            }
            if (!UploadHistoricFile(countryCode, exportType, filePathZip, subdirectoryInBucket, dateFrom, dateTo))
            {
                return false;
            }

            Log.WriteInfo("filesupdater", fileNameZip);
            return true;
        }

        private static List<(string, string)> GetMetadata(DateOnly? dateFrom, DateOnly? dateTo)
        {
            List<(string, string)> metadata = [];
            if (dateFrom.HasValue)
            {
                //string dateFromValue = dateFrom.Value.ToString("dd-MM-yyyy");
                string dateFromValue = dateFrom.Value.ToString();
                metadata.Add((keyDateFrom, dateFromValue));
            }
            if (dateTo.HasValue)
            {
                //string dateToValue = dateTo.Value.ToString("dd-MM-yyyy");
                string dateToValue = dateTo.Value.ToString();
                metadata.Add((keyDateTo, dateToValue));
            }
            return metadata;
        }

        private static bool UploadHistoricFile(CountryCode countryCode, ExportType exportType, string filePathZip, string subdirectoryInBucket, DateOnly? dateFrom, DateOnly? dateTo)
        {
            string fileNameWithoutExtension = GetFileName(countryCode, exportType);            
            string newFileNameZip = GetFileNameWidhDate(Yesterday(), fileNameWithoutExtension, "zip");
            string subdirectory = GetLocalSubdirectory(countryCode, exportType);
            string newFilePathZip = GetFilePath(subdirectory, newFileNameZip);

            File.Copy(filePathZip, newFilePathZip, true);
            var metadata = GetMetadata(dateFrom, dateTo);
            bool sucess = new S3().UploadToDownloadsBucket(newFilePathZip, newFileNameZip, subdirectoryInBucket, metadata);
            File.Delete(newFilePathZip);
            return sucess;
        }

        private static DateOnly GetDateFrom()
        {
            var dateFrom = Yesterday();
            var objectKey = GetObjectKey(CountryCode.ES, ExportType.Updates, "zip");
            var metaDataValue = new S3().GetMetadataValue(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey, keyDateTo);
            if (metaDataValue != null)
            {
                if (DateOnly.TryParse(metaDataValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateOnly dateTo))
                {
                    if (dateTo.AddDays(1) < dateFrom)
                    {
                        dateFrom = dateTo.AddDays(1);
                    }
                }
            }
            return dateFrom;
        }

        private static DateOnly Yesterday()
        {
            return DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        }
    }
}
