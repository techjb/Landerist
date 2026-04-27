using System.Globalization;
using System.Net;
using System.Text;
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
                UpdateUpdatedAt();

                foreach (ExportType exportType in Enum.GetValues(typeof(ExportType)))
                {
                    UpdateDownloadsTemplate(CountryCode.ES, exportType);
                }

                UpdateHostsTemplate();

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

        private static void UpdateUpdatedAt()
        {
            string updatedAtText = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            Replace("/*UPDATED_AT*/", updatedAtText);
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

            string sizeString = FormatBytes((long)contentLength);
            Replace(Comment(countryCode, exportType, "Size"), sizeString);

            var url = $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
            string fileName = GetFileName(countryCode, exportType, "zip");
            string hyperlink = $"<a title=\"{fileName}\" href=\"{url}\">zip</a>";
            Replace(Comment(countryCode, exportType, "Hyperlink"), hyperlink);
        }

        private static void UpdateHostsTemplate()
        {
            Replace("/*HOSTS*/", GetHostsTableRows());
        }

        private static string GetHostsTableRows()
        {
            StringBuilder rows = new();

            foreach (var website in Websites.Websites.GetApplySpecialRules()
                .OrderBy(website => website.Host, StringComparer.OrdinalIgnoreCase))
            {
                int listingsCount = website.GetNumListings();
                int publishedListingsCount = website.GetNumPublishedListings();
                int unpublishedListingsCount = website.GetNumUnpublishedListings();

                rows.AppendLine(GetHostTableRow(
                    website,
                    listingsCount,
                    publishedListingsCount,
                    unpublishedListingsCount));
            }

            return rows.ToString();
        }

        private static string GetHostTableRow(
            Website website,
            int listingsCount,
            int publishedListingsCount,
            int unpublishedListingsCount)
        {
            return
                "                <tr>" + Environment.NewLine +
                $"                    <td>{WebUtility.HtmlEncode(website.Host)}</td>" + Environment.NewLine +
                $"                    <td>{GetHostDownloadCellText(website.Host, "listings", "json", listingsCount)}</td>" + Environment.NewLine +
                $"                    <td>{GetHostDownloadCellText(website.Host, "listings_published", "json", publishedListingsCount)}</td>" + Environment.NewLine +
                $"                    <td>{GetHostDownloadCellText(website.Host, "listings_unpublished", "json", unpublishedListingsCount)}</td>" + Environment.NewLine +
                "                </tr>";
        }

        private static string GetHostDownloadCellText(string host, string downloadType, string extension, int counter)
        {
            string counterText = counter.ToString(CultureInfo.InvariantCulture);
            if (counter <= 0)
            {
                return counterText;
            }

            string? url = GetHostDownloadUrl(host, downloadType, extension);
            if (string.IsNullOrWhiteSpace(url))
            {
                return counterText;
            }

            string fileName = GetHostFileName(host, downloadType, extension);
            return $"<a title=\"Download\" href=\"{WebUtility.HtmlEncode(url)}\" download=\"{WebUtility.HtmlEncode(fileName)}\">{counterText}</a>";
        }

        private static string? GetHostDownloadUrl(string host, string downloadType, string extension)
        {
            string objectKey = GetHostObjectKey(host, downloadType, extension);
            var (lastModified, contentLength) = new S3().GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);

            if (lastModified is null || contentLength is null)
            {
                return null;
            }

            return $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
        }

        private static string GetHostObjectKey(string host, string downloadType, string extension)
        {
            return $"ES/Hosts/{GetHostFileName(host, downloadType, extension)}";
        }

        private static string GetHostFileName(string host, string downloadType, string extension)
        {
            return $"{host}_{downloadType}.{extension}";
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
