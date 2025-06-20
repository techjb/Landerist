using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Websites;

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
            string objectKey = GetObjectKey(countryCode, exportType, "zip");
            var (lastModified, contentLength) = new S3().GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);
            if (lastModified is null || contentLength is null)
            {
                return;
            }

            var counter = new S3().GetMetadataValue(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey, FilesUpdater.METADATA_KEY_COUNTER);

            string commentCounter = Comment(countryCode, exportType, "Counter");            
            Replace(commentCounter, counter);

            string commentModified = Comment(countryCode, exportType, "Modified");            
            string lastMofifiedString = ((DateTime)lastModified).ToShortDateString();
            Replace(commentModified, lastMofifiedString);


            string commentSize = Comment(countryCode, exportType, "Size");            
            string sizeString = FormatBytes((long)contentLength);
            Replace(commentSize, sizeString);

            string comentHyperlink = Comment(countryCode, exportType, "Hyperlink");
            var url = $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
            string fileName = GetFileName(countryCode, exportType, "zip");
            var hyperlink = "<a title=\"" + fileName + "\" href=\"" + url + "\">" + fileName + "</a>";
            Replace(comentHyperlink, hyperlink);
        }

        private static string Comment(CountryCode countryCode, ExportType exportType, string key)
        {
            return "<!--" + countryCode + "_" + exportType + "_" + key + "-->";
        }

        private static void Replace(string comment, string? text)
        {
            if (comment != null)
            {
                DownloadsTemplate = DownloadsTemplate.Replace(comment, text);
            }            
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
                return "0 B";

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
