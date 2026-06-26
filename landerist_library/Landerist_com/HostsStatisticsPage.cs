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
        private const int RecentParseListingPagesDays = 3;

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
            int recentParseListingPages = website.GetNumPagesParseListingSince(DateTime.Now.AddDays(-RecentParseListingPagesDays));
            int totalListings = website.GetNumListings();
            int recentListings = website.GetNumListingsSinceListingDate(DateTime.Now.AddDays(-RecentListingsDays));
            int publishedListings = website.GetNumPublishedListings();
            int unpublishedListings = website.GetNumUnpublishedListings();

            return
                "                        <tr>" + Environment.NewLine +
                $"                            {FormatTableCell(FormatHostLink(website.Host), website.Host)}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatWebsiteDate(website.RobotsTxtUpdated), FormatDateSortValue(website.RobotsTxtUpdated))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatWebsiteDate(website.SitemapUpdated), FormatDateSortValue(website.SitemapUpdated))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatNumber(totalPages), FormatNumberSortValue(totalPages))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(recentScrapedPages, totalPages), FormatPercentageSortValue(recentScrapedPages, totalPages))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(recentInsertedPages, totalPages), FormatPercentageSortValue(recentInsertedPages, totalPages))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(recentParseListingPages, totalPages), FormatPercentageSortValue(recentParseListingPages, totalPages))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatNumber(totalListings), FormatNumberSortValue(totalListings))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(recentListings, totalListings), FormatPercentageSortValue(recentListings, totalListings))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(publishedListings, totalListings), FormatPercentageSortValue(publishedListings, totalListings))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(unpublishedListings, totalListings), FormatPercentageSortValue(unpublishedListings, totalListings))}" + Environment.NewLine +
                "                        </tr>";
        }

        private static string FormatTableCell(string html, string sortValue)
        {
            return $"<td data-sort=\"{WebUtility.HtmlEncode(sortValue)}\">{html}</td>";
        }

        private static string FormatDateSortValue(DateTime? dateTime)
        {
            return dateTime?.ToString("yyyyMMdd", CultureInfo.InvariantCulture) ?? "0";
        }

        private static string FormatNumberSortValue(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatPercentageSortValue(int value, int total)
        {
            if (total <= 0)
            {
                return decimal.Zero.ToString(CultureInfo.InvariantCulture);
            }

            return ((decimal)value / total).ToString(CultureInfo.InvariantCulture);
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
