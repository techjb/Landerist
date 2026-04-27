using System.Globalization;
using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Websites;

namespace landerist_library.Landerist_com
{
    public class DownloadsPage : Landerist_com
    {
        private static readonly string DownloadsTemplateHtmlFile =
            Path.Combine(Config.LANDERIST_COM_TEMPLATES!, "downloads_template.html");

        private static readonly string DownloadsHtmlFile =
            Path.Combine(Config.LANDERIST_COM_OUTPUT!, "downloads.html");

        private static string DownloadsTemplate = string.Empty;

        public static void Update()
        {
            try
            {
                DownloadsTemplate = File.ReadAllText(DownloadsTemplateHtmlFile);

                foreach (ExportType exportType in Enum.GetValues(typeof(ExportType)))
                {
                    UpdateDownloadsTemplate(CountryCode.ES, exportType);
                }

                if (UploadDownloadsFile())
                {
                    Log.WriteInfo("DownloadsPage", "Updated downloads page");
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("DownloadsPage Update", exception);
            }
        }

        private static void UpdateDownloadsTemplate(CountryCode countryCode, ExportType exportType)
        {
            var s3 = new S3();
            string objectKey = GetObjectKey(countryCode, exportType, "zip");

            var (lastModified, contentLength) = s3.GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);
            if (lastModified is null || contentLength is null)
            {
                return;
            }

            var counter = s3.GetMetadataValue(
                PrivateConfig.AWS_S3_DOWNLOADS_BUCKET,
                objectKey,
                DownloadsUpdater.METADATA_KEY_COUNTER);

            Replace(Comment(countryCode, exportType, "Counter"), counter ?? "-");

            string lastModifiedString = ((DateTime)lastModified).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            Replace(Comment(countryCode, exportType, "Modified"), lastModifiedString);

            string sizeString = FormatBytes((long)contentLength);
            Replace(Comment(countryCode, exportType, "Size"), sizeString);

            var url = $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
            string fileName = GetFileName(countryCode, exportType, "zip");
            string hyperlink = $"<a title=\"{fileName}\" href=\"{url}\">zip</a>";
            Replace(Comment(countryCode, exportType, "Hyperlink"), hyperlink);
        }

        private static string Comment(CountryCode countryCode, ExportType exportType, string key)
        {
            return $"<!--{countryCode}_{exportType}_{key}-->";
        }

        private static void Replace(string comment, string? text)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return;
            }

            DownloadsTemplate = DownloadsTemplate.Replace(comment, text ?? string.Empty);
        }

        private static bool UploadDownloadsFile()
        {
            File.WriteAllText(DownloadsHtmlFile, DownloadsTemplate);
            return new S3().UploadToWebsiteBucket(DownloadsHtmlFile, "index.html", "downloads");
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

            if (bytes == 0)
            {
                return "0 B";
            }

            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }
}
