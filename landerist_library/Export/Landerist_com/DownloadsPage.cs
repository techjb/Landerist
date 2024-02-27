using landerist_library.Websites;
using System.Globalization;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using landerist_library.Configuration;
using Amazon;
using landerist_library.Logs;


namespace landerist_library.Export.Landerist_com
{

    public class DownloadsPage : Landerist_com
    {
        private static readonly string DownloadsTemplateHtmlFile =
            Config.LANDERIST_COM_DIRECTORY + "downloads_template.html";

        private static readonly string DownloadsHtmlFile =
            Config.LANDERIST_COM_DIRECTORY + "downloads.html";

        private static string DownloadsTemplate = string.Empty;


        public DownloadsPage()
        {
            DownloadsTemplate = File.ReadAllText(DownloadsTemplateHtmlFile);
        }
        public void Update()
        {
            try
            {
                UpdateDownloadsTemplate(CountryCode.ES, ExportType.Listings);
                UpdateDownloadsTemplate(CountryCode.ES, ExportType.Updates);
                if (UploadDownloadsFile())
                {
                    Log.WriteLogInfo("DownloadsPage", "Updated");
                }

            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("DownloadsPage Update", exception);
            }
        }

        private static void UpdateDownloadsTemplate(CountryCode countryCode, ExportType exportType)
        {
            string objectKey = GetObjectKey(countryCode, exportType, "zip");
            var (lastModified, contentLength) = new S3().GetFileInfo(Config.AWS_S3_DOWNLOADS_BUCKET, objectKey);
            if (lastModified is null || contentLength is null)
            {
                return;
            }

            string commentCountry = GetTemplateComment(countryCode, exportType, "Country");
            Replace(commentCountry, countryCode.ToString());

            string commentModified = GetTemplateComment(countryCode, exportType, "Modified");
            string lastMofifiedString = ((DateTime)lastModified).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            Replace(commentModified, lastMofifiedString);


            string commentSize = GetTemplateComment(countryCode, exportType, "Size");
            var cultureInfo = CultureInfo.InvariantCulture;
            NumberFormatInfo numberFormatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
            numberFormatInfo.NumberGroupSeparator = ".";
            numberFormatInfo.NumberGroupSizes = [3];
            string sizeString = ((long)contentLength).ToString("#,0", numberFormatInfo) + " bytes";
            Replace(commentSize, sizeString);

            string comentHyperlink = GetTemplateComment(countryCode, exportType, "Hyperlink");
            var url = $"https://{Config.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
            string fileName = GetFileName(countryCode, exportType, "zip");
            var hyperlink = "<a title=\"" + fileName + "\" href=\"" + url + "\">Download</a>";
            Replace(comentHyperlink, hyperlink);
        }

        private static string GetTemplateComment(CountryCode countryCode, ExportType exportType, string key)
        {
            return "<!--" + countryCode + "_" + exportType + "_" + key + "-->";
        }

        private static void Replace(string comment, string text)
        {
            DownloadsTemplate = DownloadsTemplate.Replace(comment, text);
        }

        private static bool UploadDownloadsFile()
        {
            File.WriteAllText(DownloadsHtmlFile, DownloadsTemplate);
            if (!new S3().UploadToWebsiteBucket(DownloadsHtmlFile, "index.html", "downloads"))
            {
                return false;
            }
            return InvalidateCloudFront();
        }


        public static bool InvalidateCloudFront()
        {
            var client = new AmazonCloudFrontClient(Config.AWS_ACESSKEYID, Config.AWS_SECRETACCESSKEY, RegionEndpoint.EUWest3);
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
                DistributionId = Config.AWS_CLOUDFRONT_DISTRIBUTION_ID_WEBSITE,
                InvalidationBatch = invalidationBatch
            };

            try
            {
                var response = client.CreateInvalidationAsync(request).Result;
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("DownloadsPage InvalidateCloudFront", exception);
            }
            return false;
        }
    }
}
