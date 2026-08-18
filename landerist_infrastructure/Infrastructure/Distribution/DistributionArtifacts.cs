using landerist_library.Application.Websites;
using landerist_library.Application.Distribution;
using landerist_library.Infrastructure.Distribution.Cloud;
using landerist_library.Infrastructure.Distribution.FileSystem;
using landerist_library.Application.Statistics;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Globalization;

namespace landerist_library.Infrastructure.Distribution
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

    public class DistributionArtifacts
    {
        protected readonly DistributionOptions? Options;

        protected DistributionArtifacts(DistributionOptions? options = null)
        {
            Options = options;
        }

        protected string GetFilePath(string subdirectory)
        {
            return Path.Combine(GetOptions().ExportDirectory, subdirectory);
        }
        protected string GetFilePath(string subdirectory, string fileName)
        {
            return Path.Combine(GetOptions().ExportDirectory, subdirectory, fileName);
        }

        private DistributionOptions GetOptions() => Options
            ?? throw new InvalidOperationException("Distribution options are required for filesystem operations.");

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
                ExportType.Listings => $"{country}-listings",
                ExportType.Updates => $"{country}-listings-updates",
                ExportType.Published => $"{country}-listings-published",
                ExportType.Unpublished => $"{country}-listings-unpublished",
                ExportType.PublishedUpdates => $"{country}-listings-published-updates",
                ExportType.UnpublishedUpdates => $"{country}-listings-unpublished-updates",
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
                "listings",
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
                "listings",
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

        public static void UpdateDownloadsPage(
            IDistributionWebsiteMetrics websiteMetrics,
            IWebsiteCatalog websites,
            DistributionOptions options)
        {
            new DownloadsPage(websiteMetrics, websites, options, CreateStorage(), CreateFileSystem()).Update();
            InvalidateCloudFront(options);
        }

        public static void UpdateStatisticsPage(
            GlobalStatistics globalStatistics,
            IPageStatisticsRepository pageStatistics,
            DistributionOptions options)
        {
            new StatisticsPage(globalStatistics, pageStatistics, options, CreateStorage(), CreateFileSystem()).UpdateCharts();
            InvalidateCloudFront(options);
        }

        public static void UpdateHostStatisticsPage(
            HostStatistics hostStatistics,
            IDistributionWebsiteMetrics websiteMetrics,
            IWebsiteCatalog websites,
            DistributionOptions options)
        {
            new HostStatisticsPage(hostStatistics, websiteMetrics, websites, options, CreateStorage(), CreateFileSystem()).Update();
            InvalidateCloudFront(options);
        }

        public static void UpdateHostsStatisticsPage(
            IDistributionWebsiteMetrics websiteMetrics,
            IWebsiteCatalog websites,
            DistributionOptions options)
        {
            new HostsStatisticsPage(websiteMetrics, websites, options, CreateStorage(), CreateFileSystem()).Update();
            InvalidateCloudFront(options);
        }

        public static void UpdateAllPages(
            GlobalStatistics globalStatistics,
            HostStatistics hostStatistics,
            IPageStatisticsRepository pageStatistics,
            IDistributionWebsiteMetrics websiteMetrics,
            IWebsiteCatalog websites,
            DistributionOptions options)
        {
            IWebsiteArtifactStorage storage = CreateStorage();
            IDistributionFileSystem files = CreateFileSystem();
            new DownloadsPage(websiteMetrics, websites, options, storage, files).Update();
            new StatisticsPage(globalStatistics, pageStatistics, options, storage, files).UpdateCharts();
            new HostStatisticsPage(hostStatistics, websiteMetrics, websites, options, storage, files).Update();
            new HostsStatisticsPage(websiteMetrics, websites, options, storage, files).Update();
            InvalidateCloudFront(options);
        }

        public static bool InvalidateCloudFront(DistributionOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            return new CloudFrontCdnInvalidator(
                options.AwsAccessKeyId,
                options.AwsSecretAccessKey,
                options.CloudFrontDistributionId).InvalidateAll();
        }

        private static IWebsiteArtifactStorage CreateStorage() =>
            new S3WebsiteArtifactStorage();

        private static IDistributionFileSystem CreateFileSystem() =>
            new SystemDistributionFileSystem();
    }
}
