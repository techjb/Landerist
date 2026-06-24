using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;
using System.Globalization;
using System.Net;
using System.Text;

namespace landerist_library.Landerist_com
{
    public class HostsStatisticsPage : Landerist_com
    {
        private static readonly string HostsStatisticsTemplateHtmlFile =
            Path.Combine(Config.LANDERIST_COM_TEMPLATES!, "hosts-statistics", "hosts_statistics_template.html");

        private static readonly string HostsStatisticsHtmlFile =
            Path.Combine(Config.LANDERIST_COM_OUTPUT!, "hosts_statistics.html");

        private static readonly CultureInfo SummaryCulture = CultureInfo.GetCultureInfo("es-ES");

        private const int RecentListingsDays = 7;
        private const int RecentScrapedPagesDays = 3;
        private const int RecentInsertedPagesDays = 3;

        public static void Update()
        {
            try
            {
                var template = File.ReadAllText(HostsStatisticsTemplateHtmlFile);
                var websites = Websites.Websites.GetAll()
                    .OrderBy(website => website.Host, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                template = template.Replace("/*UPDATED_AT*/", DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
                template = template.Replace("/*HOSTS_STATISTICS_ROWS*/", GetHostsStatisticsRows(websites));

                File.WriteAllText(HostsStatisticsHtmlFile, template);

                if (new S3().UploadToWebsiteBucket(HostsStatisticsHtmlFile, "index.html", "hosts-statistics"))
                {
                    Log.WriteInfo("HostsStatisticsPage", "Updated hosts statistics page");
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("HostsStatisticsPage Update", exception);
            }
        }

        private static string GetHostsStatisticsRows(IEnumerable<Websites.Website> websites)
        {
            StringBuilder rows = new();

            foreach (var website in websites)
            {
                rows.AppendLine(GetHostsStatisticsRow(website));
            }

            return rows.ToString();
        }

        private static string GetHostsStatisticsRow(Websites.Website website)
        {
            int totalPages = website.GetNumPages();
            int recentScrapedPages = website.GetNumPagesScrapedSince(DateTime.Now.AddDays(-RecentScrapedPagesDays));
            int recentInsertedPages = website.GetNumPagesInsertedSince(DateTime.Now.AddDays(-RecentInsertedPagesDays));
            int totalListings = website.GetNumListings();
            int recentListings = website.GetNumListingsSinceListingDate(DateTime.Now.AddDays(-RecentListingsDays));
            int publishedListings = website.GetNumPublishedListings();
            int unpublishedListings = website.GetNumUnpublishedListings();

            return
                "                        <tr>" + Environment.NewLine +
                $"                            <td>{FormatHostLink(website.Host)}</td>" + Environment.NewLine +
                $"                            <td>{FormatWebsiteDate(website.RobotsTxtUpdated)}</td>" + Environment.NewLine +
                $"                            <td>{FormatWebsiteDate(website.SitemapUpdated)}</td>" + Environment.NewLine +
                $"                            <td>{FormatNumber(totalPages)}</td>" + Environment.NewLine +
                $"                            <td>{FormatPercentage(recentScrapedPages, totalPages)}</td>" + Environment.NewLine +
                $"                            <td>{FormatPercentage(recentInsertedPages, totalPages)}</td>" + Environment.NewLine +
                $"                            <td>{FormatNumber(totalListings)}</td>" + Environment.NewLine +
                $"                            <td>{FormatPercentage(recentListings, totalListings)}</td>" + Environment.NewLine +
                $"                            <td>{FormatPercentage(publishedListings, totalListings)}</td>" + Environment.NewLine +
                $"                            <td>{FormatPercentage(unpublishedListings, totalListings)}</td>" + Environment.NewLine +
                "                        </tr>";
        }

        private static string FormatHostLink(string host)
        {
            string href = "/host-statistics/#" + Uri.EscapeDataString(host);
            return $"<a href=\"{WebUtility.HtmlEncode(href)}\">{WebUtility.HtmlEncode(host)}</a>";
        }

        private static string FormatWebsiteDate(DateTime? dateTime)
        {
            return dateTime?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string FormatNumber(int value)
        {
            return value.ToString("N0", SummaryCulture);
        }

        private static string FormatPercentage(int value, int total)
        {
            if (total <= 0)
            {
                return decimal.Zero.ToString("P1", SummaryCulture);
            }

            return ((decimal)value / total).ToString("P1", SummaryCulture);
        }
    }
}
