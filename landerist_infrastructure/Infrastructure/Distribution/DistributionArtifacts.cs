using landerist_library.Application.Websites;
using landerist_library.Application.Distribution;
using landerist_library.Infrastructure.Distribution.Cloud;
using landerist_library.Infrastructure.Distribution.FileSystem;
using landerist_library.Application.Statistics;
using landerist_library.Websites;

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

    public static class DistributionArtifacts
    {
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
