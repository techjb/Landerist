using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Websites;
using System.Globalization;
using System.Net;
using System.Text;

namespace landerist_library.Landerist_com
{
    public class HostsPage : Landerist_com
    {
        private static readonly string HostsTemplateHtmlFile =
            Path.Combine(Config.LANDERIST_COM_TEMPLATES!, "hosts_template.html");

        private static readonly string HostsHtmlFile =
            Path.Combine(Config.LANDERIST_COM_OUTPUT!, "hosts.html");

        private const string EmptyValue = "-";

        private static string HostsTemplate = string.Empty;


        public static void Update()
        {
            try
            {
                HostsTemplate = File.ReadAllText(HostsTemplateHtmlFile);
                UpdateHostsTemplate();

                if (UploadHostsFile())
                {
                    Log.WriteInfo("HostsPage", "Updated hosts page");
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("HostsPage Update", exception);
            }
        }

        private static void UpdateHostsTemplate()
        {
            string updatedAtText = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            StringBuilder rows = new();

            foreach (var website in Websites.Websites.GetAll()
                .Where(website => website.ApplySpecialRules)
                .OrderBy(website => website.Host, StringComparer.OrdinalIgnoreCase))
            {
                int pagesCount = website.GetNumPages();
                int listingsCount = website.GetNumListings();
                int publishedListingsCount = website.GetNumPublishedListings();
                int unpublishedListingsCount = website.GetNumUnpublishedListings();

                rows.AppendLine(GetTableRow(
                    website,
                    pagesCount,
                    listingsCount,
                    publishedListingsCount,
                    unpublishedListingsCount));
            }

            HostsTemplate = HostsTemplate.Replace("/*UPDATED_AT*/", updatedAtText);
            HostsTemplate = HostsTemplate.Replace("/*CHARTS*/", rows.ToString());
        }

        private static string GetTableRow(
            Website website,
            int pagesCount,
            int listingsCount,
            int publishedListingsCount,
            int unpublishedListingsCount)
        {
            return
                "                <tr>" + Environment.NewLine +
                $"                    <td>{WebUtility.HtmlEncode(website.Host)}</td>" + Environment.NewLine +
                $"                    <td>{GetDownloadCellText(website.Host, "pages", "csv", pagesCount)}</td>" + Environment.NewLine +
                $"                    <td>{GetDownloadCellText(website.Host, "listings", "json", listingsCount)}</td>" + Environment.NewLine +
                $"                    <td>{GetDownloadCellText(website.Host, "listings_published", "json", publishedListingsCount)}</td>" + Environment.NewLine +
                $"                    <td>{GetDownloadCellText(website.Host, "listings_unpublished", "json", unpublishedListingsCount)}</td>" + Environment.NewLine +
                "                </tr>";
        }

        private static string GetDownloadCellText(string host, string downloadType, string extension, int counter)
        {
            string counterText = counter.ToString(CultureInfo.InvariantCulture);
            if (counter <= 0)
            {
                return counterText;
            }

            string? url = GetDownloadUrl(host, downloadType, extension);
            if (string.IsNullOrWhiteSpace(url))
            {
                return counterText;
            }

            string fileName = GetFileName(host, downloadType, extension);
            return $"<a title=\"Download\" href=\"{WebUtility.HtmlEncode(url)}\" download=\"{WebUtility.HtmlEncode(fileName)}\">{counterText}</a>";
        }

        private static string? GetDownloadUrl(string host, string downloadType, string extension)
        {
            string objectKey = GetObjectKey(host, downloadType, extension);
            var (lastModified, contentLength) = new S3().GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);

            if (lastModified is null || contentLength is null)
            {
                return null;
            }

            return $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
        }

        private static string GetObjectKey(string host, string downloadType, string extension)
        {
            return $"ES/Hosts/{GetFileName(host, downloadType, extension)}";
        }

        private static string GetFileName(string host, string downloadType, string extension)
        {
            return $"{host}_{downloadType}.{extension}";
        }

        private static bool UploadHostsFile()
        {
            File.WriteAllText(HostsHtmlFile, HostsTemplate);
            return new S3().UploadToWebsiteBucket(HostsHtmlFile, "index.html", "hosts");
        }
    }
}
