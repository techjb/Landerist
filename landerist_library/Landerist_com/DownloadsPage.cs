using landerist_library.Websites;
using System.Globalization;
using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Export;

namespace landerist_library.Landerist_com
{

    public class DownloadsPage : Landerist_com
    {
        private static readonly string DownloadsTemplateHtmlFile =
            Config.LANDERIST_COM_TEMPLATES + "downloads_template.html";

        private static readonly string DownloadsHtmlFile =
            Config.LANDERIST_COM_OUTPUT + "downloads.html";

        private static string DownloadsTemplate = string.Empty;


        public static void Update()
        {
            try
            {
                DownloadsTemplate = File.ReadAllText(DownloadsTemplateHtmlFile);
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
            var (lastModified, contentLength) = new S3().GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);
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
            var url = $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
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
            return new S3().UploadToWebsiteBucket(DownloadsHtmlFile, "index.html", "downloads");
        }
    }
}
