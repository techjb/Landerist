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
            var pagesDownload = GetDownloadInfo(website.Host, "pages", "Pages");
            var listingsDownload = GetDownloadInfo(website.Host, "listings", "Listings");

            return
                "                <tr>" + Environment.NewLine +
                $"                    <td>{WebUtility.HtmlEncode(website.Host)}</td>" + Environment.NewLine +
                $"                    <td>{pagesCount.ToString(CultureInfo.InvariantCulture)}</td>" + Environment.NewLine +
                $"                    <td>{listingsCount.ToString(CultureInfo.InvariantCulture)}</td>" + Environment.NewLine +
                $"                    <td>{publishedListingsCount.ToString(CultureInfo.InvariantCulture)}</td>" + Environment.NewLine +
                $"                    <td>{unpublishedListingsCount.ToString(CultureInfo.InvariantCulture)}</td>" + Environment.NewLine +
                $"                    <td>{GetDownloadsText(pagesDownload.Hyperlink, listingsDownload.Hyperlink)}</td>" + Environment.NewLine +
                "                </tr>";
        }

        private static HostDownloadInfo GetDownloadInfo(string host, string downloadType, string linkText)
        {
            string objectKey = GetObjectKey(host, downloadType);
            string fileName = GetFileName(host, downloadType);
            var s3 = new S3();
            var (lastModified, contentLength) = s3.GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);

            if (lastModified is null || contentLength is null)
            {
                return new HostDownloadInfo(null, null, EmptyValue);
            }

            string url = $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
            string hyperlink = $"<a title=\"{WebUtility.HtmlEncode(fileName)}\" href=\"{url}\">{WebUtility.HtmlEncode(linkText)}</a>";
            return new HostDownloadInfo(lastModified, contentLength, hyperlink);
        }

        private static string GetObjectKey(string host, string downloadType)
        {
            return $"ES/Hosts/{GetFileName(host, downloadType)}";
        }

        private static string GetFileName(string host, string downloadType)
        {
            return $"{host}_{downloadType}.csv";
        }

        private static string GetDownloadsText(string pagesLink, string listingsLink)
        {
            List<string> links = [];

            if (!string.IsNullOrWhiteSpace(pagesLink) && !string.Equals(pagesLink, EmptyValue, StringComparison.Ordinal))
            {
                links.Add(pagesLink);
            }

            if (!string.IsNullOrWhiteSpace(listingsLink) && !string.Equals(listingsLink, EmptyValue, StringComparison.Ordinal))
            {
                links.Add(listingsLink);
            }

            return links.Count == 0
                ? EmptyValue
                : string.Join(" / ", links);
        }

        private static bool UploadHostsFile()
        {
            File.WriteAllText(HostsHtmlFile, HostsTemplate);
            return new S3().UploadToWebsiteBucket(HostsHtmlFile, "index.html", "hosts");
        }

        private readonly record struct HostDownloadInfo(DateTime? LastModified, long? ContentLength, string Hyperlink);
    }
}
