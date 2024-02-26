using System.Globalization;

namespace landerist_library.Export
{
    public class DownloadsPage
    {
        private static readonly string DownloadsTemplateFile = Configuration.Config.EXPORT_DIRECTORY + "downloads_template.html";
        private static readonly string DownloadsFile = Configuration.Config.EXPORT_DIRECTORY + "downloads.html";

        private static string DownloadsTemplate = string.Empty;


        public DownloadsPage()
        {
            DownloadsTemplate = File.ReadAllText(DownloadsTemplateFile);
        }
        public async void Update()
        {
            try
            {
                await UpdateDownloadsTemplate("LISTINGS", "ES", "ES/listings/es_listings.zip");
                await UpdateDownloadsTemplate("UPDATES", "ES", "ES/updated/es_listings_update.zip");
                File.WriteAllText(DownloadsFile, DownloadsTemplate);

            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("DownloadsPage Update", exception);
            }
        }

        private static async Task UpdateDownloadsTemplate(string key, string country, string objectKey)
        {
            var (lastModified, contentLength) = await new S3().GetFileInfo(Configuration.Config.AWS_S3_PUBLIC_BUCKET, objectKey);
            if (lastModified is null || contentLength is null)
            {
                return;
            }

            UpdateDownloadsTemplate("<!--" + key + "_COUNTRY-->", country);

            string lastMofifiedString = ((DateTime)lastModified).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            UpdateDownloadsTemplate("<!--" + key + "_MODIFIED-->", lastMofifiedString);

            var cultureInfo = CultureInfo.InvariantCulture;
            NumberFormatInfo numberFormatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
            numberFormatInfo.NumberGroupSeparator = ".";
            numberFormatInfo.NumberGroupSizes = [3];

            string sizeString = ((long)contentLength).ToString("#,0", numberFormatInfo) + " bytes";

            UpdateDownloadsTemplate("<!--" + key + "_SIZE-->", sizeString);

            var url = $"https://{Configuration.Config.AWS_S3_PUBLIC_BUCKET}.s3.amazonaws.com/{objectKey}";
            var hyperlink = "<a title=\"Download\" href=\"" + url + "\">Download</a>";
            UpdateDownloadsTemplate("<!--" + key + "_HYPERLINK-->", hyperlink);
            Console.WriteLine(key);
        }

        private static void UpdateDownloadsTemplate(string key, string text)
        {
            text = key + text;
            DownloadsTemplate = DownloadsTemplate.Replace(key, text);
        }
    }
}
