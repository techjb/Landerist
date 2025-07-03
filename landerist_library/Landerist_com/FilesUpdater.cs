using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Globalization;

namespace landerist_library.Landerist_com
{
    public class FilesUpdater : Landerist_com
    {

        public const string METADATA_KEY_DATEFROM = "dateFrom";
        public const string METADATA_KEY_DATETO = "dateTo";
        public const string METADATA_KEY_COUNTER = "counter";

        public static void Update()
        {
            try
            {
                UpdateListings();
                UpdateUpdates();
                UpdatePublished();
                UpdateUnpublished();
                UpdateWebsites();
            }
            catch (Exception exception)
            {
                Log.WriteError("Update", exception);
            }
        }

        public static void UpdateListings()
        {
            Console.WriteLine("Reading all listings ..");
            var listings = ES_Listings.GetAll(true, true);
            if (!Update(listings, CountryCode.ES, ExportType.Listings, null, null))
            {
                Log.WriteError("filesupdater", "Error updating all listings");
            }
        }

        public static void UpdateUpdates()
        {
            Console.WriteLine("Reading Updates ..");
            DateOnly dateFrom = GetDateFrom();
            DateOnly dateTo = Yesterday();
            var listings = ES_Listings.GetListings(true, true, dateFrom, dateTo);
            if (!Update(listings, CountryCode.ES, ExportType.Updates, dateFrom, dateTo))
            {
                Log.WriteError("filesupdater", "Error updating Updates");
            }
        }

        public static void UpdatePublished()
        {
            Console.WriteLine("Reading published ..");
            var listings = ES_Listings.GetPublished();
            if (!Update(listings, CountryCode.ES, ExportType.Published, null, null))
            {
                Log.WriteError("filesupdater", "Error updating Published");
            }
        }
        public static void UpdateUnpublished()
        {
            Console.WriteLine("Reading unpublished ..");
            var listings = ES_Listings.GetUnPublished();
            if (!Update(listings, CountryCode.ES, ExportType.Unpublished, null, null))
            {
                Log.WriteError("filesupdater", "Error updating Unpublished");
            }
        }

        public static bool UpdateWebsites()
        {
            Console.WriteLine("Reading Websites ..");
            var websites = Websites.Websites.GetHosts();
            if (websites.Count.Equals(0))
            {
                return true;
            }
            string fileName = GetFileName(CountryCode.ES, ExportType.Websites, "csv");
            string subdirectory = GetLocalSubdirectory(CountryCode.ES, ExportType.Websites);
            string filePath = GetFilePath(subdirectory, fileName);
            if (!Directory.Exists(GetFilePath(subdirectory)))
            {
                Directory.CreateDirectory(GetFilePath(subdirectory));
            }
            if (!Tools.Csv.Write(websites, filePath))
            {
                Log.WriteError("filesupdater", "Error writing Websites CSV file");
                return false;
            }

            return Update(filePath, CountryCode.ES, ExportType.Websites, websites.Count);
        }

        private static bool Update(SortedSet<Listing> listings, CountryCode countryCode, ExportType exportType, DateOnly? dateFrom, DateOnly? dateTo)
        {
            Console.WriteLine("Updating " + exportType.ToString() + "..");
            if (listings.Count.Equals(0))
            {
                return true;
            }

            string subdirectory = GetLocalSubdirectory(countryCode, exportType);
            string filePath = GetFilePath(subdirectory);

            string fileNameJson = GetFileName(countryCode, exportType, "json");
            string filePathJson = GetFilePath(subdirectory, fileNameJson);

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            if (!Json.ExportListings(listings, filePathJson))
            {
                return false;
            }
            return Update(filePathJson, countryCode, exportType, listings.Count, dateFrom, dateTo);
        }

        private static bool Update(string filePath, CountryCode countryCode, ExportType exportType, int counter)
        {
            return Update(filePath, countryCode, exportType, counter, null, null);
        }
        private static bool Update(string filePath, CountryCode countryCode, ExportType exportType, int counter, DateOnly? dateFrom, DateOnly? dateTo)
        {
            string subdirectory = GetLocalSubdirectory(countryCode, exportType);
            string fileNameZip = GetFileName(countryCode, exportType, "zip");
            string filePathZip = GetFilePath(subdirectory, fileNameZip);
            string subdirectoryInBucket = subdirectory.Replace("\\", "/");
            if (!Zip.Compress(filePath, filePathZip))
            {
                return false;
            }
            var metadata = GetMetadata(counter, dateFrom, dateTo);
            if (!new S3().UploadToDownloadsBucket(filePathZip, fileNameZip, subdirectoryInBucket, metadata))
            {
                return false;
            }
            if (!UploadHistoricFile(countryCode, exportType, filePathZip, subdirectoryInBucket, counter, dateFrom, dateTo))
            {
                return false;
            }

            Log.WriteInfo("filesupdater", fileNameZip);
            return true;
        }

        private static List<(string, string)> GetMetadata(int counter, DateOnly? dateFrom, DateOnly? dateTo)
        {
            List<(string, string)> metadata = [];
            metadata.Add((METADATA_KEY_COUNTER, counter.ToString()));
            if (dateFrom.HasValue)
            {
                metadata.Add((METADATA_KEY_DATEFROM, dateFrom.Value.ToString()));
            }
            if (dateTo.HasValue)
            {
                metadata.Add((METADATA_KEY_DATETO, dateTo.Value.ToString()));
            }
            return metadata;
        }

        private static bool UploadHistoricFile(CountryCode countryCode, ExportType exportType, string filePathZip, string subdirectoryInBucket,
            int counter, DateOnly? dateFrom, DateOnly? dateTo)
        {
            string fileNameWithoutExtension = GetFileName(countryCode, exportType);
            string newFileNameZip = GetFileNameWidhDate(Yesterday(), fileNameWithoutExtension, "zip");
            string subdirectory = GetLocalSubdirectory(countryCode, exportType);
            string newFilePathZip = GetFilePath(subdirectory, newFileNameZip);

            File.Copy(filePathZip, newFilePathZip, true);
            var metadata = GetMetadata(counter, dateFrom, dateTo);
            bool sucess = new S3().UploadToDownloadsBucket(newFilePathZip, newFileNameZip, subdirectoryInBucket, metadata);
            File.Delete(newFilePathZip);
            return sucess;
        }

        private static DateOnly GetDateFrom()
        {
            var dateFrom = Yesterday();
            var objectKey = GetObjectKey(CountryCode.ES, ExportType.Updates, "zip");
            var metaDataValue = new S3().GetMetadataValue(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey, METADATA_KEY_DATETO);
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
