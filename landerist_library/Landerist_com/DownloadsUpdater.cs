using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;
using System.Globalization;

namespace landerist_library.Landerist_com
{
    public class DownloadsUpdater : Landerist_com
    {
        public const string METADATA_KEY_DATEFROM = "dateFrom";
        public const string METADATA_KEY_DATETO = "dateTo";
        public const string METADATA_KEY_COUNTER = "counter";
        private const string METADATA_DATE_FORMAT = "yyyy-MM-dd";
        private const string HOSTS_SUBDIRECTORY = "ES\\Hosts";
        private const string LISTINGS_BY_OPERATION_PROPERTY_TYPE_SUBDIRECTORY = "ES\\OperationPropertyTypes";

        public static void Update()
        {
            try
            {
                UpdateWebsites();
                UpdateFullDataSet(ListingStatus.published);
                UpdateFullDataSet(ListingStatus.unpublished);
                UpdateListingsUpdates();                
                UpdateListingsByOperationPropertyType();
                UpdateListingsByWebsite();
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

        public static void UpdateListingsUpdates()
        {
            Console.WriteLine("Reading updates ..");
            DateOnly dateFrom = GetDateFrom(ExportType.PublishedUpdates, ExportType.UnpublishedUpdates);
            DateOnly dateTo = Yesterday();

            var publishedListings = ES_Listings.GetListings(ListingStatus.published, true, true, dateFrom, dateTo);
            if (!Update(publishedListings, CountryCode.ES, ExportType.PublishedUpdates, dateFrom, dateTo))
            {
                Log.WriteError("filesupdater", "Error updating PublishedUpdates");
            }

            var unpublishedListings = ES_Listings.GetListings(ListingStatus.unpublished, true, true, dateFrom, dateTo);
            if (!Update(unpublishedListings, CountryCode.ES, ExportType.UnpublishedUpdates, dateFrom, dateTo))
            {
                Log.WriteError("filesupdater", "Error updating UnpublishedUpdates");
            }
        }

        public static void UpdateFullDataSet(ListingStatus listingStatus)
        {
            Console.WriteLine("Reading " + listingStatus + " ..");

            var exportType = listingStatus == ListingStatus.published
                ? ExportType.Published
                : ExportType.Unpublished;

            var listings = ES_Listings.GetListings(listingStatus);
            if (!Update(listings, CountryCode.ES, exportType, null, null))
            {
                Log.WriteError("filesupdater", "Error updating " + exportType);
            }
        }

        public static bool UpdateWebsites()
        {
            Console.WriteLine("Reading Websites ..");
            var websites = Websites.Websites.GetDataTableAll();
            if (websites.Rows.Count.Equals(0))
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

            try
            {
                Tools.Csv.Write(websites, filePath, true);
            }
            catch (Exception exception)
            {
                Log.WriteError("filesupdater", "Error writing Websites CSV file", exception);
                return false;
            }

            return UploadWebsitesFile(filePath, CountryCode.ES, websites.Rows.Count);
        }

        public static bool UpdateListingsByWebsite()
        {
            Console.WriteLine("Reading hosts ..");
            var websites = Websites.Websites.GetAll()
                .OrderBy(website => website.Host, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (websites.Count == 0)
            {
                return true;
            }

            string directory = GetFilePath(HOSTS_SUBDIRECTORY);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (var website in websites)
            {
                if (!UpdateHostListings(website, ListingStatus.published) ||
                    !UpdateHostListings(website, ListingStatus.unpublished))
                {
                    Log.WriteError("filesupdater", "Error updating host files: " + website.Host);
                    return false;
                }
            }

            return true;
        }

        public static bool UpdateListingsByOperationPropertyType()
        {
            Console.WriteLine("Reading listings by operation and property type ..");

            string directory = GetFilePath(LISTINGS_BY_OPERATION_PROPERTY_TYPE_SUBDIRECTORY);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (Operation operation in Enum.GetValues<Operation>())
            {
                foreach (PropertyType propertyType in Enum.GetValues<PropertyType>())
                {
                    if (!UpdateListingsByOperationPropertyType(operation, propertyType, ListingStatus.published) ||
                        !UpdateListingsByOperationPropertyType(operation, propertyType, ListingStatus.unpublished))
                    {
                        Log.WriteError("filesupdater", $"Error updating operation/property type files: {operation} {propertyType}");
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool UpdateListingsByOperationPropertyType(
            Operation operation,
            PropertyType propertyType,
            ListingStatus listingStatus)
        {
            var listings = ES_Listings.GetListings(
                listingStatus,
                operation,
                propertyType,
                true,
                true);

            if (listings.Count == 0)
            {
                return true;
            }

            string fileName = GetListingsByOperationPropertyTypeFileName(CountryCode.ES, operation, propertyType, listingStatus, "json");
            string filePath = GetFilePath(LISTINGS_BY_OPERATION_PROPERTY_TYPE_SUBDIRECTORY, fileName);

            if (!Json.ExportListings(listings, filePath))
            {
                return false;
            }

            return UploadListingsByOperationPropertyTypeFile(filePath, operation, propertyType, listingStatus, listings.Count, "json");
        }

        private static bool UpdateHostListings(Website website, ListingStatus listingStatus)
        {
            var listings = ES_Listings.GetListings(website.Host, listingStatus);
            if (listings.Count == 0)
            {
                return true;
            }

            string fileName = GetHostListingsFileName(CountryCode.ES, website.Host, listingStatus, "json");
            string filePath = GetFilePath(HOSTS_SUBDIRECTORY, fileName);

            if (!Json.ExportListings(listings, filePath))
            {
                return false;
            }

            return UploadHostFile(filePath, website.Host, listingStatus, listings.Count, "json");
        }

        private static bool UploadListingsByOperationPropertyTypeFile(
            string filePath,
            Operation operation,
            PropertyType propertyType,
            ListingStatus listingStatus,
            int counter,
            string extension)
        {
            string fileName = GetListingsByOperationPropertyTypeFileName(CountryCode.ES, operation, propertyType, listingStatus, extension);
            string subdirectoryInBucket = LISTINGS_BY_OPERATION_PROPERTY_TYPE_SUBDIRECTORY.Replace("\\", "/");
            string contentDisposition = $"attachment; filename=\"{fileName}\"";

            var metadata = GetMetadata(counter, null, null);
            if (!new S3().UploadToDownloadsBucket(filePath, fileName, subdirectoryInBucket, metadata, contentDisposition))
            {
                return false;
            }

            Log.WriteInfo("filesupdater", fileName);
            return true;
        }

        private static bool UploadHostFile(string filePath, string host, ListingStatus listingStatus, int counter, string extension)
        {
            string fileName = GetHostListingsFileName(CountryCode.ES, host, listingStatus, extension);
            string subdirectoryInBucket = HOSTS_SUBDIRECTORY.Replace("\\", "/");
            string contentDisposition = $"attachment; filename=\"{fileName}\"";

            var metadata = GetMetadata(counter, null, null);
            if (!new S3().UploadToDownloadsBucket(filePath, fileName, subdirectoryInBucket, metadata, contentDisposition))
            {
                return false;
            }

            Log.WriteInfo("filesupdater", fileName);
            return true;
        }

        private static bool Update(SortedSet<Listing> listings, CountryCode countryCode, ExportType exportType, DateOnly? dateFrom, DateOnly? dateTo)
        {
            Console.WriteLine("Updating " + exportType + "..");
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

            return UploadListingsFile(filePathJson, countryCode, exportType, listings.Count, dateFrom, dateTo);
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

            if (!UploadHistoricFile(countryCode, exportType, filePathZip, subdirectoryInBucket, counter, dateFrom, dateTo, "zip"))
            {
                return false;
            }

            Log.WriteInfo("filesupdater", fileNameZip);
            return true;
        }

        private static bool UploadListingsFile(string filePath, CountryCode countryCode, ExportType exportType, int counter, DateOnly? dateFrom, DateOnly? dateTo)
        {
            string subdirectory = GetLocalSubdirectory(countryCode, exportType);
            string fileNameJson = GetFileName(countryCode, exportType, "json");
            string subdirectoryInBucket = subdirectory.Replace("\\", "/");
            string contentDisposition = $"attachment; filename=\"{fileNameJson}\"";

            var metadata = GetMetadata(counter, dateFrom, dateTo);
            if (!new S3().UploadToDownloadsBucket(filePath, fileNameJson, subdirectoryInBucket, metadata, contentDisposition))
            {
                return false;
            }

            if (!UploadHistoricFile(countryCode, exportType, filePath, subdirectoryInBucket, counter, dateFrom, dateTo, "json"))
            {
                return false;
            }

            Log.WriteInfo("filesupdater", fileNameJson);
            return true;
        }

        private static bool UploadWebsitesFile(string filePath, CountryCode countryCode, int counter)
        {
            ExportType exportType = ExportType.Websites;
            string subdirectory = GetLocalSubdirectory(countryCode, exportType);
            string fileNameCsv = GetFileName(countryCode, exportType, "csv");
            string subdirectoryInBucket = subdirectory.Replace("\\", "/");
            string contentDisposition = $"attachment; filename=\"{fileNameCsv}\"";

            var metadata = GetMetadata(counter, null, null);
            if (!new S3().UploadToDownloadsBucket(filePath, fileNameCsv, subdirectoryInBucket, metadata, contentDisposition))
            {
                return false;
            }

            if (!UploadHistoricFile(countryCode, exportType, filePath, subdirectoryInBucket, counter, null, null, "csv"))
            {
                return false;
            }

            Log.WriteInfo("filesupdater", fileNameCsv);
            return true;
        }

        private static List<(string, string)> GetMetadata(int counter, DateOnly? dateFrom, DateOnly? dateTo)
        {
            List<(string, string)> metadata = [];
            metadata.Add((METADATA_KEY_COUNTER, counter.ToString(CultureInfo.InvariantCulture)));

            if (dateFrom.HasValue)
            {
                metadata.Add((METADATA_KEY_DATEFROM, dateFrom.Value.ToString(METADATA_DATE_FORMAT, CultureInfo.InvariantCulture)));
            }

            if (dateTo.HasValue)
            {
                metadata.Add((METADATA_KEY_DATETO, dateTo.Value.ToString(METADATA_DATE_FORMAT, CultureInfo.InvariantCulture)));
            }

            return metadata;
        }

        private static bool UploadHistoricFile(
            CountryCode countryCode,
            ExportType exportType,
            string filePath,
            string subdirectoryInBucket,
            int counter,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            string extension)
        {
            string newFileName = GetHistoricFileName(countryCode, exportType, dateFrom, dateTo, extension);
            string subdirectory = GetLocalSubdirectory(countryCode, exportType);
            string newFilePath = GetFilePath(subdirectory, newFileName);

            try
            {
                File.Copy(filePath, newFilePath, true);
                var metadata = GetMetadata(counter, dateFrom, dateTo);
                string contentDisposition = $"attachment; filename=\"{newFileName}\"";
                return new S3().UploadToDownloadsBucket(newFilePath, newFileName, subdirectoryInBucket, metadata, contentDisposition);
            }
            finally
            {
                if (File.Exists(newFilePath))
                {
                    File.Delete(newFilePath);
                }
            }
        }

        private static string GetHistoricFileName(
            CountryCode countryCode,
            ExportType exportType,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            string extension)
        {
            string fileNameWithoutExtension = GetFileName(countryCode, exportType);
            if (IsUpdatesExportType(exportType) && dateFrom.HasValue && dateTo.HasValue)
            {
                return GetFileNameWithDateRange(dateFrom.Value, dateTo.Value, fileNameWithoutExtension, extension);
            }

            if (UsesModernHistoricFileName(exportType))
            {
                return GetFileNameWithDate(Yesterday(), fileNameWithoutExtension, extension);
            }

            return GetLegacyFileNameWithDate(Yesterday(), fileNameWithoutExtension, extension);
        }

        private static bool IsUpdatesExportType(ExportType exportType)
        {
            return exportType is ExportType.PublishedUpdates or ExportType.UnpublishedUpdates;
        }

        private static bool UsesModernHistoricFileName(ExportType exportType)
        {
            return exportType is ExportType.Published
                or ExportType.Unpublished
                or ExportType.PublishedUpdates
                or ExportType.UnpublishedUpdates
                or ExportType.Websites;
        }

        private static DateOnly GetDateFrom(params ExportType[] exportTypes)
        {
            var dateFrom = Yesterday();
            var s3 = new S3();

            foreach (var exportType in exportTypes)
            {
                foreach (string objectKey in GetObjectKeysForMetadata(exportType))
                {
                    var metaDataValue = s3.GetMetadataValue(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey, METADATA_KEY_DATETO);
                    if (metaDataValue != null &&
                        DateOnly.TryParseExact(metaDataValue, METADATA_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateTo))
                    {
                        if (dateTo.AddDays(1) < dateFrom)
                        {
                            dateFrom = dateTo.AddDays(1);
                        }

                        break;
                    }
                }
            }

            return dateFrom;
        }

        private static List<string> GetObjectKeysForMetadata(ExportType exportType)
        {
            List<string> objectKeys =
            [
                GetObjectKey(CountryCode.ES, exportType, "json"),
                GetObjectKey(CountryCode.ES, exportType, "zip"),
                GetLegacyObjectKey(CountryCode.ES, exportType, "json"),
                GetLegacyObjectKey(CountryCode.ES, exportType, "zip")
            ];

            return objectKeys.Distinct(StringComparer.Ordinal).ToList();
        }

        private static DateOnly Yesterday()
        {
            return DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        }
    }
}
