using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Websites;

namespace landerist_library.Landerist_com
{
    public enum ExportType
    {
        Listings,
        Updates
    }

    public class Landerist_com
    {

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
            return countryCode.ToString() + "_" + exportType.ToString();
        }

        protected static string GetObjectKey(CountryCode countryCode, ExportType exportType, string fileExtension)
        {
            string fileName = GetFileName(countryCode, exportType, fileExtension);
            return countryCode.ToString() + "/" + exportType.ToString() + "/" + fileName;
        }

        protected static string GetFileNameWidhDate(DateTime dateTime, string prefix, string extension)
        {
            string datePart = dateTime.ToString("yyyyMMdd");
            return prefix + "_" + datePart + "." + extension;
        }

        public static void UpdateDownloadsAndStatisticsPages()
        {
            DownloadsPage.Update();
            StatisticsPage.Update();
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
                Log.WriteLogErrors("InvalidateCloudFront", exception);
            }
            return false;
        }
    }
}
