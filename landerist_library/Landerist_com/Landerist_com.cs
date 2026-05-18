using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Globalization;

namespace landerist_library.Landerist_com
{
    public enum ExportType
    {
        Listings,
        Updates,
        Published,
        Unpublished,
        PublishedUpdates,
        UnpublishedUpdates,
        Websites
    }

    public class Landerist_com
    {

        protected static string GetFilePath(string subdirectory)
        {
            return Config.EXPORT_DIRECTORY + subdirectory;
        }
        protected static string GetFilePath(string subdirectory, string fileName)
        {
            return Config.EXPORT_DIRECTORY + subdirectory + "\\" + fileName;
        }

        protected static string GetLocalSubdirectory(CountryCode countryCode, ExportType exportType)
        {
            return countryCode.ToString() + "\\" + exportType.ToString();
        }

        protected static string GetFileName(CountryCode countryCode, ExportType exportType, string fileExtension)
        {
            return GetFileName(countryCode, exportType) + "." + fileExtension;
        }

        protected static string GetFileName(CountryCode countryCode, ExportType exportType)
        {
            string country = GetCountryCodeFileNamePart(countryCode);
            return exportType switch
            {
                ExportType.Published => $"{country}-published",
                ExportType.Unpublished => $"{country}-unpublished",
                ExportType.PublishedUpdates => $"{country}-published-updates",
                ExportType.UnpublishedUpdates => $"{country}-unpublished-updates",
                ExportType.Websites => $"{country}-websites",
                _ => countryCode.ToString() + "_" + exportType.ToString()
            };
        }

        protected static string GetObjectKey(CountryCode countryCode, ExportType exportType, string fileExtension)
        {
            string fileName = GetFileName(countryCode, exportType, fileExtension);
            return countryCode.ToString() + "/" + exportType.ToString() + "/" + fileName;
        }

        protected static string GetLegacyObjectKey(CountryCode countryCode, ExportType exportType, string fileExtension)
        {
            string fileName = GetLegacyFileName(countryCode, exportType, fileExtension);
            return countryCode.ToString() + "/" + exportType.ToString() + "/" + fileName;
        }

        protected static string GetLegacyFileName(CountryCode countryCode, ExportType exportType, string fileExtension)
        {
            return countryCode.ToString() + "_" + exportType.ToString() + "." + fileExtension;
        }

        protected static string GetFileNameWidhDate(DateOnly dateOnly, string prefix, string extension)
        {
            return GetLegacyFileNameWithDate(dateOnly, prefix, extension);
        }

        protected static string GetFileNameWithDate(DateOnly dateOnly, string prefix, string extension)
        {
            string datePart = dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return $"{prefix}-{datePart}.{extension}";
        }

        protected static string GetFileNameWithDateRange(DateOnly dateFrom, DateOnly dateTo, string prefix, string extension)
        {
            string dateFromPart = dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string dateToPart = dateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return $"{prefix}-{dateFromPart}-to-{dateToPart}.{extension}";
        }

        protected static string GetLegacyFileNameWithDate(DateOnly dateOnly, string prefix, string extension)
        {
            string datePart = dateOnly.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return prefix + "_" + datePart + "." + extension;
        }

        protected static string GetListingsByOperationPropertyTypeFileName(
            CountryCode countryCode,
            Operation operation,
            PropertyType propertyType,
            ListingStatus listingStatus,
            string extension)
        {
            return string.Join(
                "-",
                GetCountryCodeFileNamePart(countryCode),
                GetListingStatusFileNamePart(listingStatus),
                operation.ToString().ToLowerInvariant(),
                propertyType.ToString().ToLowerInvariant()) + "." + extension;
        }

        protected static string GetHostListingsFileName(
            CountryCode countryCode,
            string host,
            ListingStatus listingStatus,
            string extension)
        {
            return string.Join(
                "-",
                GetCountryCodeFileNamePart(countryCode),
                GetListingStatusFileNamePart(listingStatus),
                host.ToLowerInvariant()) + "." + extension;
        }

        private static string GetCountryCodeFileNamePart(CountryCode countryCode)
        {
            return countryCode.ToString().ToLowerInvariant();
        }

        private static string GetListingStatusFileNamePart(ListingStatus listingStatus)
        {
            return listingStatus.ToString().ToLowerInvariant();
        }

        public static void UpdateDownloadsPage()
        {
            DownloadsPage.Update();
            InvalidateCloudFront();
        }

        public static void UpdateStatisticsPage()
        {
            StatisticsPage.UpdateCharts();
            InvalidateCloudFront();
        }

        public static void UpdateHostStatisticsPage()
        {
            HostStatisticsPage.Update();
            InvalidateCloudFront();
        }

        public static void UpdateAllPages()
        {
            DownloadsPage.Update();
            StatisticsPage.UpdateCharts();
            HostStatisticsPage.Update();
            InvalidateCloudFront();
        }

        public static bool InvalidateCloudFront()
        {
            var client = new AmazonCloudFrontClient(PrivateConfig.AWS_ACESSKEYID, PrivateConfig.AWS_SECRETACCESSKEY, RegionEndpoint.EUWest3);
            var invalidationBatch = new InvalidationBatch
            {
                CallerReference = DateTime.UtcNow.Ticks.ToString(),
                Paths = new Paths
                {
                    Quantity = 1,
                    Items = ["/*"]
                }
            };

            var request = new CreateInvalidationRequest
            {
                DistributionId = PrivateConfig.AWS_CLOUDFRONT_DISTRIBUTION_ID_WEBSITE,
                InvalidationBatch = invalidationBatch
            };

            try
            {
                var response = client.CreateInvalidationAsync(request).Result;
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("InvalidateCloudFront", exception);
            }
            return false;
        }
    }
}
